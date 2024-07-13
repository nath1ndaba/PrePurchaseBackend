using System;
using System.Collections.Generic;

namespace PrePurchase.Models
{

    public class AdminManualClockings
    {
        public string StaffMemberId { get; set; }
        public List<clockingdate> ClockingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAdminClocking { get; set; }

    }
    public class clockingdate
    {
        public DateTime ClockingDate { get; set; }
    }
}

