using System;

namespace BackendServices.Models.Inventory;
public class StockCountDto
{
    public string Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool DeletedIndicator { get; set; }
    public string ShopId { get; set; }
    public DateTime CountDate { get; set; }
    public string CountType { get; set; }
    public string ProductID { get; set; }
    public int CountedQuantity { get; set; }
}
