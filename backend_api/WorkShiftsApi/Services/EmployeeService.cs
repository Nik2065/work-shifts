using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using WorkShiftsApi.Controllers;
using WorkShiftsApi.DbEntities;
using WorkShiftsApi.DTO;
using DataTable = System.Data.DataTable;

namespace WorkShiftsApi.Services
{
    //public interface IEmployeeService
    //{

    //}


    public class EmployeeService
    {

        public EmployeeService(AppDbContext context)
        {
            //_config = config;
            _context = context;
        }

        //private readonly IConfiguration _config;
        private AppDbContext _context { get; set; }
        private NLog.Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();


        public DataTable GetReportForEmplList(DateTime begin, DateTime end, List<int> emplList)
        {
            var table = new DataTable();
            table.Columns.Add("# id", typeof(int));
            table.Columns.Add("ФИО", typeof(string));
            table.Columns.Add("Дней", typeof(int));
            //table.Columns.Add("Дата", typeof(DateTime));
            table.Columns.Add("Сумма за работу", typeof(int));
            table.Columns.Add("Списания", typeof(int));
            table.Columns.Add("Начисления", typeof(int));
            table.Columns.Add("Итого", typeof(int));
            //table.Columns.Add("EmplOption", typeof(string));
            //table.Columns.Add("BankName", typeof(string));

            //table.Rows.Add(1, "Иван", DateTime.Now, 1500.50m);
            //table.Rows.Add(2, "Мария", DateTime.Now.AddDays(-1), 2300.75m);
            //table.Rows.Add(3, "Алексей", DateTime.Now.AddDays(-2), 1890.00m);

            /*var rows = _context.Database.SqlQuery<ReportItem>(@$"select e.id, e.fio
                from work_hours w 
                left join employees e on w.employee_id=e.id
                where w.work_date >= '2025-11-01'
                and w.work_date<'2025-12-10'");
            */

            var list = string.Join(',', emplList);

            /*string sql = $@"
            select fio, count(*) days, 
            IFNULL(sum(revenue),0) as revenue, 
            IFNULL(sum(Nachislenia),0) as Nachislenia
            , IFNULL(sum(Spisania),0) as Spisania, 
            (IFNULL((revenue), 0) + IFNULL(sum(Nachislenia),0) - IFNULL(sum(Spisania),0)) as Total
            , EmplOption
            , BankName

            from (

                select  e.fio, cast(w.work_date as date), 
                sum(w.hours * w.rate) as revenue, 
                sum(notPenalty.sum) as Nachislenia,
                sum(penalty.sum) as Spisania
                , e.empl_options as EmplOption
                , e.bank_name as BankName
    
                from employees e
                left join  work_hours w on w.employee_id=e.id
                left join fin_operations penalty on penalty.employee_id=e.id  and cast(penalty.date as date)=cast(w.work_date as date)  and penalty.is_penalty=true
                left join fin_operations notPenalty on notPenalty.employee_id=e.id and cast(notPenalty.date as date)=cast(w.work_date as date) and notPenalty.is_penalty=false
                where w.work_date >= '{begin.ToString("yyyy-MM-dd")}'
                and w.work_date< '{end.ToString("yyyy-MM-dd")}'
                and e.id in ({list})
                group by fio, cast(w.work_date as date), e.empl_options, e.bank_name

            )
            as q
            group by fio, revenue, Nachislenia, Spisania, EmplOption, BankName;
            ";*/

            string sql = $@"
                select 
                employeeid,
                fio,
                count(wdate) as days, 
                coalesce(sum(revenue), 0) as totalRevenue,
                coalesce(sum(Spisania), 0) as totalSpisania,
                coalesce(sum(Nachislenia), 0) as totalNachislenia,
                coalesce(sum(revenue) + sum(Nachislenia) - sum(Spisania), 0) as Total
            from (
                select 
                    e.id as employeeid, 
                    e.fio, 
                    cast(w.work_date as date) as wdate, 
                    sum(w.hours * w.rate) as revenue, 
                    sum(ifnull(notPenalty.sum, 0)) as Nachislenia,
                    sum(ifnull(penalty.sum, 0)) as Spisania
                from employees e
                left join work_hours w on w.employee_id = e.id
                    and w.work_date >= '{begin.ToString("yyyy-MM-dd")}'
                    and w.work_date < '{end.ToString("yyyy-MM-dd")}'
                left join fin_operations penalty on penalty.employee_id = e.id 
                    and cast(penalty.date as date) = cast(w.work_date as date) 
                    and penalty.is_penalty = true
                left join fin_operations notPenalty on notPenalty.employee_id = e.id 
                    and cast(notPenalty.date as date) = cast(w.work_date as date)  
                    and notPenalty.is_penalty = false
                where e.id in ({list})
                group by 
                    e.id, 
                    e.fio,
                    cast(w.work_date as date)
            ) as q
            group by fio, employeeid
            order by employeeid
            ";


            //FormattableString formattable = sql.ToFormattableString(begin.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"), list);

            var formattable = FormattableStringFactory.Create(sql, []);
            var rows = _context.Database.SqlQuery<ReportItem>(formattable);

            var a = rows.ToList();

            foreach (var row in a)
            {
                var arr = new object[]
                {
                    row?.Employeeid,
                    row?.Fio ?? "",
                    row?.Days,
                    row?.TotalRevenue,
                    row?.TotalSpisania,
                    row?.TotalNachislenia,
                    row?.Total//, 
                    //row?.EmplOption,
                    //row?.BankName
                };

                var r = table.NewRow();
                r.ItemArray = arr;
                table.Rows.Add(r);
            }

            return table;
        }



        //private string GetSqlCommand(DateTime begin, DateTime end, List<int> emplList)
        //{
        //string result = 
        //return result;
        //}


        //Подготавливаем список таблиц с данными отчета
        public List<TableDataDto> CreateMainReportTablesList(DateTime startDate, DateTime endDate, List<EmployeesDb> emplList)
        {
            var tables = new List<TableDataDto>();

            //для ведомости
            var vedEmplList = emplList.Where(x => x.EmplOptions == EmplOptionEnums.Vedomost);
            var vedIds = vedEmplList.Select(x => x.Id).ToList();
            emplList = emplList.Where(x => !vedIds.Contains(x.Id)).ToList();

            GetReportForEmplList2(startDate, endDate, vedIds, out DataTable resultTable1, out int tableSum);
            tables.Add(new TableDataDto { Title = "Расчет по ведомости", DataTable = resultTable1, TotalSum = tableSum });

            //разные типы карт
            var banks = _context.Banks.ToList();
            foreach (var b in banks)
            {
                var empListN = emplList.Where(x => x.BankId == b.Id);
                if (empListN.Count() == 0)
                    continue;

                var ids = empListN.Select(x => x.Id).ToList();
                GetReportForEmplList2(startDate, endDate, ids, out DataTable resultTableN, out int tableSumN);
                tables.Add(new TableDataDto
                {
                    Title = "Расчет для карт банка " + b,
                    DataTable = resultTableN,
                    TotalSum = tableSumN
                });
            }

            return tables;
        }

