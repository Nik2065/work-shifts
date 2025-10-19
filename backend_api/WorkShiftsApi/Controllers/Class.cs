using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkShiftsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;
        EmployeeController(IConfiguration config) 
        {
            _config = config;
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

    }
}
