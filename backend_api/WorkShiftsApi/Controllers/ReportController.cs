using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using WorkShiftsApi.DbEntities;
using WorkShiftsApi.DTO;
using WorkShiftsApi.Services;


namespace WorkShiftsApi.Controllers
{
    [ApiController]
    [Route("api/report")]
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
        [Authorize]
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

        /// <summary>
        /// Детальный финансовый отчет для одного сотрудника
        /// (объединяет work_hours, work_days и fin_operations)
        /// </summary>
        [HttpPost("GetEmployeeFinancialReport")]
        [Authorize]
        public ActionResult GetEmployeeFinancialReport([FromBody] GetReportRequest request)
        {
            var result = new EmployeeFinancialReportResponse
            {
                IsSuccess = true,
                Message = "Отчет сформирован"
            };

            try
            {
                var canParseStart = DateTime.TryParse(request.StartDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(request.EndDate, out DateTime end);

                if (!canParseStart || !canParseEnd)
                    throw new Exception("Ошибка при разборе даты");

                end = end.AddDays(1);

                var employeeId = request.EmployeeId;

                // Рабочие часы
                var workHours = (from wh in _context.WorkHours
                                 join emp in _context.Employees on wh.EmployeeId equals emp.Id
                                 join obj in _context.Objects on emp.ObjectId equals obj.Id
                                 where wh.EmployeeId == employeeId
                                       && wh.WorkDate.Date >= start.Date
                                       && wh.WorkDate.Date < end.Date
                                 select new
                                 {
                                     wh.Id,
                                     Date = wh.WorkDate.Date,
                                     wh.Hours,
                                     wh.Rate,
                                     ObjectName = obj.Name
                                 }).ToList();

                var workHourIds = workHours.Select(x => x.Id).ToList();
                var whReportRecords = _context.RevenueReportsWh
                    .Where(x => workHourIds.Contains(x.WorkHoursId))
                    .GroupBy(x => x.WorkHoursId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.ReportNumber).First());

                // Рабочие дни
                var workDays = (from wd in _context.WorkDays
                                join emp in _context.Employees on wd.EmployeeId equals emp.Id
                                join obj in _context.Objects on emp.ObjectId equals obj.Id
                                where wd.EmployeeId == employeeId
                                      && wd.WorkDate.Date >= start.Date
                                      && wd.WorkDate.Date < end.Date
                                select new
                                {
                                    wd.Id,
                                    Date = wd.WorkDate.Date,
                                    wd.Rate,
                                    ObjectName = obj.Name
                                }).ToList();

                // Финансовые операции
                var finOps = (from fo in _context.FinOperations
                              join t in _context.FinOperationTypes on fo.TypeId equals t.Id into ft
                              from t in ft.DefaultIfEmpty()
                              where fo.EmployeeId == employeeId
                                    && fo.Date.Date >= start.Date
                                    && fo.Date.Date < end.Date
                              select new
                              {
                                  fo.Id,
                                  Date = fo.Date.Date,
                                  fo.Sum,
                                  fo.IsPenalty,
                                  fo.Comment,
                                  TypeId = fo.TypeId,
                                  TypeName = t != null ? t.OperationName : null
                              }).ToList();

                var finOpIds = finOps.Select(x => x.Id).ToList();
                var finReportRecords = _context.RevenueReportsFin
                    .Where(x => finOpIds.Contains(x.FinOperationId))
                    .GroupBy(x => x.FinOperationId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.ReportNumber).First());

                var items = new List<EmployeeFinancialReportRowDto>();

                // Заполняем строки по рабочим часам
                foreach (var wh in workHours)
                {
                    var amount = wh.Hours * wh.Rate;
                    var description =
                        $"Объект: {wh.ObjectName}. Рабочие часы: {wh.Hours}. Ставка в час: {wh.Rate} руб.";

                    var accountingInfo = "--";
                    bool? payOff = null;
                    if (whReportRecords.TryGetValue(wh.Id, out var rrWh))
                    {
                        accountingInfo = $"Учтен в отчете #{rrWh.ReportNumber}";
                        payOff = rrWh.PayOff != 0;
                    }

                    items.Add(new EmployeeFinancialReportRowDto
                    {
                        Date = wh.Date,
                        Description = description,
                        Amount = amount,
                        AccountingInfo = accountingInfo,
                        PayOff = payOff
                    });

                    result.TotalHours += wh.Hours;
                    result.TotalWorkHoursAmount += amount;
                }

                // Заполняем строки по рабочим дням
                foreach (var wd in workDays)
                {
                    var amount = wd.Rate;
                    var description =
                        $"Объект: {wd.ObjectName}. Рабочая смена. Ставка за смену: {wd.Rate} руб.";

                    // Для рабочих дней пока нет отметок об учете
                    var accountingInfo = "--";

                    items.Add(new EmployeeFinancialReportRowDto
                    {
                        Date = wd.Date,
                        Description = description,
                        Amount = amount,
                        AccountingInfo = accountingInfo,
                        PayOff = null
                    });

                    result.TotalWorkDays += 1;
                    result.TotalWorkDaysAmount += amount;
                }

                // Заполняем строки по финансовым операциям
                foreach (var fo in finOps)
                {
                    var isPenalty = fo.IsPenalty;
                    var sign = isPenalty ? -1 : 1;
                    var amount = sign * fo.Sum;

                    var typeName = string.IsNullOrWhiteSpace(fo.TypeName) ? "Без типа" : fo.TypeName;

                    var descriptionPrefix = isPenalty ? "Списание" : "Начисление";
                    var description =
                        $"{descriptionPrefix}. {typeName}. {fo.Sum} руб.";

                    if (!string.IsNullOrWhiteSpace(fo.Comment))
                    {
                        description += $" Комментарий: {fo.Comment}";
                    }

                    var accountingInfo = "--";
                    bool? payOff = null;
                    if (finReportRecords.TryGetValue(fo.Id, out var rrFin))
                    {
                        accountingInfo = $"Учтен в отчете #{rrFin.ReportNumber}";
                        payOff = rrFin.PayOff != 0;
                    }

                    items.Add(new EmployeeFinancialReportRowDto
                    {
                        Date = fo.Date,
                        Description = description,
                        Amount = amount,
                        AccountingInfo = accountingInfo,
                        PayOff = payOff
                    });

                    if (isPenalty)
                    {
                        result.TotalPenalties += fo.Sum;
                    }
                    else
                    {
                        result.TotalBonuses += fo.Sum;
                    }
                }

                // Сортировка по дате
                result.Items = items
                    .OrderBy(x => x.Date)
                    .ThenBy(x => x.Description)
                    .ToList();

                // Итоговая сумма: работа + начисления - списания
                result.TotalSalary =
                    result.TotalWorkHoursAmount +
                    result.TotalWorkDaysAmount +
                    result.TotalBonuses -
                    result.TotalPenalties;

                result.ItemsCount = result.Items.Count;
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
        /// Финансовый отчёт ver.3: неоплаченные смены/часы и операции за период [startDate, endDate).
        /// </summary>
        [HttpGet("GetMainReportVer3AsXls")]
        [AllowAnonymous]
        public ActionResult GetMainReportVer3AsXls([FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] string employees)
        {
            try
            {
                if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
                    return BadRequest("Некорректные даты.");

                var list = employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => int.Parse(x))
                    .ToList();

                var emplList = _context.Employees.Where(x => list.Contains(x.Id)).ToList();
                var mrData = _employeeService.PrepareMainReportDataVer3(start, end, emplList);
                _employeeService.GenerateTableForMainReportVer3(mrData, out var table);
                var fileBytes = _excelGenerator.CreateExcelFromMainReportVer3Table(table);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Отчет_ver_3_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка генерации Excel (ver.3): {ex.Message}");
            }
        }


