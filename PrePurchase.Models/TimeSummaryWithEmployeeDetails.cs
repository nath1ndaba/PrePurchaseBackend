using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models;
[BsonIgnoreExtraElements]

public record TimeSummaryWithEmployeeDetails
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; } 
    public DateTime CreatedDate { get; set; }
    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId? CreatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId? UpdatedBy { get; set; }
    public bool? DeletedIndicator { get; set; }
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonRequired]
    public ObjectId CompanyId { get; set; }
    public CompanyEmployeeDto CompanyEmployee { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }

    public List<ClockData> Clocks { get; set; } = new();

    public decimal Amount => Clocks.Sum(c => c.Amount);
    public decimal DailyBonus { get; set; }

}
[BsonIgnoreExtraElements]
public record ClockDataDto
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; } 
    public DateTime ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public Shift Shift { get; set; }
    public Rate Rate { get; set; }
    public RateType RateType { get; set; }

    public double TotalHours { get; set; }

    public double OverTimeHours { get; set; }

    public decimal Amount { get; set; }
    public bool IsProcessed { get; set; }
    public bool IsAdminClocking { get; set; }
    public bool IsAnnualLeaveDays { get; set; }
    public bool IsSickLeaveDays { get; set; }
    public bool IsFamilyLeaveDays { get; set; }
    public bool IsClockInAdjusted { get; set; }
    public bool IsClockOutAdjusted { get; set; }
    public DateTime OldClockInValue { get; set; }
    public DateTime OldClockOutValue { get; set; }
    public List<RestTime> RestTimes { get; set; }
}

public record ShiftDto
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; } 
    
    public string Name { get; set; }
    [JsonConverter(typeof(TimeOnlyConverter))]
    public TimeOnly ShiftStartTime { get; set; }
    [JsonConverter(typeof(TimeOnlyConverter))]
    public TimeOnly ShiftEndTime { get; set; }
}

public record RateDto
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; }
    public string NameOfPosition { get; set; }
    public decimal StandardDaysRate { get; set; }
    public decimal SundaysRate { get; set; }
    public decimal SaturdaysRate { get; set; }
    public decimal PublicHolidaysRate { get; set; }
    public decimal DailyBonus { get; set; }
    public decimal OverTimeRate { get; set; }
}

public record CompanyEmployeeDto
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string NickName { get; set; }
    public string FullName => Name + " " + Surname;
    public string EmployeeId { get; set; }
    public string Department { get; set; }
    public string CompanyId { get; set; }
    public string Position { get; set; }
    public DateTime Timestamp { get; set; }
}