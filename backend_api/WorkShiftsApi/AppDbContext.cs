using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkShiftsApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SiteUserDb> SiteUsers { get; set; }

        public DbSet<EmployeesDb> Employees { get; set; }

        public DbSet<WorkShiftsDb> WorkShifts { get; set; }

        public DbSet<WorkHoursDb> WorkHours { get; set; }
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

        [Column("role")]
        public string Role { get; set; }

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

        [Column("age")]
        public int? Age { get; set; }

        [Column("chop_certificate")]
        public bool ChopCertificate { get; set; }

        [Column("object")]
        public string? Object { get; set; }


        //варианты трудоустройства
        [Column("empl_options")]
        public string? EmplOptions { get; set; }

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

        [Column("date")]
        public DateTime Date { get; set; }
    }
}
