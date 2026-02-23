using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using WorkShiftsApi.DTO;
using WorkShiftsApi.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WorkShiftsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private NLog.Logger _logger;

        private readonly ExcelGenerator _excelGenerator = new();
        private EmployeeService _employeeService;

        public ReportController(AppDbContext context)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _context = context;
            _employeeService = new EmployeeService(context);
        }


        /// <summary>
        /// Таблица на сайте для одного сотрудника
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetWorkHoursForPeriod")]
        public ActionResult GetWorkHoursForPeriod([FromBody] GetReportRequest request)
        {
            var result = new GetReportResponse { IsSuccess = true, Message = "Отчет сформирован" };

            try
            {
                //парсим даты 
                var canParseStart = DateTime.TryParse(request.StartDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(request.EndDate, out DateTime end);

                if (!canParseStart || !canParseEnd)
                    throw new Exception("Ошибка при разборе даты");

                end = end.AddDays(1);//до начала следующей даты

                var items = _context.WorkHours.Where(x => x.EmployeeId == request.EmployeeId
                && x.WorkDate.Date >= start.Date
                && x.WorkDate.Date < end.Date).Select(x => new WorkHourDto
                {
                    EmployeeId = x.EmployeeId,
                    Created = x.Created,
                    Date = x.WorkDate.Date,
                    Hours = x.Hours,
                    Id = x.Id,
                    Rate = x.Rate,
                    ItemSalary = x.Hours * x.Rate
                }).OrderBy(x => x.Date)
                .ToList();

                //начисления / списания
                result.FinOperations = _context.FinOperations.Where(x => x.EmployeeId == request.EmployeeId
                && x.Date.Date >= start.Date
                && x.Date.Date < end.Date).Select(x => new FinOperationDto
                {
                    Id = x.Id,
                    Created = x.Created,
                    Date = x.Date.Date,
                    EmployeeId = x.EmployeeId,
                    IsPenalty = x.IsPenalty,
                    Sum = x.Sum,
                    TypeId = x.TypeId
                }).ToList();

                result.TotalHours = items.Select(x => x.Hours).Sum();
                result.ItemsCount = items.Count;
                result.WorkHoursList = items;
                //
                var totalPenalty = result.FinOperations.Where(x => x.IsPenalty).Sum(x => x.Sum);
                var totalBonus = result.FinOperations.Where(x => !x.IsPenalty).Sum(x => x.Sum);
                var sum = 0;
                foreach (var item in items) {
                    sum += item.Hours * item.Rate;
                }

                result.TotalSalary = sum + totalBonus - totalPenalty;

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.ToString();
            }

            Thread.Sleep(1000);

            return Ok(result);
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public ActionResult Test()
        {

            var resultTable = _employeeService.GetReportForEmplList(new DateTime(2025, 10, 1), new DateTime(2025, 12, 30), new List<int> { 1, 2 });


            //var dataTable = GetSampleDataTable();

            try
            {
                var fileBytes = _excelGenerator.CreateExcelFromDataTable(resultTable, "Отчет");

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка генерации Excel: {ex.Message}");
            }


            //return Ok();
        }


        [HttpGet("GetMainReportForPeriodAsXls")]
        [AllowAnonymous]
        public ActionResult GetMainReportForPeriodAsXls([FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] string employees)
        {

            try
            {
                var canParseStart = DateTime.TryParse(startDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(endDate, out DateTime end);

                var list = employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => Int32.Parse(x))
                    .ToList();


                var resultTable = _employeeService.GetReportForEmplList(start, end, list);
                var fileBytes = _excelGenerator.CreateExcelFromDataTable(resultTable, "Отчет");

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка генерации Excel: {ex.Message}");
            }

        }

        private void CreateReportTableFromTablesArray(List<TableDataDto> tables)
        {
            var allTablesSum = 0;

            int rowNumber = 1;
            foreach (var tData in tables)
            {

            }

        }

        /// <summary>
        /// Выгрузка на сайте по списку сотрудников с разбивкой на банки
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="employees"></param>
        /// <returns></returns>
        [HttpGet("GetMainReportForPeriodAsXlsWithBanks")]
        [AllowAnonymous]
        public ActionResult GetMainReportForPeriodAsXlsWithBanks([FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] string employees)
        {

            try
            {
                var canParseStart = DateTime.TryParse(startDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(endDate, out DateTime end);

                var list = employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => Int32.Parse(x))
                    .ToList();

                var emplList = _context.Employees.Where(x => list.Contains(x.Id)).ToList();

                var tables = _employeeService.CreateMainReportTablesList(start, end, emplList);
                var fileBytes = _excelGenerator.CreateExcelFromDataTables(tables, "Отчет");

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка генерации Excel: {ex.Message}");
            }
        
        }





        /// <summary>
        /// Отчет для отображения на странице с разделением по банкам
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="employees"></param>
        /// <returns></returns>
        [HttpGet("GetMainReportForPeriodAsTableWithBanks")]
        [AllowAnonymous]
        public ActionResult GetMainReportForPeriodAsTableWithBanks([FromQuery] string startDate,
                [FromQuery] string endDate,
                [FromQuery] string employees)
        {

            var result = new GetMainReportForPeriodAsTableResponse { IsSuccess = true, Message = "" };

            try
            {
                var canParseStart = DateTime.TryParse(startDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(endDate, out DateTime end);

                var list = employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => Int32.Parse(x))
                    .ToList();


                var empList = _context.Employees.Where(x => list.Contains(x.Id)).ToList();
                var tables = _employeeService.CreateMainReportTablesList(start, end, empList);
                var table = _employeeService.SplitMainReportTablesList(tables);

                //пока одна таблица
                //var table = tables[0].DataTable;

                var resultTable = new MainReportTable();
                int rowCount = table.Rows.Count;
                int colCount = table.Columns.Count;

                resultTable.Items = new string[rowCount][];

                for (int i = 0; i < rowCount; i++)
                {
                    resultTable.Items[i] = new string[colCount];

                    for (int j = 0; j < colCount; j++)
                    {
                        resultTable.Items[i][j] = table.Rows[i][j]?.ToString() ?? string.Empty;
                    }
                }


                /*
                resultTable.Rows = new MrRow[table.Rows.Count];


                var i = 0;
                foreach (DataRow row in table.Rows)
                {
                    var ggg = row.ItemArray;
                    var ggg1 = GetStrArray(row.ItemArray);

                    resultTable.Rows[i] = new MrRow();
                    resultTable.Rows[i].RowItems = ggg1;
                    i += 1;

                }*/


                result.Table = resultTable;
            }
            catch (Exception ex)
            {
                //return StatusCode(500, $"Ошибка генерации Excel: {ex.Message}");
                result.IsSuccess = false;
                result.Message = ex.Message;
                return Problem(ex.Message, "", (int)HttpStatusCode.InternalServerError);
            }


            return Ok(result);

        }


        /*
        [HttpGet("GetMainReportForPeriodAsTable")]
        [AllowAnonymous]
        public ActionResult GetMainReportForPeriodAsTable([FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] string employees)
        {

            var result = new GetMainReportForPeriodAsTableResponse { IsSuccess = true, Message = "" };

            try
            {
                var canParseStart = DateTime.TryParse(startDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(endDate, out DateTime end);

                var list = employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => Int32.Parse(x))
                    .ToList();

                var resultTable = _employeeService.GetReportForEmplList(start, end, list);

                var resultList = new List<string[]>();
                foreach (DataRow row in resultTable.Rows)
                {
                    var arr = row.ItemArray.Select(x => x?.ToString()).ToArray();
                    resultList.Add(arr);
                }

                var table = new MainReportTable();
                table.Items = resultList.ToArray();
                result.Table = table;
            }
            catch (Exception ex)
            {
                //return StatusCode(500, $"Ошибка генерации Excel: {ex.Message}");
                result.IsSuccess = false;
                result.Message = ex.Message;
                return Problem(ex.Message, "", (int)HttpStatusCode.InternalServerError);
            }


            return Ok(result);

        }*/


        /// <summary>
        /// //отчет для списка сотрудников для отображения на странице. версия 2
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="employees"></param>
        /// <returns></returns>
        /*[HttpGet("GetMainReportForPeriodAsTable2")]
        [AllowAnonymous]
        public ActionResult GetMainReportForPeriodAsTable2([FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] string employees)
        {

            var result = new GetMainReportForPeriodAsTableResponse { IsSuccess = true, Message = "" };

            try
            {
                var canParseStart = DateTime.TryParse(startDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(endDate, out DateTime end);

                var list = employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => Int32.Parse(x))
                    .ToList();

                var resultTable = _employeeService.GetReportForEmplList(start, end, list);

                var resultList = new List<string[]>();
                foreach (DataRow row in resultTable.Rows)
                {
                    var arr = row.ItemArray.Select(x => x?.ToString()).ToArray();
                    resultList.Add(arr);
                }

                var table = new MainReportTable();
                table.Items = resultList.ToArray();
                result.Table = table;
            }
            catch (Exception ex)
            {
                //return StatusCode(500, $"Ошибка генерации Excel: {ex.Message}");
                result.IsSuccess = false;
                result.Message = ex.Message;
                return Problem(ex.Message, "", (int)HttpStatusCode.InternalServerError);
            }


            return Ok(result);

        }*/

        /// <summary>
        /// Сохранить отметки об оплате для отчета
        /// </summary>
        [HttpPost("SavePayoutMarks")]
        [Authorize]
        public async Task<IActionResult> SavePayoutMarks([FromBody] SavePayoutMarksRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Отметки об оплате сохранены" };

            try
            {
                var canParseStart = DateTime.TryParse(request.StartDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(request.EndDate, out DateTime end);

                if (!canParseStart || !canParseEnd)
                {
                    result.IsSuccess = false;
                    result.Message = "Ошибка при разборе даты";
                    return Ok(result);
                }

                var employeeIds = request.Employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => Int32.Parse(x))
                    .ToList();

                if (employeeIds.Count == 0)
                {
                    result.IsSuccess = false;
                    result.Message = "Не указаны сотрудники";
                    return Ok(result);
                }

                // Получаем следующий номер отчета
                int reportNumber = 1;
                if (_context.RevenueReportsFin.Any())
                {
                    var maxReportNumber = _context.RevenueReportsFin
                        .Select(x => x.ReportNumber)
                        .Max();
                    
                    reportNumber = (int)(maxReportNumber + 1);
                }

                // Сохраняем запись в MainReportNumbers
                var createAuthor = User.Identity?.Name ?? "";
                var mainReportRecord = new MainReportNumbersDb
                {
                    Created = DateTime.Now,
                    CreateAuthor = createAuthor,
                    ReportNumber = reportNumber,
                    StartDate = start,
                    EndDate = end
                };
                _context.MainReportNumbers.Add(mainReportRecord);

                // Получаем все work_hours для выбранных сотрудников за период
                var workHoursList = _context.WorkHours
                    .Where(x => employeeIds.Contains(x.EmployeeId)
                        && x.WorkDate.Date >= start.Date
                        && x.WorkDate.Date < end.Date)
                    .ToList();

                // Сохраняем work_hours в revenue_reports_wh
                foreach (var wh in workHoursList)
                {
                    var revenueWh = new RevenueReportWhDb
                    {
                        Created = DateTime.Now,
                        WorkHoursId = wh.Id,
                        WhHours = wh.Hours,
                        WhRate = wh.Rate,
                        WhSum = wh.Hours * wh.Rate,
                        WhWorkDate = wh.WorkDate,
                        ReportNumber = reportNumber
                    };
                    _context.RevenueReportsWh.Add(revenueWh);
                }

                // Получаем все fin_operations для выбранных сотрудников за период
                var finOperationsList = _context.FinOperations
                    .Where(x => employeeIds.Contains(x.EmployeeId)
                        && x.Date.Date >= start.Date
                        && x.Date.Date < end.Date)
                    .ToList();

                // Сохраняем fin_operations в revenue_reports_fin
                foreach (var fo in finOperationsList)
                {
                    var revenueFin = new RevenueReportFinDb
                    {
                        Created = DateTime.Now,
                        FinOperationId = fo.Id,
                        FoSum = fo.Sum,
                        FoIsPenalty = fo.IsPenalty,
                        FoTypeId = fo.TypeId,
                        ReportNumber = reportNumber
                    };
                    _context.RevenueReportsFin.Add(revenueFin);
                }

                await _context.SaveChangesAsync();

                result.Message = $"Отметки об оплате сохранены. Номер отчета: {reportNumber}";
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return Ok(result);
        }

        /// <summary>
        /// Восстановить отчет по сохраненным отметкам об оплате (по report_number)
        /// </summary>
        [HttpGet("GetMainReportFromPayoutMarks")]
        [Authorize]
        public ActionResult GetMainReportFromPayoutMarks([FromQuery] int reportNumber)
        {
            var result = new GetMainReportForPeriodAsTableResponse { IsSuccess = true, Message = "" };

            try
            {
                // Проверяем, существует ли отчет с таким номером
                if (!_context.RevenueReportsFin.Any(x => x.ReportNumber == reportNumber) &&
                    !_context.RevenueReportsWh.Any(x => x.ReportNumber == reportNumber))
                {
                    result.IsSuccess = false;
                    result.Message = $"Отчет с номером {reportNumber} не найден";
                    return Ok(result);
                }

                // Получаем список сотрудников из сохраненных данных для разбивки по банкам
                var whIds = _context.RevenueReportsWh
                    .Where(x => x.ReportNumber == reportNumber)
                    .Select(x => x.WorkHoursId)
                    .ToList();

                var workHoursList = _context.WorkHours
                    .Where(x => whIds.Contains(x.Id))
                    .ToList();

                var employeeIds = workHoursList.Select(x => x.EmployeeId).Distinct().ToList();
                var empList = _context.Employees.Where(x => employeeIds.Contains(x.Id)).ToList();

                // Создаем таблицы с разбивкой по банкам
                var tables = new List<TableDataDto>();

                // Для ведомости
                var vedEmplList = empList.Where(x => x.EmplOptions == EmplOptionEnums.Vedomost);
                var vedIds = vedEmplList.Select(x => x.Id).ToList();
                var otherEmplList = empList.Where(x => !vedIds.Contains(x.Id)).ToList();

                if (vedIds.Count > 0)
                {
                    _employeeService.GetReportForEmplListFromPayoutMarksByEmployees(reportNumber, vedIds, out DataTable resultTable1, out int tableSum);
                    if (resultTable1.Rows.Count > 0)
                    {
                        tables.Add(new TableDataDto { Title = "Расчет по ведомости", DataTable = resultTable1, TotalSum = tableSum });
                    }
                }

                // Разные типы карт
                foreach (var b in Banks.BanksList)
                {
                    var empListN = otherEmplList.Where(x => x.BankName?.Trim().ToLower() == b.Trim().ToLower());
                    if (empListN.Count() == 0)
                        continue;

                    var ids = empListN.Select(x => x.Id).ToList();
                    _employeeService.GetReportForEmplListFromPayoutMarksByEmployees(reportNumber, ids, out DataTable resultTableN, out int tableSumN);
                    if (resultTableN.Rows.Count > 0)
                    {
                        tables.Add(new TableDataDto
                        {
                            Title = "Расчет для карт банка " + b,
                            DataTable = resultTableN,
                            TotalSum = tableSumN
                        });
                    }
                }

                // Если нет разбивки по банкам, создаем одну таблицу для всех
                if (tables.Count == 0)
                {
                    _employeeService.GetReportForEmplListFromPayoutMarks(reportNumber, out DataTable resultTable1, out int tableSum);
                    tables.Add(new TableDataDto { Title = "Отчет", DataTable = resultTable1, TotalSum = tableSum });
                }

                var table = _employeeService.SplitMainReportTablesList(tables);

                var resultTable = new MainReportTable();
                int rowCount = table.Rows.Count;
                int colCount = table.Columns.Count;

                resultTable.Items = new string[rowCount][];

                for (int i = 0; i < rowCount; i++)
                {
                    resultTable.Items[i] = new string[colCount];

                    for (int j = 0; j < colCount; j++)
                    {
                        resultTable.Items[i][j] = table.Rows[i][j]?.ToString() ?? string.Empty;
                    }
                }

                result.Table = resultTable;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                result.IsSuccess = false;
                result.Message = ex.Message;
                return Problem(ex.Message, "", (int)HttpStatusCode.InternalServerError);
            }

            return Ok(result);
        }

        /// <summary>
        /// Список сохраненных отчетов (MainReportNumbers)
        /// </summary>
        [HttpGet("GetMainReportNumbersList")]
        [Authorize]
        public ActionResult GetMainReportNumbersList()
        {
            var result = new GetMainReportNumbersListResponse { IsSuccess = true, Message = "" };

            try
            {
                result.Items = _context.MainReportNumbers
                    .OrderByDescending(x => x.Created)
                    .Select(x => new MainReportNumberItemDto
                    {
                        Id = x.Id,
                        Created = x.Created,
                        CreateAuthor = x.CreateAuthor ?? "",
                        ReportNumber = x.ReportNumber,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return Ok(result);
        }



    }

    public class MainReportNumberItemDto
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string CreateAuthor { get; set; } = "";
        public int ReportNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetMainReportNumbersListResponse : ResponseBase
    {
        public List<MainReportNumberItemDto> Items { get; set; } = new List<MainReportNumberItemDto>();
    }

    public class GetMainReportForPeriodAsTableResponse : ResponseBase
    {
        [JsonPropertyName("mainReportTable")]
        public MainReportTable Table { get; set; }
    }

    public class MainReportTable
    {
        [JsonPropertyName("items")]
        public string[][] Items { get; set; }

        [JsonPropertyName("rows")]
        public MrRow[] Rows { get; set; }
    }

    public class MrRow
    {
        public string[] RowItems { get; set; }
    }

    /*
    table.Columns.Add("ФИО", typeof(string));
    table.Columns.Add("Дней", typeof(int));
    table.Columns.Add("Сумма за работу", typeof(int));
    table.Columns.Add("Начисления", typeof(int));
    table.Columns.Add("Списания", typeof(int));
    table.Columns.Add("Итого", typeof(int));
    */

    public class GetReportRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public int EmployeeId { get; set; }
    }

    public class GetReportResponse : ResponseBase
    {
        public List<WorkHourDto> WorkHoursList { get; set; }

        public List<FinOperationDto> FinOperations { get; set; }

        public decimal TotalSalary { get; set; }
        public int ItemsCount { get; set; }//всего записей о начислении и списании
        public int TotalHours { get; set; }

    }

    public class GetMainReportForPeriodRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Employees { get; set; }
    }

    public class SavePayoutMarksRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Employees { get; set; }
    }

    public class EmplOptionEnums
    {
        public static readonly string Vedomost = "Ведомость";
        public static readonly string Card = "Карта";
    }

    public class Banks
    {
        public static List<string> BanksList { get; set; } = new List<string> {"ВТБ", "Альфа", "Т-Банк", "Сбер", "ПСБ" };
    }


}

