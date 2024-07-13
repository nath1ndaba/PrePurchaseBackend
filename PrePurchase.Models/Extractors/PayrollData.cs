using System;

namespace PrePurchase.Models.Extractors
{
    public class PayrollData
    {
        public string Id { get; set; }

        public string ProcessedDate { get; set; }
        public string BatchCode { get; set; }
        public bool IsPaid { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string TotalHours { get; set; }

        public string TotalShifts { get; set; }

        public decimal BasicAmount { get; set; }
        public string TotalBonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal Loans { get; set; }
        public decimal NetAmount { get; set; }
    }
}
