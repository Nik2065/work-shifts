using System.ComponentModel.DataAnnotations.Schema;

namespace WorkShiftsApi.DbEntities
{
    /// <summary>
    /// Отметки об оплате для fin_operations
    /// </summary>
    [Table("revenue_reports_fin")]
    public class RevenueReportFinDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [Column("fin_operation_id")]
        public int FinOperationId { get; set; }

        [Column("fo_sum")]
        public int FoSum { get; set; }

        [Column("fo_is_penalty")]
        public bool FoIsPenalty { get; set; }

        [Column("fo_type_id")]
        public int? FoTypeId { get; set; }

        [Column("report_number")]
        public int ReportNumber { get; set; }

        [Column("pay_off")]
        public int PayOff { get; set; }//Выплата произведена

    }
}
