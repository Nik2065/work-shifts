using System.ComponentModel.DataAnnotations.Schema;

namespace WorkShiftsApi.DbEntities
{
    /// <summary>
    /// Отметки об оплате для work_hours
    /// </summary>
    [Table("revenue_reports_wh")]
    public class RevenueReportWhDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [Column("work_hours_id")]
        public int WorkHoursId { get; set; }

        [Column("wh_hours")]
        public int WhHours { get; set; }

        [Column("wh_rate")]
        public int WhRate { get; set; }

        [Column("wh_sum")]
        public int WhSum { get; set; }

        [Column("wh_work_date")]
        public DateTime WhWorkDate { get; set; }

        [Column("report_number")]
        public int ReportNumber { get; set; }

        [Column("pay_off")]
        public int PayOff { get; set; }//Выплата произведена
    }
}
