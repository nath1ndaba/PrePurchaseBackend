using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record EmployeeLoginModel([Required] string EmployeeId, [Required] string Password, string Deviceinfo)
    {
        public EmployeeLoginModel Sanitize()
        => this with
        {
            EmployeeId = string.Join(string.Empty, EmployeeId.ToLowerInvariant().Split('-'))
        };
    }
}
