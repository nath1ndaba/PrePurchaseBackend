using System;

namespace BackendServices.Models.Inventory;
public class ProductDto
{
    public string Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool DeletedIndicator { get; set; }
    public string ShopId { get; set; }
    public string Name { get; set; }
    public byte[] ItemImage { get; set; }

    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Barcode { get; set; }
    public string CategoryID { get; set; }
    public string SupplierID { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
    public int BulkQuantity { get; set; }
    public string BulkUnit { get; set; }
}
