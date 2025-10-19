using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Xml.Serialization;

namespace WorkShiftsApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SiteUserDb> SiteUsers { get; set; }
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
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column("salt")]
        public string Salt { get; set; } = string.Empty;


        [Column("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [Column("deleted")]
        public bool Deleted { get; set; } = true;


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
    }

}