        //Подготавливаем данные для будущего отчета 
        //
        //новая версия отчета с новым типом данных
        //2026-04-15
        //теперь не обращаем внимание на дату старта отчета и учитываем все неоплаченные часы/дни/операции
        public MainReportDto PrepareMainReportDataVer3(DateTime startDate, DateTime endDate, List<EmployeesDb> emplList)
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            var result = new MainReportDto
            {
                Employees = emplList ?? new List<EmployeesDb>(),
                StartDate = start,
                EndDate = end
            };

            try
            {


                foreach (var employee in result.Employees)
                {
                    var fd = new EmployeeFinData
                    {
                        EmployeeId = employee.Id,
                        Fio = employee.Fio ?? ""
                    };

                    var finInPeriod = _context.FinOperations
                        .Where(x => x.EmployeeId == employee.Id
                            //&& x.Date.Date >= start
                            && x.Date.Date < end
                            && x.Payed != true)
                        .ToList();

                    var avans = finInPeriod.FirstOrDefault(x => x.Payed != true
                        && x.TypeId == (int)FinOperationTypeEnum.AdvancePayment);

                    if(avans != null)
                    {
                        fd.AdvancePaymentInPeriod = true;
                        fd.TotalSumForPeriod = avans.Sum;

                        fd.FinOperations = new List<FinOperationItem>{ new FinOperationItem
                            {
                                Sum = avans.Sum,
                                TypeId = avans.TypeId
                            }
                        };
                    }
                    else
                    {
                        var totalSumForPeriod = 0;



                    }

                        fd.AdvancePaymentInEarlyPeriod = _context.FinOperations
                            .Where(x => x.EmployeeId == employee.Id
                                //&& x.Date.Date < start
                                && x.TypeId == (int)FinOperationTypeEnum.AdvancePayment
                                && x.Payed == true
                                && x.DecreaseTotalBecauseOfAdvancePayment != true)//еще не вычитали этот аванс
                            .Select(x => x.Sum)
                            .DefaultIfEmpty()
                            .Sum();

                    var workdays = _context.WorkDays.Where(x => x.EmployeeId == employee.Id
                    && x.Rate != 0
                        //&& x.WorkDate.Date >= start
                        && x.WorkDate.Date < end
                        && x.Payed != true);

                    var dayRateVariants = workdays.Where(x => x.Rate != 0).Select(x => x.Rate).Distinct().ToList();
                    foreach (var rate in dayRateVariants)
                    {
                        fd.WorkDays.Add(new WorkDayFinItem
                        {
                            Rate = rate,
                            WorkDaysCount = workdays.Count(x => x.Rate == rate)
                        });
                    }

                    var workhours = _context.WorkHours.Where(x => x.EmployeeId == employee.Id
                    && x.Rate != 0
                        //&& x.WorkDate.Date >= start
                        && x.WorkDate.Date < end
                        && x.Payed != true);

                    var hourRateVariants = workhours.Where(x => x.Rate != 0).Select(x => x.Rate).Distinct().ToList();
                    foreach (var rate in hourRateVariants)
                    {
                        //var tmp = workhours.Where(x => x.Rate == rate).ToList();
                        var s = workhours.Where(x => x.Rate == rate).Sum(x => x.Hours);

                        fd.WorkHours.Add(new WorkHourFinItem
                        {
                            Rate = rate,
                            Hours = s
                        });
                    }

                    foreach (var g in finInPeriod.Where(x => x.Payed != true).GroupBy(x => x.TypeId))
                    {
                        fd.FinOperations.Add(new FinOperationItem
                        {
                            TypeId = g.Key,
                            Sum = g.Sum(x => x.Sum)
                        });
                    }

                    result.EmployeeFinDatas.Add(fd);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return result;
        }

        /// <summary>
        /// Получить данные одного из отчетов
        /// </summary>
        /// <returns></returns>
        public MainReportDto GetMainReportDataVer3(int reportNumber)
        {
            var result = new MainReportDto();

            try
            {
                var mr = _context.MainReportNumbers.FirstOrDefault(x => x.ReportNumber == reportNumber);

                if (mr == null)
                    throw new Exception($"Отчет {reportNumber} не найден");

                var ids = mr.EmployeeIds?
                    .Split(',', StringSplitOptions.TrimEntries)
                    .Select(x => Int32.Parse(x))
                    .ToList();

                if (ids == null || ids.Count() == 0)
                    throw new Exception("Ошибочные данные отчета");

                result.StartDate = mr.StartDate;
                result.EndDate = mr.EndDate;
                result.Employees = _context.Employees.Where(x => ids.Contains(x.Id)).ToList();

                foreach (var employee in result.Employees)
                {
                    var fd = new EmployeeFinData
                    {
                        EmployeeId = employee.Id,
                        Fio = employee.Fio ?? "",
                        BankName = employee.Bank?.BankName ?? "",
                        Vedomost = employee.EmplOptions == EmplOptionEnums.Vedomost
                    };


                    
                    var finOperations = _context.FinOperations
                     .Where(x => x.EmployeeId == employee.Id
                     && x.ReportNumber == reportNumber);
                    


                    //добавляем учет АВАНСА и 
                    //АВАНСА в предыдущем периоде
                    var avans = finOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.AdvancePayment);
                    fd.FoAvansId = avans?.Id;
                    
                    if (avans != null)
                    {
                        //пишем только аванс
                        //пустые списки
                        //fd.WorkHours 
                        //fd.WorkDays
                        //fd.FinOperations
                        fd.AdvancePaymentInPeriod = true;
                        fd.TotalSumForPeriod = avans.Sum;
                        fd.FinOperations = new List<FinOperationItem>{ new FinOperationItem
                            {
                                Sum = avans.Sum,
                                TypeId = avans.TypeId
                            } 
                        };

                    }
                    else
                    {
                        var totalSumForPeriod = 0;

                        fd.FinOperations = finOperations.Select(x => new FinOperationItem
                        {
                            Sum = x.Sum,
                            TypeId = x.TypeId
                        })
                        .ToList();
                        //теперь и айдишники запоминаем для более простой обработке
                        fd.FinOperationIds = finOperations.Select(x => x.Id).ToList();

                        //...............................................................
                        var workHours = _context.WorkHours.Where(x => x.EmployeeId == employee.Id
                        && x.ReportNumber == reportNumber);
                        fd.WorkHoursIds = workHours.Select(x => x.Id).ToList();

                        var whSum = 0;
                        var hourrateVariants = workHours.Select(x => x.Rate).Distinct().ToList();
                        foreach (var rate in hourrateVariants)
                        {
                            var f = new WorkHourFinItem
                            {
                                Hours = workHours.Where(x => x.Rate == rate).Select(x => x.Hours).Sum(),
                                Rate = rate,
                            };
                            fd.WorkHours.Add(f);
                            whSum += f.Hours * f.Rate;
                        }


                        //суммируем
                        totalSumForPeriod += whSum;
                        //....................


                        var workDays = _context.WorkDays.Where(x => x.EmployeeId == employee.Id
                        && x.ReportNumber == reportNumber);
                        fd.WorkDaysIds = workDays.Select(x => x.Id).ToList();

                        var wdSum = 0;
                        var dayrateVariants = workDays.Select(x => x.Rate).Distinct().ToList();
                        foreach (var rate in dayrateVariants)
                        {
                            var f = new WorkDayFinItem
                            {
                                Rate = rate,
                                WorkDaysCount = workDays.Where(x => x.Rate == rate).Count()
                            };
                            fd.WorkDays.Add(f);
                            wdSum += f.WorkDaysCount * f.Rate;
                        }

                        //суммируем
                        totalSumForPeriod += wdSum;

                        //....................
                        //сумма по фин операциям 
                        //минусы
                        var shtraf = FinOpSum(fd.FinOperations, FinOperationTypeEnum.Shtraf);
                        var forma = FinOpSum(fd.FinOperations, FinOperationTypeEnum.Forma);
                        var ucho = FinOpSum(fd.FinOperations, FinOperationTypeEnum.Ucho);
                        var other = FinOpSum(fd.FinOperations, FinOperationTypeEnum.Other);
                        
                        totalSumForPeriod = totalSumForPeriod - shtraf - forma - ucho - other;

                        var otherPayroll = FinOpSum(fd.FinOperations, FinOperationTypeEnum.OtherPayroll);
                        totalSumForPeriod = totalSumForPeriod + otherPayroll;

                        //узнаем были у этого сотрудника неучтеные авансы
                        //т.е. авансы которые выплатили но не учли в следующем периоде
                        //prevAvansList - неучтеные авансы
                        var prevAvans = _context.FinOperations
                            .FirstOrDefault(x => x.EmployeeId == employee.Id
                            && x.TypeId == (int)FinOperationTypeEnum.AdvancePayment
                            && x.Payed == true && x.DecreaseTotalBecauseOfAdvancePayment != true);

                        fd.AvansInPrevPeriodId = prevAvans?.Id;

                        var advancePaymentInEarlyPeriod = prevAvans?.Sum ?? 0;
                        totalSumForPeriod = totalSumForPeriod - advancePaymentInEarlyPeriod;

                        fd.AdvancePaymentInEarlyPeriod = advancePaymentInEarlyPeriod;
                        fd.TotalSumForPeriod = totalSumForPeriod;
                    }

                    result.EmployeeFinDatas.Add(fd);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return result;
        }

        //помечаем все записи о платежах для конкретного сотрудника 
        //исполненными (payed=true)
        //можно написать альтернативный вариант потому что теперь внутри FinData есть id сущностей
        public void MarkPayoutRowLogic_old(int reportNumber, int employeeId, bool payed)
        {
            var report = GetMainReportDataVer3(reportNumber);
            if (report == null)
                throw new Exception("Отчет номер " + reportNumber + " не найден");
            var reportForEmployee = report.EmployeeFinDatas.FirstOrDefault(x => x.EmployeeId == employeeId);
            if (reportForEmployee == null)
                throw new Exception("В отчете не найдены данные для сотрудника");

            //если есть аванс то помечаем выплаченым только его
            if (reportForEmployee.AdvancePaymentInPeriod)
            {
                //ищем
                var fo = GetAvansByReportNumber(reportNumber, employeeId);

                if (fo == null)
                    throw new Exception("Не найден аванс");

                fo.Payed = payed;
            }
            else
            {
                var wh = _context.WorkHours.Where(x => x.ReportNumber == reportNumber
                && x.EmployeeId == employeeId);

                foreach (var f in wh)
                    f.Payed = payed;

                var wd = _context.WorkDays.Where(x => x.ReportNumber == reportNumber
                && x.EmployeeId == employeeId);

                foreach (var f in wd)
                    f.Payed = payed;

                //прочие фин. операции
                var fo = _context.FinOperations.Where(x => x.ReportNumber == reportNumber
                && x.EmployeeId == employeeId);

                foreach (var f in wd)
                    f.Payed = payed;

            }

            _context.SaveChanges();
        }


        //Новый вариант, с учетом аванса за предыдущий период
        public async Task MarkPayoutRowLogic2(int reportNumber, int employeeId, bool payed)
        {
            var report = GetMainReportDataVer3(reportNumber);
            if (report == null)
                throw new Exception("Отчет номер " + reportNumber + " не найден");
            var reportForEmployee = report.EmployeeFinDatas.FirstOrDefault(x => x.EmployeeId == employeeId);
            if (reportForEmployee == null)
                throw new Exception("В отчете не найдены данные для сотрудника");

            //Если аванс в текущем периоде
            if(reportForEmployee.FoAvansId != null)
            {
                var fo = _context.FinOperations.FirstOrDefault(x => x.Id == reportForEmployee.FoAvansId);
                if (fo == null)
                    throw new Exception("Не найдена финансовая операция");

                fo.Payed = payed;
            }
            else
            {
                var workHours = _context.WorkHours.Where(x => reportForEmployee.WorkHoursIds.Contains(x.Id));
                foreach(var workHour in workHours) 
                    workHour.Payed = payed;
                
                var workDays = _context.WorkDays.Where(x => reportForEmployee.WorkDaysIds.Contains(x.Id));
                foreach(var workDay in workDays) 
                    workDay.Payed = payed;

                var finOperations = _context.FinOperations.Where(x => reportForEmployee.FinOperationIds.Contains(x.Id));
                foreach (var finOperation in finOperations)
                    finOperation.Payed = payed;

                if(reportForEmployee.AvansInPrevPeriodId != null && reportForEmployee.AvansInPrevPeriodId != 0)
                {
                    var pp = _context.FinOperations.FirstOrDefault(x => x.Id == reportForEmployee.AvansInPrevPeriodId);
                    if (pp != null && payed)
                        pp.DecreaseTotalBecauseOfAdvancePayment = true;
                    if(pp!=null && !payed)
                        pp.DecreaseTotalBecauseOfAdvancePayment = null;
                }
            }

            await _context.SaveChangesAsync();
        }

        public EmployeeFinancialReportResponse GetEmployeeFinancialReportLogic(DateTime start, DateTime end, int employeeId)
        {

            var result = new EmployeeFinancialReportResponse();

            // Рабочие часы
            var workHours = (from wh in _context.WorkHours
                             join emp in _context.Employees on wh.EmployeeId equals emp.Id
                             join obj in _context.Objects on emp.ObjectId equals obj.Id
                             where wh.EmployeeId == employeeId
                                   && wh.WorkDate.Date >= start.Date
                                   && wh.WorkDate.Date < end.Date
                             select new
                             {
                                 wh.Id,
                                 Date = wh.WorkDate.Date,
                                 wh.Hours,
                                 wh.Rate,
                                 ObjectName = obj.Name,
                                 wh.Payed,
                                 wh.ReportNumber
                             }).ToList();

            var workHourIds = workHours.Select(x => x.Id).ToList();
            var whReportRecords = _context.RevenueReportsWh
                .Where(x => workHourIds.Contains(x.WorkHoursId))
                .GroupBy(x => x.WorkHoursId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.ReportNumber).First());

            // Рабочие дни
            var workDays = (from wd in _context.WorkDays
                            join emp in _context.Employees on wd.EmployeeId equals emp.Id
                            join obj in _context.Objects on emp.ObjectId equals obj.Id
                            where wd.EmployeeId == employeeId
                                  && wd.WorkDate.Date >= start.Date
                                  && wd.WorkDate.Date < end.Date
                            select new
                            {
                                wd.Id,
                                Date = wd.WorkDate.Date,
                                wd.Rate,
                                ObjectName = obj.Name,
                                wd.Payed,
                                wd.ReportNumber
                            }).ToList();

            // Финансовые операции
            var finOps = (from fo in _context.FinOperations
                          join t in _context.FinOperationTypes on fo.TypeId equals t.Id into ft
                          from t in ft.DefaultIfEmpty()
                          where fo.EmployeeId == employeeId
                                && fo.Date.Date >= start.Date
                                && fo.Date.Date < end.Date
                          select new
                          {
                              fo.Id,
                              Date = fo.Date.Date,
                              fo.Sum,
                              fo.IsPenalty,
                              fo.Comment,
                              TypeId = fo.TypeId,
                              TypeName = t != null ? t.OperationName : null,
                              fo.Payed,
                              fo.ReportNumber
                          }).ToList();

            var finOpIds = finOps.Select(x => x.Id).ToList();
            var finReportRecords = _context.RevenueReportsFin
                .Where(x => finOpIds.Contains(x.FinOperationId))
                .GroupBy(x => x.FinOperationId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.ReportNumber).First());

            var items = new List<EmployeeFinancialReportRowDto>();


            // Заполняем строки по рабочим часам
            foreach (var wh in workHours)
            {
                var amount = wh.Hours * wh.Rate;
                var description =
                    $"Объект: {wh.ObjectName}. Рабочие часы: {wh.Hours}. Ставка в час: {wh.Rate} руб.";

                var accountingInfo = "--";
                //bool? payOff = null;
                /*if (whReportRecords.TryGetValue(wh.Id, out var rrWh))
                {
                    accountingInfo = $"Учтен в отчете #{rrWh.ReportNumber}";
                    payOff = rrWh.PayOff != 0;
                }*/
                if(wh.ReportNumber != null)
                    accountingInfo = $"Учтен в отчете #{wh.ReportNumber}";

                items.Add(new EmployeeFinancialReportRowDto
                {
                    Date = wh.Date,
                    Description = description,
                    Amount = amount,
                    AccountingInfo = accountingInfo,
                    Payed = wh.Payed,
                    ReportNumber = wh.ReportNumber
                });

                result.TotalHours += wh.Hours;
                result.TotalWorkHoursAmount += amount;
            }

            // Заполняем строки по рабочим дням
            foreach (var wd in workDays)
            {
                var amount = wd.Rate;
                var description =
                    $"Объект: {wd.ObjectName}. Рабочая смена. Ставка за смену: {wd.Rate} руб.";

                // Для рабочих дней пока нет отметок об учете
                var accountingInfo = "--";
                if(wd.ReportNumber != null)
                    accountingInfo = $"Учтен в отчете #{wd.ReportNumber}";

                items.Add(new EmployeeFinancialReportRowDto
                {
                    Date = wd.Date,
                    Description = description,
                    Amount = amount,
                    AccountingInfo = accountingInfo,
                    Payed = wd.Payed,
                    ReportNumber = wd.ReportNumber
                });

                result.TotalWorkDays += 1;
                result.TotalWorkDaysAmount += amount;
            }

            // Заполняем строки по финансовым операциям
            foreach (var fo in finOps)
            {
                var isPenalty = fo.IsPenalty;
                var sign = isPenalty ? -1 : 1;
                var amount = sign * fo.Sum;

                var typeName = string.IsNullOrWhiteSpace(fo.TypeName) ? "Без типа" : fo.TypeName;

                var descriptionPrefix = isPenalty ? "Списание" : "Начисление";
                var description =
                    $"{descriptionPrefix}. {typeName}. {fo.Sum} руб.";

                if (!string.IsNullOrWhiteSpace(fo.Comment))
                {
                    description += $" Комментарий: {fo.Comment}";
                }

                var accountingInfo = "--";
                /*if (finReportRecords.TryGetValue(fo.Id, out var rrFin))
                {
                    accountingInfo = $"Учтен в отчете #{rrFin.ReportNumber}";
                    payOff = rrFin.PayOff != 0;
                }*/

                items.Add(new EmployeeFinancialReportRowDto
                {
                    Date = fo.Date,
                    Description = description,
                    Amount = amount,
                    AccountingInfo = accountingInfo,
                    Payed = fo.Payed,
                    ReportNumber = fo.ReportNumber
                });

                //TODO: неправильная логика

                /*if (isPenalty)
                {
                    result.TotalPenalties += fo.Sum;
                }
                else
                {
                    result.TotalBonuses += fo.Sum;
                }*/
            }

            // Сортировка по дате
            result.Items = items
                .OrderBy(x => x.Date)
                .ThenBy(x => x.Description)
                .ToList();

            // Итоговая сумма: работа + начисления - списания
            //итоговой суммы нет - потому что она есть только в отчетах

            /*result.TotalSalary =
                result.TotalWorkHoursAmount +
                result.TotalWorkDaysAmount +
                result.TotalBonuses -
                result.TotalPenalties;*/

            result.ItemsCount = result.Items.Count;


            return result;
        }


