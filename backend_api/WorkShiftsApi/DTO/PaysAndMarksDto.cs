namespace WorkShiftsApi.DTO
{
    public class PayAndMarkDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeFio { get; set; } = string.Empty;
        public decimal TotalSum { get; set; }

        public bool? HasAdvancePayment { get; set; }//был ли аванс в этом финансовом отчете
        public bool? HasAdvancePaymentInPrevPeriod { get; set; } //был ли аванс в прдыдущем финансовом отчете
    }
}
