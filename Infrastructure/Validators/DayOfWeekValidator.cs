using BackendServices;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Validators;
using BackendServices.Validators.ValidationData;
using Infrastructure.Helpers;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Validators
{
    internal class DayOfWeekValidator : IValidator<DayOfWeekValidationData, DayOfWeekValidationResult>
    {
        private readonly IValidator<CompanyEmployeeValidationData, CompanyEmployeeValidationResult> requiredValidator;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ITimeZoneProvider timeZoneProvider;

        public DayOfWeekValidator(
            IValidator<CompanyEmployeeValidationData, CompanyEmployeeValidationResult> requiredValidator,
            IDateTimeProvider dateTimeProvider,
            ITimeZoneProvider timeZoneProvider)
        {
            this.requiredValidator = requiredValidator;
            this.dateTimeProvider = dateTimeProvider;
            this.timeZoneProvider = timeZoneProvider;
        }

        public async ValueTask<DayOfWeekValidationResult> Validate(DayOfWeekValidationData value)
        {
            CompanyEmployeeValidationData companyEmployeeValidationData = new(value);
            var companyEmployeeValidationResult = await requiredValidator.Validate(companyEmployeeValidationData);

            IValidationResult previousValidationResult = companyEmployeeValidationResult;
            var qrcodeValidationResult = previousValidationResult.GetValidationResultOfType<QrCodeValidationResult>();
            var timeZone = qrcodeValidationResult.TimeZoneInfo;
            var company = qrcodeValidationResult.Company;
            var employee = companyEmployeeValidationResult.CompanyEmployee;

            var now = dateTimeProvider.Now;

            // we need to make sure that we are getting the day of the week for the person logging in
            var adjustedWithZone = timeZoneProvider.ConvertTime(now, timeZone);

            // day of week
            var tolerance = dateTimeProvider.TimeTolerance;
            // the shift is on the same day
            string dayOfWeek = dateTimeProvider.DayOfWeekString(adjustedWithZone);
            // the shift is on the next day and early clockin
            string possibleNexDayOfWeek = dateTimeProvider.DayOfWeekString(adjustedWithZone.AddMinutes(tolerance));
            // the shift is on the prevoius day and late clockin
            string possiblePreviouseDayOfWeek = dateTimeProvider.DayOfWeekString(adjustedWithZone.AddDays(-1));

            HashSet<string> weekDaysToConsider = new() { dayOfWeek, possibleNexDayOfWeek, possiblePreviouseDayOfWeek };
            var companyShiftsMap = company.Shifts.MapUnique(x => x.Id);
            var companyRatesMap = company.Rates.MapUnique(x => x.Id);


            System.TimeOnly time = dateTimeProvider.GetTime(adjustedWithZone);
            EmployeeTask? task = default;
            foreach (var weekDay in weekDaysToConsider)
            {
                bool keyFound = employee.WeekDays.ContainsKey(weekDay);
                if (!keyFound) continue;

                IEnumerable<EmployeeTask> tasks = employee.WeekDays[weekDay].Where(x => companyShiftsMap.ContainsKey(x.Shift));
                // order the tasks decending, so we can get the last shift
                // if an employee is on shift, but the shift started the day before
                tasks = weekDay == possiblePreviouseDayOfWeek ? tasks.OrderByDescending(x => companyShiftsMap[x.Shift].ShiftStartTime) : tasks;
                task = tasks.FirstOrDefault(x => time.IsBetween(companyShiftsMap[x.Shift]!.ShiftStartTime.AddMinutes(-tolerance), companyShiftsMap[x.Shift]!.ShiftEndTime));

                if (task is not null) break;
            }

            if (task is null)
            {
                int conv = tolerance / 60;
                throw new HttpResponseException(new Response(error: $"Are you on shift? Check with your HR to add you on the roaster.\nYou may only start clocking in {conv} minutes before your shift."));
            }

            bool isHoliday = HolidaysHelper.IsSAHoliday();
            if (isHoliday) task.RateType = RateType.PublicHoliday;


            ClockData clockData = new() { ClockIn = now, Rate = companyRatesMap[task.Rate], Shift = companyShiftsMap[task.Shift], RateType = task.RateType, IsAdminClocking = false, IsProcessed = false, IsClockOutAdjusted = false, IsClockInAdjusted = false };
            return new DayOfWeekValidationResult(previousValidationResult, value, clockData);
        }

    }
}
