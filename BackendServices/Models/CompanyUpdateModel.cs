using PrePurchase.Models;

namespace BackendServices.Models
{
#nullable enable
    public record CompanyUpdateModel
        (string? RegisterationNumber, 
        string? CompanyName, 
        Address? Address );

#nullable restore
}
