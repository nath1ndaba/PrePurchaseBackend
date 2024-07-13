using BackendServices.Exceptions;
using BackendServices.Validators;
using BackendServices.Validators.ValidationData;
using Infrastructure.Helpers;
using MongoDB.Driver.Linq;
using PrePurchase.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Validators
{
    internal class DistanceValidator : IValidator<DistanceValidationData, DistanceValidationResult>
    {
        private readonly IValidator<QrCodeValidationData, QrCodeValidationResult> requiredValidator;

        public DistanceValidator(IValidator<QrCodeValidationData, QrCodeValidationResult> requiredValidator)
        {
            this.requiredValidator = requiredValidator;
        }

        public async ValueTask<DistanceValidationResult> Validate(DistanceValidationData value)
        {
            IValidationData validationData = value;
            var shiftValidationData = validationData.GetValidationDataOfType<ShiftValidationData>();
            QrCodeValidationData qrcodeValidationData = new(value, shiftValidationData.ClockInData);

            QrCodeValidationResult previousValidationResult = await requiredValidator.Validate(qrcodeValidationData);

            List<Location> companySites = previousValidationResult.Company.Address.ListOfSitesPerCompany;

            foreach (Location companySite in companySites)
            {
                if (ModelHelpers.WithInRadius(value.EmployeePosition, companySite, value.Tolerance, out var distance))
                {
                    return new DistanceValidationResult(previousValidationResult, value, distance);
                }
            }
            throw new HttpResponseException(error: $"Try getting closer. You need to be within {value.Tolerance:f2} meters of your work location to clock in!");

        }
    }
}
