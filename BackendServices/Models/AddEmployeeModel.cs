using PrePurchase.Models;
using PrePurchase.Models.Payments;
using System;

namespace BackendServices.Models
{
    public record AddEmployeeModel(
         string Department,
         string Position,
        string Name,
        string Surname,
        string Password,
        string EmployeeId,
        DateTime DateOfBirth,
        string NickName,
        string CellNumber,
        string Email,
        string IDNumber,
        string TaxNumber,
        DateTime DateOfEmployment,
        Address EmployeeAddress,
        BankAccountModel BankAccountInfo)
    {
        public AddEmployeeModel Sanitize()
        => this with
        {
            EmployeeId = string.IsNullOrWhiteSpace(EmployeeId) is false ? string.Join(string.Empty, EmployeeId.ToLowerInvariant().Split('-')) : EmployeeId,
            Department = Department.ToLowerInvariant(),
            Position = Position.ToLowerInvariant()
        };
    }

    public record AddNewEmployeeResponseModel(string EmployeeId, string Password)
    {
        public AddNewEmployeeResponseModel Sanitize()
            => this with
            {
                EmployeeId = $"{EmployeeId[..4]}-{EmployeeId[4..9]}-{EmployeeId[9..]}"
            };
    }

}
