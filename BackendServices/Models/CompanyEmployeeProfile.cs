using PrePurchase.Models;
using System;
using System.Collections.Generic;

namespace BackendServices.Models
{
    public record CompanyEmployeeProfile
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Surname { get; init; }
        public string NickName { get; init; }
        public string CellNumber { get; init; }
        public string FullName => Name + " " + Surname;
        public string EmployeeId { get; init; }
        public string Department { get; init; }
        public CompanyProfile Company { get; init; }
        public string Position { get; init; }
        public Dictionary<string, List<EmployeeTask>> WeekDays { get; init; }
        public IEnumerable<string> Roles { get;  init; }
        public DateTime Timestamp { get; init; }
    }

    public record CompanyProfile
    {
        public string Id { get; init; }
        public string CompanyName { get; init; }
        public string CellNumber { get; init; }
        public string Email { get; init; }
        public Address Address { get; init; }
        public string RegisterationNumber { get; init; }
    }
}
