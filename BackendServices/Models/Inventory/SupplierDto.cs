using System;

namespace BackendServices.Models.Inventory;
public class SupplierDto
{
    public string Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool DeletedIndicator { get; set; }
    public string ShopId { get; set; }
    public string SupplierName { get; set; }
    public string ContactName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}