using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record LoginModel(
         string Email,
         string UserName,
        [Required] string Password)
    {
        public LoginModel Sanitize()
            => this with { Email = Email.ToLowerInvariant() };
    }


}
