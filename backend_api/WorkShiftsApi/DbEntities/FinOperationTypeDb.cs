using System.ComponentModel.DataAnnotations.Schema;

namespace WorkShiftsApi.DbEntities
{
    /// <summary>
    /// типы финансовых операций
    /// </summary>
    [Table("op_types")]
    public class FinOperationTypeDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("op_name")]
        public string OperationName { get; set; }

        //относится ли данный тип операции к начислениям
        //если false - то это какой-то штраф
        [Column("is_payroll")]
        public bool IsPayroll { get; set; }

    }
}
