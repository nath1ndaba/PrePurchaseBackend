using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.Models;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin
{
    public class DepartmentsActions : IDepartmentsActions
    {
        public async Task<Response> AddDepartment(string department, string role, string? companyId = null)
        {

            try
            {
                Company company = await _common.ValidateCompany<Company>(role, companyId);

                department = department.Trim();
                if (company.Departments.Contains(department))
                    throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: "Department already exists!"));
                company.Departments.Add(department);
                await _companies.Update(company.Id.ToString(), company);

                return new Response<string>(department);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new Response(HttpStatusCode.InternalServerError, error: ex.Message));
            }
            finally { }
        }
        public async Task<Response> GetDepartments(string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany<Company>(role, companyId);
                List<string> departments = company.Departments;

                return new Response<List<string>>(departments);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new Response(HttpStatusCode.InternalServerError, error: ex.Message));
            }
            finally { }
        }

        public async Task<Response> RemoveDepartment(string department, string role, string? companyId = null)
        {
            try
            {
                Company company = await _common.ValidateCompany<Company>(role, companyId);

                int index = company.Departments.FindIndex(x => x == department);

                if (index > -1)
                    company.Departments.RemoveAt(index);
                else
                    throw new HttpResponseException("Department not found");

                await _companies.Update(companyId, company);

                return new Response(HttpStatusCode.OK, message: "Deleted");
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new Response(HttpStatusCode.InternalServerError, error: ex.Message));
            }
            finally
            {
            }

        }

        public DepartmentsActions(IRepository<Company> companies, ICommon common)
        {
            _companies = companies;
            _common = common;
        }
        private readonly IRepository<Company> _companies;
        private readonly ICommon _common;

    }
}
