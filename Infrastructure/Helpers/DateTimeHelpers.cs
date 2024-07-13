using BackendServices;
using System;

namespace Infrastructure.Helpers
{
#nullable enable
    public static class DateTimeHelpers
    {
        public static bool IsWithinTolerance(this DateTime dateTime, DateTime expectedTime)
        {
            var isBeforeWithTolerance = dateTime.IsLessThanWithTolerance(expectedTime);
            var isAfterWithTolerance = dateTime.IsGreaterThanWithTolerance(expectedTime);

            return isBeforeWithTolerance || isAfterWithTolerance;

        }

        public static bool IsLessThanWithTolerance(this DateTime dateTime, DateTime expectedTime)
        {
            var allowedTolerance = PrePurchaseConfig.CLOCK_IN_BEFORE_ALLOWED_TOLERANCE_MINUTES;

            var timeBeforeWithTolerance = dateTime.AddMinutes(-allowedTolerance);

            return timeBeforeWithTolerance <= expectedTime;

        }

        public static bool IsGreaterThanWithTolerance(this DateTime dateTime, DateTime expectedTime)
        {
            var allowedTolerance = PrePurchaseConfig.CLOCK_IN_BEFORE_ALLOWED_TOLERANCE_MINUTES;

            var timeAfterWithTolerance = dateTime.AddMinutes(allowedTolerance);

            return timeAfterWithTolerance >= expectedTime;

        }

    }
#nullable disable

}
