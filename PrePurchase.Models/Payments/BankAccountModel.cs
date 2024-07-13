namespace PrePurchase.Models.Payments
{
    public class BankAccountModel
    {
        public string AccountHolder { get; set; }
        public long? AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string Branch { get; set; }
        public int? BranchCode { get; set; }
        public string BankName { get; set; }
    }
}