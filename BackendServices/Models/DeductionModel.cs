using PrePurchase.Models;
using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record DeductionModel(
        [Required] string TypeOfDeduction
        , [Required] decimal AmountToDeduct
        , [Required] AmountType AmountType);
}
