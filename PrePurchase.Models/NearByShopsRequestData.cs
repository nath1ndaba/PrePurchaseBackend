using System;

namespace PrePurchase.Models
{
    public record NearByShopsRequestData
    {
        public ResidentLocation ResidentPosition { get; set; }

        /// <summary>
        /// The company QrCode will be encoded with CompanyId
        /// </summary>
        public string QRCode { get; set; } //This CompanyId was QrCode
        public string TimeZoneId { get; set; }
        //[JsonIgnore]
        //public CompanyQrCode CompanyQrCode => CompanyQrCode.Decode(QrCode);

        //public bool IsValidQrCode => CompanyQrCode.IsValidQrCode(QrCode);

    }

    public struct ClockInAndOutResponse
    {
        public DateTime? TimeStamp { get; set; }
        public string Error { get; set; }
    }
}