        private int FinOpSum(List<FinOperationDb> operations, FinOperationTypeEnum type)
        {
            var sum = operations.Where(x=>x.TypeId == (int)type).Sum(x => x.Sum);
            return sum;
        }

        public FinOperationDb? GetAvansByReportNumber(int reportNumber,int employeeId)
        {
            return _context.FinOperations
                    .FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.AdvancePayment
                    && x.EmployeeId == employeeId
                    && x.ReportNumber == reportNumber);
        }

        /// <summary>
        /// Таблица в формате, совместимом с основной ведомостью (как GetReportForEmplList2): неоплаченные смены/часы и операции за период.
        /// </summary>
        public void GenerateTableForMainReportVer3(MainReportDto mrData, out DataTable table)
        {
            table = new DataTable();
            // object: в таблице и шапка с текстом заголовков, и числовые ячейки (int в int-колонку класть нельзя)
            table.Columns.Add("ФИО", typeof(object));
            table.Columns.Add("Дней", typeof(object));
            table.Columns.Add("Ставка", typeof(object));
            table.Columns.Add("Сумма за работу", typeof(object));
            table.Columns.Add("Списания: Штрафы", typeof(object));
            table.Columns.Add("Списания: Форма", typeof(object));
            table.Columns.Add("Списания: УЧО", typeof(object));
            table.Columns.Add("Списания: Другое", typeof(object));
            table.Columns.Add("Начисления: Другое", typeof(object));
            table.Columns.Add("Итого", typeof(object));
            table.Columns.Add("Подпись сотрудника", typeof(object));
            table.Columns.Add("Примечание", typeof(object));

            var meta = table.NewRow();
            meta["ФИО"] = "Финансовый отчёт (версия 3)";
            table.Rows.Add(meta);

            meta = table.NewRow();
            meta["ФИО"] = $"Период: {mrData.StartDate:dd.MM.yyyy} — {mrData.EndDate.AddDays(-1):dd.MM.yyyy} (включительно)";
            table.Rows.Add(meta);

            meta = table.NewRow();
            table.Rows.Add(meta);

            var headerRow = table.NewRow();
            foreach (DataColumn col in table.Columns)
                headerRow[col.ColumnName] = col.ColumnName;
            table.Rows.Add(headerRow);

            var grandTotal = 0;
            foreach (var fd in mrData.EmployeeFinDatas.OrderBy(x => x.Fio))
            {
                var rates = fd.WorkDays.Select(x => x.Rate)
                    .Union(fd.WorkHours.Select(x => x.Rate))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                if (rates.Count == 0)
                    rates.Add(0);

                var firstRateRow = true;
                var employeeRevenue = 0;
                var employeeSpisania = 0;
                var employeeOtherPayroll = 0;
                DataRow? firstRow = null;

                foreach (var rate in rates)
                {
                    var daysAtRate = fd.WorkDays.Where(x => x.Rate == rate).Sum(x => x.WorkDaysCount);
                    var hoursAtRate = fd.WorkHours.Where(x => x.Rate == rate).Sum(x => x.Hours);
                    var revenueFromDays = daysAtRate * rate;
                    var revenueFromHours = hoursAtRate * rate;
                    var revenue = revenueFromDays + revenueFromHours;

                    if (rate == 0 && daysAtRate == 0 && hoursAtRate == 0 && !fd.FinOperations.Any() && !fd.AdvancePaymentInPeriod)
                        continue;

                    var row = table.NewRow();
                    if (firstRateRow)
                    {
                        firstRow = row;
                        row["ФИО"] = fd.Fio;
                    }

                    row["Дней"] = daysAtRate;
                    row["Ставка"] = rate;
                    if (fd.AdvancePaymentInPeriod)
                        revenue = 0;

                    row["Сумма за работу"] = revenue;
                    employeeRevenue += revenue;

                    if (firstRateRow)
                    {
                        var shtraf = FinOpSum(fd, FinOperationTypeEnum.Shtraf);
                        var forma = FinOpSum(fd, FinOperationTypeEnum.Forma);
                        var ucho = FinOpSum(fd, FinOperationTypeEnum.Ucho);
                        var other = FinOpSum(fd, FinOperationTypeEnum.Other);
                        var otherPayroll = FinOpSum(fd, FinOperationTypeEnum.OtherPayroll)
                            + FinOpSum(fd, FinOperationTypeEnum.AdvancePayment);

                        row["Списания: Штрафы"] = shtraf;
                        row["Списания: Форма"] = forma;
                        row["Списания: УЧО"] = ucho;
                        row["Списания: Другое"] = other == 0 ? "" : other.ToString();
                        row["Начисления: Другое"] = otherPayroll;
                        employeeOtherPayroll = otherPayroll;
                        employeeSpisania = shtraf + forma + ucho + other;

                        if (fd.AdvancePaymentInPeriod)
                            row["Примечание"] = "В периоде есть неоплаченный аванс: сумма за смены/часы в итог не включена.";
                        else if (fd.AdvancePaymentInEarlyPeriod != 0)
                            row["Примечание"] = $"Ранее выплаченный аванс (до периода), сумма: {fd.AdvancePaymentInEarlyPeriod}";
                    }

                    table.Rows.Add(row);
                    firstRateRow = false;
                }

                if (firstRow != null)
                {
                    var empTotal = fd.AdvancePaymentInPeriod
                        ? employeeOtherPayroll - employeeSpisania
                        : employeeRevenue + employeeOtherPayroll - employeeSpisania;
                    grandTotal += empTotal;
                    firstRow["Итого"] = empTotal;
                }
            }

            var totalRow = table.NewRow();
            totalRow["ФИО"] = "";
            totalRow["Списания: Другое"] = "Общий итог:";
            totalRow["Итого"] = grandTotal;
            table.Rows.Add(totalRow);
        }

