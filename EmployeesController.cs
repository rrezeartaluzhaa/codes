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
using System.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MvcPagedList.Core;
using X.PagedList;
using Microsoft.Extensions.Configuration;
using X.PagedList.Extensions;
using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Composition;
using System.Data;



namespace EmployeeManagementApp.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ILogger<EmployeesController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public EmployeesController(IWebHostEnvironment env, ILogger<EmployeesController> logger, IConfiguration config, ApplicationDbContext context)
        {
            _env = env;
            _logger = logger;
            _configuration = config;
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string searchQuery, int page = 1, int pageSize = 10)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<Employee> employees = new List<Employee>();

            using (SqlConnection sqlConnect = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM Employees WHERE (@SearchQuery = '' OR FirstName LIKE '%' + @SearchQuery + '%' OR LastName LIKE '%' + @SearchQuery + '%')";

                using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnect))
                {
                    sqlCommand.Parameters.AddWithValue("@SearchQuery", string.IsNullOrEmpty(searchQuery) ? "" : searchQuery);

                    sqlConnect.Open();

                    using (SqlDataReader reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Employee employee = new Employee
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                EMBG = reader["EMBG"].ToString(),
                                Address = reader["Address"].ToString()
                            };

                            employees.Add(employee);
                        }
                    }
                }
            }

            var sortedEmployees = employees.OrderBy(e => e.FirstName);
            var pagedEmployees = sortedEmployees.ToPagedList(page, pageSize);
            ViewBag.SearchQuery = searchQuery;
            return View(pagedEmployees);
        }

        // GET: Employees/Reports
        public async Task<IActionResult> Reports()
        {
            // Fetch all employees for the dropdown
            var employees = await _context.Employees.ToListAsync();

            var reportViewModel = new ReportSelectionViewModel
            {
                Employees = employees,
            };

            return View(reportViewModel);
        }

        // POST: Employees/GenerateReport
        [HttpPost]
        public async Task<IActionResult> GenerateReport(int employeeId)
        {
            try
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeID == employeeId);

                if (employee == null)
                {
                    return NotFound();
                }

                var report = new Report();
                var reportPath = _configuration.GetConnectionString("ReportPath");
                report.Load(reportPath);

                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("GetEmployeeData", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@EmployeeID", employeeId));

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            dataAdapter.Fill(ds);

                            if (ds.Tables.Count > 0)
                            {
                                report.RegisterData(ds.Tables[0], "Employee");

                                if (ds.Tables.Count > 1)
                                {
                                    report.RegisterData(ds.Tables[1], "WorkHistories");
                                }
                            }
                        }

                        DataBand db1 = (DataBand)report.FindObject("Data1");
                        if (db1 != null)
                        {
                            db1.DataSource = report.GetDataSource("Employee");
                            report.GetDataSource("Employee").Enabled = true;
                        }

                        DataBand db2 = (DataBand)report.FindObject("Data2");
                        if (db2 != null)
                        {
                            db2.DataSource = report.GetDataSource("WorkHistories");
                            report.GetDataSource("WorkHistories").Enabled = true;
                        }

                        report.Prepare();

                        using (var ms = new MemoryStream())
                        {
                            var pdfExport = new PDFSimpleExport();
                            report.Export(pdfExport, ms);
                            return File(ms.ToArray(), "application/pdf", "EmployeeReport.pdf");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating report: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        [HttpGet]
        public IActionResult AddWorkHistory(int id, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 6)
        {
            ViewBag.EmployeeID = id;
            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeID == id);

            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.Employee = employee;
            var query = _context.WorkHistories.Where(w => w.EmployeeID == id);

            if (startDate.HasValue)
            {
                query = query.Where(w => w.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(w => w.EndDate <= endDate.Value);
            }

            var workHistories = query
                                .OrderByDescending(w => w.StartDate)
                                .ToPagedList(page, pageSize);

            ViewBag.WorkHistories = workHistories;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(new WorkHistory());
        }

        [HttpPost]
        public async Task<IActionResult> AddWorkHistory(WorkHistory history)
        {
            _context.Add(history);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.WorkHistories) // Eager loading WorkHistories
                .FirstOrDefaultAsync(m => m.EmployeeID == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,EMBG,Address")] Employee employee)
        {
            try
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return View(employee);
            }
        }



        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.WorkHistories)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }


        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeID,FirstName,LastName,EMBG,Address")] Employee employee)
        {
            if (id != employee.EmployeeID)
            {
                return NotFound();
            }

            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.EmployeeID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }



        // GET: Employees/EditWorkHistory/{id}
        [HttpGet]
        public async Task<IActionResult> EditWorkHistory(int id)
        {
            var history = await _context.WorkHistories.FindAsync(id);

            if (history == null)
            {
                return NotFound();
            }

            return View(history);
        }

        // POST: Employees/EditWorkHistory/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWorkHistory(int id, WorkHistory history)
        {
            if (id != history.WorkHistoryID)
            {
                return NotFound();
            }

            try
            {
                _context.Update(history);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkHistoryExists(history.WorkHistoryID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(AddWorkHistory), new { id = history.EmployeeID });
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.EmployeeID == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }
        private bool WorkHistoryExists(int id)
        {
            return _context.WorkHistories.Any(e => e.WorkHistoryID == id);
        }
    }

}