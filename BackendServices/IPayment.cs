using System.Collections.Generic;
using System.Threading.Tasks;
using BackendServices.Models;
using BackendServices.Models.Payments;
using PrePurchase.Models;
using PrePurchase.Models.Payments;

namespace BackendServices
{
    public interface IPayment
    {
        Task<Response<string>> CatureUnPaid(string data);
        Task<Response<PayrollResponse>> ProcessPayroll(PayOutModel records);
        Task<Response<string>> RequestCheckout(PaymentModel model);
        Task<Response<TransactionModel>> RequestPaymentStatus(string id);
    }
}