using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkShiftsApi.Tests
{
    [TestFixture]
    //тестирование отчета для одного сотрудника
    internal class ReportForSingleEmployeeTests
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


    }
}
