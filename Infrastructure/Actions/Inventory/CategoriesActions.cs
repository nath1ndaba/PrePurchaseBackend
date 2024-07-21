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
        private readonly IRepository<Category> _categoryRepository;
        private readonly ICommon _common;

        public CategoriesActions(IRepository<Category> categoryRepository, ICommon common)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _common = common ?? throw new ArgumentNullException(nameof(common));
        }

        public async Task<Response> GetCategories(string role, string shopId)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                IEnumerable<Category> categories = await _categoryRepository.Find(u => u.ShopId == shop.Id);
                if (categories == null || !categories.Any())
                    throw new HttpResponseException($"No categories found for {shop.Name}");
                return new Response<IEnumerable<Category>>(categories);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> AddCategory(string createdBy, string updatedBy, Category category, string role, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                Category existingCategory = await _categoryRepository.FindOne(u => u.CategoryName == category.CategoryName && u.ShopId == shop.Id);
                if (existingCategory != null)
                {
                    throw new HttpResponseException($"Category with name '{category.CategoryName}' already exists!");
                }

                category.CreatedBy = ObjectId.Parse(createdBy);
                category.UpdatedBy = ObjectId.Parse(updatedBy);
                category.CreateDate = DateTime.UtcNow;
                category.UpdateDate = DateTime.UtcNow;
                category.DeletedIndicator = false;
                category.ShopId = shop.Id;

                if (category.Level == 0)
                {
                    category.ParentCategoryId = ObjectId.Empty; // Ensure no parent for level 0
                }
                else
                {
                    if (category.ParentCategoryId == ObjectId.Empty)
                        throw new HttpResponseException("ParentCategoryId is required for non-top-level categories");

                    var parentCategory = await _categoryRepository.FindById(category.ParentCategoryId.ToString());
                    if (parentCategory == null)
                        throw new HttpResponseException("Parent category not found");

                    parentCategory.SubcategoriesIds.Add(category.Id);
                    await _categoryRepository.Update(parentCategory.Id.ToString(), parentCategory);
                }

                await _categoryRepository.Insert(category);

                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> UpdateCategory(string updatedBy, Category category, string role, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                Category existingCategory = await _categoryRepository.FindById(category.Id.ToString());
                if (existingCategory == null || existingCategory.ShopId != shop.Id)
                    throw new HttpResponseException("Category not found");

                existingCategory.CategoryName = category.CategoryName;
                existingCategory.UpdatedBy = ObjectId.Parse(updatedBy);
                existingCategory.UpdateDate = DateTime.UtcNow;

                await _categoryRepository.Update(existingCategory.Id.ToString(), existingCategory);

                return new Response<Category>(existingCategory);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetCategory(string id, string role, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                Category category = await _categoryRepository.FindById(id);
                if (category == null || category.ShopId != shop.Id)
                    throw new HttpResponseException("Category not found");
                return new Response<Category>(category);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> SoftDeleteCategory(string updatedBy, string id, string role, string? shopId = null)
        {
            try
            {
                Shop shop = await _common.ValidateCompany<Shop>(role, shopId);
                Category category = await _categoryRepository.FindById(id);
                if (category == null || category.ShopId != shop.Id)
                    throw new HttpResponseException("Category not found");

                category.DeletedIndicator = true;
                category.UpdatedBy = ObjectId.Parse(updatedBy);
                category.UpdateDate = DateTime.UtcNow;

                await _categoryRepository.Update(category.Id.ToString(), category);

                return new Response<Category>(category);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }
    }
}
