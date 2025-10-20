using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public EmployeeController( AppDbContext context) 
        {
            //_employeeService = employeeService;
            _context = context;
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
                        Id = one.Id,
                        Fio = one.Fio,
                        Created = one.Created
                    };
                    
                }
            }
            catch (Exception ex)
            {
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
                        ChopCertificate = x.ChopCertificate
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
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
                result.IsSuccess = false;
                result.Message = ex.ToString();
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



}
