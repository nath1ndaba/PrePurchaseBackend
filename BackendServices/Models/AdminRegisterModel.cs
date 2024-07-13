using PrePurchase.Models;
using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record AdminRegisterModel(
        [Required] string Name,
        [Required] string Surname,
        [Required] [EmailAddress] string Email,
        [Required] [Phone] string PhoneNumber,
        [Required] Address Address,
        [Required] string Password
    )
    {
    }
}