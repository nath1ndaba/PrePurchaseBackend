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
    public class PositionsNewActions : IPositionsNewActions
    {
        public async Task<Response> AddPosition(string createdBy, string updatedBy, CompanyPositions model, string role, string? companyid = null)
        {
            if (ObjectId.TryParse(companyid, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");
            Company company = await _common.ValidateCompany<Company>(role, companyid);

            CompanyPositions? exists = await _companyPositions.FindOne(x => x.PositionName.ToLower() == model.PositionName.ToLower().Trim() && x.CompanyId == company.Id);
            if (exists is not null) throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@" ""{model.PositionName}"" already exists!"));

            model.CreatedBy = ObjectId.Parse(createdBy);
            model.UpdatedBy = ObjectId.Parse(updatedBy);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedIndicator = false;
            model.CompanyId = company.Id;

            await _companyPositions.Insert(model);

            return new Response<CompanyPositions>(model, HttpStatusCode.Created);
        }

        public async Task<Response> UpdatePosition(string updatedBy, CompanyPositions model, string role, string? companyid = null)
        {
            if (ObjectId.TryParse(companyid, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");
            Company company = await _common.ValidateCompany<Company>(role, companyid);

            CompanyPositions exists = await _companyPositions.FindOne(x => x.Id == model.Id && x.CompanyId == company.Id);
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" ""{model.PositionName}"" does not exists!"));

            exists.PositionName = model.PositionName;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            CompanyPositions department = await _companyPositions.Update(exists.Id.ToString(), exists);
            return new Response<CompanyPositions>(department, HttpStatusCode.Accepted);
        }

        public async Task<Response> GetPositions(string role, string? companyid = null)
        {

            try
            {
                if (ObjectId.TryParse(companyid, out var _companyId) is false)
                    throw new HttpResponseException("Invalid companyId!!");
                Company company = await _common.ValidateCompany<Company>(role, companyid);

                IEnumerable<CompanyPositions> departments = await _companyPositions.Find(d => d.CompanyId == company.Id);
                if (departments is null) throw new HttpResponseException($"No Departments for at {company.CompanyName}");
                return new Response<IEnumerable<CompanyPositions>>(departments);
            }
            catch { throw; }
            finally { }
        }

        public async Task<Response> GetPosition(string id, string role, string? companyid = null)
        {

            try
            {
                if (ObjectId.TryParse(companyid, out var _companyId) is false)
                    throw new HttpResponseException("Invalid companyId!!");
                Company company = await _common.ValidateCompany<Company>(role, companyid);

                CompanyPositions results = await _companyPositions.FindOne(d => d.Id == ObjectId.Parse(id) && d.CompanyId == company.Id);
                if (results is null) throw new HttpResponseException($"No such position for {company.CompanyName}");
                return new Response<CompanyPositions>(results);
            }
            catch { throw; }
            finally { }
        }

        public async Task<Response> SoftDeletePosition(string updatedBy, string id, string role, string? companyid = null)
        {
            if (ObjectId.TryParse(companyid, out var _companyId) is false)
                throw new HttpResponseException("Invalid companyId!!");
            Company company = await _common.ValidateCompany<Company>(role, companyid);

            CompanyPositions exists = await _companyPositions.FindOne(x => x.Id == ObjectId.Parse(id) && x.CompanyId == company.Id);
            if (exists is null) throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: $@" This position does not exists!"));

            exists.DeletedIndicator = true;
            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;

            CompanyPositions contribution = await _companyPositions.Update(exists.Id.ToString(), exists);
            return new Response<CompanyPositions>(contribution, HttpStatusCode.Accepted);
        }

        public PositionsNewActions(IRepository<CompanyPositions> companyPositions, ICommon common)
        {
            _companyPositions = companyPositions ?? throw new ArgumentNullException(nameof(companyPositions));
            _common = common ?? throw new ArgumentNullException(nameof(common));
        }

        private readonly IRepository<CompanyPositions> _companyPositions;
        private readonly ICommon _common;

    }
}
