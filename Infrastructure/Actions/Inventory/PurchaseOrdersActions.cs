using BackendServices;
using BackendServices.Actions.Inventory;
using BackendServices.Exceptions;
using BackendServices.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PurchaseOrdersActions : IPurchaseOrdersActions
    {
        private readonly IRepository<PurchaseOrder> _purchaseOrdersRepository;
        private readonly ICommon _common;

        public PurchaseOrdersActions(IRepository<PurchaseOrder> purchaseOrdersRepository, ICommon common)
        {
            _purchaseOrdersRepository = purchaseOrdersRepository ?? throw new ArgumentNullException(nameof(purchaseOrdersRepository));
            _common = common ?? throw new ArgumentNullException(nameof(common));
        }

        public async Task<Response> GetPurchaseOrders(string role, string companyId)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                IEnumerable<PurchaseOrder> purchaseOrderss = await _purchaseOrdersRepository.Find(u => u.CompanyId == company.Id);
                if (purchaseOrderss == null || !purchaseOrderss.Any())
                    throw new HttpResponseException($"No purchaseOrderss found for {company.CompanyName}");
                return new Response<IEnumerable<PurchaseOrder>>(purchaseOrderss);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> AddPurchaseOrder(string createdBy, string updatedBy, PurchaseOrder purchaseOrders, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                PurchaseOrder existingPurchaseOrders = await _purchaseOrdersRepository.FindOne(u => u.PurchaseOrderNumber == purchaseOrders.PurchaseOrderNumber && u.CompanyId == company.Id);
                if (existingPurchaseOrders != null)
                    throw new HttpResponseException($"PurchaseOrders with id '{purchaseOrders.Id}' already exists!");

                purchaseOrders.CreatedBy = ObjectId.Parse(createdBy);
                purchaseOrders.UpdatedBy = ObjectId.Parse(updatedBy);
                purchaseOrders.CreateDate = DateTime.UtcNow;
                purchaseOrders.UpdateDate = DateTime.UtcNow;
                purchaseOrders.DeletedIndicator = false;
                purchaseOrders.CompanyId = company.Id;

                await _purchaseOrdersRepository.Insert(purchaseOrders);

                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> UpdatePurchaseOrder(string updatedBy, PurchaseOrder purchaseOrders, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                PurchaseOrder existingPurchaseOrder = await _purchaseOrdersRepository.FindById(purchaseOrders.Id.ToString());
                if (existingPurchaseOrder == null || existingPurchaseOrder.CompanyId != company.Id)
                    throw new HttpResponseException("PurchaseOrders not found");

                existingPurchaseOrder.SupplierID = purchaseOrders.SupplierID;
                existingPurchaseOrder.PurchaseOrderNumber = purchaseOrders.PurchaseOrderNumber;
                existingPurchaseOrder.DeliveryDate = purchaseOrders.DeliveryDate;

                existingPurchaseOrder.UpdatedBy = ObjectId.Parse(updatedBy);
                existingPurchaseOrder.UpdateDate = DateTime.UtcNow;

                await _purchaseOrdersRepository.Update(existingPurchaseOrder.Id.ToString(), existingPurchaseOrder);

                return new Response<PurchaseOrder>(existingPurchaseOrder);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetPurchaseOrder(string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                PurchaseOrder purchaseOrders = await _purchaseOrdersRepository.FindById(id);
                if (purchaseOrders == null || purchaseOrders.CompanyId != company.Id)
                    throw new HttpResponseException("PurchaseOrder not found");
                return new Response<PurchaseOrder>(purchaseOrders);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> SoftDeletePurchaseOrder(string updatedBy, string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                PurchaseOrder purchaseOrders = await _purchaseOrdersRepository.FindById(id);
                if (purchaseOrders == null || purchaseOrders.CompanyId != company.Id)
                    throw new HttpResponseException("PurchaseOrders not found");

                purchaseOrders.DeletedIndicator = true;
                purchaseOrders.UpdatedBy = ObjectId.Parse(updatedBy);
                purchaseOrders.UpdateDate = DateTime.UtcNow;

                await _purchaseOrdersRepository.Update(purchaseOrders.Id.ToString(), purchaseOrders);

                return new Response<PurchaseOrder>(purchaseOrders);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

    }
}
