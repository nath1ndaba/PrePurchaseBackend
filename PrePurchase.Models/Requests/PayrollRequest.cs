using PrePurchase.Models.Payments;

namespace PrePurchase.Models.Requests
{
    public record PayrollRequest(
        Service Service,
        ServiceType ServiceType,
        string BatchCode
    );
}