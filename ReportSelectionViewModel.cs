namespace EmployeeManagementApp.Models
{
    public class ReportSelectionViewModel
    {
        public List<Employee> Employees { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
