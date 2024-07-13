using BackendServices;
using System;

namespace Infrastructure
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.UtcNow;
        public int TimeTolerance => PrePurchaseConfig.CLOCK_IN_BEFORE_ALLOWED_TOLERANCE_MINUTES;
    }
}