        /// <summary>
        /// Самая актуальная версия отчета на 1.04.2026
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="employees"></param>
        /// <returns></returns>
        [HttpGet("GetMainReportVer4AsXls")]
        [AllowAnonymous]
        public ActionResult GetMainReportVer4AsXls([FromQuery] string startDate, [FromQuery] string endDate,
            [FromQuery] string employees)
        {
            try
            {
                if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
                    return BadRequest("Некорректные даты.");

                var list = employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => int.Parse(x))
                    .ToList();

                var emplList = _context.Employees.Where(x => list.Contains(x.Id)).ToList();
                var mrData = _employeeService.PrepareMainReportDataVer3(start, end, emplList);
                var fileBytes = _excelGenerator.CreateExcelFromMainReportVer4Table(mrData);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Отчет для списка сотрудников_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка генерации Excel (ver.4): {ex.Message}");
            }
        }

        /// <summary>
        /// Та же таблица, что в GetMainReportVer4AsXls, для отображения на странице (JSON).
        /// </summary>
        [HttpGet("GetMainReportVer4AsTable")]
        [AllowAnonymous]
        public ActionResult GetMainReportVer4AsTable([FromQuery] string startDate, [FromQuery] string endDate,
            [FromQuery] string employees)
        {
            var result = new GetMainReportVer4AsTableResponse { IsSuccess = true, Message = "" };

            try
            {
                if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
                    return BadRequest("Некорректные даты.");

                var list = employees
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => int.Parse(x))
                    .ToList();

                var emplList = _context.Employees.Where(x => list.Contains(x.Id)).ToList();
                var mrData = _employeeService.PrepareMainReportDataVer3(start, end, emplList);
                result.Rows = MainReportVer4TableBuilder.Build(mrData);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                return Problem(ex.Message, "", (int)HttpStatusCode.InternalServerError);
            }

            return Ok(result);
        }


        /*private void CreateReportTableFromTablesArray(List<TableDataDto> tables)
        {
            var allTablesSum = 0;

            int rowNumber = 1;
            foreach (var tData in tables)
            {

            }

        }*/

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
                end = end.AddDays(1);

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
                end = end.AddDays(1);//выбранная дата "до" тоже учитывается в отчете

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

                // Получаем следующий номер отчета: макс. report_number из main_report_numbers + 1
                int reportNumber = 1;
                if (_context.MainReportNumbers.Any())
                {
                    reportNumber = _context.MainReportNumbers.Max(x => x.ReportNumber) + 1;
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
        /// Сохранить отметки об оплате для отчета.
        /// Новая версия. 8.04.2026
        /// </summary>
        [HttpPost("SavePayoutMarks2")]
        [Authorize]
        public async Task<IActionResult> SavePayoutMarks2([FromBody] SavePayoutMarksRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Отметки об оплате сохранены" };

            try
            {
                var canParseStart = DateTime.TryParse(request.StartDate, out DateTime start);
                var canParseEnd = DateTime.TryParse(request.EndDate, out DateTime end);
                end = end.AddDays(1);//день "до" тоже включаем в выгрузку

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
                var createAuthor = User.Identity?.Name ?? "";
                var reportNumber = await _employeeService.SavePayoutMarksLogic(createAuthor, start, end, employeeIds);
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


        //Устаревший вариант через RevenueReportsFin

        /// <summary>
        /// Восстановить отчет по сохраненным отметкам об оплате (по report_number)
        /// </summary>
        /*[HttpGet("GetMainReportFromPayoutMarks")]
        [Authorize]
        public ActionResult GetMainReportFromPayoutMarks([FromQuery] int reportNumber)
        {
            var result = new GetMainReportForPeriodAsTableResponse { IsSuccess = true, Message = "" };

            try
            {
                // Проверяем наличие отчета по таблице main_report_numbers
                if (!_context.MainReportNumbers.Any(x => x.ReportNumber == reportNumber))
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

                var banks = _context.Banks.ToList();

                // Разные типы карт
                foreach (var b in banks)
                {
                    var empListN = otherEmplList.Where(x => x.Id == b.Id);
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
        }*/

        /// <summary>
        /// Восстановить отчет по сохраненным отметкам об оплате (по report_number)
        /// новая версия
        /// </summary>
        [HttpGet("GetMainReportFromPayoutMarks2")]
        [Authorize]
        public ActionResult GetMainReportFromPayoutMarks2([FromQuery] int reportNumber)
        {
            var result = new GetMainReportFromPayoutMarks2Response { IsSuccess = true, Message = "" };

            try
            {
                // Проверяем наличие отчета по таблице main_report_numbers
                if (!_context.MainReportNumbers.Any(x => x.ReportNumber == reportNumber))
                {
                    result.IsSuccess = false;
                    result.Message = $"Отчет с номером {reportNumber} не найден";
                    return Ok(result);
                }
             
                result.Items = _employeeService.GetPayoutMarksTableData(reportNumber);

            }
            catch(Exception ex)
            {
                _logger.Error(ex.ToString());
                result.IsSuccess = false;
                result.Message = ex.Message;
                return Problem(ex.Message, "", (int)HttpStatusCode.InternalServerError);
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public ActionResult MarkPayoutRow([FromBody] MarkPayoutRowRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Отметка о выплате проставлена" };

            try
            {
                //todo проверки



                var report = _employeeService.GetMainReportDataVer3(request.ReportNumber);
                if (report == null)
                    throw new Exception("Отчет номер " + request.ReportNumber + " не найден");
                var reportForEmployee = report.EmployeeFinDatas.FirstOrDefault(x=>x.EmployeeId == request.EmployeeId);
                if (reportForEmployee == null)
                    throw new Exception("В отчете не найдены данные для сотрудника");

                //если есть аванс то помечаем выплаченым только его
                if (reportForEmployee.AdvancePaymentInPeriod)
                {
                    //ищем
                    var fo = _context.FinOperations
                        .Where(x => x.TypeId == (int)FinOperationTypeEnum.AdvancePayment
                        && x.EmployeeId == request.EmployeeId
                        && x.ReportNumber == request.ReportNumber);
                    
                    if (fo == null)
                        throw new Exception("Не найден аванс");
                }
                else
                {
                    var wh = _context.WorkHours.Where(x => x.ReportNumber == request.ReportNumber 
                    && x.EmployeeId == request.EmployeeId);

                    foreach (var f in wh)
                        f.Payed = true;

                    var wd = _context.WorkDays.Where(x => x.ReportNumber == request.ReportNumber
                    && x.EmployeeId == request.EmployeeId);

                    foreach (var f in wd)
                        f.Payed = true;

                    //фин операции
                    var fo = _context.FinOperations.Where(x => x.ReportNumber == request.ReportNumber
                    && x.EmployeeId == request.EmployeeId);

                    foreach (var f in wd)
                        f.Payed = true;



                }

                _context.SaveChanges();


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
        /// Получить статусы выплат (PayOff) по сотрудникам для сохраненного отчета
        /// </summary>
        [HttpGet("GetPayOffStatusesForReport")]
        [Authorize]
        public ActionResult GetPayOffStatusesForReport([FromQuery] int reportNumber)
        {
            var result = new GetPayOffStatusesForReportResponse { IsSuccess = true, Message = "" };

            try
            {
                // Проверяем наличие отчета по таблице main_report_numbers
                if (!_context.MainReportNumbers.Any(x => x.ReportNumber == reportNumber))
                {
                    result.IsSuccess = false;
                    result.Message = $"Отчет с номером {reportNumber} не найден";
                    return Ok(result);
                }

                // Собираем данные по PayOff из обеих таблиц, сгруппированные по сотруднику
                var whQuery = from rr in _context.RevenueReportsWh
                              join wh in _context.WorkHours on rr.WorkHoursId equals wh.Id
                              where rr.ReportNumber == reportNumber
                              select new { wh.EmployeeId, rr.PayOff };

                var finQuery = from rf in _context.RevenueReportsFin
                               join fo in _context.FinOperations on rf.FinOperationId equals fo.Id
                               where rf.ReportNumber == reportNumber
                               select new { fo.EmployeeId, rf.PayOff };

                var grouped = whQuery
                    .Concat(finQuery)
                    .GroupBy(x => x.EmployeeId)
                    .Select(g => new
                    {
                        EmployeeId = g.Key,
                        PayOff = g.Max(v => v.PayOff) > 0
                    })
                    .ToList();

                if (grouped.Count == 0)
                {
                    result.Items = new List<EmployeePayOffStatusDto>();
                    return Ok(result);
                }

                var empIds = grouped.Select(x => x.EmployeeId).ToList();
                var employees = _context.Employees
                    .Where(e => empIds.Contains(e.Id))
                    .ToList();

                result.Items = grouped
                    .Join(
                        employees,
                        g => g.EmployeeId,
                        e => e.Id,
                        (g, e) => new EmployeePayOffStatusDto
                        {
                            EmployeeId = g.EmployeeId,
                            Fio = e.Fio,
                            PayOff = g.PayOff
                        })
                    .ToList();
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
        /// Установить/снять отметку выплаты (PayOff) для сотрудника в сохраненном отчете
        /// </summary>
        [HttpPost("SetPayOffForEmployeeInReport")]
        [Authorize]
        public async Task<IActionResult> SetPayOffForEmployeeInReport([FromBody] SetPayOffForEmployeeInReportRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Статус выплаты обновлен" };

            try
            {
                if (string.IsNullOrWhiteSpace(request.Fio))
                {
                    result.IsSuccess = false;
                    result.Message = "Не указано ФИО сотрудника";
                    return Ok(result);
                }

                // Находим сотрудников по ФИО
                var employeeIds = _context.Employees
                    .Where(e => e.Fio == request.Fio)
                    .Select(e => e.Id)
                    .ToList();

                if (employeeIds.Count == 0)
                {
                    result.IsSuccess = false;
                    result.Message = $"Сотрудник с ФИО '{request.Fio}' не найден";
                    return Ok(result);
                }

                var payOffValue = request.PayOff ? 1 : 0;

                // Обновляем PayOff для записей revenue_reports_wh
                var whToUpdate = from rr in _context.RevenueReportsWh
                                 join wh in _context.WorkHours on rr.WorkHoursId equals wh.Id
                                 where rr.ReportNumber == request.ReportNumber
                                       && employeeIds.Contains(wh.EmployeeId)
                                 select rr;

                foreach (var rr in whToUpdate)
                {
                    rr.PayOff = payOffValue;
                }

                // Обновляем PayOff для записей revenue_reports_fin
                var finToUpdate = from rf in _context.RevenueReportsFin
                                  join fo in _context.FinOperations on rf.FinOperationId equals fo.Id
                                  where rf.ReportNumber == request.ReportNumber
                                        && employeeIds.Contains(fo.EmployeeId)
                                  select rf;

                foreach (var rf in finToUpdate)
                {
                    rf.PayOff = payOffValue;
                }

                await _context.SaveChangesAsync();
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

    public class GetMainReportVer4AsTableResponse : ResponseBase
    {
        [JsonPropertyName("rows")]
        public List<MainReportVer4WebRowDto> Rows { get; set; } = new();
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

    public class EmployeeFinancialReportRowDto
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = "";
        public int Amount { get; set; }
        public string AccountingInfo { get; set; } = "";
        /// <summary>Признак выплаты (из revenue_reports_wh / revenue_reports_fin). null — не в отчете.</summary>
        public bool? PayOff { get; set; }
    }

    public class EmployeeFinancialReportResponse : ResponseBase
    {
        public List<EmployeeFinancialReportRowDto> Items { get; set; } = new List<EmployeeFinancialReportRowDto>();

        public int ItemsCount { get; set; }

        public int TotalHours { get; set; }
        public int TotalWorkDays { get; set; }

        public int TotalWorkHoursAmount { get; set; }
        public int TotalWorkDaysAmount { get; set; }

        public int TotalPenalties { get; set; }
        public int TotalBonuses { get; set; }

        public int TotalSalary { get; set; }
    }

    public class SavePayoutMarksRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Employees { get; set; }
    }

    public class SetPayOffForEmployeeInReportRequest
    {
        public int ReportNumber { get; set; }
        public string Fio { get; set; } = "";
        public bool PayOff { get; set; }
    }

    public class EmployeePayOffStatusDto
    {
        public int EmployeeId { get; set; }
        public string Fio { get; set; } = "";
        public bool PayOff { get; set; }
    }

    public class GetPayOffStatusesForReportResponse : ResponseBase
    {
        public List<EmployeePayOffStatusDto> Items { get; set; } = new List<EmployeePayOffStatusDto>();
    }

    public class EmplOptionEnums
    {
        public static readonly string Vedomost = "Ведомость";
        public static readonly string Card = "Карта";
    }

    /*public class Banks
    {
        public static List<string> BanksList { get; set; } = new List<string> {"ВТБ", "Альфа", "Т-Банк", "Сбер", "ПСБ" };
    }*/

    public class MarkPayoutRowRequest
    {
        public int ReportNumber { get; set; }
        public int EmployeeId { get; set; }
    }

}

