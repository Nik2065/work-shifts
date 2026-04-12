namespace WorkShiftsApi.DTO
{
    public class GetMainReportFromPayoutMarks2Response : ResponseBase
    {
        public List<PayAndMarkDto> Items { get; set; } = new List<PayAndMarkDto>();

    }
}
