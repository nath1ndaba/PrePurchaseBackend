using BackendServices;
using BackendServices.Actions.Inventory;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Models.Inventory;
using Infrastructure.Helpers;
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

        public async Task<Response> GetProducts(string shopId)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(shopId);
                IEnumerable<Product> products = await _productRepository.Find(u => u.ShopId == shop.Id);
                if (products == null || !products.Any())
                    throw new HttpResponseException($"No products found for {shop.Name}");
                List<ProductDto> dtos = new List<ProductDto>();
                foreach (var product in products)
                {
                    ProductDto productDto = new ProductDto();
                    productDto.DtoFromProduct(product);
                    dtos.Add(productDto);
                }

                return new Response<IEnumerable<ProductDto>>(dtos, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }
        public async Task<Response> GetProductsForCategory(string categoryId, string shopId)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(shopId);
                IEnumerable<Product> products = await _productRepository.Find(x => x.ShopId == shop.Id && x.CategoryID == ObjectId.Parse(categoryId));
                if (products == null || !products.Any())
                    throw new HttpResponseException($"No products found for {shop.Name}");
                return new Response<IEnumerable<Product>>(products, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> AddProduct(string createdBy, string updatedBy, ProductDto model, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(shopId);
                Product existingProducts = await _productRepository.FindOne(u => u.Name == model.Name && u.ShopId == shop.Id);
                if (existingProducts != null)
                    throw new HttpResponseException($"Products with productname '{model.Name}' already exists!");

                model.CreatedBy = createdBy;
                model.UpdatedBy = updatedBy;
                model.CreateDate = DateTime.UtcNow;
                model.UpdateDate = DateTime.UtcNow;
                model.DeletedIndicator = false;
                model.ShopId = shop.Id.ToString();

                Product product = new Product();
                product.DtoToProduct(model);

                await _productRepository.Insert(product);

                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> UpdateProduct(string updatedBy, Product product, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(shopId);
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

        public async Task<Response> GetProduct(string id, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(shopId);
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

        public async Task<Response> SoftDeleteProduct(string updatedBy, string id, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateOwner<Shop>(shopId);
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
