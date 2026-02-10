using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
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
                    var o = _context.Objects.FirstOrDefault(y => y.Id == one.ObjectId);

                    result.Employee = new EmployeeDto
                    {
                        Created = one.Created,
                        Fio = one.Fio,
                        Id = one.Id,
                        DateOfBirth = one.DateOfBirth,
                        BankName = one.BankName,
                        ChopCertificate = one.ChopCertificate,
                        ObjectName = o?.Name ?? "",
                        ObjectId = one.ObjectId,
                        EmplOptions = one.EmplOptions,
                        Dismissed = one.Dismissed
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


        ///// <summary>
        ///// Сохранить данные сотрудника по id
        ///// </summary>
        ///// <param name="employeeId"></param>
        ///// <returns></returns>
        //[HttpPost("SaveEmployee")]
        //public IActionResult SaveEmployee([FromBody] SaveEmployeeRequestDto request)
        //{
        //    var result = new ResponseBase { IsSuccess = true, Message = "Изменения сохранены" };

        //    try
        //    {

        //    }
        //    catch(Exception ex)
        //    {

        //    }

        //    return Ok(result);
        //}


        [HttpGet("GetEmployeeList")]
        public IActionResult GetEmployeeList([FromQuery]int? objectId)
        {

            var result = new GetEmployeeListResponse { IsSuccess = true, Message = "" };

            try
            {
                

                result.EmployeesList = (from emp in _context.Employees 
                                       join o in _context.Objects on emp.ObjectId equals o.Id
                                       //join ws in _context.WorkShifts on emp.Id equals ws.EmployeeId into empWorkshifts
                                       //from workShift in empWorkshifts.DefaultIfEmpty()
                                        select 
                                     new EmployeeDto
                                        {
                                            Created = emp.Created,
                                            Fio = emp.Fio,
                                            Id = emp.Id,
                                            DateOfBirth = emp.DateOfBirth,
                                            BankName = emp.BankName,
                                            ChopCertificate = emp.ChopCertificate,
                                            ObjectName = o.Name,
                                            ObjectId = emp.ObjectId,
                                            EmplOptions = emp.EmplOptions,
                                            Dismissed = emp.Dismissed,
                                            WorkShiftList = _context.WorkShifts.Where(x=>x.EmployeeId == emp.Id).Select(x=> 
                                            new WorkShiftDto 
                                            { Created=x.Created, Id=x.Id, End=x.End, Start=x.Start}).ToList()
                                     })
                                    .ToList();
                if (objectId != null)
                {
                    result.EmployeesList = result.EmployeesList.Where(x => x.ObjectId == (int)objectId).ToList();
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


        /// <summary>
        /// Отдает данные для таблицы "учет времени"
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="objectId"></param>
        /// <param name="isInWorkShift">отображать только тех кто сейчас на вахте</param>
        /// <returns></returns>
        [HttpGet("GetEmployeeWithFinOpList")]
        public IActionResult GetEmployeeWithFinOpList([FromQuery] string Date, [FromQuery] int objectId, [FromQuery] int isInWorkShift)
        {

            var result = new GetEmployeeListResponse { IsSuccess = true, Message = "" };

            try
            {

                var canParseDate = DateTime.TryParse(Date, out DateTime selectedDate);
                if (!canParseDate)
                    throw new Exception("Передана ошибочная дата");

                var employees = _context.Employees.AsQueryable();

                if (objectId!=-1)
                {
                    employees = employees.Where(x => x.ObjectId == objectId);
                }

                result.EmployeesList = (from emp in employees
                                        select new EmployeeDto
                                        {
                                            Created = emp.Created,
                                            Fio = emp.Fio,
                                            Id = emp.Id,
                                            DateOfBirth = emp.DateOfBirth,
                                            BankName = emp.BankName,
                                            ChopCertificate = emp.ChopCertificate,
                                            EmplOptions = emp.EmplOptions,
                                            Dismissed = emp.Dismissed,
                                            ObjectId = emp.ObjectId,
                                            ObjectName = emp.Object.Name,

                                            FinOperations = emp.FinOperations
                                            .Where(x =>x.Date.Date == selectedDate.Date)
                                            .Select(x=> 
                                            new FinOpDto 
                                            { 
                                                Comment = x.Comment,
                                                Id = x.Id,
                                                Date = selectedDate,
                                                IsPenalty = x.IsPenalty,
                                                Sum = x.Sum,
                                                TypeId = x.TypeId,
                                                TypeName = x.FinOperationType.OperationName,
                                            }
                                            ).ToList(),

                                            WorkShiftList = emp.WorkShifts.Select(x => new WorkShiftDto
                                            {
                                                Id = x.Id,
                                                Created = x.Created,
                                                End = x.End,
                                                Start = x.Start
                                                

                                            }).ToList(),


                                        }).ToList();


                foreach (var emp in result.EmployeesList)
                {
                    var check = CheckWorkShifts(emp.WorkShiftList);
                    if (check.IsInWorkShift)
                    {
                        emp.IsInWorkShift = check.IsInWorkShift;
                        emp.WorkShiftStart = check.Start;
                        emp.WorkShiftEnd = check.End;
                    }
                }

                //только на вахте
                if (isInWorkShift == 1) 
                {
                    result.EmployeesList = result.EmployeesList.Where(x => x.IsInWorkShift == true).ToList();
                }
                //только не на вахте
                else if (isInWorkShift == 2)
                {
                    result.EmployeesList = result.EmployeesList.Where(x => x.IsInWorkShift == false).ToList();
                }
                //если -1 то отображаем всех
                

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.ToString();
            }

            return Ok(result);
        }

        //проверяем есть ли вахта на текущую дату
        private CheckWorkShiftsDto CheckWorkShifts(List<WorkShiftDto> workShifts)
        {
            var result = new CheckWorkShiftsDto();

            var current = DateTime.Now.Date;

            foreach (var w in workShifts) 
            {
                if (w.Start.Date <= current && current <= w.End.Date)
                {
                    result.IsInWorkShift = true;
                    result.Start = w.Start.Date;
                    result.End = w.End.Date;
                    break;
                }
            }

            return result;
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

                if (string.IsNullOrEmpty(date))
                    throw new Exception("Ошибка: Передана пустая строка даты");

                var canParse = DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d);
                if (canParse)
                    list = list.Where(x => x.WorkDate.Date == d.Date);
                else
                    throw new Exception("Не удалось разобрать строку как дату:" + date);


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
            var result = new ResponseBase { IsSuccess = true, Message = "Отработанные часы успешно сохранены" };

            try
            {
                //TODO:
                // проверка полученных значений

                var one = _context.WorkHours.FirstOrDefault(x => x.EmployeeId == request.EmployeeId && x.WorkDate.Date == request.Date.Date);

                if(one == null)
                {
                    var wh = new WorkHoursDb { Created = DateTime.Now, WorkDate = request.Date, EmployeeId = request.EmployeeId, Hours = request.Hours, Rate = request.Rate };
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
                result.Message = "Ошибка при сохранении отработанных часов";
            }

            return Ok(result);

        }


        /// <summary>
        /// Получить список вахт для сотрудника
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [HttpGet("GetWorkShifts")]
        public IActionResult GetWorkShifts([FromQuery] int employeeId)
        {

            Thread.Sleep(1000);

            var result = new GetEmployeeWorkShiftsResponseDto { IsSuccess = true, Message = "" };

            try
            {
                result.WorkShiftList = _context.WorkShifts
                    .Where(x=>x.EmployeeId == employeeId)
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
                if (request.ObjectId == null)
                    throw new Exception("Не выбран объект");

                _logger.Info("-CreateEmployee-");
                var e = new EmployeesDb
                {
                    DateOfBirth = request.DateOfBirth,
                    BankName = request.BankName,
                    ChopCertificate = request.ChopCertificate,
                    Created = DateTime.Now,
                    EmplOptions = request.EmplOptions,
                    Fio = request.Fio,
                    ObjectId = (int)request.ObjectId,
                    Dismissed = request.Dismissed
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

        [HttpPost("SaveEmployee")]
        public IActionResult SaveEmployee([FromBody] SaveEmployeeRequestDto request)
        {
            var result = new CreateEmployeeResponseDto { IsSuccess = true, Message = "Данные о сотруднике изменены" };

            try
            {
                //todo: проверка данных
                if (request.ObjectId == null)
                    throw new Exception("Не выбран объект");

                _logger.Info("-SaveEmployee-");

                var one = _context.Employees.FirstOrDefault(x => x.Id == request.Id);

                if (one == null)
                    throw new Exception("Изменяемый сотрудник не найден");

                one.DateOfBirth = request.DateOfBirth;
                one.BankName = request.BankName;
                one.ChopCertificate = request.ChopCertificate;
                one.EmplOptions = request.EmplOptions;
                one.Fio = request.Fio;
                one.ObjectId = (int)request.ObjectId;
                one.Dismissed = request.Dismissed;
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


        [HttpPost("CreateWorkShift")]
        public IActionResult CreateWorkShift([FromBody] CreateWorkShiftRequestDto request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Запись добавлена" };

            try
            {
                //проверки

                var canParseStart = DateTime.TryParse(request.Start, out DateTime s);
                var canParseEnd = DateTime.TryParse(request.End, out DateTime e);

                //дата окончания должна быь позже даты начала
                //длительность вахты от 1 до 20 дней
                if (!canParseStart || !canParseEnd)
                    throw new Exception("Ошибка в формате даты");

                var dif = e - s;
                if (dif.Days < 0)
                    throw new Exception("Дата окончания вахты должна быть больше чем дата начала");

                if(dif.Days >20 || dif.Days <1)
                    throw new Exception("Длительность вахты должна быть от 1 до 20 дней");


                var ws = new WorkShiftsDb
                {
                    Created = DateTime.Now,
                    EmployeeId = request.EmployeeId,
                    Start = s,
                    End = e,
                };
                
                _context.WorkShifts.Add(ws);
                _context.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.Message.ToString();
            }

            return Ok(result);
        }

        [HttpPost("DeleteWorkShift")]
        public IActionResult DeleteWorkShift([FromBody] DeleteWorkShiftRequestDto request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Запись удалена" };

            try
            {
                var one = _context.WorkShifts.FirstOrDefault(x => x.Id == request.WorkShiftId);

                if (one == null)
                    throw new Exception("Запись не найдена");

                _context.WorkShifts.Remove(one);
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


        [HttpPost("CreateFinOperation")]
        public IActionResult CreateFinOperation([FromBody] CreateFinOperationRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Запись добавлена" };

            try
            {
                //проверка полей 
                var canParseDate = DateTime.TryParse(request.Date, out DateTime s);


                var emp = _context.Employees.FirstOrDefault(x => x.Id == request.EmployeeId);

                if (emp == null)
                    throw new Exception("Сотрудник не найден");

                var f = new FinOperationDb
                {
                    Created = DateTime.Now,
                    EmployeeId = request.EmployeeId,
                    Date = s,
                    IsPenalty = request.IsPenalty,
                    Sum = request.Sum,
                    Comment = request.Comment,
                    TypeId = request.TypeId
                };

                _context.FinOperations.Add(f);
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

        /// <summary>
        /// Удаляем дополнительную фин операцию типа штраф/премия
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("DeleteFinOperation")]
        public IActionResult DeleteFinOperation([FromBody] DeleteFinOperationRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Запись удалена" };

            try
            {
                var one = _context.FinOperations.FirstOrDefault(x=>x.Id == request.OperationId);

                if (one == null)
                    throw new Exception("Запись не найдена");

                _context.FinOperations.Remove(one);
                _context.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.Message.ToString();
            }

            return Ok(result);
        }

        [HttpGet("GetFinOperationTypes")]
        public async Task<ActionResult> GetFinOperationTypes()
        {
            var result = new GetFinOperationTypesResponse { IsSuccess = true, Message = "" };

            try
            {
                result.OperationTypes = 
                _context
                    .FinOperationTypes
                    .Select(x => new OpTypeDto 
                    { 
                        Id = x.Id, 
                        OperationName = x.OperationName, 
                        IsPayroll=x.IsPayroll 
                    }).ToList();

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

    public class FinOpDto
    {
        public int Id { get; set; }
        public int Sum { get; set; }
        public bool IsPenalty { get; set; }
        public DateTime Date { get; set; } //дата финансовой операции
        public string? Comment { get; set; }
        public int? TypeId { get; set; }
        public string? TypeName { get; set; }

    }

    public class CreateEmployeeRequestDto
    {
        public string Fio { get; set; }
        public string? BankName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool ChopCertificate { get; set; }
        public int? ObjectId { get; set; }
        public string? EmplOptions { get; set; }
        public bool Dismissed { get; set; }
    }

    public class SaveEmployeeRequestDto
    {
        public int Id { get; set; }
        public string Fio { get; set; }
        public string? BankName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool ChopCertificate { get; set; }
        public int? ObjectId { get; set; }
        public string? EmplOptions { get; set; }
        public bool Dismissed { get; set; }
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
        public DateTime Date { get; set; }
        public decimal ItemSalary { get; set; }
    }

    public class FinOperationDto
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPenalty { get; set; }
        public int Sum { get; set; }

        public int? TypeId { get; set; }
    }

    public class SaveWorkHoursItemRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Hours { get; set; }
        public int Rate { get; set; }

        public DateTime Date { get; set; }
    }

    public class CreateWorkShiftRequestDto
    {
        public int EmployeeId { get; set; }
        public string Start {  get; set; }
        public string End { get; set; }
    }


    public class DeleteWorkShiftRequestDto
    {
        public int WorkShiftId { get; set; }
    }

    public class CreateFinOperationRequest
    {
        public int EmployeeId { get; set; }
        public int Sum { get; set; }
        public bool IsPenalty { get; set; }

        public string Date { get; set; } //дата финансовой операции
        public string? Comment { get; set; }

        public int TypeId { get; set; }
    }

    public class DeleteFinOperationRequest
    {
        public int OperationId { get; set; }
    }

    public class CheckWorkShiftsDto
    {
        public DateTime? Start;
        public DateTime? End;
        public bool IsInWorkShift = false;
    }


    public class GetFinOperationTypesResponse : ResponseBase
    {
        public List<OpTypeDto> OperationTypes { get; set; }
    }

    public class OpTypeDto
    {
        public int Id { get; set; }
        public string OperationName { get; set; }
        public bool IsPayroll { get; set; }

    }
}
