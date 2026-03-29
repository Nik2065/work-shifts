using System.ComponentModel.DataAnnotations.Schema;

namespace WorkShiftsApi.DbEntities
{
    /// <summary>
    /// Номера отчетов (история сохраненных отчетов)
    /// </summary>
    [Table("main_report_numbers")]
    public class MainReportNumbersDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [Column("create_author")]
        public string? CreateAuthor { get; set; }

        [Column("report_number")]
        public int ReportNumber { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }
    }
}
