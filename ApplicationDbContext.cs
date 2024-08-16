using Microsoft.EntityFrameworkCore;
using EmployeeManagementApp.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EmployeeManagementApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public static string ConnectionString { get; set; } = "";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=ELJESA3-PC\\SQL2022;Database=EmployeeManagementDB;Trusted_Connection=True;TrustServerCertificate=true;MultipleActiveResultSets=true");
            }
            base.OnConfiguring(optionsBuilder);
        }
            
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkHistory> WorkHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the relationship between Employee and WorkHistory
            modelBuilder.Entity<WorkHistory>()
                .HasOne(e => e.Employee)
                .WithMany(e => e.WorkHistories)
                .HasForeignKey(e => e.EmployeeID)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete for WorkHistory if Employee is deleted

            base.OnModelCreating(modelBuilder);
        }

       
    }
}
