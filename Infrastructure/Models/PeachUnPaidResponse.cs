using System.Collections.Generic;
using System.Xml.Serialization;

namespace Infrastructure.Models
{
    [XmlRoot("Response")]
    public class PeachUnPaidResponse
    {
        [XmlElement("Result")]
        public string Result {get;set;}
        [XmlElement("BatchCode")]
        public string BatchCode {get;set;} //The unique code for the batch the unpaids belong to.
        
        [XmlElement("PaymentResults")]
        public List<PeachPaymentResult> PaymentResults {get;set;} //The collection of Results with more information about each unpaid.
    }

    [XmlRoot("Result")]
    public class PeachPaymentResult{
        [XmlElement("Result")]
        public string FirstName {get; set;}
        [XmlElement("Surname")]
        public string Surname {get; set;}
        [XmlElement("CustomerCode")]
        public string CustomerCode {get; set;}
        [XmlElement("Reference")]
        public string Reference {get; set;}
        [XmlElement("Result")]
        public string Result {get; set;}
        [XmlElement("ResultMessage")]
        public string ResultMessage {get;set;}
        [XmlElement("BranchCode")]
        public int BranchCode {get; set;}
        [XmlElement("AccountNumber")]
        public long AccountNumber {get; set;}
    }
}