using System.Collections.Generic;
using System.Xml.Serialization;

namespace Infrastructure.Models
{
    [XmlRoot("Response")]
    public class PeachPayrollResponse
    {
        [XmlElement("Result")]
        public string Result {get;set;}
        [XmlElement("ResultMessage")]
        public string ResultMessage {get;set;}
        [XmlElement("BatchCode")]
        public int BatchCode {get;set;} //The unique code for the batch the unpaids belong to.
        [XmlElement("BatchValueSubmitted")]
        public decimal PaidAmount {get;set;} //which contains the total value of the batch submitted (less any accounts that failed the CDV check).
        [XmlElement("TotalFeeExcludingVAT")]
        public decimal PayoutFee {get;set;} //excluding vat
        [XmlElement("CDVResults")]
        public List<PeachCDVResult> CDVResults {get;set;} //excluding vat
    }

    
    public class PeachCDVResult{

        [XmlElement("Result")]
        public PeachPayoutResult Result {get;set;}
    }

    public class PeachPayoutResult{
        [XmlElement("CustomerCode")]
        public string CustomerCode {get;set;}
        [XmlElement("Result")]
        public string Result {get;set;}
        [XmlElement("Message")]
        public string Message {get;set;}
        [XmlElement("AccountNumber")]
        public long AccountNumber {get;set;}
    }
}