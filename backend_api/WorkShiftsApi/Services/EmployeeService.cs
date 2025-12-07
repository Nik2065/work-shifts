using Microsoft.EntityFrameworkCore;
using System.Data;

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
            table.Columns.Add("# п/п", typeof(int));
            table.Columns.Add("ФИО", typeof(string));
            table.Columns.Add("Дата", typeof(DateTime));
            table.Columns.Add("Часов", typeof(int));
            table.Columns.Add("Ставка в час", typeof(int));


            //table.Rows.Add(1, "Иван", DateTime.Now, 1500.50m);
            //table.Rows.Add(2, "Мария", DateTime.Now.AddDays(-1), 2300.75m);
            //table.Rows.Add(3, "Алексей", DateTime.Now.AddDays(-2), 1890.00m);

            var rows = _context.Database.SqlQuery<ReportItem>(@$"select e.id, e.fio
                from work_hours w 
                left join employees e on w.employee_id=e.id
                where w.work_date >= '2025-11-01'
                and w.work_date<'2025-12-10'");

            var a = rows.ToList();

            return table;
        }




    }

    public class ReportItem
    {
        public int Id { get; set; }
        public string Fio { get; set; }
    }
}
