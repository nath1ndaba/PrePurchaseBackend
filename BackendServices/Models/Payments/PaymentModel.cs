using System.ComponentModel.DataAnnotations;
using PrePurchase.Models.Payments;

namespace BackendServices.Models.Payments
{
    public record PaymentModel([Required] decimal Amount, [Required] string Currency);
}