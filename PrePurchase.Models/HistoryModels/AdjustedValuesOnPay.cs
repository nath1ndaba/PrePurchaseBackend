using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace PrePurchase.Models.HistoryModels;
[BsonIgnoreExtraElements]
public class AdjustedValuesOnPay
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string HistoryId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal TotalBasic { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal InitialBonus { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal AdjustedBonus { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]

    public decimal InitialLoan { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal AdjustedLoan { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal AccessFunds { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal UIFDeduction { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal TaxDeduction { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal OriginalNet { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal AdjustedNet { get; set; }
}
