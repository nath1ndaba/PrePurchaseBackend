using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrePurchase.Models
{
    public class UniversalPositionsInfo
    {
        public string Position { get; set; }
        public List<UniversalCompanyEmployeeProfile> EmployeesUnderThisPosition { get; set; }
    }
}
