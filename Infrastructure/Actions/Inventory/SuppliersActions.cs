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
    public class SuppliersActions : ISuppliersActions
    {
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly ICommon _common;

        public SuppliersActions(IRepository<Supplier> supplierRepository, ICommon common)
        {
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
            _common = common ?? throw new ArgumentNullException(nameof(common));
        }

        public async Task<Response> GetSuppliers(string role, string companyId)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                IEnumerable<Supplier> suppliers = await _supplierRepository.Find(u => u.CompanyId == company.Id);
                if (suppliers == null || !suppliers.Any())
                    throw new HttpResponseException($"No suppliers found for {company.CompanyName}");
                return new Response<IEnumerable<Supplier>>(suppliers);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> AddSupplier(string createdBy, string updatedBy, Supplier supplier, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                Supplier existingSuppliers = await _supplierRepository.FindOne(u => u.SupplierName == supplier.SupplierName && u.CompanyId == company.Id);
                if (existingSuppliers != null)
                    throw new HttpResponseException($"Suppliers with suppliername '{supplier.SupplierName}' already exists!");

                supplier.CreatedBy = ObjectId.Parse(createdBy);
                supplier.UpdatedBy = ObjectId.Parse(updatedBy);
                supplier.CreateDate = DateTime.UtcNow;
                supplier.UpdateDate = DateTime.UtcNow;
                supplier.DeletedIndicator = false;
                supplier.CompanyId = company.Id;

                await _supplierRepository.Insert(supplier);

                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> UpdateSupplier(string updatedBy, Supplier supplier, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                Supplier existingSupplier = await _supplierRepository.FindById(supplier.Id.ToString());
                if (existingSupplier == null || existingSupplier.CompanyId != company.Id)
                    throw new HttpResponseException("Suppliers not found");

                existingSupplier.SupplierName = supplier.SupplierName;
                existingSupplier.ContactName = supplier.ContactName;
                existingSupplier.Email = supplier.Email;
                existingSupplier.Phone = supplier.Phone;
                existingSupplier.Address = supplier.Address;

                existingSupplier.UpdatedBy = ObjectId.Parse(updatedBy);
                existingSupplier.UpdateDate = DateTime.UtcNow;

                await _supplierRepository.Update(existingSupplier.Id.ToString(), existingSupplier);

                return new Response<Supplier>(existingSupplier);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetSupplier(string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                Supplier supplier = await _supplierRepository.FindById(id);
                if (supplier == null || supplier.CompanyId != company.Id)
                    throw new HttpResponseException("Suppliers not found");
                return new Response<Supplier>(supplier);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> SoftDeleteSupplier(string updatedBy, string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                Supplier supplier = await _supplierRepository.FindById(id);
                if (supplier == null || supplier.CompanyId != company.Id)
                    throw new HttpResponseException("Suppliers not found");

                supplier.DeletedIndicator = true;
                supplier.UpdatedBy = ObjectId.Parse(updatedBy);
                supplier.UpdateDate = DateTime.UtcNow;

                await _supplierRepository.Update(supplier.Id.ToString(), supplier);

                return new Response<Supplier>(supplier);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

    }
}
