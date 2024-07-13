using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SOG.Models
{
    public class SOGContributedMember
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Isifo { get; set; }
        public decimal Amount { get; set; }
    }

}
