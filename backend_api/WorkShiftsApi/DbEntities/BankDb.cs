using System.ComponentModel.DataAnnotations.Schema;

namespace WorkShiftsApi.DbEntities
{
    [Table("banks")]
    public class BankDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("bank_name")]
        public string BankName { get; set; } = string.Empty;

    }
}
