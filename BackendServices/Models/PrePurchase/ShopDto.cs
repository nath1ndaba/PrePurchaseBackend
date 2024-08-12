using System;

namespace BackendServices.Models.PrePurchase;

public class ShopDto
{
    public string Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string UpdatedBy { get; set; }
    public bool? DeletedIndicator { get; set; }

    public string Name { get; set; }
    public Address Address { get; set; }
    public string RegisterationNumber { get; set; }
    public string Email { get; set; }
    public string ContactNumber { get; set; }
    public string QRCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime? LicenseExpiryDate { get; set; }
}
