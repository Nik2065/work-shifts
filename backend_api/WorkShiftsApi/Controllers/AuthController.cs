using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkShiftsApi.DTO;
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

            /*if (request.Username != "admin@company.com" || request.Password != "password123")
                return Unauthorized(new { message = "Invalid username or password" });

            var user = new SiteUserDb
            {
                Created = DateTime.Now.Date,
                Deleted = false,
                EmailAsLogin = "admin@company.com",
                Id = 1
            };*/

            var token = _authService.GenerateJwtToken(user);
            return Ok(new { token, username = user.EmailAsLogin });
        }

        
        [HttpPost("register")]
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
        }

        [HttpGet("GetUsersList")]
        [Authorize]
        public async Task<IActionResult> GetUsersList()
        {
            var result = new GetSiteUsersListResponse { IsSuccess = true, Message = ""};
            try
            {
                result.Users = _authService.GetSiteUsersList();

            }
            catch (Exception ex)
            {
                //return BadRequest(new { message = ex.Message });
                _logger.Error(ex.ToString());
                result.IsSuccess = false;
                result.Message = ex.Message;

            }

            return Ok(result);
        }

        /*[HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Заглушка для проверки пользователя
            if (request.Username == "admin@company.com" && request.Password == "password123")
            {
                var token = GenerateJwtToken(request.Username);
                return Ok(new { token });
            }
            return Unauthorized();
        }*/

        /*private string GenerateJwtToken(string username)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Role, "admin"),
            //new Claim("role", "admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }*/
    }


    public record LoginRequest(string Username, string Password);
    public record RegisterRequest(string Username, string Password);

    public class GetSiteUsersListResponse : ResponseBase
    {
        public List<SiteUserDto> Users { get; set; }
    }



}



