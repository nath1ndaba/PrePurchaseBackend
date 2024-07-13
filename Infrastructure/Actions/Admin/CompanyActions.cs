using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.Models;
using Infrastructure.Helpers;
using MongoDB.Bson;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin
{
    public class CompanyActions : ICompanyActions
    {
        public async Task<Response> Register(AdminRegisterModel model)
        {
            Company exists = await _companies.FindOne(x => x.CompanyName.ToLowerInvariant() == model.Name);

            if (exists is not null)
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@"A company ""{model.Name}"" is already registered!"));


            Company company = new()
            {
                CompanyName = model.Name,
                Address = model.Address,
                RegisterationNumber = model.Password
            };

            await _companies.Insert(company);

            return new Response<Company>(company, HttpStatusCode.Created);
        }

        public async Task<Response> GetCompany(string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);
                return new Response<Company>(company);
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> Update(CompanyUpdateModel update, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                company.Update(update);

                await _companies.Update(company.Id.ToString(), company);

                return new Response<Company>(company);
            }
            catch
            {
                throw;
            }
            finally { }

        }


        public async Task<Response> AddCompanySites(Location location, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                List<Location>? listOfCurrentSites = company?.Address?.ListOfSitesPerCompany ?? new List<Location>();
                listOfCurrentSites.Add(location);
                company.Address.ListOfSitesPerCompany = listOfCurrentSites;

                await _companies.Update(company.Id.ToString(), company);
                return new Response<HttpStatusCode>(HttpStatusCode.Created);
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> AddPosition(string position, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                position = position.Trim();

                if (company.Positions.Contains(position))
                    throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: "Location already exists!"));

                company.Positions.Add(position);

                await _companies.Update(company.Id.ToString(), company);

                return new Response<string>(position, HttpStatusCode.Created);
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> RemovePosition(string position, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                int index = company.Positions.FindIndex(x => x == position);

                if (index > -1)
                    company.Positions.RemoveAt(index);
                else
                    throw new HttpResponseException("Location not found");

                await _companies.Update(companyId, company);

                return new Response(HttpStatusCode.OK, message: "Deleted");
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> AddShift(ShiftModel model, string role, string? companyId = null)
        {

            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                if (company.Shifts.Any(x => x.Name == model.Name))
                    throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@"A shift with name ""{model.Name}"" already exists!"));

                Shift shift = model.ToShift();

                company.Shifts.Add(shift);

                await _companies.Update(company.Id.ToString(), company);

                return new Response<Shift>(shift, HttpStatusCode.Created);
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> RemoveShift(string shiftId, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                int index = company.Shifts.FindIndex(x => x.Id == ObjectId.Parse(shiftId));

                if (index > -1)
                    company.Shifts.RemoveAt(index); // we should probably check that this shift is not assigned to anyone before removing it
                else
                    return new Response(error: "Shift not found");
                await _companies.Update(company.Id.ToString(), company);

                return new Response(HttpStatusCode.OK, message: "Deleted");
            }
            catch
            {
                throw;
            }
            finally { }
        }


        public async Task<Response> UpdateShift(string id, ShiftModel model, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                Shift? toEdit = company.Shifts.Find(x => x.Id == ObjectId.Parse(id));
                Shift updateShift = model.ToShift();

                if (toEdit is not null)
                {
                    toEdit.Update(updateShift);
                    await _companies.Update(company.Id.ToString(), company);

                }
                else
                    throw new HttpResponseException("Shift not found");

                return new Response<Shift>(toEdit, HttpStatusCode.Accepted);
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> AddRate(RateModel model, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                if (company.Rates.Any(x => x.NameOfPosition.Trim() == model.NameOfPosition))
                    throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: @$"A rate for position ""{model.NameOfPosition}"" already exists!"));

                Rate rate = model.ToRate();

                company.Rates.Add(rate);

                await _companies.Update(company.Id.ToString(), company);


                return new Response<Rate>(rate, HttpStatusCode.Created);
            }
            catch
            {
                throw;
            }
            finally { }

        }

        public async Task<Response> RemoveRate(string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                int index = company.Rates.FindIndex(x => x.Id == ObjectId.Parse(id));

                if (index > -1)
                    company.Rates.RemoveAt(index); // should probably check that this rate is not assigned to an employee before removing it
                else
                    throw new HttpResponseException("Rate not found");

                await _companies.Update(companyId, company);

                return new Response(HttpStatusCode.OK, message: "Deleted");
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> AddDeductions(DeductionModel model, string role, string? companyId = null)
        {
            try
            {

                Company company = await _common.ValidateCompany(role, companyId);

                Deduction deduction = model.ToDeduction();
                if (company.Deductions.Any(x => x.TypeOfDeduction.Trim() == deduction.TypeOfDeduction))
                    throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@"A deduction for ""{deduction.TypeOfDeduction}"" already exists!"));
                company.Deductions.Add(deduction);
                await _companies.Update(company.Id.ToString(), company);

                return new Response<Deduction>(deduction, HttpStatusCode.Created);
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> UpdateDeduction(string id, DeductionModel model, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                Deduction? toEdit = company.Deductions.Find(x => x.Id == ObjectId.Parse(id));
                Deduction updatedDeduction = model.ToDeduction();

                if (toEdit is not null)
                {
                    toEdit.Update(updatedDeduction);
                    await _companies.Update(company.Id.ToString(), company);
                }
                else
                    throw new HttpResponseException("Deduction not found");

                return new Response<Deduction>(toEdit, HttpStatusCode.Accepted);
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> RemoveDeduction(string id, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                int index = company.Deductions.FindIndex(x => x.Id == ObjectId.Parse(id));

                if (index > -1)
                    company.Deductions.RemoveAt(index);
                else
                    throw new HttpResponseException("Deduction not found");

                await _companies.Update(companyId, company);

                return new Response(HttpStatusCode.OK, message: "Deleted");
            }
            catch
            {
                throw;
            }
            finally { }
        }

        public async Task<Response> UpdateRate(string id, RateModel model, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany(role, companyId);

                Rate? toEdit = company.Rates.Find(x => x.Id == ObjectId.Parse(id));
                Rate updatedRate = model.ToRate();

                if (toEdit is not null)
                {
                    toEdit.Update(updatedRate);
                    await _companies.Update(company.Id.ToString(), company);

                }
                else
                {
                    throw new HttpResponseException("Rate not found");
                }

                return new Response<Rate>(toEdit, HttpStatusCode.Accepted);
            }
            catch
            {
                throw;
            }
            finally { }
        }


        public CompanyActions(IRepository<Company> companies, ICommon common)
        {
            _companies = companies;
            _common = common;
        }
        private readonly IRepository<Company> _companies;
        private readonly ICommon _common;

    }
}
