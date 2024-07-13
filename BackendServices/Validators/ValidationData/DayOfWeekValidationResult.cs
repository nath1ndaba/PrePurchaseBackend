using PrePurchase.Models;

namespace BackendServices.Validators.ValidationData
{
    internal struct DayOfWeekValidationResult: IValidationResult
    {
        public IValidationResult PreviousValidationResult { get; }
        public ClockData ClockData { get; }
        public DayOfWeekValidationData ValidationData { get; }

        public DayOfWeekValidationResult(IValidationResult previousValidationResult,
            DayOfWeekValidationData validationData, ClockData clockData)
        {
            PreviousValidationResult = previousValidationResult;
            ValidationData = validationData;
            ClockData = clockData;
        }
    }
}
