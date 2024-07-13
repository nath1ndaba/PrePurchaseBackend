using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record ChangePasswordModel([Required] string CurrentPassword, [Required] string NewPassword);
}
