using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using PrePurchase.Models.Converters;
using System;

namespace SOG.Models
{
    public class SOGIzifo
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId(); //The ID is required by the database for the Primary Key
        public DateTime CreatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? UpdatedBy { get; set; }
        public bool? DeletedIndicator { get; set; }

        public string DeaceasedSurname { get; set; }
        public string DeaceasedName { get; set; }
        public string KnownAs { get; set; }

        public bool IsMember { get; set; }
        public bool IsAllPaid { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? IsMemberId { get; set; }


    }
}
