using System;

namespace BackendServices.Models.Inventory;

public class RolePermissionDto
{
    public string Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool DeletedIndicator { get; set; }
    public string RoleID { get; set; }
    public string PermissionID { get; set; }
    public string ShopId { get; set; }
}
