using PrePurchase.Models.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackendServices.Models
{
    public record ShiftModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly ShiftStartTime { get; set; }
        [Required] 
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly ShiftEndTime { get; set; }

        public void Deconstruct(out string name, out TimeOnly shiftStartTime, out TimeOnly shiftEndTime)
        {
            name = Name;
            shiftStartTime = ShiftStartTime;
            shiftEndTime = ShiftEndTime;
        }

        public ShiftModel Sanitize()
         => this with { Name = Name.Trim() };

    }
}
