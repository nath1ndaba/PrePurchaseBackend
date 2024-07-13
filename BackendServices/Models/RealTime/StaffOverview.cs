using System.Collections.Generic;

namespace BackendServices.Models.RealTime
{
    public class StaffOverview
    {
        public string DepartmentName { get; set; }
        public int TotalStaffOn { get; set; }
        public List<string> StaffNamesOn { get; set; }
    }
}