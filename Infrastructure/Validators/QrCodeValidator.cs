using BackendServices;
using BackendServices.Exceptions;
using BackendServices.Validators;
using BackendServices.Validators.ValidationData;
using PrePurchase.Models;
using System.Threading.Tasks;

namespace Infrastructure.Validators
{
    internal class QrCodeValidator : IValidator<QrCodeValidationData, QrCodeValidationResult>
    {
        readonly IRepository<Company> companies;

        public QrCodeValidator(IRepository<Company> companies)
        {
            this.companies = companies;
        }

        public async ValueTask<QrCodeValidationResult> Validate(QrCodeValidationData value)
        {
            if (IDateTimeProvider.TryParseTimeZoneId(value.ClockInAndOutData.TimeZoneId, out var zone) is false)
                throw new HttpResponseException(error: "Something is wrong with your request, use the supported tools to clock in!");

            //if (value.ClockInAndOutData.IsValidQrCode is false)
            //    throw new HttpResponseException(error: "Please use a valid company qr-code!");

            var company = await companies.FindById(value.ClockInAndOutData.QRCode);

            if (company is null)
                throw new HttpResponseException(error: "Invalid Qr-code!!");

            return new QrCodeValidationResult(value, company, zone);
        }
    }
}
