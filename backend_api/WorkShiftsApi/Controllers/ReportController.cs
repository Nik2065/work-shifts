using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkShiftsApi.DTO;
using WorkShiftsApi.Services;
using System.Data;

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


        [HttpPost("GetWorkHoursForPeriod")]
        public ActionResult GetWorkHoursForPeriod([FromBody] GetReportRequest request)
        {
            var result = new GetReportResponse {IsSuccess = true, Message = "Отчет сформирован"};

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
                && x.WorkDate.Date < end.Date).Select(x=> new WorkHourDto
                {
                    EmployeeId = x.EmployeeId,
                    Created = x.Created,
                    Date = x.WorkDate.Date,
                    Hours = x.Hours,
                    Id = x.Id,
                    Rate = x.Rate,
                    ItemSalary = x.Hours * x.Rate
                }).OrderBy(x=>x.Date)
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
                    Sum = x.Sum
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

            var result = _employeeService.GetReportForEmplList(new DateTime(2025, 10, 1), new DateTime(2025, 12, 30), new List<int> { 1 });



            return Ok();
        }


        [HttpGet("GetMainReportForPeriod")]
        [AllowAnonymous]
        public ActionResult GetMainReportForPeriod([FromQuery] string startDate, 
            [FromQuery] string endDate, 
            [FromQuery] string employees)
        {
            var dataTable = GetSampleDataTable();

            try
            {
                var fileBytes = _excelGenerator.CreateExcelFromDataTable(dataTable, "Отчет");

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

        private DataTable GetSampleDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Имя", typeof(string));
            table.Columns.Add("Дата", typeof(DateTime));
            table.Columns.Add("Сумма", typeof(decimal));

            table.Rows.Add(1, "Иван", DateTime.Now, 1500.50m);
            table.Rows.Add(2, "Мария", DateTime.Now.AddDays(-1), 2300.75m);
            table.Rows.Add(3, "Алексей", DateTime.Now.AddDays(-2), 1890.00m);

            return table;
        }

        //private DataTable GetMainReportData(DateTime start, DateTime end, List<int> emplIds)
        //{
            
        //}


    }



    public class GetReportRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public int EmployeeId { get; set; }
    }

    public class GetReportResponse : ResponseBase
    {
        public List<WorkHourDto> WorkHoursList { get; set; }

        public List<FinOperationDto> FinOperations {  get; set; }

        public decimal TotalSalary {  get; set; }
        public int ItemsCount { get; set; }//всего записей о начислении и списании
        public int TotalHours { get; set; }

    }

    public class GetMainReportForPeriodRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Employees { get; set; }
    }



}

