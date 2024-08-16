using System;

namespace EmployeeManagementApp.Models
{
    public class WorkHistory
    {
        public int WorkHistoryID { get; set; }
        public string Employer { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Position { get; set; }

        // Foreign key for Employee
        public int EmployeeID { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
