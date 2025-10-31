using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WorkShiftsApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<SiteUserDb?> AuthenticateAsync(string login, string password)
        {
            var user = await _context.SiteUsers
                .FirstOrDefaultAsync(u => u.EmailAsLogin == login && !u.Deleted);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }


        public async Task<SiteUserDb> RegisterAsync(string username, string password, string roleCode)
        {
            if (await UserExistsAsync(username))
                throw new Exception("Username already exists");

            //проверяем роль 
            if (roleCode != UserRoleCodeEnum.Admin
                && roleCode != UserRoleCodeEnum.ObjectManager
                && roleCode != UserRoleCodeEnum.Buh)
                throw new Exception("Роль не найдена. Выберите другую роль для создания пользователя");

            var user = new SiteUserDb
            {
                EmailAsLogin = username,
                Created = DateTime.Now,
                Deleted = false,
                RoleCode = roleCode,
                PasswordHash = HashPassword(password)
            };

            _context.SiteUsers.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UserExistsAsync(string login)
        {
            return await _context.SiteUsers.AnyAsync(u => u.EmailAsLogin == login);
        }

        public string GenerateJwtToken(SiteUserDb user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                //new Claim(JwtRegisteredClaimNames.Sub, user.EmailAsLogin),
                //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                new Claim(ClaimTypes.Name, user.EmailAsLogin),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.RoleCode),
                new Claim("Login", user.EmailAsLogin),
                new Claim("Id", user.Id.ToString()),
                new Claim("Role", user.RoleCode)

            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }


        public static string GetRoleByRoleCode(string roleCode)
        {
            if (roleCode?.ToLower() == "admin")
                return "Администратор";
            else if (roleCode?.ToLower() == "object_manager")
                return "Начальник объекта";
            else if (roleCode?.ToLower() == "buh")
                return "Бухгалтерия";
            else
                return "-";
        }



    }

    public class UserRoleCodeEnum
    {
        public const string Admin = "admin"; //Администратор
        public const string ObjectManager = "object_manager";//Начальник объекта
        public const string Buh = "buh";//Бухгалтерия
    }

}
