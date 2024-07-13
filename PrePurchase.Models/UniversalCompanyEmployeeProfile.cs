using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PrePurchase.Models
{
    public class UniversalCompanyEmployeeProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string NickName { get; set; }
        public string FullName { get; set; }
        public string EmployeeId { get; set; }
        public string Department { get; set; }
        public string CellNumber { get; set; }
        public string Email { get; set; }
        public Address EmployeeAddress { get; set; }
        public string IdNumber { get; set; }
        public string TaxNumber { get; set; }
        public DateTime DateOfEmployment { get; set; }
        public Company Company { get; set; } = new();
        public string Position { get; set; }
        public IDictionary<string, List<EmployeeTask>> WeekDays { get; set; }
        public List<string> Roles { get; set; } = new();
        //[JsonConverter(typeof(DateTimeConverter))]
        //public DateTime TimeStamp { get; set; }
    }
}
