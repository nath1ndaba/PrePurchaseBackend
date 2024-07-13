using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Linq;
using PrePurchase.Models.Converters;
using PrePurchase.Models.Payments;

namespace PrePurchase.Models
{

    /// <summary>
    /// Summary of the Employee Details on the Company model
    /// This model should be generated using EmployeeDetails model
    /// </summary>
    [BsonIgnoreExtraElements]
    public class CompanyEmployee
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public DateTime CreatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? UpdatedBy { get; set; }
        public bool? DeletedIndicator { get; set; }
        [BsonRequired]
        public string Name { get; set; }
        [BsonRequired]
        public string Surname { get; set; }
        [BsonRequired]
        public string NickName { get; set; }
        [BsonRequired]
        public string CellNumber { get; set; }
        [BsonIgnore]
        public string FullName => Name + " " + Surname;
        [BsonRequired]
        public string EmployeeId { get; set; }
        [BsonRequired]
        public string Department { get; set; }
        public string Email { get; set; }
        public Address EmployeeAddress { get; set; }
        public string TaxNumber { get; set; }
        public DateTime DateOfEmployment { get; set; }
        public string IdNumber { get; set; }
        public BankAccountModel BankAccountInfo { get; set; }
        [BsonRequired]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyId { get; set; }

        [BsonRequired]
        public string Position { get; set; }
        [BsonRequired]
        public Dictionary<string, List<EmployeeTask>> WeekDays { get; set; } = new();

        public List<string> Roles = new();

        [BsonRequired]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        internal bool IsFor(ObjectId companyId, ObjectId employeeId)
            => CompanyId == companyId && Id == employeeId;
        internal bool IsFor(string companyId, string employeeId)
            => IsFor(ObjectId.Parse(companyId), ObjectId.Parse(employeeId));
        internal bool IsFor(Company company, EmployeeDetails employee)
            => IsFor(company.Id, employee.Id);
    }

    [BsonIgnoreExtraElements]
    public class EmployeeTask
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        //[BsonRequired]
        public Location AlocatedSite { get; set; }
        public string TaskName { get; set; }
        //[BsonRequired]
        public ObjectId Rate { get; set; }
        //[BsonRequired]
        public ObjectId Shift { get; set; }
        //[BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public RateType RateType { get; set; } = RateType.Standard;

    }

    public class DetailedEmployeeTask : EmployeeTask
    {
        private Rate _rate;
        public new Rate Rate
        {
            get => _rate;
            set
            {
                _rate = value;
                base.Rate = value.Id;
            }
        }

        private Shift _shift;
        public new Shift Shift
        {
            get => _shift;
            set
            {
                _shift = value;
                base.Shift = value.Id;
            }
        }

        internal static DetailedEmployeeTask FromEmployeeTask(EmployeeTask task, Rate rate, Shift shift, Location alocatedSite)
        {
            return new DetailedEmployeeTask()
            {
                Id = task.Id,
                TaskName = task.TaskName,
                Rate = rate,
                Shift = shift,
                RateType = task.RateType,
                AlocatedSite = alocatedSite
            };
        }
    }

    public class DetailedCompanyEmployee : CompanyEmployee
    {
        public new Dictionary<string, IEnumerable<DetailedEmployeeTask>> WeekDays { get; set; } = new();

        internal static Dictionary<string, IEnumerable<DetailedEmployeeTask>> CreateDetailedTasks(Dictionary<string, List<EmployeeTask>> weekDays, Dictionary<ObjectId, Shift> shifts, Dictionary<ObjectId, Rate> rates, Dictionary<ObjectId, Location> alocatedSites)
        {
            Dictionary<string, IEnumerable<DetailedEmployeeTask>> _weekDays = new(weekDays.Count);

            foreach (var weekDay in weekDays)
            {
                List<DetailedEmployeeTask> detailedTasks = new(weekDay.Value.Count);

                foreach (var task in weekDay.Value)
                {
                    detailedTasks.Add(DetailedEmployeeTask.FromEmployeeTask(task, rates[task.Rate], shifts[task.Shift], task.AlocatedSite));
                }
                _weekDays[weekDay.Key] = detailedTasks;
            }
            return _weekDays;
        }

        internal static DetailedCompanyEmployee FromCompanyEmployee(CompanyEmployee employee, Dictionary<ObjectId, Shift> shifts, Dictionary<ObjectId, Rate> rates, Dictionary<ObjectId, Location> alocatedSites)
        {
            var _weekDays = CreateDetailedTasks(employee.WeekDays, shifts, rates, alocatedSites);

            return new()
            {
                Id = employee.Id,
                DeletedIndicator = employee.DeletedIndicator,
                CreatedBy = employee.CreatedBy,
                CreatedDate = employee.CreatedDate,
                UpdatedBy = employee.UpdatedBy,
                UpdatedDate = employee.UpdatedDate,
                DateOfEmployment = employee.DateOfEmployment,
                Email = employee.Email,
                EmployeeAddress = employee.EmployeeAddress,

                Name = employee.Name,
                Surname = employee.Surname,
                NickName = employee.NickName,
                CellNumber = employee.CellNumber,
                EmployeeId = employee.EmployeeId,
                Department = employee.Department,
                BankAccountInfo = employee.BankAccountInfo,
                CompanyId = employee.CompanyId,
                Position = employee.Position,
                Roles = employee.Roles,
                Timestamp = employee.Timestamp,
                WeekDays = _weekDays
            };
        }
    }
}
