using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkShiftsApi.Services;

namespace WorkShiftsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;
        private EmployeeService employeeService; 
        EmployeeController(IConfiguration config) 
        {
            _config = config;
            employeeService = new EmployeeService(_config);
        }


        [HttpGet("GetEmployee")]
        public IActionResult GetEmployee([FromQuery] int employeeId) 
        {
            try
            {

            }
            catch 
            { 
            
            
            }



            return Ok("test");
        }

        [HttpGet("GetEmployeeList")]
        public IActionResult GetEmployeeList([FromQuery] int employeeId)
        {
            try
            {

            }
            catch
            {


            }



            return Ok("test");
        }

    }
}
