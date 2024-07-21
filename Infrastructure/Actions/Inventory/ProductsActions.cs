using BackendServices;
using BackendServices.Actions.Inventory;
using BackendServices.Exceptions;
using BackendServices.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models;
using PrePurchase.Models.Inventory;
using PrePurchase.Models.PrePurchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ProductsActions : IProductsActions
    {
        private readonly IRepository<Product> _productRepository;
        private readonly ICommon _common;

        public ProductsActions(IRepository<Product> productRepository, ICommon common)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _common = common ?? throw new ArgumentNullException(nameof(common));
        }

        public async Task<Response> GetProducts(string role, string shopId)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                IEnumerable<Product> products = await _productRepository.Find(u => u.ShopId == shop.Id);
                if (products == null || !products.Any())
                    throw new HttpResponseException($"No products found for {shop.Name}");
                return new Response<IEnumerable<Product>>(products, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> AddProduct(string createdBy, string updatedBy, Product product, string role, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                Product existingProducts = await _productRepository.FindOne(u => u.Name == product.Name && u.ShopId == shop.Id);
                if (existingProducts != null)
                    throw new HttpResponseException($"Products with productname '{product.Name}' already exists!");

                product.CreatedBy = ObjectId.Parse(createdBy);
                product.UpdatedBy = ObjectId.Parse(updatedBy);
                product.CreateDate = DateTime.UtcNow;
                product.UpdateDate = DateTime.UtcNow;
                product.DeletedIndicator = false;
                product.ShopId = shop.Id;

                await _productRepository.Insert(product);

                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> UpdateProduct(string updatedBy, Product product, string role, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                Product existingProduct = await _productRepository.FindById(product.Id.ToString());
                if (existingProduct == null || existingProduct.ShopId != shop.Id)
                    throw new HttpResponseException("Products not found");

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Barcode = product.Barcode;
                existingProduct.CategoryID = product.CategoryID;
                existingProduct.SupplierID = product.SupplierID;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.ReorderLevel = product.ReorderLevel;
                existingProduct.ReorderQuantity = product.ReorderQuantity;
                existingProduct.BulkQuantity = product.BulkQuantity;
                existingProduct.BulkUnit = product.BulkUnit;

                existingProduct.UpdatedBy = ObjectId.Parse(updatedBy);
                existingProduct.UpdateDate = DateTime.UtcNow;

                await _productRepository.Update(existingProduct.Id.ToString(), existingProduct);

                return new Response<Product>(existingProduct);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetProduct(string id, string role, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                Product product = await _productRepository.FindById(id);
                if (product == null || product.ShopId != shop.Id)
                    throw new HttpResponseException("Products not found");
                return new Response<Product>(product);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> SoftDeleteProduct(string updatedBy, string id, string role, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                Product product = await _productRepository.FindById(id);
                if (product == null || product.ShopId != shop.Id)
                    throw new HttpResponseException("Products not found");

                product.DeletedIndicator = true;
                product.UpdatedBy = ObjectId.Parse(updatedBy);
                product.UpdateDate = DateTime.UtcNow;

                await _productRepository.Update(product.Id.ToString(), product);

                return new Response<Product>(product);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

    }
}
