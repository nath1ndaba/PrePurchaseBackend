using Microsoft.Extensions.DependencyModel;

namespace PrePurchase.Models;

public class AllTheLeave
{
    public string AnnualLeaveDescription { get; set; } = "Annual";
    public  decimal AnnualLeaveStart { get; set; }
    public  int AnnualLeaveTaken { get; set; }
    public  decimal AnnualLeaveAccrued { get; set; }
    public  decimal AnnualLeaveEndKnownAsBalance { get; set; }  
    
    public  string SickLeaveDescription { get; set; } = "Sick";
    public  decimal SickLeaveStart { get; set; }
    public  int SickLeaveTaken { get; set; }
    public  decimal SickLeaveAccrued { get; set; }
    public  decimal SickLeaveEndKnownAsBalance { get; set; }   
    
    
    public  string FamilyLeaveLeaveDescription { get; set; } = "Family";
    public  decimal FamilyLeaveStart { get; set; }
    public  int FamilyLeaveTaken { get; set; }
    public  decimal FamilyLeaveAccrued { get; set; }
    public  decimal FamilyLeaveEndKnownAsBalance { get; set; }
}