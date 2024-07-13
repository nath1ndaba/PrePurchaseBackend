using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    public class DetailedAd
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
        public string CompanyName { get; set; }

        public bool IsDeletedIndicator { get; set; }
        public byte[] MainAdImage { get; set; }
        public List<Ad> Ads { get; set; }
    }
    public class Ad
    {
        public string Heading { get; set; }
        public string Paragraph { get; set; }
        public byte[] AdImage { get; set; }
    }

}
