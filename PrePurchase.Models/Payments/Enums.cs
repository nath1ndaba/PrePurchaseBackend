namespace PrePurchase.Models.Payments
{
    public enum Currency
    {
        ZAR
    }

    public enum PaymentMethod{
        Deposit,Subscription,Payroll
    }

    public enum Service
    {
        Wages, //For payments relating to wages.
        Salaries, //For payments relating to salaries.
        Creditors, //For payments relating to salaries.
    }

    public enum ServiceType
    {
        OneDayOnly, //For 1 day payments.
        SDV, //The same day value service
    }
}