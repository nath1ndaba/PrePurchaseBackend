using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models;


[BsonIgnoreExtraElements]
public class ProcessedTimesSummary
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
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

    public string MyBatch { get; set; }
    public string HistoryBatchCode { get; set; }


    public List<TimeSummaryWithEmployeeDetails> timeSummaries { get; set; } = new();

}