        private static int FinOpSum(EmployeeFinData fd, FinOperationTypeEnum type)
        {
            var id = (int)type;
            return fd.FinOperations.Where(x => x.TypeId == id).Sum(x => x.Sum);
        }

        private int FinOpSum(List<FinOperationItem> operations, FinOperationTypeEnum type)
        {
            var id = (int)type;
            return operations.Where(x => x.TypeId == id).Sum(x => x.Sum);
        }

        public DataTable SplitMainReportTablesList(List<TableDataDto> tables)
        {
            var result = new DataTable();

            var columnsCount = 11;

            var allTablesSum = 0;
            //int rowNumber = 0;
            for (int i = 0; i < columnsCount; i++)
                result.Columns.Add("");


            foreach (var tData in tables)
            {
                var row = result.NewRow();
                row.ItemArray = new string[] { tData.Title, "", "", "", "", "", "", "", "", "", "" };
                result.Rows.Add(row);
                
                // Заголовки столбцов
                row = result.NewRow();
                var a = new string[columnsCount];
                for (int i = 0; i < columnsCount; i++)
                    a[i] = tData.DataTable.Columns[i].ColumnName;
                row.ItemArray = a;
                result.Rows.Add(row);

                // Данные
                foreach (DataRow sourceRow in tData.DataTable.Rows)
                {
                    row = result.NewRow();
                    row.ItemArray = sourceRow.ItemArray;
                    result.Rows.Add(row);
                }

                //row.ItemArray = 
            }


            return result;
        }



