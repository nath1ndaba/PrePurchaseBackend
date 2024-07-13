using PrePurchase.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record AddEmployeeToRostaModel(
        [Required] List<RostaTaskModel> RostaTasks,
        [Required] string EmployeeId, 
        [Required] string ShiftName)
    {
        public AddEmployeeToRostaModel Sanitize()
        {
            return this with { EmployeeId = string.Join(string.Empty, EmployeeId.Split("-")) };
        }
    }

    public record RostaTaskModel(
        [Required] RateType RateType, 
        [Required] string TaskName,
        [Required] Location AlocatedSite,
        [Required] string Weekday);

}
