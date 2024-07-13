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
    public class StockCountsActions : IStockCountsActions
    {
        private readonly IRepository<StockCount> _stockCountRepository;
        private readonly ICommon _common;

        public StockCountsActions(IRepository<StockCount> stockCountRepository, ICommon common)
        {
            _stockCountRepository = stockCountRepository ?? throw new ArgumentNullException(nameof(stockCountRepository));
            _common = common ?? throw new ArgumentNullException(nameof(common));
        }

        public async Task<Response> GetStockCounts(string role, string companyId)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                IEnumerable<StockCount> stockCounts = await _stockCountRepository.Find(u => u.CompanyId == company.Id);
                if (stockCounts == null || !stockCounts.Any())
                    throw new HttpResponseException($"No stockCounts found for {company.CompanyName}");
                return new Response<IEnumerable<StockCount>>(stockCounts);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> AddStockCount(string createdBy, string updatedBy, StockCount stockCount, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                StockCount existingStockCounts = await _stockCountRepository.FindOne(u => u.ProductID == stockCount.ProductID && u.CompanyId == company.Id);
                if (existingStockCounts != null)
                    throw new HttpResponseException($"StockCounts with id '{stockCount.Id}' already exists!");

                stockCount.CreatedBy = ObjectId.Parse(createdBy);
                stockCount.UpdatedBy = ObjectId.Parse(updatedBy);
                stockCount.CreateDate = DateTime.UtcNow;
                stockCount.UpdateDate = DateTime.UtcNow;
                stockCount.DeletedIndicator = false;
                stockCount.CompanyId = company.Id;

                await _stockCountRepository.Insert(stockCount);

                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> UpdateStockCount(string updatedBy, StockCount stockCount, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                StockCount existingStockCount = await _stockCountRepository.FindById(stockCount.Id.ToString());
                if (existingStockCount == null || existingStockCount.CompanyId != company.Id)
                    throw new HttpResponseException("Stock Count not found");

                existingStockCount.CountedQuantity = stockCount.CountedQuantity;
                existingStockCount.ProductID = stockCount.ProductID;
                existingStockCount.CountDate = stockCount.CountDate;
                existingStockCount.CountType = stockCount.CountType;

                existingStockCount.UpdatedBy = ObjectId.Parse(updatedBy);
                existingStockCount.UpdateDate = DateTime.UtcNow;

                await _stockCountRepository.Update(existingStockCount.Id.ToString(), existingStockCount);

                return new Response<StockCount>(existingStockCount);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetStockCount(string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                StockCount stockCount = await _stockCountRepository.FindById(id);
                if (stockCount == null || stockCount.CompanyId != company.Id)
                    throw new HttpResponseException("StockCounts not found");
                return new Response<StockCount>(stockCount);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> SoftDeleteStockCount(string updatedBy, string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                StockCount stockCount = await _stockCountRepository.FindById(id);
                if (stockCount == null || stockCount.CompanyId != company.Id)
                    throw new HttpResponseException("StockCounts not found");

                stockCount.DeletedIndicator = true;
                stockCount.UpdatedBy = ObjectId.Parse(updatedBy);
                stockCount.UpdateDate = DateTime.UtcNow;

                await _stockCountRepository.Update(stockCount.Id.ToString(), stockCount);

                return new Response<StockCount>(stockCount);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

    }
}
