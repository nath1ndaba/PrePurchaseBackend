using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models.Inventory;

[BsonIgnoreExtraElements]
public class Category
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();


    public DateTime CreateDate { get; set; }

    [BsonRequired]
    public DateTime UpdateDate { get; set; }

    [BsonRequired]
    [JsonConverter(typeof(ObjectIdConverter))]

    public ObjectId CreatedBy { get; set; }

    [BsonRequired]
    [JsonConverter(typeof(ObjectIdConverter))]

    public ObjectId UpdatedBy { get; set; }

    [BsonRequired]
    public bool DeletedIndicator { get; set; }

    public string CategoryName { get; set; }

    [BsonRequired]
    [JsonConverter(typeof(ObjectIdConverter))]

    public ObjectId ShopId { get; set; }
    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId ParentCategoryId { get; set; }

    public List<ObjectId> SubcategoriesIds { get; set; }

    public List<ObjectId> ProductsIds { get; set; }
    public int Level { get; set; } // Depth of nesting
    public Category()
    {
        SubcategoriesIds = new List<ObjectId>();
        ProductsIds = new List<ObjectId>();
    }
}