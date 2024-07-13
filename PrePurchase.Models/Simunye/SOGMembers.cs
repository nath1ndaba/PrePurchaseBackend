using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;
using System;
using System.Text.Json.Serialization;

namespace SOG.Models
{
    public class SOGMembers
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
        public bool? IsDeceased { get; set; }

        public string Ibizo { get; set; }
        public string Isibongo { get; set; }

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? PositionId { get; set; }
        public MemberLogin Login { get; set; } = new MemberLogin();


    }
}
