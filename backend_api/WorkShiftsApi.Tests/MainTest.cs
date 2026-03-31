using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Internal;
using System.Data;
using System.Text.Json.Serialization;
using WorkShiftsApi.Controllers;
using WorkShiftsApi.DTO;
using WorkShiftsApi.Services;

namespace WorkShiftsApi.Tests
{
    public class MainTest
    {
        public MainTest()
        {

        }

        private readonly string _connectionString = "server=localhost;database=workshiftsdb;user=workshifts_user;password=kjsdhH547";
        private AppDbContext _dbContext;


        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                _connectionString,
                ServerVersion.AutoDetect(_connectionString),
                options =>
                {
                    options.EnableRetryOnFailure(3);
                    options.CommandTimeout(30);
                })
            .Options;

            _dbContext = new AppDbContext(options);
        }

        [TearDown]
        public async Task TearDown()
        {
            // Очистка таблиц после каждого теста (но не удаление БД)
            if (_dbContext != null)
            {
                await _dbContext.DisposeAsync();
            }
        }

        // проверка работы отчета
        [Test]
        public void TestReportResult()
        {
            var empService = new EmployeeService(_dbContext);

            var start = new DateTime(2026, 3, 1);
            var end = new DateTime(2026, 3, 30);
            var empList = new List<int> { 1, 2 };
            var employees = _dbContext.Employees.Where(x => empList.Contains(x.Id)).ToList();

            //empService.GetReportForEmplList2(begin, end, empList, out DataTable table, out int totalSum);
            
            //собираем данные для отчета
            var reportData = empService.CreateMainReportVer3(start, end, employees);

            //перекладываем данные отчета в таблицу 
            //empService.GenerateTableForMainReportVer3(mrData, out DataTable table);
            //дальше можн из этой таблицы делать или эксель или таблицу сайта

            var gen = new ExcelGenerator();
            var bytes = gen.CreateExcelFromMainReportVer4Table(reportData);

            File.WriteAllBytes(@"c:\tmp\my.xlsx", bytes);


        }


        [Test]
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

        }

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
