using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;
using System.Text.Json.Serialization;
using WorkShiftsApi.DTO;
using WorkShiftsApi.Services;

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
                var tables = new List<TableDataDto>();

                var canParseStart = DateTime.TryParse(startDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(endDate, out DateTime end);

                var list = employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => Int32.Parse(x))
                    .ToList();

                var emplList = _context.Employees.Where(x => list.Contains(x.Id)).ToList();
                //для ведомости
                var vedEmplList = emplList.Where(x => x.EmplOptions == EmplOptionEnums.Vedomost);
                var vedIds = vedEmplList.Select(x => x.Id).ToList();
                emplList = emplList.Where(x => !vedIds.Contains(x.Id)).ToList();


                var resultTable1 = _employeeService.GetReportForEmplList2(start, end, vedIds);
                tables.Add(new TableDataDto { Title = "Расчет по ведомости", DataTable = resultTable1 });

                //карты

                foreach (var b in Banks.BanksList)
                {
                    var empListN = emplList.Where(x => x.BankName.Trim().ToLower() == b.Trim().ToLower());
                    if (empListN.Count() == 0)
                        continue;


                    var resultTableN = _employeeService.GetReportForEmplList2(start, end, empListN.Select(x => x.Id).ToList());
                    tables.Add(new TableDataDto { Title = "Расчет для карт банка " + b , DataTable = resultTableN });

                }

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

        private void GetEmplList(List<int> ids)
        {

        }

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

        }



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

    public class EmplOptionEnums
    {
        public static readonly string Vedomost = "Ведомость";
        public static readonly string Card = "Карта";
    }

    public class Banks
    {
        public static List<string> BanksList { get; set; } = new List<string> {"ВТБ", "Альфа", "Т-Банк", "Сбер Банк" };
    }


}

