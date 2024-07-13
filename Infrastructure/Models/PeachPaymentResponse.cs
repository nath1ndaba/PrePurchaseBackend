using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace Infrastructure.Models
{
    public class PeachPaymentResponse
    {
        public string? Id {get; set;}
        public string? PaymentType {get; set;}
        public string? PaymentBrand {get; set;}
        public string Amount {get; set;}
        public string? Currency {get; set;}
        public string? Descriptor {get; set;}
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp {get; set;}
        public PeachResult Result {get; set;}
        public CustomParameters? CustomParameters {get; set;}
        //parameterErrors
    }

    public class PeachResult{
        public string Code {get; set;}
        public string Description {get; set;}
    }

    public class CustomParameters{
        public string? SHOPPER_EndToEndIdentity {get; set;}
        public string? CTPE_DESCRIPTOR_TEMPLATE {get; set;}
    }
}