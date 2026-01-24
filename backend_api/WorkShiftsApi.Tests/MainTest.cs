using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Internal;
using System.Data;
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

            var begin = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 20);
            var empList = new List<int> { 1 };

            empService.GetReportForEmplList2(begin, end, empList, out DataTable table, out int totalSum);





            Assert.Pass();
        }
    }
}
