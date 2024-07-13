using System;

namespace PrePurchase.Models
{
    public class ProcessedBatch
    {
        public int BatchCode { get; set; }
        public decimal PaidAmount { get; set; } //which contains the total value of the batch submitted (less any accounts that failed the CDV check).
        public decimal BatchPayoutFee { get; set; } //excluding vat
        public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;
    }
}