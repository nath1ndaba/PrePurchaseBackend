using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record RemoveEmployeeFromRostaModel([Required] string EmployeeId,  [Required] string Weekday)
    {
        internal string RostaTaskId { get; set; }

        public RemoveEmployeeFromRostaModel WithTaskId(string id)
            => this with { RostaTaskId = id };
    }
}
