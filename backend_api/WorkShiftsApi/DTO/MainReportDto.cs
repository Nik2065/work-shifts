namespace WorkShiftsApi.DTO
{
    //для хранения данных отчета для всех пользователей списка
    public class MainReportDto
    {
        //public List<int> EmpIds { get; set; } = new List<int>();
        public List<EmployeesDb> Employees { get; set; }
        public DateTime EndDate { get; set; }

        public List<EmployeeFinData> EmployeeFinDatas { get; set; } = new List<EmployeeFinData>();
    }

    //Финансовые данные о сотруднике за период
    public class EmployeeFinData
    {
        public int EmployeeId { get; set; }
        public string Fio {  get; set; }


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
        public List<WorkHourFinItem> NotPayedWorkHours { get; set; } = new List<WorkHourFinItem>();

        public List<WorkDayFinItem> NotPayedWorkDays { get; set; } = new List<WorkDayFinItem>();

        public List<FinOperationItem> NotPayedFinOperations { get; set; } = new List<FinOperationItem>();


         
    }


    //содержимое из таблицы work_hours
    public class WorkHourFinItem
    {
        public int Id { get; set; }
        public int Hours { get; set; }
        public int Rate { get; set; }
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

    }

}
