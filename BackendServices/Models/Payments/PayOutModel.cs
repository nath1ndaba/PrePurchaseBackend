using PrePurchase.Models.Payments;
using System.Collections.Generic;
using System.Linq;

namespace BackendServices.Models.Payments
{
    public class PayOutModel
    {
        public string Reference { get; set; }
        public Service Service { get; set; }
        public ServiceType ServiceType { get; set; }
        public IEnumerable<PayOutPaymentModel> Payments { get; set; }

        #region Totals
        public int Records => Payments.Count();
        public long BranchHash => Payments.Sum(x => x.BranchCode);
        public long AccountHash => Payments.Sum(x => x.AccountNumber);
        public decimal TotalAmount => Payments.Sum(x => x.FileAmount);
        #endregion
    }

    public class PayOutPaymentModel
    {
        public string Initials { get; set; }
        public string FirstNames { get; set; }
        public string Surname { get; set; }
        public string CustomerCode { get; set; }
        public decimal FileAmount { get; set; }
        public decimal AmountMultiplier { get; set; }
        public long BranchCode { get; set; }
        public long AccountNumber { get; set; }
        public int AccountType { get; set; }
    }
}