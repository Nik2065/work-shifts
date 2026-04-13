using WorkShiftsApi;

namespace WorkShiftsApi.DTO
{
    //для хранения данных отчета для всех пользователей списка
    public class MainReportDto
    {
        //public List<int> EmpIds { get; set; } = new List<int>();
        public List<EmployeesDb> Employees { get; set; } = new List<EmployeesDb>();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<EmployeeFinData> EmployeeFinDatas { get; set; } = new List<EmployeeFinData>();
    }

    //Финансовые данные о сотруднике за период
    public class EmployeeFinData
    {
        public int EmployeeId { get; set; }
        public string Fio { get; set; } = "";

        //ведомость
        public bool Vedomost { get; set; }
        //или банк
        public string? BankName { get; set; }

        /// <summary>
        /// Если в выбранном периоде есть выплата авансового платежа, то в качестве результата ЗП не учитываются рабочаи часы и дни
        /// </summary>
        public bool AdvancePaymentInPeriod { get; set; }
        
        /// <summary>
        /// Если в предыдущем периоде был выплачен аванс то в этом периоде его минусуем
        /// </summary>
        public int AdvancePaymentInEarlyPeriod { get; set; }

        //public int NotPayedWorkHours { get; set; }
        //public int PayedWorkHours { get; set; }
        public List<WorkHourFinItem> WorkHours { get; set; } = new List<WorkHourFinItem>();

        public List<WorkDayFinItem> WorkDays { get; set; } = new List<WorkDayFinItem>();

        public List<FinOperationItem> FinOperations { get; set; } = new List<FinOperationItem>();


        public decimal TotalSumForPeriod { get; set; }
    }


    //содержимое из таблицы work_hours
    public class WorkHourFinItem
    {
        public int Hours { get; set; }//общее количество часов за период
        public int Rate { get; set; }//ставка для этих часов
        //Общая стоимость, salary = Hours*Rate
    }

    //данные из таблицы work_days
    /*public class WorkDayFinItem
    {
        public int WorkDayId { get; set; }//идентификатор из таблицы
        public int Salary { get; set; }
        //Общая стоимость, salary
    }*/

    public class WorkDayFinItem 
    { 
        public int WorkDaysCount { get; set; }//количество дней
        public int Rate { get; set; } //ставка для этих дней
    
    }

    public class FinOperationItem
    {
        public int? TypeId { get; set; }
        public int Sum { get; set; }
    }

}
