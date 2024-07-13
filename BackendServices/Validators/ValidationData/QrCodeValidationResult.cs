using PrePurchase.Models;
using System;

namespace BackendServices.Validators.ValidationData
{
    internal struct QrCodeValidationResult : IValidationResult
    {
        public QrCodeValidationData ValidationData { get; }
        //public bool IsValidQrCode => ValidationData.ClockInAndOutData.IsValidQrCode;

        public Company Company { get; }
        public TimeZoneInfo TimeZoneInfo { get; }

        public IValidationResult PreviousValidationResult { get; }

        public QrCodeValidationResult(IValidationResult previousValidationResult, QrCodeValidationData validationData, Company company, TimeZoneInfo timeZoneInfo)
        {
            ValidationData = validationData;
            Company = company;
            PreviousValidationResult = previousValidationResult;
            TimeZoneInfo = timeZoneInfo;
        }

        public QrCodeValidationResult(QrCodeValidationData data, Company company, TimeZoneInfo timeZoneInfo)
            : this(null, data, company, timeZoneInfo)
        {}
    }
}
