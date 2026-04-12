using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Data;
using WorkShiftsApi.Controllers;
using WorkShiftsApi.Services;



namespace WorkShiftsApi.Tests
{
    [TestFixture]
    public class MainTest
    {
        public MainTest()
        {

        }

        //private readonly string _connectionString = "server=localhost;database=workshiftsdb;user=workshifts_user;password=kjsdhH547";
        private static AppDbContext _dbContext;

        // Выполняется один раз перед всеми тестами
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {

            //для работы с реальной базой

            /*var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                _connectionString,
                ServerVersion.AutoDetect(_connectionString),
                options =>
                {
                    options.EnableRetryOnFailure(3);
                    options.CommandTimeout(30);
                })
            .Options;

            _dbContext = new AppDbContext(options);*/
            //для работы с виртуальной базой

            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);

            //заполняем основные данные
            var initdb = new InitDb();
            initdb.FillDbDictionary(_dbContext);
        }

        // Выполняется один раз после всех тестов
        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
            _dbContext?.Dispose();      // Освобождаем ресурсы – БД «удаляется» из памяти
            _dbContext = null;
        }


        [Test]
        //создаем основной фин отчет (MainReport)
        //заполняем только рабочие часы
        public void CreateMainReportData_checkFillWorkHours()
        {
            //заполняем период данными 
            //период 5-11 января 2026
            var es = new EmployeeService(_dbContext);
            var start = new DateTime(5, 1, 2026);
            var end = new DateTime(11, 1, 2026);
            var empIds = new List<int> { 1 };
            var employees = _dbContext.Employees
                .Where(x => empIds.Contains(x.Id))
                .ToList();

            var mainReportData = es.PrepareMainReportDataVer3(start, end, employees);

            //Оцениваем результат

        }


        [Test]
        //получаем данные сохраненного отчета
        //для отображения в коротком виде на странице с отметками
        public void GetMainReportData()
        {
            //
        }


        // проверка работы отчета
        /*[Test]
        public void TestReportResult()
        {
            var empService = new EmployeeService(_dbContext);

            var start = new DateTime(2026, 3, 1);
            var end = new DateTime(2026, 3, 30);
            var empList = new List<int> { 1, 2, 3, 4, 5 };
            var employees = _dbContext.Employees.Include(e => e.Bank).Where(x => empList.Contains(x.Id)).ToList();

            //empService.GetReportForEmplList2(begin, end, empList, out DataTable table, out int totalSum);
            
            //собираем данные для отчета
            var reportData = empService.PrepareMainReportDataVer3(start, end, employees);

            //перекладываем данные отчета в таблицу 
            //empService.GenerateTableForMainReportVer3(mrData, out DataTable table);
            //дальше можн из этой таблицы делать или эксель или таблицу сайта

            var gen = new ExcelGenerator();
            var bytes = gen.CreateExcelFromMainReportVer4Table(reportData);

            File.WriteAllBytes(@"c:\tmp\my.xlsx", bytes);


        }*/




        /*[Test]
        public void TestReportResult2()
        {
            var empService = new EmployeeService(_dbContext);

            var begin = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 20);
            var empList = new List<int> { 1 };

            empService.GetReportForEmplList2(begin, end, empList, out DataTable table, out int totalSum);

            var resultTable = new MainReportTable();

            resultTable.Rows = new MrRow[table.Rows.Count];

            var i = 0;
            foreach (DataRow row in table.Rows)
            {
                var ggg = row.ItemArray;
                var ggg1 = GetStrArray(row.ItemArray);

                resultTable.Rows[i] = new MrRow();
                resultTable.Rows[i].RowItems = ggg1;
                i += 1;
                
            }

        }*/

        private string[] GetStrArray(object[] items)
        {
            var length = items.Length;
            var ggg1 = new string[length];
            for (int j = 0; j < length; j++)
            {
                var result = "";
                if (items[j] != null) result = items[j].ToString();
                ggg1[j] = result;
            }
            return ggg1;
        }


    }




}