        public void GetReportForEmplList2(DateTime begin, DateTime end, List<int> emplList, out DataTable table, out int totalSum)
        {
            table = new DataTable();
            totalSum = 0;
            //table.Columns.Add("# id", typeof(int));
            table.Columns.Add("ФИО", typeof(string));
            table.Columns.Add("Дней", typeof(int));
            table.Columns.Add("Ставка", typeof(int));

            //table.Columns.Add("Дата", typeof(DateTime));
            table.Columns.Add("Сумма за работу", typeof(int));
            table.Columns.Add("Списания: Штрафы", typeof(int));
            table.Columns.Add("Списания: Форма", typeof(int));
            table.Columns.Add("Списания: УЧО", typeof(int));
            table.Columns.Add("Списания: Другое", typeof(string));
            //table.Columns.Add("Начисления: Другое", typeof(int));
            table.Columns.Add("Начисления: Другое", typeof(int));
            table.Columns.Add("Итого", typeof(int));
            table.Columns.Add("Подпись сотрудника", typeof(string));




            if (emplList.Count > 0)
            {

                //var rows = new List<DataRow>();

                var empListSum = 0;
                foreach (var employeeId in emplList)
                {
                    var wh = _context.WorkHours
                        .Where(x => x.EmployeeId == employeeId
                        && x.WorkDate.Date >= begin.Date
                        && x.WorkDate.Date < end.Date);

                    //узнаем какие были ставки у этого сотрудника
                    var emplRates = wh.Select(x=>x.Rate).Distinct().ToList();

                    var emp = _context.Employees.FirstOrDefault(x => x.Id == employeeId);
                    if (emp == null) continue;

                    //fin 
                    var finOperation = _context.FinOperations
                        .Where(x => x.EmployeeId == employeeId
                        && x.Date.Date >= begin.Date.Date
                        && x.Date.Date < end.Date.Date).ToList();

                    //делаем для сотрудника столько строк сколько было ставок
                    bool firstRateRow = true;
                    int employeeSpisania = 0;
                    int employeeRevenue = 0;
                    int employeeOtherPayroll = 0;
                    DataRow row1 = null;
                    foreach (var rate in emplRates)
                    {
                        var row = table.NewRow();
                        if (firstRateRow)
                        {
                            row1 = row;//запоминаем первую строку
                            row[0] = emp.Fio;
                        }

                        //дни у каждой ставки будут свои
                        var days = wh.Where(x=>x.Rate == rate).Select(x => x.WorkDate.Date)
                                    .Distinct()
                                    .Count();
                        row[1] = days;
                        row[2] = rate;

                        var revenue = wh.Where(x => x.Rate == rate)
                            .Sum(x => x.Hours * x.Rate);//сумма за работу
                        row[3] = revenue;
                        employeeRevenue += revenue;//складываем выплаты с разной ставкой

                        //заполняем прочие доходы/расходы только для 1й строки
                        if (firstRateRow)
                        {
                            //списания
                            //типы списаний: штрафы
                            var shtraf = finOperation
                                .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Shtraf)
                                .Select(x => x.Sum)
                                .Sum();
                            row[4] = shtraf;

                            //Forma
                            var forma = finOperation
                                .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Forma)
                                .Select(x => x.Sum)
                                .Sum();
                            row[5] = forma;

                            //ucho
                            var ucho = finOperation
                                .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Ucho)
                                .Select(x => x.Sum)
                                .Sum();
                            row[6] = ucho;

                            //Other
                            var other = finOperation
                                .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Other)
                                .Select(x => x.Sum)
                                .Sum();
                            row[7] = other;

                            //начисления
                            //другие
                            var otherPayroll = finOperation
                                .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.OtherPayroll)
                                .Select(x => x.Sum)
                                .Sum();

