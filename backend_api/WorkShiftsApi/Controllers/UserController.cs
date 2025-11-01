using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using WorkShiftsApi.DTO;
using WorkShiftsApi.Services;

namespace WorkShiftsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private NLog.Logger _logger;
        private readonly IAuthService _authService;

        public UserController(IAuthService authService, AppDbContext context) 
        {
            _context = context;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _authService = authService;
        }


        [HttpGet("GetUsersList")]
        [Authorize]
        public async Task<IActionResult> GetUsersList()
        {
            var result = new GetSiteUsersListResponse { IsSuccess = true, Message = "" };
            try
            {

                result.Users = GetSiteUsersList();

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

        private List<SiteUserDto> GetSiteUsersList()
        {
            var result = _context.SiteUsers
                .Where(x => x.Deleted == false)
                .Select(x => new SiteUserDto
                {
                    Created = x.Created,
                    Id = x.Id,
                    Login = x.EmailAsLogin,
                    RoleName = AuthService.GetRoleByRoleCode(x.RoleCode)
                }).ToList();


            return result;
        }

        [HttpGet("GetUser")]
        [Authorize]
        public async Task<IActionResult> GetUser([FromQuery] int userId)
        {
            var result = new GetUserResponse { IsSuccess = true, Message = "" };
            try
            {
                var one = _context.SiteUsers.FirstOrDefault(x => x.Id == userId);

                if (one == null)
                    throw new Exception($"Пользователь с id={userId} не найден");

                result.User = new SiteUserDto
                {
                    Created = one.Created,
                    Id= one.Id,
                    Login = one.EmailAsLogin,
                    RoleName = AuthService.GetRoleByRoleCode(one.RoleCode),
                    ObjectsList = GetUserObjects(one.Id)
                };

                
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


        [HttpGet("GetAllObjects")]
        [Authorize]
        public async Task<IActionResult> GetAllObjects()
        {
            var result = new GetAllObjectsResponse { IsSuccess = true, Message = "" }; 
            try
            {
                //result.Objects = _context.Objects.Select(x=> new KeyValuePair<int, string>(x.Id, x.Name));

                var list = _context.Objects.ToList();
                foreach (var obj in list)
                {
                    var p = new P();
                    p.Id = obj.Id;
                    p.Name = obj.Name;
                    result.Objects.Add(p);
                }
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
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CreateUser")]
        [Authorize]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Пользователь добавлен" }; 

            try
            {
                if (string.IsNullOrEmpty(request.Login))
                    throw new Exception("Не задан логин пользователя");


                //1. проверяем что логин это адрес почты
                var r = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                if (!r.IsMatch(request.Login))
                    throw new Exception("Ошибка в формате адреса логина. Он должен быть в виде email адреса");
                //2. проверяем что логина нет в базе
                var one = _context.SiteUsers.FirstOrDefault(x => x.EmailAsLogin == request.Login);
                if (one != null)
                    throw new Exception("Пользователь с логином:" + request.Login + " уже добавлен в базу данных");

                if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
                    throw new Exception("Необходимо задать пароль длиной не менее 8 символов");

                //как только создаем пользователя отправляем ему на почту пароль?
                //или пусть задают пароль вручную?

                var u = await _authService.RegisterAsync(request.Login, request.Password, request.RoleCode);


            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return Ok(result);
        }




        private List<ObjectDb> GetUserObjects(int userId)
        {
            var objs = _context.UserToObject
                .Where(x=>x.SiteUserId == userId).Select(x=>x.ObjectId)
                .ToList();
            return _context.Objects
                .Where(x => objs.Contains(x.Id))
                .ToList();
        }



    }

    public class GetSiteUsersListResponse : ResponseBase
    {
        public List<SiteUserDto> Users { get; set; }
    }

    public class GetUserResponse : ResponseBase
    {
        public SiteUserDto User { get; set; }
    }

    public class CreateUserRequest
    {
        public string Login { get; set; }//email
        public string RoleCode { get; set; }

        public string Password { get; set; }

        //public List<ObjectDb> ObjectsList { get; set; }
        public string ObjectsList { get; set; }//объекты через ";"
    }

    public class GetAllObjectsResponse : ResponseBase
    {
        public List<P> Objects { get; set; } = new List<P>();
    }

    public record P
    {
        public int Id { get; set; }
        public string Name { get; set; } 
    }

}
