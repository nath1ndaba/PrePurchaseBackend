using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrePurchase.Models
{
    public class AddExistingEmployeeToNewCompany
    {
        public string EmployeeId { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }

    }
}
