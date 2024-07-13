namespace PrePurchase.Models
{
    public enum ClokingType
    {
        Clockin,
        Clockout,
        StartBreak,
        EndBreak
    }

    public enum TypeOfLeave
    {
        Annual,
        Sick,
        Family,
        Martenity,
        Parental,
        Unpaid,
        Other
    }

    public enum LeaveStatus
    {
        New,
        Declined,
        Accepted
    }

    public enum Project
    {
        Payroll,
        Inventory,
        POS
    }
}
