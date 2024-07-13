using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendServices.Models
{
    public record UniversalLoginModel(
         [Required] string Email,
         [Required] string Password)
    {
        public UniversalLoginModel Sanitize()
            => this with { Email = Email.ToLowerInvariant() };
    }
}
