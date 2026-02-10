using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                result.Users = GetSiteUsersAsQuarable().ToList();
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

        private IQueryable<SiteUserDto> GetSiteUsersAsQuarable(int? userId = null)
        {

            var result = (from u in _context.SiteUsers
                          join o in _context.UserToObject on u.Id equals o.SiteUserId into userObjects
                          where u.Deleted == false
                          select  new SiteUserDto
                          {
                              Created = u.Created,
                              Id = u.Id,
                              Login = u.EmailAsLogin,
                              RoleName = AuthService.GetRoleByRoleCode(u.RoleCode),
                              RoleCode = u.RoleCode,
                              ObjectsListIds = userObjects.Select(x=>x.ObjectId).ToList(),
                              ObjectsList = _context.Objects.Where(x=>  userObjects.Select(x => x.ObjectId).Contains(x.Id)).ToList(),
                          });

            if(userId!=null)
            {
                result = result.Where(x => x.Id == (int)userId);
            }

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

                result.User = GetSiteUsersAsQuarable(userId)?.FirstOrDefault();


            }
            catch (Exception ex)
            {
                //return BadRequest(new { message = ex.Message });
                _logger.Error(ex.ToString());
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            Thread.Sleep(1000);
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
                    p.Address = obj.Address;
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

        [HttpPost("SaveObject")]
        [Authorize]
        public async Task<IActionResult> SaveObject([FromBody] SaveObjectRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Объект сохранён" };
            try
            {
                var one = _context.Objects.FirstOrDefault(x => x.Id == request.Id);
                if (one == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Объект не найден";
                    return Ok(result);
                }
                one.Name = request.Name ?? one.Name;
                one.Address = request.Address;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                result.IsSuccess = false;
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost("DeleteObject")]
        [Authorize]
        public async Task<IActionResult> DeleteObject([FromBody] DeleteObjectRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Объект удалён" };
            try
            {
                var one = _context.Objects.FirstOrDefault(x => x.Id == request.Id);
                if (one == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Объект не найден";
                    return Ok(result);
                }
                var hasEmployees = _context.Employees.Any(x => x.ObjectId == request.Id);
                if (hasEmployees)
                {
                    result.IsSuccess = false;
                    result.Message = "Невозможно удалить: к объекту привязаны сотрудники";
                    return Ok(result);
                }
                _context.Objects.Remove(one);
                await _context.SaveChangesAsync();
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

                var objects = request
                    .ObjectsList
                    .Split(";", StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.Parse(x))
                    .ToArray();


                var u = await _authService.RegisterAsync(request.Login, request.Password, request.RoleCode, objects);


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
        [HttpPost("SaveUser")]
        [Authorize]
        public async Task<IActionResult> SaveUser([FromBody] SaveUserRequest request)
        {

            var result = new ResponseBase { IsSuccess = true, Message = "Данные пользователя обновлены" };

            try
            {
                if (string.IsNullOrEmpty(request.Login))
                    throw new Exception("Не задан логин пользователя");


                //1. проверяем что логин это адрес почты
                var r = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                if (!r.IsMatch(request.Login))
                    throw new Exception("Ошибка в формате адреса логина. Он должен быть в виде email адреса");
                
                
                //2. проверяем что логин есть в базе
                var one = _context.SiteUsers.FirstOrDefault(x => x.EmailAsLogin == request.Login);
                if (one == null)
                    throw new Exception("Пользователя с логином:" + request.Login + " нет в базе данных");


                if(string.IsNullOrEmpty(request.Password))
                {
                    //то не меняем предыдущий пароль
                }
                else if (request.Password.Length < 8)
                    throw new Exception("Необходимо задать пароль длиной не менее 8 символов");

                //как только создаем пользователя отправляем ему на почту пароль?
                //или пусть задают пароль вручную?

                var objects = request
                    .ObjectsList
                    .Split(";", StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.Parse(x))
                    .ToArray();
                
                await _authService.UpdateUserAsync(request.Login, request.Password, request.RoleCode, objects);

            }
            catch(Exception ex)
            {
                _logger.Error(ex.ToString());
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return Ok(result);
        }


        /*private List<ObjectDb> GetUserObjects(int userId)
        {
            var objs = _context.UserToObject
                .Where(x=>x.SiteUserId == userId).Select(x=>x.ObjectId)
                .ToList();
            return _context.Objects
                .Where(x => objs.Contains(x.Id))
                .ToList();
        }*/





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
        public string? Address { get; set; }
    }

    public class SaveObjectRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
    }

    public class DeleteObjectRequest
    {
        public int Id { get; set; }
    }

    public class SaveUserRequest 
    {
        public int Id { get; set; }
        public string Login { get; set; }//email
        public string RoleCode { get; set; }
        public string Password { get; set; }
        public string ObjectsList { get; set; }//объекты через ";"
    }

}
