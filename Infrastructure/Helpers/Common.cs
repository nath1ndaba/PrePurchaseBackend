using BackendServices;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using PrePurchase.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public class Common : ICommon
    {
        public async Task<Company> ValidateCompany(string role, string companyId)
        {

            Response response = await GetCompany(role, companyId);
            if (response is not Response<Company> CompanyResponse)
                throw new HttpResponseException(response);

            Company company = CompanyResponse.Data!;
            if (company.LicenseExpiryDate < DateTime.UtcNow)
            {
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));
            }
            return company;

        }

        private async Task<Response> GetCompany(string role, string? id = null)
        {
            async Task<Company> Data()
                => await _companies.FindById(id);

#nullable disable
            Response response;



            switch (role)
            {
                case AuthRoles.Manager:
                    if (id is null)
                        response = new(error: "Company id is not specified!");
                    else
                        response = new Response<Company>() { Data = await Data() };
                    break;

                case AuthRoles.Owner:
                    response = new Response<Company>() { Data = await Data() };
                    break;
                default:
                    response = new Response(HttpStatusCode.Unauthorized, error: "You don't have access to this resource!");
                    break;
            }

            return response;

        }

        public Common(IRepository<Company> companies)
        {
            _companies = companies;
        }


        private readonly IRepository<Company> _companies;

    }
}
