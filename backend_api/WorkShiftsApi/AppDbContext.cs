using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkShiftsApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //fluent api
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinOperationDb>()
                .HasOne(f => f.Employee)
                .WithMany(e => e.FinOperations)
                .HasForeignKey(f => f.EmployeeId);

            modelBuilder.Entity<WorkShiftsDb>()
                .HasOne(f => f.Employee)
                .WithMany(e => e.WorkShifts)
                .HasForeignKey(f => f.EmployeeId);

            // Один ко многим
            modelBuilder.Entity<FinOperationDb>()
                .HasOne(f => f.FinOperationType)   // FinOperation имеет один Type
                .WithMany()      
                .HasForeignKey(f => f.TypeId) // Внешний ключ в FinOperation
                .IsRequired(false);           // Не обязательная связь (TypeId может быть null)

            modelBuilder.Entity<EmployeesDb>()
               .HasOne(e => e.Object) // У сотрудника один объект
               .WithMany() // У объекта много сотрудников
               .HasForeignKey(e => e.ObjectId)
               .IsRequired(); // object_id NOT NULL
        }

        public DbSet<SiteUserDb> SiteUsers { get; set; }

        public DbSet<EmployeesDb> Employees { get; set; }

        public DbSet<WorkShiftsDb> WorkShifts { get; set; }

        public DbSet<WorkHoursDb> WorkHours { get; set; }

        public DbSet<ObjectDb> Objects { get; set; }

        public DbSet<UserToObjectDb> UserToObject { get; set; }

        public DbSet<FinOperationDb> FinOperations { get; set; }

        public DbSet<FinOperationTypeDb> FinOperationTypes { get; set; }

        public DbSet<RevenueReportWhDb> RevenueReportsWh { get; set; }

        public DbSet<RevenueReportFinDb> RevenueReportsFin { get; set; }

        public DbSet<MainReportNumbersDb> MainReportNumbers { get; set; }

    }


    [Table("site_users")]
    public class SiteUserDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("email_as_login")]
        public string EmailAsLogin { get; set; }

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Required]
        [Column("salt")]
        public string Salt { get; set; } = string.Empty;

        [Column("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [Column("deleted")]
        public bool Deleted { get; set; }

        [Column("role_code")]
        public string RoleCode { get; set; }

    }

    [Table("employees")]
    public class EmployeesDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("fio")]
        public string Fio { get; set; }

        [Column("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [Column("bank_name")]
        public string? BankName { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("chop_certificate")]
        public bool ChopCertificate { get; set; }

        [Column("object_id")]
        public int ObjectId { get; set; }


        //варианты трудоустройства
        [Column("empl_options")]
        public string? EmplOptions { get; set; }

        //
        public virtual ICollection<FinOperationDb> FinOperations { get; set; }

        public virtual ICollection<WorkShiftsDb> WorkShifts { get; set; }

        public virtual ObjectDb Object { get; set; }


        [Column("dismissed")]
        public bool Dismissed { get; set; }
    }


    [Table("work_shifts")]
    public class WorkShiftsDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [Column("start")]
        public DateTime Start { get; set; }

        [Column("end")]
        public DateTime End { get; set; }

        public virtual EmployeesDb Employee { get; set; }
    }


    [Table("work_hours")]
    public class WorkHoursDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [Column("hours")]
        public int Hours { get; set; }

        [Column("rate")]
        public int Rate { get; set; }

        [Column("work_date")]
        public DateTime WorkDate { get; set; }
    }

    //рабочие объекты
    [Table("objects")]
    public class ObjectDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("object_name")]
        public string Name { get; set; }

        [Column("address")]
        public string? Address { get; set; }
    }

    //связь пользователя с объектами
    [Table("users_to_objects")]
    public class UserToObjectDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int SiteUserId { get; set; }

        [Column("object_id")]
        public int ObjectId { get; set; }
    }


    [Table("fin_operations")]
    public class FinOperationDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("sum")]
        public int Sum { get; set; }

        [Column("is_penalty")]
        public bool IsPenalty { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }


        [Column("type_id")]
        public int? TypeId { get; set; }

        public virtual EmployeesDb Employee { get; set; }

        public virtual FinOperationTypeDb FinOperationType { get; set; }
    }

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
    }

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
    }

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
