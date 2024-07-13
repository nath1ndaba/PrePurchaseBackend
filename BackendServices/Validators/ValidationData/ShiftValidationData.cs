using PrePurchase.Models;

namespace BackendServices.Validators.ValidationData
{
    internal struct ShiftValidationData : IValidationData
    {
        public ClockInAndOutData ClockInData { get; }
        public string EmployeeId { get; }
        public string EmployeeDetailsId { get; set; }
        public double TimeTolerance { get; }
        public double DistanceTolerance { get; }

        public IValidationData PreviousValidationData { get; }

        public ShiftValidationData(ClockInAndOutData clockInData, string employeeId, string employeeDetailsId, double tolerance, double distanceTolerance)
        {
            ClockInData = clockInData;
            EmployeeId = employeeId;
            EmployeeDetailsId = employeeDetailsId;
            PreviousValidationData = null;
            TimeTolerance = tolerance;
            DistanceTolerance = distanceTolerance;
        }
    }
}
