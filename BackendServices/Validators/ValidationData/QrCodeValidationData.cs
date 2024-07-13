using PrePurchase.Models;

namespace BackendServices.Validators.ValidationData
{
    internal struct QrCodeValidationData : IValidationData
    {
        public ClockInAndOutData ClockInAndOutData { get;}

        public IValidationData PreviousValidationData { get; }

        public QrCodeValidationData(IValidationData previousValidationData, ClockInAndOutData clockInAndOutData)
        {
            ClockInAndOutData = clockInAndOutData;
            PreviousValidationData = previousValidationData;
        }
    }
}
