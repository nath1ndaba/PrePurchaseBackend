using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.Models;
using MongoDB.Bson;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin
{
    public class DepartmentsnewActions : IDepartmentsNewActions
    {
        public async Task<Response> GetDepartments(string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateOwner<Company>(role, companyId);

                IEnumerable<CompanyDepartments> departments = await _companyDepartments.Find(d => d.CompanyId == company.Id);
                if (departments is null) throw new HttpResponseException($"No Departments for at {company.CompanyName}");
                return new Response<IEnumerable<CompanyDepartments>>(departments);
            }
            catch { throw; }
            finally { }
        }

        public async Task<Response> AddDepartments(string createdBy, string updatedBy, CompanyDepartments model, string role, string? companyid = null)
        {

            if (ObjectId.TryParse(companyid, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");
            Company company = await _common.ValidateOwner<Company>(role, companyid);


            CompanyDepartments? exists = await _companyDepartments.FindOne(x => x.DepartmentName.ToLower() == model.DepartmentName.ToLower().Trim() && x.CompanyId == company.Id);

            if (exists is not null)
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@" ""{model.DepartmentName}"" already exists!"));


            model.CreatedBy = ObjectId.Parse(createdBy);
            model.UpdatedBy = ObjectId.Parse(updatedBy);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedIndicator = false;

            await _companyDepartments.Insert(model);

            return new Response<CompanyDepartments>(model, HttpStatusCode.Created);
        }

        public async Task<Response> UpdateDepartment(string updatedBy, CompanyDepartments model, string role, string? companyid = null)
        {

            CompanyDepartments exists = await _companyDepartments.FindOne(x => x.Id == model.Id);
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" ""{model.DepartmentName}"" does not exists!"));

            exists.DepartmentName = model.DepartmentName;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            CompanyDepartments department = await _companyDepartments.Update(exists.Id.ToString(), exists);
            return new Response<CompanyDepartments>(department, HttpStatusCode.Accepted);
        }

        public async Task<Response> GetDepartment(string id, string role, string? companyid = null)
        {

            try
            {
                Company company = await _common.ValidateOwner<Company>(role, companyid);


                CompanyDepartments results = await _companyDepartments.FindOne(d => d.Id == ObjectId.Parse(id) && d.CompanyId == company.Id);
                if (results is null) throw new HttpResponseException($"No such department for {company.CompanyName}");
                return new Response<CompanyDepartments>(results);
            }
            catch { throw; }
            finally { }
        }

        public async Task<Response> SoftDeleteDepartment(string updatedBy, string id, string role, string? companyid = null)
        {
            Company company = await _common.ValidateOwner<Company>(role, companyid);

            CompanyDepartments exists = await _companyDepartments.FindOne(x => x.Id == ObjectId.Parse(id) && x.CompanyId == company.Id);
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" This department does not exists!"));

            exists.DeletedIndicator = true;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            CompanyDepartments contribution = await _companyDepartments.Update(exists.Id.ToString(), exists);
            return new Response<CompanyDepartments>(contribution, HttpStatusCode.Accepted);
        }

        public DepartmentsnewActions(IRepository<CompanyDepartments> companyDepartments, ICommon common)
        {
            _common = common ?? throw new ArgumentNullException(nameof(common));
            _companyDepartments = companyDepartments ?? throw new ArgumentNullException(nameof(companyDepartments));
        }
        private readonly IRepository<CompanyDepartments> _companyDepartments;
        private readonly ICommon _common;
    }
}
