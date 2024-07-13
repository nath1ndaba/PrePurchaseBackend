using PrePurchase.Models;

namespace BackendServices.Validators.ValidationData
{
    internal struct CompanyEmployeeValidationResult : IValidationResult
    {
        public IValidationResult PreviousValidationResult { get; }
        public CompanyEmployeeValidationData ValidationData { get; }
        public CompanyEmployee CompanyEmployee { get; }
        public TimeSummary TimeSummary { get; }

        public CompanyEmployeeValidationResult(IValidationResult previousValidationResult,
            CompanyEmployeeValidationData validationData, CompanyEmployee companyEmployee,
            TimeSummary timeSummary)
        {
            PreviousValidationResult = previousValidationResult;
            ValidationData = validationData;
            CompanyEmployee = companyEmployee;
            TimeSummary = timeSummary;
        }
    }
}
