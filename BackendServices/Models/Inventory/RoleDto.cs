using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendServices.Models.Inventory;
public class RoleDto
{
    public string Id { get; set; }
    public string ShopId { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool DeletedIndicator { get; set; }
    public string RoleName { get; set; }
    public string Description { get; set; }
}


