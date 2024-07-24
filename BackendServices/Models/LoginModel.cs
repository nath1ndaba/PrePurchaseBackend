using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record LoginModel(
         [Required] string Username,
        [Required] string Password)
    {
        public LoginModel Sanitize()
            => this with { Username = Username.ToLowerInvariant() };
    }


}
