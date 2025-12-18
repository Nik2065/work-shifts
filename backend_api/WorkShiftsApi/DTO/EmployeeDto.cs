using WorkShiftsApi.Controllers;

namespace WorkShiftsApi.DTO
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Fio { get; set; }
        public DateTime Created { get; set; }
        public string? BankName { get; set; }
        public int? Age { get; set; }
        public bool ChopCertificate { get; set; }
        public string ObjectName { get; set; }
        public int ObjectId { get; set; }
        public string? EmplOptions { get; set; }

        public List<WorkShiftDto> WorkShiftList { get; set; }

        public List<FinOpDto>? FinOperations { get; set; }

        //даты вахты если текущая дата попадает в диапазон вахт
        public DateTime? WorkShiftStart { get; set; }
        public DateTime? WorkShiftEnd { get; set; }
        public bool IsInWorkShift { get; set; }//есть ли сейчас вахта
    }
}
