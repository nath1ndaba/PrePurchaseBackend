using System.Text.Json;
using System.Text;
using System;

namespace PrePurchase.Models
{
    public record CompanyQrCode
    {
        public string CompanyId { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public string Encode()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(ToJson()));
        }

        public static CompanyQrCode Decode(string base64EncodedString)
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedString));
            return JsonSerializer.Deserialize<CompanyQrCode>(json);
        }

        public static bool IsValidQrCode(string base64EncodedString)
        {
            try
            {
                return Decode(base64EncodedString) is not null;
            }
            catch
            {
                return false;
            }
        }

    }
}
