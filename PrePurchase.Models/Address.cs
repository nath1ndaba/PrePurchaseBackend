using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace PrePurchase.Models
{
    [BsonNoId]
    [BsonIgnoreExtraElements]
    public record Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public List<Location> ListOfSitesPerCompany { get; set; }
    }


}
