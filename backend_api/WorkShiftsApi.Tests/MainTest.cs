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
        //тест функции PrepareMainReportDataVer3
        public void CreateMainReportData_checkFillWorkHours()
        {
            //...Исходные данные

            //заполняем период данными 
            //период 5-11 января 2026
            int selectedEmployeeId1 = 1;



            //...Логика
            //........................................................
            var es = new EmployeeService(_dbContext);
            var start = new DateTime(2026,1, 5);
            var end = new DateTime(2026, 1, 11);
            var empIds = new List<int> { selectedEmployeeId1 };
            var employees = _dbContext.Employees
                .Where(x => empIds.Contains(x.Id))
                .ToList();
            FillWorkHours_Example1(selectedEmployeeId1);


            _dbContext.SaveChanges();

            var mainReportData = es.PrepareMainReportDataVer3(start, end, employees);

            //...Оцениваем результат
            //........................................................
            //должна быть одна запись о финансах
            var cnt = mainReportData.EmployeeFinDatas.Count();
            int expected = 1;

            Assert.That(expected, Is.EqualTo(cnt));

            var fd1= mainReportData.EmployeeFinDatas.FirstOrDefault();
            var whFinList = fd1?.WorkHours;

            //должны быть 2 записи о рабочих часах потому что использовались разные ставки
            int expected2 = 2;
            Assert.That(expected2, Is.EqualTo(whFinList.Count()));
        }



        [Test]
        //забираем данные отчета в нужный формат
        //проверяем формат
        //example 1 - 2 записи о рабочих часах
        public async Task Check_GetMainReportDataVer3_example1()
        {
            //заполняем период данными 
            //период 5-11 января 2026
            int selectedEmployeeId1 = 1;
            //записи о работе ....................................
            var es = new EmployeeService(_dbContext);
            var start = new DateTime(2026, 1, 5);
            var end = new DateTime(2026, 1, 11);
            var empIds = new List<int> { selectedEmployeeId1 };
            var employees = _dbContext.Employees
                .Where(x => empIds.Contains(x.Id))
                .ToList();
            FillWorkHours_Example1(selectedEmployeeId1);

            //делаем из них отчет.
            var _employeeService = new EmployeeService(_dbContext);
            var createAuthor = "noname";
            var reportNumber = await _employeeService.SavePayoutMarksLogic(createAuthor, start, end, empIds);

            //получаем отчет с помощью метода. проверяем результат
            var report = _employeeService.GetMainReportDataVer3(reportNumber);

            //...проверки........
            int expectedTotalSum = 5*200 + 5*300; //см.example1

            var expectedWdSum = 0;
            var wdTotalSum = report.EmployeeFinDatas
                .FirstOrDefault(x => x.EmployeeId == selectedEmployeeId1)?.WorkDays.Sum(x => x.Rate * x.WorkDaysCount);
            
            Assert.That(expectedWdSum, Is.EqualTo(wdTotalSum));

            var expectedWhSum = expectedTotalSum;
            var whTotalSum = report.EmployeeFinDatas
                .FirstOrDefault(x => x.EmployeeId == selectedEmployeeId1)?.WorkHours.Sum(x => x.Rate * x.Hours);

            Assert.That(expectedWhSum, Is.EqualTo(whTotalSum));


        }

        //аванс
        [Test]
        //забираем данные отчета в нужный формат
        //проверяем формат
        //example 1 - 2 записи о рабочих часах
        public async Task Check_GetMainReportDataVer3_example2()
        {
            //заполняем период данными 
            //период 5-11 января 2026
            int selectedEmployeeId1 = 1;
            //записи о работе ....................................
            var es = new EmployeeService(_dbContext);
            var start = new DateTime(2026, 1, 5);
            var end = new DateTime(2026, 1, 11);
            var empIds = new List<int> { selectedEmployeeId1 };
            var employees = _dbContext.Employees
                .Where(x => empIds.Contains(x.Id))
                .ToList();
            FillWorkHours_Example2(selectedEmployeeId1);

            //делаем из них отчет.
            var _employeeService = new EmployeeService(_dbContext);
            var createAuthor = "noname";
            var reportNumber = await _employeeService.SavePayoutMarksLogic(createAuthor, start, end, empIds);

            //получаем отчет с помощью метода. проверяем результат
            var report = _employeeService.GetMainReportDataVer3(reportNumber);

            //...проверки........
            int expectedTotalSum = 5000; //см.example


            /*
            var expectedWdSum = 0;
            var wdTotalSum = report.EmployeeFinDatas
                .FirstOrDefault(x => x.EmployeeId == selectedEmployeeId1)?.WorkDays.Sum(x => x.Rate * x.WorkDaysCount);

            Assert.That(expectedWdSum, Is.EqualTo(wdTotalSum));

            var expectedWhSum = expectedTotalSum;
            var whTotalSum = report.EmployeeFinDatas
                .FirstOrDefault(x => x.EmployeeId == selectedEmployeeId1)?.WorkHours.Sum(x => x.Rate * x.Hours);

            Assert.That(expectedWhSum, Is.EqualTo(whTotalSum));
            */

        }




        [Test]
        //получаем данные сохраненного отчета
        //для отображения в коротком виде на странице с отметками
        public async Task GetMainReportData()
        {
            //заполняем период данными 
            //период 5-11 января 2026
            int selectedEmployeeId1 = 1;
            //записи о работе ....................................
            var es = new EmployeeService(_dbContext);
            var start = new DateTime(2026, 1, 5);
            var end = new DateTime(2026, 1, 11);
            var empIds = new List<int> { selectedEmployeeId1 };
            var employees = _dbContext.Employees
                .Where(x => empIds.Contains(x.Id))
                .ToList();
            FillWorkHours_Example1(selectedEmployeeId1);

            //делаем из них отчет.
            var _employeeService = new EmployeeService(_dbContext);
            var createAuthor = "noname";
            var reportNumber = await _employeeService.SavePayoutMarksLogic(createAuthor, start, end, empIds);
            //Проверяем как отчет сохранился .................................

            var wh = _dbContext.WorkHours.Where(x => x.ReportNumber == reportNumber).ToList();

            //всего 2 плдатежа в базе и у них появился признак что они добавлены в отчет номер reportNumber
            var expectedCount = 2;
            Assert.That(expectedCount, Is.EqualTo(wh.Count));

            //а теперь попробуем получить результаты этого отчета в новом формате
            //для отображения на странице с отметками

            var shortReport = _employeeService.GetPayoutMarksTableData(reportNumber);

            
        }

        //[Test]
        //TODO: Добавить тест для поиска предыдущего аванса в периоде


        /*[Test]
        //получаем данные сохраненного отчета
        //для отображения в коротком виде на странице с отметками
        public async Task Check_GetPayoutMarksTableData()
        {
            //заполняем период данными 
            //период 5-11 января 2026
            int selectedEmployeeId1 = 1;
            //записи о работе ....................................
            var es = new EmployeeService(_dbContext);
            var start = new DateTime(2026, 1, 5);
            var end = new DateTime(2026, 1, 11);
            var empIds = new List<int> { selectedEmployeeId1 };
            var employees = _dbContext.Employees
                .Where(x => empIds.Contains(x.Id))
                .ToList();
            FillWorkHours_Example1(selectedEmployeeId1);




        }*/


        private void FillWorkHours_Example1(int selectedEmployeeId1)
        {
            //заполняем период данными 
            //период 5-11 января 2026
            //2 записи о рабочих часах
            //заполняем 1-ю запись о рабочих часах
            var wh1 = new WorkHoursDb
            {
                Created = DateTime.Now,
                EmployeeId = selectedEmployeeId1,
                Hours = 5,
                Rate = 200,
                WorkDate = new DateTime(2026, 1, 5)
            };
            _dbContext.WorkHours.Add(wh1);
            //заполняем 2-ю запись о рабочих часах
            var wh2 = new WorkHoursDb
            {
                Created = DateTime.Now,
                EmployeeId = selectedEmployeeId1,
                Hours = 5,
                Rate = 300,
                WorkDate = new DateTime(2026, 1, 6)
            };
            _dbContext.WorkHours.Add(wh2);


            _dbContext.SaveChanges();
        }


        //пример с авансом
        private void FillWorkHours_Example2(int selectedEmployeeId1)
        {
            //заполняем период данными 
            //период 5-11 января 2026
            //2 записи о рабочих часах
            //заполняем 1-ю запись о рабочих часах
            var wh1 = new WorkHoursDb
            {
                Created = DateTime.Now,
                EmployeeId = selectedEmployeeId1,
                Hours = 5,
                Rate = 200,
                WorkDate = new DateTime(2026, 1, 5)
            };
            _dbContext.WorkHours.Add(wh1);
            //заполняем 2-ю запись о рабочих часах
            var wh2 = new WorkHoursDb
            {
                Created = DateTime.Now,
                EmployeeId = selectedEmployeeId1,
                Hours = 5,
                Rate = 300,
                WorkDate = new DateTime(2026, 1, 6)
            };
            _dbContext.WorkHours.Add(wh2);

            //аванс в периоде
            var fo = new FinOperationDb
            {
                Comment = "test test",
                Created = DateTime.Now,
                TypeId = (int)FinOperationTypeEnum.AdvancePayment,
                Date = new DateTime(2026, 1, 5),
                EmployeeId = selectedEmployeeId1,
                IsPenalty = false,
                Sum = 5000,
            };

            _dbContext.FinOperations.Add(fo);

            _dbContext.SaveChanges();
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
