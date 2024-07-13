using BackendServices;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Validators;
using BackendServices.Validators.ValidationData;
using MongoDB.Bson;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Validators
{
    internal class CompanyEmployeeValidator : IValidator<CompanyEmployeeValidationData, CompanyEmployeeValidationResult>
    {
        private readonly IValidator<DistanceValidationData, DistanceValidationResult> requiredValidator;
        private readonly IRepository<CompanyEmployee> companyEmployees;
        private readonly ITimeSummaryRepository timeSummaries;

        public CompanyEmployeeValidator(IValidator<DistanceValidationData,
            DistanceValidationResult> requiredValidator,
            IRepository<CompanyEmployee> companyEmployees,
            ITimeSummaryRepository timeSummaries)
        {
            this.requiredValidator = requiredValidator;
            this.companyEmployees = companyEmployees;
            this.timeSummaries = timeSummaries;
        }

        public async ValueTask<CompanyEmployeeValidationResult> Validate(CompanyEmployeeValidationData value)
        {
            IValidationData validationData = value;
            ShiftValidationData shiftValidationData = validationData.GetValidationDataOfType<ShiftValidationData>();

            DistanceValidationData distanceValidationData = new(value, shiftValidationData.ClockInData.EmployeePosition, shiftValidationData.DistanceTolerance);
            DistanceValidationResult distanceValidationResult = await requiredValidator.Validate(distanceValidationData);

            IValidationResult previousValidationResult = distanceValidationResult;
            QrCodeValidationResult qrcodeValidationResult = previousValidationResult.GetValidationResultOfType<QrCodeValidationResult>();
            Company company = qrcodeValidationResult.Company;

            string employeeId = shiftValidationData.EmployeeId.ToLowerInvariant();
            string employeeDetailsId = shiftValidationData.EmployeeDetailsId;
            // get the company employee that is clocking in
            Task<CompanyEmployee> employeeTask = companyEmployees.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLowerInvariant() == employeeId);

            // get the time summary that is recording this employee's data
            Task<TimeSummary> timeSummaryTask = timeSummaries.TimeSummaryByEmployeeDetailsAndCompanyId(employeeDetailsId, company.Id.ToString());

            List<Task> tasks = new()
            {
                employeeTask,
                timeSummaryTask
            };
            Task.WaitAll(tasks.ToArray());

            CompanyEmployee? employee = await employeeTask;
            TimeSummary timeSummary = await timeSummaryTask;

            if (employee is null)
                throw new HttpResponseException(new Response(error: $"You do not work at {company.CompanyName}. Check the company QrCode and try again."));

            timeSummary ??= new()
            {
                CreatedBy = ObjectId.Parse(employeeDetailsId),
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = ObjectId.Parse(employeeDetailsId),
                UpdatedDate = DateTime.UtcNow,
                EmployeeDetailsId = ObjectId.Parse(employeeDetailsId),
                EmployeeId = employee.EmployeeId,
                CompanyId = company.Id,
                DeletedIndicator = false,
            };

            return new CompanyEmployeeValidationResult(previousValidationResult, value, employee, timeSummary);
        }
    }
}
