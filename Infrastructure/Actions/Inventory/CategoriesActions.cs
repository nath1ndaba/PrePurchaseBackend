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
    public class CategoriesActions : ICategoriesActions
    {
        private readonly IRepository<Category> _userRepository;
        private readonly ICommon _common;

        public CategoriesActions(IRepository<Category> userRepository, ICommon common)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _common = common ?? throw new ArgumentNullException(nameof(common));
        }

        public async Task<Response> GetCategories(string role, string companyId)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, companyId);
                IEnumerable<Category> users = await _userRepository.Find(u => u.ShopId == shop.Id);
                if (users == null || !users.Any())
                    throw new HttpResponseException($"No users found for {shop.Name}");
                return new Response<IEnumerable<Category>>(users);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> AddCategory(string createdBy, string updatedBy, Category user, string role, string? companyId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, companyId);
                Category existingCategories = await _userRepository.FindOne(u => u.CategoryName == user.CategoryName && u.ShopId == shop.Id);
                if (existingCategories != null)
                    throw new HttpResponseException($"Categories with username '{user.CategoryName}' already exists!");

                user.CreatedBy = ObjectId.Parse(createdBy);
                user.UpdatedBy = ObjectId.Parse(updatedBy);
                user.CreateDate = DateTime.UtcNow;
                user.UpdateDate = DateTime.UtcNow;
                user.DeletedIndicator = false;
                user.ShopId = shop.Id;

                await _userRepository.Insert(user);

                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> UpdateCategory(string updatedBy, Category user, string role, string? companyId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, companyId);
                Category existingCategory = await _userRepository.FindById(user.Id.ToString());
                if (existingCategory == null || existingCategory.ShopId != shop.Id)
                    throw new HttpResponseException("Categories not found");

                existingCategory.CategoryName = user.CategoryName;

                existingCategory.UpdatedBy = ObjectId.Parse(updatedBy);
                existingCategory.UpdateDate = DateTime.UtcNow;

                await _userRepository.Update(existingCategory.Id.ToString(), existingCategory);

                return new Response<Category>(existingCategory);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetCategory(string id, string role, string? companyId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, companyId);
                Category user = await _userRepository.FindById(id);
                if (user == null || user.ShopId != shop.Id)
                    throw new HttpResponseException("Categories not found");
                return new Response<Category>(user);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> SoftDeleteCategory(string updatedBy, string id, string role, string? companyId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, companyId);
                Category user = await _userRepository.FindById(id);
                if (user == null || user.ShopId != shop.Id)
                    throw new HttpResponseException("Categories not found");

                user.DeletedIndicator = true;
                user.UpdatedBy = ObjectId.Parse(updatedBy);
                user.UpdateDate = DateTime.UtcNow;

                await _userRepository.Update(user.Id.ToString(), user);

                return new Response<Category>(user);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

    }
}
