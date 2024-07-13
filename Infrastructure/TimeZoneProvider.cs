using BackendServices;
using System;

namespace Infrastructure
{
    public class TimeZoneProvider : ITimeZoneProvider
    {
        private readonly IDateTimeProvider dateTimeProvider;

        public TimeZoneProvider(IDateTimeProvider dateTimeProvider)
        {
            this.dateTimeProvider = dateTimeProvider;
        }

        public DateTime ConvertTime(DateTime dateTime, TimeZoneInfo timeZoneInfo)
        {
            return TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
        }

        public DateTime Now(TimeZoneInfo timeZoneInfo = null)
        {
            var now = dateTimeProvider.Now;
            return timeZoneInfo is null ? now : ConvertTime(now, timeZoneInfo);
        }
    }
}
