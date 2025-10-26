using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Runtime.Serialization;
using WorkShiftsApi.DTO;

namespace WorkShiftsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        //private IEmployeeService _employeeService;
        private readonly AppDbContext _context;
        private NLog.Logger _logger;

        public EmployeeController( AppDbContext context) 
        {
            //_employeeService = employeeService;
            _context = context;
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok();
        }

        [HttpGet("GetEmployee")]
        public IActionResult GetEmployee([FromQuery] int employeeId)
        {
            var result = new GetEmployeeResponse { IsSuccess = true, Message = ""};
            try
            {
                var one = _context.Employees.FirstOrDefault(x => x.Id == employeeId);
                if (one != null)
                {
                    result.Employee = new EmployeeDto
                    {
                        Created = one.Created,
                        Fio = one.Fio,
                        Id = one.Id,
                        Age = one.Age,
                        BankName = one.BankName,
                        ChopCertificate = one.ChopCertificate,
                        Object = one.Object,
                        EmplOptions = one.EmplOptions
                    };

                    var ws = _context.WorkShifts
                        .Where(x => x.EmployeeId == employeeId)
                        .Select(x => new WorkShiftDto
                        {
                            Created = x.Created,
                            Id = x.Id,
                            Start = x.Start,
                            End = x.End
                        }).ToList();

                    result.Employee.WorkShiftList = ws;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.ToString();
            }



            return Ok(result);
        }

        [HttpGet("GetEmployeeList")]
        public IActionResult GetEmployeeList()
        {

            var result = new GetEmployeeListResponse { IsSuccess = true, Message = "" };

            try
            {
                

                result.EmployeesList = _context.Employees
                    .Select(x => new EmployeeDto 
                    { 
                        Created = x.Created, 
                        Fio = x.Fio, 
                        Id = x.Id,
                        Age = x.Age,
                        BankName = x.BankName,
                        ChopCertificate = x.ChopCertificate,
                        Object = x.Object,
                        EmplOptions = x.EmplOptions,
                        //WorkShiftList = 
                    })
                    .ToList();


            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.ToString();
            }

            return Ok(result);
        }

        /// <summary>
        /// Получить список отработанных часов для даты по списку работников
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("GetWorkHours")]
        public IActionResult GetWorkHours([FromQuery] string? date )
        {
            var result = new GetWorkHoursResponse { IsSuccess = true, Message = "" };

            try
            {
                var list = _context.WorkHours.AsQueryable();

                if (!string.IsNullOrEmpty(date))
                {
                    var canParse = DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d);
                    if(canParse)
                    {
                        list = list.Where(x=> x.Date.Date == d.Date);
                    }
                }

                var listDto = list.Select(x => new WorkHourDto 
                    { 
                        Created = x.Created,
                        EmployeeId = x.EmployeeId,
                        Hours = x.Hours,
                        Id = x.Id,
                        Rate = x.Rate,
                    }             
                ).ToList();

                result.WorkHoursList = listDto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.ToString();
            }

            return Ok(result);

        }


        [HttpPost("SaveWorkHoursItem")]
        public IActionResult SaveWorkHoursItem([FromBody] SaveWorkHoursItemRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "" };

            try
            {
                //TODO:
                // проверка полученных значений



                var one = _context.WorkHours.FirstOrDefault(x => x.EmployeeId == request.EmployeeId && x.Date == request.Date);

                if(one == null)
                {
                    var wh = new WorkHoursDb { Created = DateTime.Now, Date = request.Date, EmployeeId = request.EmployeeId, Hours = request.Hours, Rate = request.Rate };
                    _context.WorkHours.Add(wh);
                }
                else
                {
                    one.Hours = request.Hours;
                    one.Rate = request.Rate;
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.ToString();
            }

            return Ok(result);

        }



        [HttpGet("getEmployeeWorkShifts")]
        public IActionResult GetEmployeeWorkShifts([FromQuery] int employeeId)
        {

            var result = new GetEmployeeWorkShiftsResponseDto { IsSuccess = true, Message = "" };

            try
            {
                result.WorkShiftList = _context.WorkShifts
                    .Select(x => new WorkShiftDto
                    {
                        Created = x.Created,
                        End = x.End,
                        Start = x.Start,
                        Id = x.Id,
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.Message.ToString();
            }

            return Ok(result);
        }

        /// <summary>
        /// Добавление сотрудника
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateEmployee")]
        public IActionResult CreateEmployee([FromBody] CreateEmployeeRequestDto request)
        {
            var result = new CreateEmployeeResponseDto { IsSuccess = true, Message = "Сотрудник добавлен в базу данных" };

            try
            {
                _logger.Info("-CreateEmployee-");
                var e = new EmployeesDb
                {
                    Age = request.Age,
                    BankName = request.BankName,
                    ChopCertificate = request.ChopCertificate,
                    Created = DateTime.Now,
                    EmplOptions = request.EmplOptions,
                    Fio = request.Fio,
                    Object = request.Object
                };


                _context.Employees.Add(e);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.Message.ToString();
            }

            return Ok(result);
        }


    }

    public class GetEmployeeResponse : ResponseBase
    {
        public EmployeeDto? Employee { get; set; }
    }

    public class GetEmployeeListResponse : ResponseBase
    {
        public List<EmployeeDto> EmployeesList { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Fio { get; set; }
        public DateTime Created { get; set; }
        public string? BankName { get; set; }
        public int? Age { get; set; }
        public bool ChopCertificate { get; set; }
        public string? Object {get;set;}
        public string? EmplOptions { get; set; }

        public List<WorkShiftDto> WorkShiftList { get; set; }
    }

    public class GetEmployeeWorkShiftsResponseDto : ResponseBase
    {
        public List<WorkShiftDto> WorkShiftList { get; set; }

    }

    public class WorkShiftDto
    {
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime Created { get; set; }
    }

    public class CreateEmployeeRequestDto
    {
        public string Fio { get; set; }
        public string? BankName { get; set; }
        public int? Age { get; set; }
        public bool ChopCertificate { get; set; }
        public string? Object { get; set; }
        public string? EmplOptions { get; set; }
    }

    public class CreateEmployeeResponseDto : ResponseBase
    {

    }

    public class GetWorkHoursResponse : ResponseBase
    {
        public List<WorkHourDto> WorkHoursList { get; set; }
    }

    public class WorkHourDto
    {
        public int Id {get;set;}
        public DateTime Created { get; set;}
        public int EmployeeId { get;set;}
        public int Hours {  get; set; }
        public int Rate { get; set; }
    }

    public class SaveWorkHoursItemRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Hours { get; set; }
        public int Rate { get; set; }

        public DateTime Date { get; set; }
    }

}
