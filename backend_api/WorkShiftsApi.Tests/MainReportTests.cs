using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Data;
using WorkShiftsApi.Controllers;
using WorkShiftsApi.Services;

namespace WorkShiftsApi.Tests
{
    /// <summary>
    /// Тестирование действий с основным финансовым отчетом 
    /// </summary>
    [TestFixture]
    internal class MainReportTests
    {
        private static AppDbContext _dbContext;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //для работы с виртуальной базой
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


        //
        // [ТЕСТЫ]
        //

        /* основная логика работы следующая:
         * вносим записи о работе и доп финансовые операции 
         * таблицы work_days, work_hours, fin_operations
         * на основе этих записей формируем финансовый отчет 
         * структура MainReportDto
         * для отчета есть методы - подготовить данные отчета чтобы его посмотреть или выгрузить 
         * (для просмотра на страницы и выгрузки есть отдельные методы которые работают с MainReportDto)
         * PrepareMainReportDataVer3
         * дальше этот отчет можно сохранить как "отчет с отметками о выплатах"
         * для сохранения вот такой метод и он работает с базой напрямую 
         * SavePayoutMarksLogic
         * 
         * по сохраненному отчету на выплаты (у каждого свой номер в таблице main_report_numbers)
         * можно заново получить структуру MainReportDto с помощью метода GetMainReportDataVer3
         * дальше необходимо в этом отчете подтвердить все выплаты
         * это через метод MarkPayoutRow (этот метод пока в контроллере, переношу его в логику)
         * 
         * Получить данные для отчета с отметками для отображения на сайте GetPayoutMarksTableData
         * */



        /* комбо тест
         * создаем записи на неделю 5-11 января
         * 2 записи на часы и аванс. 
         * сохраняем как отчет. проверяем что к оплате по отчету у нас аванс
         * помечаем аванс выплаченным
         * создаем записи на неделю 12-18 января
         * 2 записи на часы. 
         * сохраняем как отчет. проверяем что по отчету у нас 4 записи о часах. 
         * проверяем что логика суммирует зп и вычетает аванс прдыдущего периода
         * 
         */
        [Test]
        public async Task TwoPeriodAvansComboTest()
        {
            FillWorkHours_Week1();
            FillWorkHours_Week2();

            var es = new EmployeeService(_dbContext);
            var start1 = new DateTime(2026, 1, 5);
            var end1 = new DateTime(2026, 1, 11);
            var empIds = new List<int> { selectedEmployeeId1 };
            var employees = _dbContext.Employees
                .Where(x => empIds.Contains(x.Id))
                .ToList();

            var reportNumber = await es.SavePayoutMarksLogic("mail@mail.ru", start1, end1, empIds);
            //проверяем что в выплатах только аванс
            var report = es.GetMainReportDataVer3(reportNumber);
            var fd = report.EmployeeFinDatas.FirstOrDefault();
            int expectedTotalSum = 5000;
            Assert.That(expectedTotalSum, Is.EqualTo(fd.TotalSumForPeriod));

            //помечаем аванс выплаченным или отменяем оплату
            //int ReportNumber
            //int EmployeeId
            //bool Checked
            es.MarkPayoutRowLogic(reportNumber, selectedEmployeeId1, true);


            //отменяем оплату?


        }






        public int selectedEmployeeId1 = 1;
        public int selectedEmployeeId2 = 2;


        //week1
        private void FillWorkHours_Week1()
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

        private void FillWorkHours_Week2()
        {
            //заполняем период данными 
            //период 12-18 января 2026
            //2 записи о рабочих часах
            //заполняем 1-ю запись о рабочих часах
            var wh1 = new WorkHoursDb
            {
                Created = DateTime.Now,
                EmployeeId = selectedEmployeeId1,
                Hours = 1,
                Rate = 200,
                WorkDate = new DateTime(2026, 1, 12)
            };
            _dbContext.WorkHours.Add(wh1);
            //заполняем 2-ю запись о рабочих часах
            var wh2 = new WorkHoursDb
            {
                Created = DateTime.Now,
                EmployeeId = selectedEmployeeId1,
                Hours = 1,
                Rate = 300,
                WorkDate = new DateTime(2026, 1, 13)
            };
            _dbContext.WorkHours.Add(wh2);



            _dbContext.SaveChanges();
        }

    }
}
