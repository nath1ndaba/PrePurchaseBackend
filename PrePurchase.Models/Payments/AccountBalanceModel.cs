namespace PrePurchase.Models.Payments
{
    public class AccountBalanceModel
    {
        public decimal CurrentBalance {get; set;}
        public Currency LocalCurrency {get; set;} = Currency.ZAR;
    }
}