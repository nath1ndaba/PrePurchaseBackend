//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using Newtonsoft.Json;
//using PrePurchase.Models.Converters;

//namespace PrePurchase.Models.PrePurchase;

//public class ProductDto
//{
//    [JsonConverter(typeof(ObjectIdConverter))]
//    [BsonId]
//    [BsonRequired]
//    public string Id { get; set; }

//    public DateTime CreatedDate { get; set; }

//    [JsonConverter(typeof(ObjectIdConverter))]
//    public string CreatedBy { get; set; }

//    public DateTime UpdatedDate { get; set; }

//    [JsonConverter(typeof(ObjectIdConverter))]
//    public string UpdatedBy { get; set; }

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

//    public int StockQuantity { get; set; }

//    public ItemStatus Status { get; set; }

//    public List<string> Tags { get; set; }

//    public string QRCode { get; set; }
//    public byte[] ItemImage { get; set; }
//    public decimal RechargeBalance { get; set; }/// <summary>
//                                                /// needed only when fetching data and specifically for resident side
//                                                /// </summary>

//    public ApprovalStatus ApprovalStatus { get; set; }
//}