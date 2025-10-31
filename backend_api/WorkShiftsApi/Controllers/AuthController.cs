using Microsoft.AspNetCore.Mvc;
using WorkShiftsApi.Services;

namespace WorkShiftsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
       
        private readonly IAuthService _authService;
        private NLog.Logger _logger;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
            _logger = NLog.LogManager.GetCurrentClassLogger();

        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("test");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var token = _authService.GenerateJwtToken(user);
            return Ok(new { token, username = user.EmailAsLogin });
        }

        
        /*[HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.RegisterAsync(request.Username, request.Password);
                var token = _authService.GenerateJwtToken(user);

                return Ok(new
                {
                    token,
                    username = user.EmailAsLogin,
                    message = "User registered successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }*/







    }


    public record LoginRequest(string Username, string Password);
    public record RegisterRequest(string Username, string Password);



}



