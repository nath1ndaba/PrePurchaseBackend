using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    public class Customization
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public DateTime CreatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? UpdatedBy { get; set; }
        public bool? DeletedIndicator { get; set; }
        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyId { get; set; }
        public string CustomAuth1 { get; set; }
        public string CustomText1 { get; set; }
        public string CustomAuth2 { get; set; }
        public string CustomText2 { get; set; } 
        public string CustomAuth3 { get; set; }
        public string CustomText3 { get; set; } 
        public string CustomAuth4 { get; set; }
        public string CustomText4 { get; set; } 
        public string CustomAuth5 { get; set; }
        public string CustomText5 { get; set; }
        public string CustomAuth6 { get; set; }
        public string CustomText6 { get; set; } 
        public string CustomAuth7 { get; set; }
        public string CustomText7 { get; set; }
        public string CustomAuth8 { get; set; }
        public string CustomText8 { get; set; }
    }
}
