using X.PagedList;

namespace EmployeeManagementApp.Models
{
    public class ReportViewModel
    {
        public Employee Employee { get; set; }
        public IPagedList<WorkHistory> WorkHistories { get; set; }
    }
}
