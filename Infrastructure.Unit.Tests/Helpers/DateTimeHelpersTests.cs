using Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using BackendServices;
using System.Globalization;

namespace Infrastructure.Helpers.Unit.Tests
{
    internal class MockDateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => new(2021,08,29, 3, 5, 0, 0);

        public int TimeTolerance { get; }

        public MockDateTimeProvider(int tollerance = 10)
        {
            TimeTolerance = tollerance;
        }
    }

    internal struct DateTimeWithZone
    {
        private readonly DateTime utcDateTime;
        private readonly TimeZoneInfo timeZone;

        public DateTimeWithZone(DateTime dateTime, TimeZoneInfo timeZone)
        {
            var dateTimeUnspec = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeUnspec, timeZone);
            this.timeZone = timeZone;
        }

        public DateTime UniversalTime => utcDateTime;

        public TimeZoneInfo TimeZone => timeZone;

        public DateTime LocalTime => TimeZoneInfo.ConvertTime(utcDateTime, timeZone);
    }

    public class DateTimeHelpersTests
    {
        private readonly ITestOutputHelper Output;
        private readonly IDateTimeProvider dateTime;
        public DateTimeHelpersTests(ITestOutputHelper outputHelper)
        {
            Output = outputHelper;
            dateTime = new MockDateTimeProvider();
        }


        [Fact]
        public void With_in_tolerance_no_zone_should_pass_Test()
        {
            StellaConfig.SetEnv(nameof(StellaConfig.CLOCK_IN_BEFORE_ALLOWED_TOLERANCE_MINUTES), "5m", EnvironmentVariableTarget.Process);
            var now = dateTime.Now;
            TimeSpan time = new(3, 0, 0);
            var adjusted = now.Add(-time);

            Assert.True(now.IsWithinTolerance(adjusted));
        }

        [Theory]
        [InlineData("South Africa Standard Time", 2021, 08, 29, 3, 5, 0, 3, 0, 0)]
        [InlineData("South Africa Standard Time", 2021, 08, 29, 2, 55, 0, 3, 0, 0)]
        [InlineData("South Africa Standard Time", 2021, 08, 28, 11, 55, 0, 0, 0, 0)]
        [InlineData("South Africa Standard Time", 2021, 08, 29, 00, 0, 1, 0, 0, 0)]
        public void With_in_tolerance_with_zone_should_pass_Test(string zone, int year, int month, int day, int hours, int minutes, int seconds, int thours, int tminutes, int tseconds)
        {
            StellaConfig.SetEnv(nameof(StellaConfig.CLOCK_IN_BEFORE_ALLOWED_TOLERANCE_MINUTES), "5m", EnvironmentVariableTarget.Process);

            var now = dateTime.Now;
            TimeZoneInfo timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zone);
            
            TimeSpan timeSpan = new(thours, tseconds, tminutes);
            DateTimeWithZone dateTimeWithZone = new(new(year, month, day, hours, minutes, seconds, 0), timezoneInfo);

            Output.WriteLine($"{dateTimeWithZone.LocalTime.Add(timeSpan)}");
            Assert.True(now.IsWithinTolerance(dateTimeWithZone.LocalTime.Add(timeSpan)));
        }
    }
}