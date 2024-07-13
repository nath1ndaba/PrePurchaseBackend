using System;

namespace BackendServices
{
    public interface ITimeZoneProvider
    {
        public DateTime ConvertTime(DateTime dateTime, TimeZoneInfo timeZoneInfo);

        public DateTime Now(TimeZoneInfo timeZoneInfo = null);
    }
}
