namespace BackendServices.Validators.ValidationData
{
    internal struct DayOfWeekValidationData : IValidationData
    {
        public IValidationData PreviousValidationData { get; }

        public DayOfWeekValidationData(IValidationData previousValidationData)
        {
            PreviousValidationData = previousValidationData;
        }
    }
}
