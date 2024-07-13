using System.Collections.Generic;

namespace BackendServices.Models.RealTime
{
    public class DashboardUIData
    {
        public List<StaffOverview> StaffOnShift { get; set; }
        public List<StaffOverview> StaffOnBreak { get; set; }
        public string StaffOnShiftPercentage { get; set; }
        public string StaffOnBreakPercentage { get; set; }
        public List<EmployeeDataAndTimes> EmployeeDataAndTimes { get; set; }
        public int StaffWorked { get; set; }
        public decimal AverageCost { get; set; }
    }
}
