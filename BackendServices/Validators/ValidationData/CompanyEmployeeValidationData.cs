
namespace BackendServices.Validators.ValidationData
{
    internal struct CompanyEmployeeValidationData : IValidationData
    {
        public IValidationData PreviousValidationData { get; set; }

        public CompanyEmployeeValidationData(IValidationData previousValidationData)
        {
            PreviousValidationData = previousValidationData;
        }
    }
}
