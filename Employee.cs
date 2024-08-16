using EmployeeManagementApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EMBG { get; set; }
        public string Address { get; set; }

        // Navigation property for related work histories
        public virtual ICollection<WorkHistory> WorkHistories { get; set; }
    }
}
