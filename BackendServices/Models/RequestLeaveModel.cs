using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models;
using PrePurchase.Models.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackendServices.Models
{
    [BsonIgnoreExtraElements]
    public record RequestLeaveModel
        ([Required] TypeOfLeave TypeOfLeave,
        [Required] int DaysToTake)
    {
        [JsonConverter(typeof(DateTimeConverter))]
        [Required]
        public DateTime LeaveStartDate { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        [Required]
        public DateTime LeaveEndDate { get; set; }
    }
#nullable enable
    public record QueryLeaveModel(
          string? Comment,
        int? DaysToTake,
        string? CompanyId,
        TypeOfLeave? TypeOfLeave,
        string? EmployeeId,
        LeaveStatus? Status)
    {
        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? LeaveStartDate { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? LeaveEndDate { get; set; }
    }



#nullable disable
}
