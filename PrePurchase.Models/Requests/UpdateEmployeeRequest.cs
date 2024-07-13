using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using PrePurchase.Models.Payments;

namespace PrePurchase.Models.Requests
{
    public record UpdateEmployeeRequest()
    {
        public string NickName { get; set; }
        public string CellNumber { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public Address EmployeeAddress { get; set; }
        public BankAccountModel BankAccountInfo { get; set; }

        //tempo update
        public string TaxNumber { get; set; }
        public string IDNumber { get; set; }
        public DateTime DateOfEmployment { get; set; }
        public DateTime DateOfBirth { get; set; }
        //tempo update
    }
}
