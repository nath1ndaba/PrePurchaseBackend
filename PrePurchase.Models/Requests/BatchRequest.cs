using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PrePurchase.Models.Requests
{
    public record BatchRequest(
        DateTimeOffset StartDate,
        DateTimeOffset EndDate,
        IEnumerable<string> PayrollEmployeesIds,
        BatchType BatchType
    );
    public enum BatchType
    {
        Monthly, ForthNight, Weekly, Daily
    }
}