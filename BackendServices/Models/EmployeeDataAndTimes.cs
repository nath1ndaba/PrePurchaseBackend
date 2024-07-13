using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendServices.Models
{
    public class EmployeeDataAndTimes
    {
        public List<ClockData> ClockData { get; set; }
        public CompanyEmployee CompanyEmployee { get; set; }
    }
}
