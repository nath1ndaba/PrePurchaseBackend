using System;

namespace BackendServices
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
        int TimeTolerance { get; }

        public string DayOfWeekString(DayOfWeek dayOfWeek)
            => dayOfWeek switch
            {
                DayOfWeek.Monday => nameof(DayOfWeek.Monday),
                DayOfWeek.Tuesday => nameof(DayOfWeek.Tuesday),
                DayOfWeek.Wednesday => nameof(DayOfWeek.Wednesday),
                DayOfWeek.Thursday => nameof(DayOfWeek.Thursday),
                DayOfWeek.Friday => nameof(DayOfWeek.Friday),
                DayOfWeek.Saturday => nameof(DayOfWeek.Saturday),
                DayOfWeek.Sunday => nameof(DayOfWeek.Sunday),
                _ => default
            };

        public string DayOfWeekString(DateTime dateTime)
            => DayOfWeekString(dateTime.DayOfWeek);

        public static bool TryParseTimeZoneId(string zoneId, out TimeZoneInfo timeZoneInfo)
        {
            try
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
                return true;
            }
            catch
            {
                timeZoneInfo = default;
                return false;
            }
        }

        public bool IsValidTimeZoneId(string zoneId)
            => TryParseTimeZoneId(zoneId, out var _);

        public bool IsShiftWithinTolerance(DateTime clockInOutTime, TimeOnly shirtStartTime)
            => IsShiftWithinTolerance(TimeOnly.FromDateTime(clockInOutTime), shirtStartTime);

        public bool IsShiftWithinTolerance(TimeOnly clockInOutTime, TimeOnly shirtStartTime)
        {
            return clockInOutTime.IsBetween(shirtStartTime.AddMinutes(-TimeTolerance), shirtStartTime);
        }

        public bool IsWithinTolerance(DateTime dateTime, DateTime expectedTime)
        {
            var isBeforeWithTolerance = IsLessThanWithTolerance(dateTime, expectedTime);
            var isAfterWithTolerance = IsGreaterThanWithTolerance(dateTime, expectedTime);

            return isBeforeWithTolerance || isAfterWithTolerance;

        }

        public bool IsLessThanWithTolerance(DateTime dateTime, DateTime expectedTime)
        {

            var timeBeforeWithTolerance = dateTime.AddMinutes(-TimeTolerance);

            return timeBeforeWithTolerance <= expectedTime;

        }

        public bool IsGreaterThanWithTolerance(DateTime dateTime, DateTime expectedTime)
        {
            var timeAfterWithTolerance = dateTime.AddMinutes(TimeTolerance);

            return timeAfterWithTolerance >= expectedTime;

        }

        TimeOnly GetTime(DateTime dateTime)
        {
            return TimeOnly.FromDateTime(dateTime);
        }

        TimeOnly GetTime(TimeSpan timeSpan)
        {
            return TimeOnly.FromTimeSpan(timeSpan);
        }
    }


}
