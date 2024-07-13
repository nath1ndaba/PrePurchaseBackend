using BackendServices;
using BackendServices.Models;
using MongoDB.Bson;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GeographicLib;
using PrePurchase.Models.Payments;

[assembly: InternalsVisibleTo("Infrastructure.Unit.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace Infrastructure.Helpers
{
    internal static class ModelHelpers
    {

        public static Company Update(this Company company, CompanyUpdateModel updates)
        {
            company.Address = updates.Address ?? company.Address;
            company.CompanyName = updates.CompanyName ?? company.CompanyName;
            company.RegisterationNumber = updates.RegisterationNumber ?? company.RegisterationNumber;

            return company;
        }

        public static CompanyEmployee CreateCompanyEmployeeFrom(this EmployeeDetails employeeDetails, string department, string position, ObjectId companyId)
        {
            return new()
            {
                CreatedDate = employeeDetails.CreatedDate,
                CreatedBy = employeeDetails.CreatedBy,
                UpdatedDate = employeeDetails.UpdatedDate,
                UpdatedBy = employeeDetails.UpdatedBy,
                DeletedIndicator = employeeDetails.DeletedIndicator,
                Name = employeeDetails.Name,
                Surname = employeeDetails.Surname,
                NickName = employeeDetails.NickName,
                CellNumber = employeeDetails.CellNumber,
                IdNumber = employeeDetails.IDNumber,
                Email = employeeDetails.Email,
                TaxNumber = employeeDetails.TaxNumber,
                DateOfEmployment = employeeDetails.DateOfEmployment,
                EmployeeAddress = employeeDetails.EmployeeAddress,
                BankAccountInfo = employeeDetails.BankAccountInfo,
                EmployeeId = employeeDetails.EmployeeId,
                Department = department,
                CompanyId = companyId,
                Position = position
            };
        }

        public static Deduction ToDeduction(this DeductionModel model)
        {
            model = model with { TypeOfDeduction = model.TypeOfDeduction.Trim() };
            var (TypeOfDeduction, AmountToDeduct, AmountType) = model;

            return new Deduction
            {
                TypeOfDeduction = TypeOfDeduction,
                AmountToDeduct = AmountToDeduct,
                AmountType = AmountType
            };
        }

        public static Rate ToRate(this RateModel model)
        {
            model = model with { NameOfPosition = model.NameOfPosition.Trim() };

            var (NameOfPosition, StandardDaysRate, SaturdaysRate, SundaysRate, PublicHolidaysRate, DailyBonus, OverTimeRate) = model;

            return new Rate
            {
                NameOfPosition = NameOfPosition,
                StandardDaysRate = StandardDaysRate,
                SaturdaysRate = SaturdaysRate,
                SundaysRate = SundaysRate,
                PublicHolidaysRate = PublicHolidaysRate,
                DailyBonus = DailyBonus,
                OverTimeRate = OverTimeRate
            };
        }

        public static Shift ToShift(this ShiftModel model)
        {
            model = model with { Name = model.Name.Trim() };

            (string Name, TimeOnly ShiftStartTime, TimeOnly ShiftEndTime) = model;

            return new Shift
            {
                Name = Name,
                ShiftStartTime = ShiftStartTime,
                ShiftEndTime = ShiftEndTime
            };
        }

        public static Loan Update(this Loan loan, CompanyUpdateLoan model, IDateTimeProvider dateTimeProvider, string updatedby)
        {
            loan.LoanStatus = model.Status;
            loan.UpdatedBy = ObjectId.Parse(updatedby);
            if (model.Payment.HasValue)
            {
                loan.AmountPayed += model.Payment.Value;
                loan.LastPayment = model.Payment.Value;
                loan.LastPaymentDate = dateTimeProvider.Now;
            }

            return loan;
        }

        public static Leave Update(this Leave leave, QueryLeaveModel model)
        {
            leave.Status = (LeaveStatus)model.Status!;

            leave.Comment = model.Comment ?? leave.Comment;
            leave.DaysToTake = model.DaysToTake ?? leave.DaysToTake;

            return leave;
        }

        public static CompanyEmployeeProfile From(CompanyEmployee companyEmployee, IEnumerable<Company> companies)
        {
            return new()
            {
                Id = companyEmployee.Id.ToString(),
                Name = companyEmployee.Name,
                Surname = companyEmployee.Surname,
                NickName = companyEmployee.NickName,
                CellNumber = companyEmployee.CellNumber,
                EmployeeId = companyEmployee.EmployeeId,
                Department = companyEmployee.Department,
                Company = companies.First().ToCompanyProfile(),
                Position = companyEmployee.Position,
                WeekDays = companyEmployee.WeekDays,
                Roles = companyEmployee.Roles,
                Timestamp = companyEmployee.Timestamp
            };
        }

        public static Rate ToDataTransferObject(this Rate rate)
        {
            return new()
            {
                Id = rate.Id,
                NameOfPosition = rate.NameOfPosition,
                OverTimeRate = rate.OverTimeRate,
                PublicHolidaysRate = rate.PublicHolidaysRate,
                StandardDaysRate = rate.StandardDaysRate,
                SundaysRate = rate.SundaysRate,
                SaturdaysRate = rate.SaturdaysRate,
                DailyBonus = rate.DailyBonus

            };
        }

        public static Shift ToDataTransferObject(this Shift shift)
        {
            return new()
            {
                Id = shift.Id,
                ShiftStartTime = shift.ShiftStartTime,
                ShiftEndTime = shift.ShiftEndTime,
                Name = shift.Name
            };
        }

        public static ClockDataDto ToDataTransferObject(this ClockData clock)
        {
            return new()
            {
                Id = clock.Id,
                Amount = clock.Amount,
                ClockIn = clock.ClockIn,
                ClockOut = clock.ClockOut,
                OverTimeHours = clock.OverTimeHours,
                Rate = clock.Rate.ToDataTransferObject(),
                Shift = clock.Shift.ToDataTransferObject(),
                RateType = clock.RateType,
                TotalHours = clock.TotalHours,

                IsProcessed = clock.IsProcessed,
                IsAdminClocking = clock.IsAdminClocking,

                IsClockInAdjusted = clock.IsClockInAdjusted,
                IsClockOutAdjusted = clock.IsClockOutAdjusted,

                IsSickLeaveDays = clock.IsSickLeaveDays,
                IsAnnualLeaveDays = clock.IsAnnualLeaveDays,
                IsFamilyLeaveDays = clock.IsFamilyLeaveDays,

                OldClockInValue = clock.OldClockInValue,
                OldClockOutValue = clock.OldClockOutValue,

                RestTimes = clock.RestTimes,

            };
        }

        public static CompanyEmployeeDto ToDataTransferObject(this CompanyEmployee employee)
        {

            return new()
            {
                Id = employee.Id,
                Name = employee.Name,
                Surname = employee.Surname,
                NickName = employee.NickName,
                EmployeeId = employee.EmployeeId,
                Department = employee.Department,
                CompanyId = employee.CompanyId.ToString(),
                Position = employee.Position,
                Timestamp = employee.Timestamp
            };
        }

        public static TimeSummaryWithEmployeeDetails From(TimeSummary timeSummary, CompanyEmployee employees)
        {

            return new()
            {
                Id = timeSummary.Id,
                CompanyEmployee = employees.ToDataTransferObject(),
                Clocks = timeSummary.Clocks,//timeSummary.Clocks.Select(x => x.ToDataTransferObject()).ToList(),
                CompanyId = timeSummary.CompanyId,
                EndDate = timeSummary.EndDate,
                StartDate = timeSummary.StartDate
            };
        }

        public static IEnumerable<CompanyProfile> ToCompanyProfiles(this IEnumerable<Company> companies)
        {
            return companies.Select(x => x.ToCompanyProfile());
        }

        public static CompanyProfile ToCompanyProfile(this Company company)
        {
            return new()
            {
                Id = company.Id.ToString(),
                Address = company.Address,
                CompanyName = company.CompanyName,
                CellNumber = company.CellNumber,
                Email = company.Email,
                RegisterationNumber = company.RegisterationNumber
            };
        }

        public static double Distance(this Location position, Location position1)
        {
            Geodesic.WGS84.Inverse(position.Latitude, position.Longitude, position1.Latitude, position1.Longitude, out var distance);
            return distance;
        }

        public static bool WithInRadius(Location position1, Location position2, double radiusInMeters)
        {
            double distance = position1.Distance(position2);
            return distance <= radiusInMeters;
        }

        public static bool WithInRadius(Location position1, Location position2, double radiusInMeters, out double distance)
        {
            distance = position1.Distance(position2);
            return distance <= radiusInMeters;
        }

        public static List<ClockData> GetClocksInRange(this TimeSummary timeSummary, DateTime start, DateTime end)
        {
            List<ClockData> clocksInRange = timeSummary.Clocks
                .Where(clock => clock != null && clock.ClockIn >= start && clock.ClockIn <= end)
                .OrderBy(clock => clock.ClockIn)
                .ToList();

            List<ClockData> mergedClocks = new();

            if (!clocksInRange.Any()) return mergedClocks;

            ClockData currentClock = clocksInRange[0];
            foreach (var clock in clocksInRange.Skip(1))
            {
                if (clock.ClockIn <= currentClock.ClockOut)
                {
                    currentClock.ClockOut = clock.ClockOut > currentClock.ClockOut ? clock.ClockOut : currentClock.ClockOut;
                }
                else
                {
                    mergedClocks.Add(currentClock);
                    currentClock = clock;
                }
            }
            mergedClocks.Add(currentClock);

            return mergedClocks;
        }

        public static TimeSummary WithClocksInRange(this TimeSummary timeSummary, DateTime start, DateTime end)
        {
            return new()
            {
                Id = timeSummary.Id,
                EmployeeDetailsId = timeSummary.EmployeeDetailsId,
                EmployeeId = timeSummary.EmployeeId,
                CompanyId = timeSummary.CompanyId,
                StartDate = start,
                EndDate = end,
                Clocks = timeSummary.GetClocksInRange(start, end)
            };
        }

        public static Dictionary<TKey, TValue> MapUnique<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector) where TKey : notnull
        {

            Dictionary<TKey, TValue> result;

            if (values.TryGetNonEnumeratedCount(out var count))
            {
                result = new(count);
            }
            else
            {
                result = new();
            }

            foreach (var value in values)
            {
                var key = keySelector(value);

                if (result.ContainsKey(key))
                    continue;

                result[key] = value;
            }

            return result;
        }

        public static Dictionary<TKey, TOut> MapUnique<TKey, TValue, TOut>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector, Func<TKey, TValue, TOut> tranform) where TKey : notnull
        {

            Dictionary<TKey, TOut> result;

            if (values.TryGetNonEnumeratedCount(out var count))
            {
                result = new(count);
            }
            else
            {
                result = new();
            }

            foreach (var value in values)
            {
                var key = keySelector(value);

                if (result.ContainsKey(key))
                    continue;

                result[key] = tranform(key, value);
            }

            return result;
        }
    }

}
