using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using WorkShiftsApi.Controllers;
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
            foreach (var b in Banks.BanksList)
            {
                var empListN = emplList.Where(x => x.BankName.Trim().ToLower() == b.Trim().ToLower());
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

        public DataTable SplitMainReportTablesList(List<TableDataDto> tables)
        {
            var result = new DataTable();

            var columnsCount = 10;

            var allTablesSum = 0;
            //int rowNumber = 0;
            for (int i = 0; i < columnsCount; i++)
                result.Columns.Add("");


            foreach (var tData in tables)
            {
                var row = result.NewRow();
                row.ItemArray = new string[] { tData.Title, "", "", "", "", "", "", "", "", "" };
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
