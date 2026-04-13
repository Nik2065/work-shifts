
using WorkShiftsApi.Controllers;

namespace WorkShiftsApi.Tests
{
    internal class InitDb
    {
        public void FillDbDictionary(AppDbContext db)
        {
            //банки
            db.Banks.Add(new DbEntities.BankDb { BankName = "Альфа тест", Id = 1 });

            //объекты
            var o1 = new ObjectDb { Id = 1, Address = "Улица", Name = "Объект А" };
            var o2 = new ObjectDb { Id = 2, Address = "Улица", Name = "Объект Б" };

            db.Objects.Add(o1);
            db.Objects.Add(o2);
            db.SaveChanges();

            //сотрудники
            var e1 = new EmployeesDb
            {
                Id = 1,
                Created = DateTime.Now.AddDays(-1),
                BankId = 1,
                EmplOptions = EmplOptionEnums.Card,
                DateOfBirth = DateTime.Now.AddYears(-20),
                Fio = "Тестовый 1",
                ObjectId = 1,

            };

            var e2 = new EmployeesDb
            {
                Id=2,
                Created= DateTime.Now.AddDays(-1), 
                BankId = null,
                EmplOptions = EmplOptionEnums.Vedomost,
                Fio = "Тестовый 2",
                ObjectId = 1
            };

            db.Employees.Add(e1);
            db.Employees.Add(e2);
            db.SaveChanges();
        }
    }
}
