using System;

namespace BackendServices.Models.Inventory;
public class PurchaseOrderDto
{
    public string Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool DeletedIndicator { get; set; }
    public string ShopId { get; set; }
    public string SupplierID { get; set; }
    public string PurchaseOrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public decimal TotalCost { get; set; }
}

