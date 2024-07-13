using BackendServices.Models;
using PrePurchase.Models;
using PrePurchase.Models.HistoryModels;
using PrePurchase.Models.LeaveDays;
using PrePurchase.Models.Requests;
using PrePurchase.Models.StatementsModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendServices.Actions
{
    /// <summary>
    /// This interface defines the contract of the actions a company is able to perform on our system
    /// </summary>
#nullable enable
    public interface IAllActions
    {
        Task<Response> GetDeatiledAds(string role, string companyId = null);

        Task<Response> UploadAds(DetailedAd model, string createdBy, string updatedBy, string role, string companyId = null);

        Task<Response> GetCustomization(string role, string companyId = null);

        Task<Response> Customization(string role, Customization customization, string companyId = null);

        Task<Response> AddEmployeeToRosta(AddEmployeeToRostaModel model, string role, string companyId = null);

        Task<Response> AddEmployeesToRosta(List<AddEmployeeToRostaModel> model, string role, string companyId = null);

        Task<Response> RemoveEmployeeFromRosta(RemoveEmployeeFromRostaModel model, string role, string companyId = null);

        Task<Response> UpdateRate(string id, RateModel rate, string role, string companyId = null);

        Task<Response> GetLeaves(QueryLeaveModel model, string role);

        Task<Response> UpdateLeave(string id, QueryLeaveModel model, string role, string companyId = null);

        Task<Response> GetLeaveStore(string role, string companyId = null);

        Task<Response> UpdateLeaveStore(List<UpdateLeaveStore> leave, string role, string companyId = null);

        Task<Response> AddSupplier(string role, Supplier supplier, string companyId = null);

        //Task<Response> UpdateSupplier(string role, Supplier supplier, string companyId = null);

        Task<Response> GetSuppliers(string role, string companyId = null);

        Task<Response> GetSuppliersByPaymentMethod(string paymentMethod, string role, string companyId = null);

        Task<Response> AddPaymentMethod(string paymentMethod, string role, string companyId = null);

        Task<Response> RemovePaymentMethod(string deparment, string role, string companyId = null);

        Task<Response> AddSupplierInvoices(string role, SupplierInvoices invoice, string companyId = null);

        Task<Response> GetSuppliersInvoices(string role, string companyId = null, string? supplierId = null);

    }

#nullable disable
}
