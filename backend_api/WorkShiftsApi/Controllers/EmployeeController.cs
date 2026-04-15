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
                    var bank = _context.Banks.FirstOrDefault(b => b.Id == one.BankId);

                    result.Employee = new EmployeeDto
                    {
                        Created = one.Created,
                        Fio = one.Fio,
                        Id = one.Id,
                        DateOfBirth = one.DateOfBirth,
                        BankId = one.BankId,
                        BankName = bank?.BankName,
                        ChopCertificate = one.ChopCertificate,
                        UlchoDate = one.UlchoDate,
                        ObjectName = o?.Name ?? "",
                        ObjectId = one.ObjectId,
                        EmplOptions = one.EmplOptions,
                        Dismissed = one.Dismissed,
                        Payout = one.Payout
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



        /// <summary>
        /// Установить признак «на выплаты» у сотрудника
        /// </summary>
        [HttpPost("SetEmployeePayout")]
        public async Task<IActionResult> SetEmployeePayout([FromBody] SetEmployeePayoutRequest request)
        {
            var result = new ResponseBase { IsSuccess = true, Message = "Признак обновлен" };
            try
            {
                var emp = _context.Employees.FirstOrDefault(x => x.Id == request.EmployeeId);
                if (emp == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Сотрудник не найден";
                    return Ok(result);
                }
                emp.Payout = request.Payout;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.IsSuccess = false;
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        [HttpGet("GetEmployeeList")]
        public IActionResult GetEmployeeList([FromQuery]int? objectId)
        {

            var result = new GetEmployeeListResponse { IsSuccess = true, Message = "" };

            var employees = _context.Employees.Where(x => x.Dismissed != true);

            try
            {
                var query = from emp in employees
                            join o in _context.Objects on emp.ObjectId equals o.Id
                            join b in _context.Banks on emp.BankId equals b.Id into empBanks
                            from bank in empBanks.DefaultIfEmpty()
                                       //join ws in _context.WorkShifts on emp.Id equals ws.EmployeeId into empWorkshifts
                                       //from workShift in empWorkshifts.DefaultIfEmpty()
                            select new EmployeeDto
                            {
                                Created = emp.Created,
                                Fio = emp.Fio,
                                Id = emp.Id,
                                DateOfBirth = emp.DateOfBirth,
                                BankId = emp.BankId,
                                BankName = bank != null ? bank.BankName : null,
                                ChopCertificate = emp.ChopCertificate,
                                UlchoDate = emp.UlchoDate,
                                ObjectName = o.Name,
                                ObjectId = emp.ObjectId,
                                EmplOptions = emp.EmplOptions,
                                Dismissed = emp.Dismissed,
                                Payout = emp.Payout,
                                WorkShiftList = _context.WorkShifts.Where(x => x.EmployeeId == emp.Id).Select(x =>
                                    new WorkShiftDto
                                    { Created = x.Created, Id = x.Id, End = x.End, Start = x.Start }).ToList()
                            };

                result.EmployeesList = query.ToList();
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

                //var employees = _context.Employees.Where(x => x.Dismissed != true);
                var employees = _context.Employees
                    .Include(e => e.Object)
                    .Include(e => e.Bank)
                    .Include(e => e.FinOperations)
                    .Where(x => x.Dismissed != true)
                    .AsQueryable();

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
                                            BankId = emp.BankId,
                                            BankName = emp.Bank != null ? emp.Bank.BankName : null,
                                            ChopCertificate = emp.ChopCertificate,
                                            UlchoDate = emp.UlchoDate,
                                            EmplOptions = emp.EmplOptions,
                                            Dismissed = emp.Dismissed,
                                            Payout = emp.Payout,
                                            ObjectId = emp.ObjectId,
                                            ObjectName = emp.Object.Name,

                                            FinOperations = emp.FinOperations
                                            .Where(x =>x.Date.Date == selectedDate.Date)
                                            .Select(x=> 
                                            new FinOpDto 
                                            { 
                                                Comment = x.Comment,
                                                Id = x.Id,
                                                Date = x.Date.Date,
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


                var whList = list.ToList();
                var wdList = _context.WorkDays
                    .Where(x => x.WorkDate.Date == d.Date)
                    .ToList();

                var empIds = whList.Select(x => x.EmployeeId)
                    .Union(wdList.Select(x => x.EmployeeId))
                    .Distinct()
                    .ToList();

                var listDto = new List<WorkHourDto>();
                foreach (var empId in empIds)
                {
                    var wh = whList.FirstOrDefault(x => x.EmployeeId == empId);
                    var wd = wdList.FirstOrDefault(x => x.EmployeeId == empId);
                    var isDaily = wd != null;

                    listDto.Add(new WorkHourDto
                    {
                        Created = wh?.Created ?? wd?.Created ?? DateTime.Now,
                        Date = d.Date,
                        EmployeeId = empId,
                        Hours = wh?.Hours ?? 0,
                        Id = wh?.Id ?? 0,
                        Rate = wh?.Rate ?? 0,
                        DayRate = wd?.Rate ?? 0,
                        WorkDayId = wd?.Id ?? 0,
                        CompensationType = isDaily ? "daily" : "hourly",
                        ItemSalary = isDaily ? (wd?.Rate ?? 0) : ((wh?.Hours ?? 0) * (wh?.Rate ?? 0)),
                    });
                }

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
                var isDaily = (!string.IsNullOrWhiteSpace(request.CompensationType)
                    && request.CompensationType.Equals("daily", StringComparison.OrdinalIgnoreCase))
                    || (request.DayRate > 0 && request.Hours == 0);

                var workDate = request.Date.Date;

                if (isDaily)
                {
                    result.Message = "Стоимость за день успешно сохранена";

                    var dayRate = request.DayRate > 0 ? request.DayRate : request.Rate;
                    var wd = _context.WorkDays.FirstOrDefault(x => x.EmployeeId == request.EmployeeId && x.WorkDate.Date == workDate);
                    if (wd == null)
                    {
                        _context.WorkDays.Add(new WorkDaysDb
                        {
                            Created = DateTime.Now,
                            WorkDate = workDate,
                            EmployeeId = request.EmployeeId,
                            Rate = dayRate,
                        });
                    }
                    else
                    {
                        wd.Rate = dayRate;
                    }

                    var whRemove = _context.WorkHours.FirstOrDefault(x => x.EmployeeId == request.EmployeeId && x.WorkDate.Date == workDate);
                    if (whRemove != null)
                        _context.WorkHours.Remove(whRemove);
                }
                else
                {
                    var wdRemove = _context.WorkDays.FirstOrDefault(x => x.EmployeeId == request.EmployeeId && x.WorkDate.Date == workDate);
                    if (wdRemove != null)
                        _context.WorkDays.Remove(wdRemove);

                    var one = _context.WorkHours.FirstOrDefault(x => x.EmployeeId == request.EmployeeId && x.WorkDate.Date == workDate);

                    if (one == null)
                    {
                        var wh = new WorkHoursDb { Created = DateTime.Now, WorkDate = workDate, EmployeeId = request.EmployeeId, Hours = request.Hours, Rate = request.Rate };
                        _context.WorkHours.Add(wh);
                    }
                    else
                    {
                        one.Hours = request.Hours;
                        one.Rate = request.Rate;
                    }
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
                    BankId = request.BankId,
                    ChopCertificate = request.ChopCertificate,
                    UlchoDate = request.UlchoDate,
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
                one.BankId = request.BankId;
                one.ChopCertificate = request.ChopCertificate;
                one.UlchoDate = request.UlchoDate;
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

        /// <summary>
        /// Список банков из таблицы banks
        /// </summary>
        [HttpGet("GetBanksList")]
        public ActionResult GetBanksList()
        {
            var result = new GetBanksListResponse { IsSuccess = true, Message = "" };
            try
            {
                result.Items = _context.Banks
                    .OrderBy(x => x.BankName)
                    .Select(x => new BankItemDto { Id = x.Id, BankName = x.BankName })
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

    }

    public class BankItemDto
    {
        public int Id { get; set; }
        public string BankName { get; set; } = "";
    }

    public class GetBanksListResponse : ResponseBase
    {
        public List<BankItemDto> Items { get; set; } = new List<BankItemDto>();
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
        public int? BankId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool ChopCertificate { get; set; }
        public DateTime? UlchoDate { get; set; }
        public int? ObjectId { get; set; }
        public string? EmplOptions { get; set; }
        public bool Dismissed { get; set; }
    }

    public class SaveEmployeeRequestDto
    {
        public int Id { get; set; }
        public string Fio { get; set; }
        public int? BankId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool ChopCertificate { get; set; }
        public DateTime? UlchoDate { get; set; }
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
        /// <summary>«hourly» — часы и ставка в час (work_hours); «daily» — стоимость дня (work_days.rate)</summary>
        public string? CompensationType { get; set; }
        public int DayRate { get; set; }
        public int WorkDayId { get; set; }
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
        /// <summary>«hourly» или «daily»</summary>
        public string? CompensationType { get; set; }
        public int DayRate { get; set; }
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

    public class SetEmployeePayoutRequest
    {
        public int EmployeeId { get; set; }
        public bool Payout { get; set; }
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
