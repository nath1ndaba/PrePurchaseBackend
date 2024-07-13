namespace BackendServices.Validators.ValidationData
{
    internal struct DistanceValidationResult : IValidationResult
    {
        public DistanceValidationData DistanceValidationData { get;}
        public double Distance { get; }

        public IValidationResult PreviousValidationResult { get; }

        public DistanceValidationResult(IValidationResult previousValidationResult, DistanceValidationData data, double distance)
        {
            PreviousValidationResult = previousValidationResult;
            DistanceValidationData = data;
            Distance = distance;
        }

    }
}
