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
    public class ProductsActions : IProductsActions
    {
        private readonly IRepository<Product> _userRepository;
        private readonly ICommon _common;

        public ProductsActions(IRepository<Product> userRepository, ICommon common)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _common = common ?? throw new ArgumentNullException(nameof(common));
        }

        public async Task<Response> GetProducts(string role, string companyId)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                IEnumerable<Product> users = await _userRepository.Find(u => u.CompanyId == company.Id);
                if (users == null || !users.Any())
                    throw new HttpResponseException($"No users found for {company.CompanyName}");
                return new Response<IEnumerable<Product>>(users);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> AddProduct(string createdBy, string updatedBy, Product user, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                Product existingProducts = await _userRepository.FindOne(u => u.Name == user.Name && u.CompanyId == company.Id);
                if (existingProducts != null)
                    throw new HttpResponseException($"Products with username '{user.Name}' already exists!");

                user.CreatedBy = ObjectId.Parse(createdBy);
                user.UpdatedBy = ObjectId.Parse(updatedBy);
                user.CreateDate = DateTime.UtcNow;
                user.UpdateDate = DateTime.UtcNow;
                user.DeletedIndicator = false;
                user.CompanyId = company.Id;

                await _userRepository.Insert(user);

                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> UpdateProduct(string updatedBy, Product user, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                Product existingProduct = await _userRepository.FindById(user.Id.ToString());
                if (existingProduct == null || existingProduct.CompanyId != company.Id)
                    throw new HttpResponseException("Products not found");

                existingProduct.Name = user.Name;
                existingProduct.Description = user.Description;
                existingProduct.Price = user.Price;
                existingProduct.Barcode = user.Barcode;
                existingProduct.CategoryID = user.CategoryID;
                existingProduct.SupplierID = user.SupplierID;
                existingProduct.StockQuantity = user.StockQuantity;
                existingProduct.ReorderLevel = user.ReorderLevel;
                existingProduct.ReorderQuantity = user.ReorderQuantity;
                existingProduct.BulkQuantity = user.BulkQuantity;
                existingProduct.BulkUnit = user.BulkUnit;

                existingProduct.UpdatedBy = ObjectId.Parse(updatedBy);
                existingProduct.UpdateDate = DateTime.UtcNow;

                await _userRepository.Update(existingProduct.Id.ToString(), existingProduct);

                return new Response<Product>(existingProduct);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetProduct(string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                Product user = await _userRepository.FindById(id);
                if (user == null || user.CompanyId != company.Id)
                    throw new HttpResponseException("Products not found");
                return new Response<Product>(user);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> SoftDeleteProduct(string updatedBy, string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                Product user = await _userRepository.FindById(id);
                if (user == null || user.CompanyId != company.Id)
                    throw new HttpResponseException("Products not found");

                user.DeletedIndicator = true;
                user.UpdatedBy = ObjectId.Parse(updatedBy);
                user.UpdateDate = DateTime.UtcNow;

                await _userRepository.Update(user.Id.ToString(), user);

                return new Response<Product>(user);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

    }
}
