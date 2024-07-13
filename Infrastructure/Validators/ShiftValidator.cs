using BackendServices;
using BackendServices.Models;
using BackendServices.Validators;
using BackendServices.Validators.ValidationData;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Validators
{
    // the validation is propagating downward from
    // from ShiftValidator to all required validators
    // all the validation results are propagated back up to the shift validator
    internal class ShiftValidator : IValidator<ShiftValidationData, ShiftValidationResult>
    {
        private readonly IValidator<DayOfWeekValidationData, DayOfWeekValidationResult> _requiredValidator;
        private readonly ITimeSummaryRepository _timeSummaries;

        public ShiftValidator(IValidator<DayOfWeekValidationData, DayOfWeekValidationResult> requiredValidator, ITimeSummaryRepository timeSummaries)
        {
            this._requiredValidator = requiredValidator;
            this._timeSummaries = timeSummaries;
        }

        public async ValueTask<ShiftValidationResult> Validate(ShiftValidationData value)
        {
            DayOfWeekValidationData dayOfWeekValidationData = new(value);
            DayOfWeekValidationResult dayOfWeekValidationResult = await _requiredValidator.Validate(dayOfWeekValidationData);

            IValidationResult previousValidationResult = dayOfWeekValidationResult;
            CompanyEmployeeValidationResult companyEmployeeValidationResult = previousValidationResult.GetValidationResultOfType<CompanyEmployeeValidationResult>();
            PrePurchase.Models.TimeSummary timeSummary = companyEmployeeValidationResult.TimeSummary;
            PrePurchase.Models.ClockData clockData = dayOfWeekValidationResult.ClockData;

            // update end date of time summary to reflect most recent time
            timeSummary!.EndDate = clockData.ClockIn;

            timeSummary.Clocks.Add(clockData);

            await _timeSummaries.Upsert(timeSummary.Id.ToString(), timeSummary);

            Response response = new() { TimeStamp = clockData.ClockIn, StatusCode = HttpStatusCode.OK, Message = "Clocked In" };
            return new ShiftValidationResult(dayOfWeekValidationResult, value, response);
        }
    }
}