                            row[8] = otherPayroll;
                            employeeOtherPayroll = otherPayroll;
                            //
                            //итог для одного сотрудника
                            employeeSpisania = shtraf + forma + ucho + other;
                            //выплаты

                            //var empTotal = revenue + otherPayroll - (shtraf + forma + ucho + other);
                            //empListSum = empListSum + empTotal;
                            //row[9] = empTotal;
                            
                        }
                        table.Rows.Add(row);

                        firstRateRow = false;
                    }

                    if (row1 != null)
                    {
                        var empTotal = employeeRevenue + employeeOtherPayroll - employeeSpisania;
                        empListSum = empListSum + empTotal;
                        row1[9] = empTotal;
                    }


                }

                //итоговая строка
                var rowTotal = table.NewRow();
                rowTotal[0] = "";
                rowTotal[7] = "Общий итог:";
                rowTotal[9] = empListSum;
                totalSum = empListSum;

                table.Rows.Add(rowTotal);
            }
        }

        /// <summary>
        /// Восстановить отчет по сохраненным отметкам об оплате (по report_number)
        /// </summary>
        public void GetReportForEmplListFromPayoutMarks(int reportNumber, out DataTable table, out int totalSum)
        {
            table = new DataTable();
            totalSum = 0;
            table.Columns.Add("ФИО", typeof(string));
            table.Columns.Add("Дней", typeof(int));
            table.Columns.Add("Ставка", typeof(int));
            table.Columns.Add("Сумма за работу", typeof(int));
            table.Columns.Add("Списания: Штрафы", typeof(int));
            table.Columns.Add("Списания: Форма", typeof(int));
            table.Columns.Add("Списания: УЧО", typeof(int));
            table.Columns.Add("Списания: Другое", typeof(string));
            table.Columns.Add("Начисления: Другое", typeof(int));
            table.Columns.Add("Итого", typeof(int));
            table.Columns.Add("Подпись сотрудника", typeof(string));

            // Получаем все work_hours_id для данного report_number
            var whIds = _context.RevenueReportsWh
                .Where(x => x.ReportNumber == reportNumber)
                .Select(x => x.WorkHoursId)
                .ToList();

            if (whIds.Count == 0)
            {
                return; // Нет данных для этого отчета
            }

            // Получаем исходные записи work_hours по ID
            var workHoursList = _context.WorkHours
                .Where(x => whIds.Contains(x.Id))
                .ToList();

            // Получаем все fin_operation_id для данного report_number
            var finOpIds = _context.RevenueReportsFin
                .Where(x => x.ReportNumber == reportNumber)
                .Select(x => x.FinOperationId)
                .ToList();

            // Получаем исходные записи fin_operations по ID
            var finOperationsList = _context.FinOperations
                .Where(x => finOpIds.Contains(x.Id))
                .ToList();

            // Группируем по сотрудникам
            var employeeIds = workHoursList.Select(x => x.EmployeeId).Distinct().ToList();

            var empListSum = 0;
            foreach (var employeeId in employeeIds)
            {
                // Получаем work_hours только для этого сотрудника из сохраненных
                var wh = workHoursList.Where(x => x.EmployeeId == employeeId);

                // Узнаем какие были ставки у этого сотрудника
                var emplRates = wh.Select(x => x.Rate).Distinct().ToList();

                var emp = _context.Employees.FirstOrDefault(x => x.Id == employeeId);
                if (emp == null) continue;

                // Получаем fin_operations только для этого сотрудника из сохраненных
                var finOperation = finOperationsList.Where(x => x.EmployeeId == employeeId).ToList();

                // Делаем для сотрудника столько строк сколько было ставок
                bool firstRateRow = true;
                int employeeSpisania = 0;
                int employeeRevenue = 0;
                int employeeOtherPayroll = 0;
                DataRow row1 = null;
                foreach (var rate in emplRates)
                {
                    var row = table.NewRow();
                    if (firstRateRow)
                    {
                        row1 = row; // Запоминаем первую строку
                        row[0] = emp.Fio;
                    }

                    // Дни у каждой ставки будут свои
                    var days = wh.Where(x => x.Rate == rate).Select(x => x.WorkDate.Date)
                                .Distinct()
                                .Count();
                    row[1] = days;
                    row[2] = rate;

                    var revenue = wh.Where(x => x.Rate == rate)
                        .Sum(x => x.Hours * x.Rate); // Сумма за работу
                    row[3] = revenue;
                    employeeRevenue += revenue; // Складываем выплаты с разной ставкой

                    // Заполняем прочие доходы/расходы только для 1й строки
                    if (firstRateRow)
                    {
                        // Списания
                        // Типы списаний: штрафы
                        var shtraf = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Shtraf)
                            .Select(x => x.Sum)
                            .Sum();
                        row[4] = shtraf;

                        // Forma
                        var forma = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Forma)
                            .Select(x => x.Sum)
                            .Sum();
                        row[5] = forma;

                        // Ucho
                        var ucho = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Ucho)
                            .Select(x => x.Sum)
                            .Sum();
                        row[6] = ucho;

                        // Other
                        var other = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Other)
                            .Select(x => x.Sum)
                            .Sum();
                        row[7] = other;

                        // Начисления
                        // Другие
                        var otherPayroll = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.OtherPayroll)
                            .Select(x => x.Sum)
                            .Sum();

                        row[8] = otherPayroll;
                        employeeOtherPayroll = otherPayroll;

                        // Итог для одного сотрудника
                        employeeSpisania = shtraf + forma + ucho + other;
                    }
                    table.Rows.Add(row);

                    firstRateRow = false;
                }

                if (row1 != null)
                {
                    var empTotal = employeeRevenue + employeeOtherPayroll - employeeSpisania;
                    empListSum = empListSum + empTotal;
                    row1[9] = empTotal;
                }
            }

            // Итоговая строка
            var rowTotal = table.NewRow();
            rowTotal[0] = "";
            rowTotal[7] = "Общий итог:";
            rowTotal[9] = empListSum;
            totalSum = empListSum;

            table.Rows.Add(rowTotal);
        }

        /// <summary>
        /// Создание отчета на выплаты
        /// </summary>
        /// <param name="createAuthor"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="employeeIds"></param>
        /// <returns></returns>
        public async Task<int> SavePayoutMarksLogic(string createAuthor, DateTime start, DateTime end, List<int> employeeIds)
        {
            // Получаем следующий номер отчета: макс. report_number из main_report_numbers + 1
            int reportNumber = 1;
            if (_context.MainReportNumbers.Any())
            {
                reportNumber = _context.MainReportNumbers.Max(x => x.ReportNumber) + 1;
            }

            //using (var transaction = _context.Database.BeginTransaction())
            {


                // Сохраняем запись в MainReportNumbers

                var mainReportRecord = new MainReportNumbersDb
                {
                    Created = DateTime.Now,
                    CreateAuthor = createAuthor,
                    ReportNumber = reportNumber,
                    StartDate = start,
                    EndDate = end,
                    EmployeeIds = string.Join(",", employeeIds)
                };
                _context.MainReportNumbers.Add(mainReportRecord);
                //await _context.SaveChangesAsync();

                //новая логика
                //проходим по всем платежам которые есть в отчете и проставляем им привязку к этому отчету
                // Получаем все work_hours для выбранных сотрудников за период
                //!не привязанные к другим отчетам
                
                
                //
                //прежде всего проверяем наличие аванса!
                //
                
                
                // Получаем все fin_operations для выбранных сотрудников за период
                var finOperationsList = _context.FinOperations
                    .Where(x => employeeIds.Contains(x.EmployeeId)
                        //&& x.Date.Date >= start.Date
                        && x.Date.Date < end.Date
                        && x.ReportNumber == null)
                    .ToList();

                var avans = finOperationsList.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.AdvancePayment);
                //тогда для этого номера отчета и для этого сотрудника 
                //привязываем в отчет только аванс
                if (avans != null)
                {
                    avans.ReportNumber = reportNumber;
                }
                else
                {
                    var workHoursList = _context.WorkHours
                        .Where(x => employeeIds.Contains(x.EmployeeId)
                            //&& x.WorkDate.Date >= start.Date
                            && x.WorkDate.Date < end.Date
                            && x.ReportNumber == null)
                        .ToList();

                    var workDaysList = _context.WorkDays
                    .Where(x => employeeIds.Contains(x.EmployeeId)
                        //&& x.WorkDate.Date >= start.Date
                        && x.WorkDate.Date < end.Date
                        && x.ReportNumber == null)
                    .ToList();

                    //привязываем к отчету
                    foreach (var wh in workHoursList)
                        wh.ReportNumber = reportNumber;

                    foreach (var wd in workDaysList)
                        wd.ReportNumber = reportNumber;

                    foreach (var f in finOperationsList)
                        f.ReportNumber = reportNumber;
                }

                await _context.SaveChangesAsync();
                //transaction.Commit();
            }

            return reportNumber;
        }

        /// <summary>
        /// Получить данные для отчета с отметками для отображения на сайте
        /// </summary>
        public List<PayAndMarkDto> GetPayoutMarksTableData(int reportNumber)
        {
            var result = new List<PayAndMarkDto>();
            //нужно заново собрать отчетную таблицу но по выбранным платежам
            var reportData = GetMainReportDataVer3(reportNumber);

            foreach (var fd in reportData.EmployeeFinDatas)
            {
                //var total = 0;

                /*if (fd.AdvancePaymentInPeriod)
                {
                    var avans = fd.FinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.AdvancePayment)?.Sum ?? 0;
                    total = avans;
                }
                else {

                    total = fd.WorkDays.Sum(x => x.WorkDaysCount * x.Rate);
                    total = total + fd.WorkHours.Sum(x => x.Hours * x.Rate);
                    var ucho = fd.FinOperations.Where(x => x.TypeId == (int)FinOperationTypeEnum.Ucho).Sum(x => x.Sum);
                    var shtraf = fd.FinOperations.Where(x => x.TypeId == (int)FinOperationTypeEnum.Shtraf).Sum(x => x.Sum);
                    var forma = fd.FinOperations.Where(x => x.TypeId == (int)FinOperationTypeEnum.Forma).Sum(x => x.Sum);
                    var other = fd.FinOperations.Where(x => x.TypeId == (int)FinOperationTypeEnum.Other).Sum(x => x.Sum);

                    total = total - ucho - shtraf - forma - other;
                    var otherPayrol = fd.FinOperations.Where(x => x.TypeId == (int)FinOperationTypeEnum.OtherPayroll).Sum(x => x.Sum);

                    total = total + otherPayrol;

                    if (fd.AdvancePaymentInEarlyPeriod != 0)
                    {
                        total = total - fd.AdvancePaymentInEarlyPeriod;
                    }
                }*/
                
                
                var item = new PayAndMarkDto
                {
                    EmployeeFio = fd.Fio,
                    EmployeeId = fd.EmployeeId,
                    HasAdvancePaymentInPrevPeriod = fd.AdvancePaymentInEarlyPeriod != 0,
                    HasAdvancePayment = fd.AdvancePaymentInPeriod,
                    TotalSum = fd.TotalSumForPeriod
                };

                result.Add(item);
            }

            return result;
        }


        //устаревший вариант через RevenueReportsFin

        /// <summary>
        /// Восстановить отчет по сохраненным отметкам об оплате для конкретных сотрудников
        /// </summary>
        /*public void GetReportForEmplListFromPayoutMarksByEmployees(int reportNumber, List<int> employeeIds, out DataTable table, out int totalSum)
        {
            table = new DataTable();
            totalSum = 0;
            table.Columns.Add("ФИО", typeof(string));
            table.Columns.Add("Дней", typeof(int));
            table.Columns.Add("Ставка", typeof(int));
            table.Columns.Add("Сумма за работу", typeof(int));
            table.Columns.Add("Списания: Штрафы", typeof(int));
            table.Columns.Add("Списания: Форма", typeof(int));
            table.Columns.Add("Списания: УЧО", typeof(int));
            table.Columns.Add("Списания: Другое", typeof(string));
            table.Columns.Add("Начисления: Другое", typeof(int));
            table.Columns.Add("Итого", typeof(int));
            table.Columns.Add("Подпись сотрудника", typeof(string));

            if (employeeIds.Count == 0)
            {
                return;
            }

            // Получаем все work_hours_id для данного report_number и сотрудников
            var whIds = _context.RevenueReportsWh
                .Where(x => x.ReportNumber == reportNumber)
                .Select(x => x.WorkHoursId)
                .ToList();

            if (whIds.Count == 0)
            {
                return;
            }

            // Получаем исходные записи work_hours по ID и фильтруем по сотрудникам
            var workHoursList = _context.WorkHours
                .Where(x => whIds.Contains(x.Id) && employeeIds.Contains(x.EmployeeId))
                .ToList();

            // Получаем все fin_operation_id для данного report_number
            var finOpIds = _context.RevenueReportsFin
                .Where(x => x.ReportNumber == reportNumber)
                .Select(x => x.FinOperationId)
                .ToList();

            // Получаем исходные записи fin_operations по ID и фильтруем по сотрудникам
            var finOperationsList = _context.FinOperations
                .Where(x => finOpIds.Contains(x.Id) && employeeIds.Contains(x.EmployeeId))
                .ToList();

            // Группируем по сотрудникам
            var empListSum = 0;
            foreach (var employeeId in employeeIds)
            {
                // Получаем work_hours только для этого сотрудника из сохраненных
                var wh = workHoursList.Where(x => x.EmployeeId == employeeId);

                if (!wh.Any())
                    continue;

                // Узнаем какие были ставки у этого сотрудника
                var emplRates = wh.Select(x => x.Rate).Distinct().ToList();

                var emp = _context.Employees.FirstOrDefault(x => x.Id == employeeId);
                if (emp == null) continue;

                // Получаем fin_operations только для этого сотрудника из сохраненных
                var finOperation = finOperationsList.Where(x => x.EmployeeId == employeeId).ToList();

                // Делаем для сотрудника столько строк сколько было ставок
                bool firstRateRow = true;
                int employeeSpisania = 0;
                int employeeRevenue = 0;
                int employeeOtherPayroll = 0;
                DataRow row1 = null;
                foreach (var rate in emplRates)
                {
                    var row = table.NewRow();
                    if (firstRateRow)
                    {
                        row1 = row; // Запоминаем первую строку
                        row[0] = emp.Fio;
                    }

                    // Дни у каждой ставки будут свои
                    var days = wh.Where(x => x.Rate == rate).Select(x => x.WorkDate.Date)
                                .Distinct()
                                .Count();
                    row[1] = days;
                    row[2] = rate;

                    var revenue = wh.Where(x => x.Rate == rate)
                        .Sum(x => x.Hours * x.Rate); // Сумма за работу
                    row[3] = revenue;
                    employeeRevenue += revenue; // Складываем выплаты с разной ставкой

                    // Заполняем прочие доходы/расходы только для 1й строки
                    if (firstRateRow)
                    {
                        // Списания
                        var shtraf = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Shtraf)
                            .Select(x => x.Sum)
                            .Sum();
                        row[4] = shtraf;

                        var forma = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Forma)
                            .Select(x => x.Sum)
                            .Sum();
                        row[5] = forma;

                        var ucho = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Ucho)
                            .Select(x => x.Sum)
                            .Sum();
                        row[6] = ucho;

                        var other = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.Other)
                            .Select(x => x.Sum)
                            .Sum();
                        row[7] = other;

                        var otherPayroll = finOperation
                            .Where(x => x.FinOperationType != null && x.FinOperationType.Id == (int)FinOperationTypeEnum.OtherPayroll)
                            .Select(x => x.Sum)
                            .Sum();

                        row[8] = otherPayroll;
                        employeeOtherPayroll = otherPayroll;
                        employeeSpisania = shtraf + forma + ucho + other;
                    }
                    table.Rows.Add(row);

                    firstRateRow = false;
                }

                if (row1 != null)
                {
                    var empTotal = employeeRevenue + employeeOtherPayroll - employeeSpisania;
                    empListSum = empListSum + empTotal;
                    row1[9] = empTotal;
                }
            }

            // Итоговая строка
            if (table.Rows.Count > 0)
            {
                var rowTotal = table.NewRow();
                rowTotal[0] = "";
                rowTotal[7] = "Общий итог:";
                rowTotal[9] = empListSum;
                totalSum = empListSum;
                table.Rows.Add(rowTotal);
            }
        }*/

    }


    public class EmployeeDataForReport
    {
        public int Employeeid { get; set; }
        public string? Fio { get; set; }


    }


    public class ReportItem
    {
        public int Employeeid { get; set; }
        public string? Fio { get; set; }
        public int? Days { get; set; }
        public int? TotalRevenue { get; set; }
        public int? TotalNachislenia { get; set; }
        public int? TotalSpisania { get; set; }
        public int? Total { get; set; }
        //public string? EmplOption { get; set; }
        //public string? BankName { get; set; }
    }


    public static class StringExtensions
    {
        public static FormattableString ToFormattableString(this string format, params object[] args)
        {
            return FormattableStringFactory.Create(format, args);
        }
    }
}
