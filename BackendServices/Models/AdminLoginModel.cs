using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record AdminLoginModel(
        [Required] string Email, 
        [Required] string Password)
    {
        public AdminLoginModel Sanitize()
            => this with { Email = Email.ToLowerInvariant() };
    }  
    
   
}
