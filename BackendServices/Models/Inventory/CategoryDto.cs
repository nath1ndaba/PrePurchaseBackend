using System;
using System.Collections.Generic;

namespace BackendServices.Models.Inventory
{
    public class CategoryDto
    {
        public string Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool DeletedIndicator { get; set; }
        public string CategoryName { get; set; }
        public string ShopId { get; set; }
        public string ParentCategoryId { get; set; }
        public List<string> SubcategoriesIds { get; set; }
        public List<string> ProductsIds { get; set; }
        public int Level { get; set; }
    }

}
