using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementApp.Data;
using EmployeeManagementApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace EmployeeManagementApp.Controllers
{
    public class WorkHistoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
    
        public WorkHistoriesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: WorkHistories
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.WorkHistories.Include(w => w.Employee);
            return View(await applicationDbContext.ToListAsync());

           /* List<WorkHistory> workHistories = new List<WorkHistory>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection sqlConnect = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM WorkHistory";

                using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnect))
                {
                    sqlConnect.Open();

                    using (SqlDataReader reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            WorkHistory workhistory = new WorkHistory
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                Employer = reader["Employer"].ToString(),
                                StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                                EndDate = DateTime.Parse(reader["EndDate"].ToString()),
                                Position = reader["Position"].ToString()
                            };

                            workHistories.Add(workhistory);
                        }
                    }
                }
            }*/

           // return View(workHistories);
        }

        // GET: WorkHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workHistory = await _context.WorkHistories
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(m => m.WorkHistoryID == id);
            if (workHistory == null)
            {
                return NotFound();
            }

            return View(workHistory);
        }

        // GET: WorkHistories/Create
        public IActionResult Create()
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "EmployeeID");
            return View();
        }

        // POST: WorkHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WorkHistoryID,Employer,StartDate,EndDate,Position,EmployeeID")] WorkHistory workHistory)
        {
           
                string sqlQuery = @"
            INSERT INTO WorkHistories (Employer, StartDate, EndDate, Position, EmployeeID)
            VALUES (@Employer, @StartDate, @EndDate, @Position, @EmployeeID);
            SELECT SCOPE_IDENTITY();"; // Retrieve the ID of the inserted row

                // Get connection string from configuration
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnection))
                    {
                        // Add parameters to the SQL query
                        sqlCommand.Parameters.AddWithValue("@Employer", workHistory.Employer);
                        sqlCommand.Parameters.AddWithValue("@StartDate", workHistory.StartDate);
                        sqlCommand.Parameters.AddWithValue("@EndDate", workHistory.EndDate);
                        sqlCommand.Parameters.AddWithValue("@Position", workHistory.Position);
                        sqlCommand.Parameters.AddWithValue("@EmployeeID", workHistory.EmployeeID);

                        await sqlConnection.OpenAsync();
                        sqlCommand.ExecuteScalarAsync();

                    }
                }
            return View(workHistory);
        }



        // GET: WorkHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workHistory = await _context.WorkHistories.FindAsync(id);
            if (workHistory == null)
            {
                return NotFound();
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "EmployeeID", workHistory.EmployeeID);
            return View(workHistory);
        }

        // POST: WorkHistories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkHistoryID,Employer,StartDate,EndDate,Position,EmployeeID")] WorkHistory workHistory)
        {
            if (id != workHistory.WorkHistoryID)
            {
                return NotFound();
            }

            try
            {
                _context.Update(workHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException) when (!WorkHistoryExists(workHistory.WorkHistoryID))
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (Exception)
            {
                return View(workHistory);
            }
        }

        // GET: WorkHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workHistory = await _context.WorkHistories
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(m => m.WorkHistoryID == id);
            if (workHistory == null)
            {
                return NotFound();
            }

            return View(workHistory);
        }

        // POST: WorkHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workHistory = await _context.WorkHistories.FindAsync(id);
            if (workHistory != null)
            {
                _context.WorkHistories.Remove(workHistory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkHistoryExists(int id)
        {
            return _context.WorkHistories.Any(e => e.WorkHistoryID == id);
        }
    }
}
