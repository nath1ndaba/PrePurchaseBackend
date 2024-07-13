using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using BackendServices;
using BackendServices.Actions;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Models.Payments;
using CsvHelper;
using Infrastructure.Models;
using Microsoft.Extensions.Logging;
using PrePurchase.Models;
using PrePurchase.Models.Payments;
using static MongoDB.Driver.WriteConcern;

namespace Infrastructure.Actions
{
    public class PeachPayment : IPayment
    {
        public readonly IHttpClientFactory _httpClientFactory;
        public ILogger<PeachPayment> _loger;
        public PeachPayment(IHttpClientFactory httpClientFactory, ILogger<PeachPayment> logger)
        {
            this._httpClientFactory = httpClientFactory;
            this._loger = logger;
        }

        public Task<Response<string>> CatureUnPaid(string data)
        {
            var xmlSerializer = new XmlSerializer(typeof(PeachPayrollResponse));

            using (var sr = new StringReader(data)){
                var obj = (PeachPayrollResponse)xmlSerializer.Deserialize(sr);
                _loger.LogInformation(JsonSerializer.Serialize(obj));
            }

            return new(null);
        }

        public async Task<Response<PayrollResponse>> ProcessPayroll(PayOutModel data)
        {
            PeachHeaderModel header = new(){
                ApiVersion = "2.0.1",
                Client = "PEA001",
                Service = "Creditors", //data.Service.ToString(),
                ServiceType = "SDV", //data.ServiceType == ServiceType.OneDayOnly ? "1Day" : data.ServiceType.ToString(),
                DueDate = GetDueDate(data.ServiceType),
                BankAccount = "Nedbank",
                CallBackUrl = "http://example.com/API/CallBack" //TO-DO: Capture the return request if of users that did not work
            };
            var xml = new PeachPayoutRequest(data, header);

            _loger.LogInformation($"xml: {xml}");

            var client = _httpClientFactory.CreateClient();

            var url = $"{PrePurchaseConfig.PAYMENT_PAYROLL_URL}?key={PrePurchaseConfig.PAYMENT_PAYROLL_APIKEY}";
            HttpRequestMessage request = new(HttpMethod.Post, url);

            var content = new MultipartFormDataContent();

            var stringContent = new StringContent(xml.ToString());
            content.Add(stringContent, "request");

            request.Content = content;

            _loger.LogInformation("content: {@content}", content);
            var response = await client.SendAsync(request);
            var xmlResponse = await response.Content.ReadAsStringAsync();
            _loger.LogInformation("xmlResponse: " + xmlResponse);
            var xmlSerializer = new XmlSerializer(typeof(PeachPayrollResponse));

            PayrollResponse? resData =  default;
            using (var sr = new StringReader(xmlResponse)){
                var obj = (PeachPayrollResponse?)xmlSerializer.Deserialize(sr);

                if (obj.Result == "Error")
                    return new(null,HttpStatusCode.BadRequest, error: obj.ResultMessage);

                _loger.LogInformation("obj: " + JsonSerializer.Serialize(obj));
                if(obj is PeachPayrollResponse _data){
                    resData = new() {
                        Message = _data.ResultMessage,
                        BatchCode = _data.BatchCode,
                        BatchPayoutFee = _data.PayoutFee,
                        PaidAmount = _data.PaidAmount,
                        UnPaids = _data.CDVResults
                        .Where(x => "Valid".EqualCaseInsesitive(x.Result.Result) is false)
                        .Select(x => new UnPaid() {
                            Reason = x.Result.Message,
                            AccountNumber = x.Result.AccountNumber
                        }).ToList()
                    };
                }
                
                
            }
            _loger.LogInformation("xmlSerializer: {@xmlSerializer}", JsonSerializer.Serialize(xmlSerializer));
            _loger.LogInformation("response: " + JsonSerializer.Serialize(resData));

            return new(resData);
        }

        public async Task<Response<string>> RequestCheckout(PaymentModel model)
        {
            Dictionary<string,string> formData = new();

            formData.Add("entityId", PrePurchaseConfig.PAYMENT_3D_SECURE_ID);
            formData.Add("amount", model.Amount.ToString());
            formData.Add("currency", model.Currency);
            formData.Add("paymentType", "DB");
            
            FormUrlEncodedContent content = new(formData);

            var client = _httpClientFactory.CreateClient();

            HttpRequestMessage request = new(HttpMethod.Post, PrePurchaseConfig.PAYMENT_CHECKOUT_URL);
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",PrePurchaseConfig.PAYMENT_AUTHORIZATION);

            var response = await client.SendAsync(request);
            
            PeachPaymentResponse? _data = default;

            if(response.IsSuccessStatusCode){
                var stream = await response.Content.ReadAsStreamAsync();

                _data = await stream.ToJson<PeachPaymentResponse>();
            }

            if(string.IsNullOrWhiteSpace(_data?.Id))
                throw new HttpResponseException(new Response(response.StatusCode, error: "Something went wrong, try again later?"));


            return new(_data?.Id, HttpStatusCode.OK, _data?.Result.Description);
        }

        public async Task<Response<TransactionModel>> RequestPaymentStatus(string id)
        {
            string url = $"{PrePurchaseConfig.PAYMENT_CHECKOUT_URL}/{id}/payment?entityId={PrePurchaseConfig.PAYMENT_3D_SECURE_ID}";

            var client = _httpClientFactory.CreateClient();

            HttpRequestMessage request = new(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",PrePurchaseConfig.PAYMENT_AUTHORIZATION);

            var response = await client.SendAsync(request);
            
            PeachPaymentResponse? _data = default;
            TransactionModel? _transaction = null;

            var stream = await response.Content.ReadAsStreamAsync();

            _data = await stream.ToJson<PeachPaymentResponse>();

            _loger.LogInformation("Peach response: {@_data}", _data);

            decimal.TryParse(_data.Amount, out decimal amount);
            if(response.IsSuccessStatusCode && amount > 0){
                Enum.TryParse(_data.Currency, out Currency currency);
                _transaction = new(
                    _data.Id,
                    amount, currency,
                    _data.PaymentType,
                    PaymentMethod.Deposit, _data.Timestamp.ToUniversalTime());
            
            }else
                return new(null,response.StatusCode,error: _data?.Result.Description);

            _loger.LogInformation("Transaction: {_transaction}", _transaction);

            return new(_transaction,message: _data?.Result.Description);
        }

        public string GetDueDate(ServiceType serviceType)
        {
            DateTime date = DateTime.UtcNow;

            if(serviceType == ServiceType.OneDayOnly)
                date = DateTime.UtcNow.AddDays(1);
            else
                date = DateTime.UtcNow.AddHours(4);
            return date.ToString("yyyyMMdd");
        }
    }
}