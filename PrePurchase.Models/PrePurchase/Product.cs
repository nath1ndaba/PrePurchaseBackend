//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using Newtonsoft.Json;
//using PrePurchase.Models.Converters;

//namespace PrePurchase.Models.PrePurchase;

//public class Product
//{
//    [JsonConverter(typeof(ObjectIdConverter))]
//    [BsonId]
//    [BsonRequired]
//    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

//    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

//    [JsonConverter(typeof(ObjectIdConverter))]
//    public ObjectId CreatedBy { get; set; }

//    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

//    [JsonConverter(typeof(ObjectIdConverter))]
//    public ObjectId UpdatedBy { get; set; }

//    public bool DeletedIndicator { get; set; }

//    [Required]
//    [StringLength(100, MinimumLength = 1)]
//    public string Name { get; set; }

//    [Required]
//    [StringLength(500)]
//    public string Description { get; set; }

//    [Required]
//    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
//    public decimal Price { get; set; }

//    public string Category { get; set; }

//    public int StockQuantity { get; set; } = 0;

//    public ItemStatus Status { get; set; } = ItemStatus.Available;

//    public List<string> Tags { get; set; } = new List<string>();

//    public string QRCode { get; set; }
//    public byte[] ItemImage { get; set; }

//    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
//}

//public enum ItemStatus
//{
//    Available,
//    OutOfStock,
//    Discontinued
//}

//public enum ApprovalStatus
//{
//    Pending,
//    Approved,
//    Rejected
//}
