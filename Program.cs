using EmployeeManagementApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Configuration;

namespace EmployeeManagementApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure database context with connection string
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer("Server=ELJESA3-PC\\SQL2022;Database=EmployeeManagementDB;Trusted_Connection=True;TrustServerCertificate=true;MultipleActiveResultSets=true"));

            builder.Logging.AddConsole();

            // Add services to the container
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Optionally verify database connection or other startup tasks
            VerifyDatabaseConnection(app);

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(); // HTTP Strict Transport Security
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Configure endpoints
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Start the application
            app.Run();
        }


        private static void VerifyDatabaseConnection(IApplicationBuilder app)
        {
            // Get an instance of ApplicationDbContext from the DI container
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                try
                {
                    dbContext.Database.EnsureCreated(); // EnsureCreated will attempt to create the database if it does not exist

                    // Log success message
                    Console.WriteLine("Successfully connected to the database.");
                }
                catch (Exception ex)
                {
                    // Log error message
                    Console.WriteLine($"Failed to connect to the database: {ex.Message}");
                }
            }
        }
    }
}