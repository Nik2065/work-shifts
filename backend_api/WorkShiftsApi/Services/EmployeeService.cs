using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

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
        private  AppDbContext  _context { get; set; }


        public DataTable GetReportForEmplList(DateTime begin, DateTime end, List<int> emplList)
        {
            var table = new DataTable();
            //table.Columns.Add("# п/п", typeof(int));
            table.Columns.Add("ФИО", typeof(string));
            table.Columns.Add("Дней", typeof(int));
            //table.Columns.Add("Дата", typeof(DateTime));
            table.Columns.Add("Сумма за работу", typeof(int));
            table.Columns.Add("Начисления", typeof(int));
            table.Columns.Add("Списания", typeof(int));
            table.Columns.Add("Итого", typeof(int));
            table.Columns.Add("EmplOption", typeof(string));
            table.Columns.Add("BankName", typeof(string));

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

            string sql = $@"
            select fio, count(*) days, IFNULL(sum(revenue),0) as revenue, IFNULL(sum(Nachislenia),0) as Nachislenia
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
                left join fin_operations penalty on penalty.employee_id=e.id and penalty.is_penalty=true
                left join fin_operations notPenalty on notPenalty.employee_id=e.id and notPenalty.is_penalty=false
	            where w.work_date >= '{begin.ToString("yyyy-MM-dd")}'
	            and w.work_date< '{end.ToString("yyyy-MM-dd")}'
                and e.id in ({list})
	            group by fio, cast(w.work_date as date)

            )
            as q
            group by fio;";

            //FormattableString formattable = sql.ToFormattableString(begin.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"), list);

            var formattable = FormattableStringFactory.Create(sql, []);
            var rows = _context.Database.SqlQuery<ReportItem>(formattable);

            var a = rows.ToList();

            foreach (var row in a) 
            {
                var arr = new object[]
                { 
                    row?.Fio ?? "", 
                    row?.Days, 
                    row?.Revenue,
                    row?.Spisania, 
                    row?.Nachislenia, 
                    row?.Total, 
                    row?.EmplOption,
                    row?.BankName
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


    }

    public class ReportItem
    {
        //public int Id { get; set; }
        public string? Fio { get; set; }
        public int? Days { get; set; }
        public int? Revenue { get; set; }
        public int? Nachislenia { get; set; }
        public int? Spisania { get; set; }
        public int? Total { get; set; }
        public string? EmplOption { get; set; }
        public string? BankName { get; set; }
    }


    public static class StringExtensions
    {
        public static FormattableString ToFormattableString(this string format, params object[] args)
        {
            return FormattableStringFactory.Create(format, args);
        }
    }
}
