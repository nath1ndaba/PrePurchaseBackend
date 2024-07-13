using PrePurchase.Models;

namespace BackendServices.Validators.ValidationData
{
    internal struct DistanceValidationData : IValidationData
    {
        public Location EmployeePosition { get; }
        public double Tolerance { get; }

        public IValidationData PreviousValidationData { get; }

        public DistanceValidationData(IValidationData previousValidationData, Location employeePosition, double tolerance)
        {
            EmployeePosition = employeePosition;
            Tolerance = tolerance;
            PreviousValidationData = previousValidationData;
        }
    }
}
