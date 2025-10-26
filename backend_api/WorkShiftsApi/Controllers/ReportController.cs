using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkShiftsApi.DTO;

namespace WorkShiftsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private NLog.Logger _logger;

        public ReportController(AppDbContext context) 
        { 
            _context = context;
            _logger = NLog.LogManager.GetCurrentClassLogger();
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
                && x.Date.Date >= start.Date
                && x.Date.Date < end.Date).Select(x=> new WorkHourDto
                {
                    EmployeeId = x.EmployeeId,
                    Created = x.Created,
                    Date = x.Date,
                    Hours = x.Hours,
                    Id = x.Id,
                    Rate = x.Rate,
                    ItemSalary = x.Hours * x.Rate
                }).OrderBy(x=>x.Date)
                .ToList();

                result.TotalHours = items.Select(x => x.Hours).Sum();
                result.ItemsCount = items.Count;
                result.WorkHoursList = items;

                var sum = 0;
                foreach (var item in items) {
                    sum += item.Hours * item.Rate;
                }

                result.TotalSalary = sum;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.ToString();
            }

            Thread.Sleep(2000);

            return Ok(result);
        }
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

        public decimal TotalSalary {  get; set; }
        public int ItemsCount { get; set; }//всего записей о начислении и списании
        public int TotalHours { get; set; }
    }




}

