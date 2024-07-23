using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.Models;
using Infrastructure.Helpers;
using MongoDB.Bson;
using PrePurchase.Models;
using PrePurchase.Models.Payments;
using PrePurchase.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin
{
    public class EmployeesActions : IEmployeesActions
    {
        public async Task<Response> GetEmployees(string role, string? companyid = null)
        {
            if (ObjectId.TryParse(companyid, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");
            Company company = await _common.ValidateOwner<Company>(role, companyid);
            IEnumerable<CompanyEmployee> employees = await _companyEmployees.Find(x => x.CompanyId == company.Id && x.DeletedIndicator == false);

            Dictionary<ObjectId, Shift> companyShiftsMapped = company.Shifts.MapUnique(x => x.Id);
            Dictionary<ObjectId, Rate> companyRatesMapped = company.Rates.MapUnique(x => x.Id);
            Dictionary<ObjectId, Location> companySitesMapped = company.Address.ListOfSitesPerCompany.MapUnique(x => x.Id);
            IEnumerable<DetailedCompanyEmployee> withRateAndShift = employees.Select(x => DetailedCompanyEmployee.FromCompanyEmployee(x, companyShiftsMapped, companyRatesMapped, companySitesMapped));

            return new Response<IEnumerable<DetailedCompanyEmployee>>(withRateAndShift);
        }

        public async Task<Response> GetArchivedEmployees(string role, string? companyid = null)
        {
            if (ObjectId.TryParse(companyid, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");
            Company company = await _common.ValidateOwner<Company>(role, companyid);
            IEnumerable<CompanyEmployee> employees = await _companyEmployees.Find(x => x.CompanyId == company.Id && x.DeletedIndicator == true);

            Dictionary<ObjectId, Shift> companyShiftsMapped = company.Shifts.MapUnique(x => x.Id);
            Dictionary<ObjectId, Rate> companyRatesMapped = company.Rates.MapUnique(x => x.Id);
            Dictionary<ObjectId, Location> companySitesMapped = company.Address.ListOfSitesPerCompany.MapUnique(x => x.Id);
            IEnumerable<DetailedCompanyEmployee> withRateAndShift = employees.Select(x => DetailedCompanyEmployee.FromCompanyEmployee(x, companyShiftsMapped, companyRatesMapped, companySitesMapped));

            return new Response<IEnumerable<DetailedCompanyEmployee>>(withRateAndShift);
        }

        public async Task<Response> GetEmployeesByDepartment(string department, string role, string? companyid = null)
        {
            if (ObjectId.TryParse(companyid, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");
            Company company = await _common.ValidateOwner<Company>(role, companyid);

            department = department.ToLowerInvariant();
            IEnumerable<CompanyEmployee> _employees = await _companyEmployees.Find(x => x.CompanyId == company.Id && x.Department.ToLowerInvariant() == department);
            Dictionary<ObjectId, Shift> companyShiftsMapped = company.Shifts.MapUnique(x => x.Id);
            Dictionary<ObjectId, Rate> companyRatesMapped = company.Rates.MapUnique(x => x.Id);
            Dictionary<ObjectId, Location> companySitesMapped = company.Address.ListOfSitesPerCompany.MapUnique(x => x.Id);
            IEnumerable<DetailedCompanyEmployee> withRateAndShift = _employees.Select(x => DetailedCompanyEmployee.FromCompanyEmployee(x, companyShiftsMapped, companyRatesMapped, companySitesMapped));

            return new Response<IEnumerable<DetailedCompanyEmployee>>(withRateAndShift);
        }

        public async Task<Response> AddEmployee(AddEmployeeModel model, string createdBy, string updatedBy, string role, string? companyid = null)
        {
            bool isNewEmployee = string.IsNullOrWhiteSpace(model.EmployeeId);

            if (ObjectId.TryParse(companyid, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");
            Company company = await _common.ValidateOwner<Company>(role, companyid);

            AddEmployeeModel model_or = model;
            model = model.Sanitize();
            string? departmentExists = company.Departments.Find(x => x.ToLower().Trim() == model.Department.ToLower().Trim());
            string? positionExists = company.Positions.Find(x => x.ToLower().Trim() == model.Position.ToLower().Trim());
            if (departmentExists is null || positionExists is null)
                throw new HttpResponseException($"Department {model.Department} on user {model.Name} do not exist in your organization.");

            if (positionExists is null)
                throw new HttpResponseException($"Position {model.Position} on user {model.Name} do not exist in your organization.");

            if (isNewEmployee)
                return await AddNewEmployee(model_or, createdBy, updatedBy, company.Id.ToString());

            AddExistingEmployeeToNewCompany exstingEmployeeToNewCompany = new();
            exstingEmployeeToNewCompany.Position = model.Position;
            exstingEmployeeToNewCompany.Department = model.Department;
            Response results = await AddExistingEmployeeToNewCompany(exstingEmployeeToNewCompany, company.Id.ToString());
            return results;
        }

        public async Task<Response> UpdateEmployee(string employeeId, UpdateEmployeeRequest model, string updatedBy, string role, string? companyId)
        {

            if (ObjectId.TryParse(companyId, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");
            Company company = await _common.ValidateOwner<Company>(role, companyId);

            EmployeeDetails myemployee = await _employees.FindOne(x => x.EmployeeId.ToLower().Contains(employeeId.ToLower().Trim()));
            CompanyEmployee? employee = await _companyEmployees.FindOne(x => x.EmployeeId.ToLower().Contains(employeeId.ToLower().Trim()) && x.CompanyId == company.Id);

            if (employee is null)
                return new Response(error: "No Employee for provided id!");

            if (!string.IsNullOrWhiteSpace(model.NickName))
                employee.NickName = model.NickName;

            if (!string.IsNullOrWhiteSpace(model.CellNumber))
                employee.CellNumber = model.CellNumber;

            if (!string.IsNullOrWhiteSpace(model.Department))
                employee.Department = model.Department;

            if (!string.IsNullOrWhiteSpace(model.Position))
                employee.Position = model.Position;

            if (model.BankAccountInfo is not null)
                employee.BankAccountInfo = model.BankAccountInfo;
            employee.UpdatedDate = DateTime.UtcNow;
            employee.UpdatedBy = ObjectId.Parse(updatedBy);
            employee.UpdatedBy = ObjectId.Parse(updatedBy);
            ///TODO: to remove this Tempo update
            ///
            if (!string.IsNullOrEmpty(model.IDNumber)) employee.IdNumber = model.IDNumber;
            if (!string.IsNullOrEmpty(model.DateOfEmployment.ToShortDateString())) employee.DateOfEmployment = model.DateOfEmployment;
            if (!string.IsNullOrEmpty(model.TaxNumber)) employee.TaxNumber = model.TaxNumber;
            //if(!string.IsNullOrEmpty(model.DateOfBirth)) employee.DateOfEmploymentDateOfBirth = model.DateOfBirth;
            ////TODO: check if DateOfBirth is available on employee

            var result = await _companyEmployees.Update(employee.Id.ToString(), employee);

            EmployeeDetails employeeDetails = new()
            {
                Id = result.Id,
                EmployeeId = result.EmployeeId,
                Name = result.Name,
                Surname = result.Surname,
                CellNumber = result.CellNumber,
                //DateOfBirth = result.da.DateOfBirth,
                NickName = result.NickName
            };

            if (result.BankAccountInfo is not null)
                employeeDetails.BankAccountInfo = result.BankAccountInfo;

            return new Response<AddNewEmployeeResponseModel>(new AddNewEmployeeResponseModel(employeeId, null).Sanitize(), HttpStatusCode.Accepted);
            //return new Response<EmployeeDetails>(employeeDetails);
        }

        public async Task<Response> ImportEmployees(List<AddEmployeeModel> model, string createdBy, string updatedBy, string role, string? companyId = null)
        {
            Response response = new Response();
            foreach (var employee in model)
            {
                if (employee.EmployeeId is null)
                {
                    response = await AddEmployee(employee, createdBy, updatedBy, role, companyId);
                }
                else
                {
                    UpdateEmployeeRequest updateEmployee = new();
                    updateEmployee.CellNumber = employee.CellNumber;
                    updateEmployee.Department = employee.Department;
                    updateEmployee.Position = employee.Position;
                    updateEmployee.Email = employee.Email;
                    updateEmployee.NickName = employee.NickName;

                    Address address = new()
                    {
                        Street = employee.EmployeeAddress?.Street,
                        Suburb = employee.EmployeeAddress?.Suburb,
                        Province = employee.EmployeeAddress?.Province,
                        PostalCode = employee.EmployeeAddress?.PostalCode,
                        City = employee.EmployeeAddress?.City,
                        ListOfSitesPerCompany = employee.EmployeeAddress?.ListOfSitesPerCompany
                    };

                    BankAccountModel bankAccount = new()
                    {
                        Branch = employee.BankAccountInfo?.Branch,
                        BranchCode = employee.BankAccountInfo?.BranchCode,
                        BankName = employee.BankAccountInfo?.BankName,
                        AccountNumber = employee.BankAccountInfo?.AccountNumber,
                        AccountHolder = employee.BankAccountInfo?.AccountHolder,
                        AccountType = employee.BankAccountInfo?.AccountType
                    };

                    ///TODO: tempo update

                    updateEmployee.TaxNumber = employee.TaxNumber;
                    updateEmployee.IDNumber = employee.IDNumber;
                    updateEmployee.DateOfEmployment = employee.DateOfEmployment;
                    updateEmployee.DateOfBirth = employee.DateOfBirth;
                    //tempo Update

                    response = await UpdateEmployee(employee.EmployeeId, updateEmployee, updatedBy, role, companyId);

                }

                if (response.StatusCode is not HttpStatusCode.Created && response.StatusCode is not HttpStatusCode.Accepted)
                {
                    throw new HttpResponseException(response);
                }
            }
            return await GetEmployees(role, companyId);
        }

        public async Task<Response> ResetEmployeePassword(string employeeId, string updatedBy)
        {
            EmployeeDetails employee = await _employees.FindOne(x => x.EmployeeId.ToLower().Contains(employeeId.Trim()));
            if (employee is null) throw new HttpResponseException("No Employee found");
            string tempPassword = "stellatimes";
            string hash = await _passwordManager.Hash(tempPassword);

            employee.Password = hash;
            employee.UpdatedDate = DateTime.UtcNow;
            employee.UpdatedBy = ObjectId.Parse(updatedBy);

            await _employees.Update(employee.Id.ToString(), employee);
            return new Response(HttpStatusCode.OK, $"Password reset! \r\n Default password is {tempPassword}");
        }

        public async Task<Response> ResetEmployeeDevice(string employeeId, string updatedBy)
        {
            EmployeeDetails employee = await _employees.FindOne(x => x.EmployeeId.ToLower().Contains(employeeId.Trim()));
            if (employee is null)
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@"employee not found"));

            employee.DeviceInfo = null;
            employee.UpdatedBy = ObjectId.Parse(updatedBy);

            await _employees.Update(employee.Id.ToString(), employee);

            return new Response(HttpStatusCode.OK, message: "Reset Sucessfully");
        }

        public async Task<Response> UnArchiveEmployee(string employeeId, string updatedBy, string? companyId = null)
        {
            string formatedEmpId = employeeId.ToLower().Trim();
            EmployeeDetails? employee = await _employees.FindOne(x => x.EmployeeId.ToLower().Contains(formatedEmpId));
            CompanyEmployee? companyEmployee = await _companyEmployees.FindOne(x => x.CompanyId == ObjectId.Parse(companyId) && x.EmployeeId.ToLower().Contains(formatedEmpId));

            if (employee is null || companyEmployee is null)
            {
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: @"Employee not found"));
            }

            employee.DeletedIndicator = false;
            employee.UpdatedBy = ObjectId.Parse(updatedBy);
            employee.UpdatedDate = DateTime.UtcNow;

            companyEmployee.DeletedIndicator = false;
            companyEmployee.UpdatedBy = ObjectId.Parse(updatedBy);
            companyEmployee.UpdatedDate = employee.UpdatedDate;

            // Update employee details asynchronously
            await _employees.Update(employee.Id.ToString(), employee);

            // Update company employee details asynchronously
            await _companyEmployees.Update(companyEmployee.Id.ToString(), companyEmployee);

            return new Response<CompanyEmployee>(companyEmployee, HttpStatusCode.Accepted);
        }

        // adds an employee that is already registered on our system to this company
        public async Task<Response> AddExistingEmployeeToNewCompany(AddExistingEmployeeToNewCompany model, string? companyId = null)
        {
            var companyEmployeeTask = _companyEmployees.FindOne(x
                => x.EmployeeId == model.EmployeeId && x.CompanyId == ObjectId.Parse(companyId));

            var employeeDetailsTask = _employees.FindOne(x => x.EmployeeId == model.EmployeeId);

            await Task.WhenAll(companyEmployeeTask, employeeDetailsTask);

            // this needs to be null for us to add
            CompanyEmployee companyEmployee = await companyEmployeeTask;
            // this must exist for us to continue
            EmployeeDetails employeeDetails = await employeeDetailsTask;

            if (companyEmployee is not null)
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: "Employee already exists!"));

            if (employeeDetails is null)
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Employee not found on our database. You can add a new employee by not providing an 'EmployeeId'."));

            companyEmployee = employeeDetails.CreateCompanyEmployeeFrom(model.Department, model.Position, ObjectId.Parse(companyId));
            await _companyEmployees.Insert(companyEmployee);
            return new Response<CompanyEmployee>(companyEmployee, HttpStatusCode.Created);
        }

        public async Task<Response> SoftDeleteEmployee(string deletedBy, string id, string companyId)
        {
            CompanyEmployee exists = await _companyEmployees.FindOne(x => x.EmployeeId == id && x.CompanyId == ObjectId.Parse(companyId));
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" employee does not exists!"));

            exists.DeletedIndicator = true;
            exists.UpdatedBy = ObjectId.Parse(deletedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            CompanyEmployee memberInfo = await _companyEmployees.Update(exists.Id.ToString(), exists);
            return new Response<CompanyEmployee>(memberInfo, HttpStatusCode.Accepted);
        }

        // adds an employee that is not registered on our system
        private async Task<Response> AddNewEmployee(AddEmployeeModel model, string createdBy, string updatedBy, string companyId)
        {
            // this needs to be null for us to add
            IAsyncEnumerator<string> asyncEnumerator = CreateEmployeeId().GetAsyncEnumerator();

            // this checks for a free employeeId that we can use, if it is true then we have a free id
            string hashedPassword = await _passwordManager.Hash(model.Password);
            bool movedNext = await asyncEnumerator.MoveNextAsync().ConfigureAwait(false);

            if (movedNext is false)
                throw new HttpResponseException(new Response(HttpStatusCode.InternalServerError, error: "Something went wrong adding this user! Try again later."));

            string employeeId = asyncEnumerator.Current;

            EmployeeDetails employeeDetails = new()
            {
                CreatedBy = ObjectId.Parse(createdBy),
                UpdatedBy = ObjectId.Parse(updatedBy),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                DeletedIndicator = false,
                EmployeeId = employeeId,
                Name = model.Name,
                Surname = model.Surname,
                Password = hashedPassword,
                CellNumber = model.CellNumber,
                DateOfBirth = model.DateOfBirth,
                NickName = model.NickName,
                Email = model.Email,
                IDNumber = model.IDNumber,
                TaxNumber = model.TaxNumber,
                DateOfEmployment = model.DateOfEmployment,
                EmployeeAddress = model.EmployeeAddress,
                BankAccountInfo = model.BankAccountInfo
            };
            CompanyEmployee companyEmployee = employeeDetails.CreateCompanyEmployeeFrom(model.Department, model.Position, ObjectId.Parse(companyId));

            await Task.WhenAll(_employees.Insert(employeeDetails), _companyEmployees.Insert(companyEmployee));

            return new Response<AddNewEmployeeResponseModel>(new AddNewEmployeeResponseModel(employeeId, model.Password).Sanitize(), HttpStatusCode.Created);
        }

        private async IAsyncEnumerable<string> CreateEmployeeId()
        {
            string employeeId;
            while (true)
            {
                // generate new id 
                employeeId = await _idGenerator.GetNewAsync();
                EmployeeDetails employeeDetails = await _employees.FindOne(x => x.EmployeeId.ToLower().Contains(employeeId.Trim()));
                if (employeeDetails is null)
                    break;

            }

            yield return employeeId;
        }

        public EmployeesActions(IRepository<EmployeeDetails> employees, ICommon common, IRepository<CompanyEmployee> companyEmployees, IPasswordManager passwordManager, IEmployeeIdGenerator idGenerator)
        {
            _employees = employees;
            _common = common;
            _companyEmployees = companyEmployees;
            _passwordManager = passwordManager;
            _idGenerator = idGenerator;
        }
        private readonly IRepository<EmployeeDetails> _employees;
        private readonly IRepository<CompanyEmployee> _companyEmployees;
        private readonly IPasswordManager _passwordManager;
        private readonly IEmployeeIdGenerator _idGenerator;
        private readonly ICommon _common;
    }
}
