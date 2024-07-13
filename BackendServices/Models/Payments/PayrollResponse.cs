using System.Collections.Generic;

namespace BackendServices.Models.Payments
{
    public class PayrollResponse
    {
        public string Message {get;set;}
        public int BatchCode {get;set;}
        public decimal PaidAmount {get;set;} //which contains the total value of the batch submitted (less any accounts that failed the CDV check).
        public decimal BatchPayoutFee {get;set;} //excluding vat
        public List<UnPaid> UnPaids {get;set;} //excluding vat
    }

    public class UnPaid{
        public string Reason {get;set;}
        public long AccountNumber {get;set;}
    }
}