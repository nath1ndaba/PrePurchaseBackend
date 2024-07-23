using BackendServices;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using PrePurchase.Models;
using PrePurchase.Models.PrePurchase;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public class Common : ICommon
    {
        public async Task<T> ValidateOwner<T>(string role, string companyId) where T : class
        {
            Response response = await GetCompany<T>(role, companyId);
            if (response is not Response<T> entityResponse)
                throw new HttpResponseException(response);

            T entity = entityResponse.Data!;
            if (entity is Company company && company.LicenseExpiryDate < DateTime.UtcNow)
            {
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "Services Discontinued😒"));
            }
            return entity;
        }

        private async Task<Response> GetCompany<T>(string role, string? id = null) where T : class
        {
            async Task<T> Data()
            {
                if (typeof(T) == typeof(Company))
                {
                    var company = await _companies.FindById(id);
                    return company as T;
                }
                else if (typeof(T) == typeof(Shop))
                {
                    var shop = await _shop.FindById(id);
                    return shop as T;
                }
                else if (typeof(T) == typeof(User))
                {
                    var user = await _user.FindById(id);
                    return user as T;
                }
                else
                {
                    throw new InvalidOperationException("Unsupported type");
                }
            }

#nullable disable
            Response response;

            switch (role)
            {
                case AuthRoles.Manager:
                    if (id is null)
                        response = new(error: "Company id is not specified!");
                    else
                        response = new Response<T>() { Data = await Data() };
                    break;

                case AuthRoles.Owner:
                    response = new Response<T>() { Data = await Data() };
                    break;
                default:
                    response = new Response(HttpStatusCode.Unauthorized, error: "You don't have access to this resource!");
                    break;
            }

            return response;
        }

        public Common(IRepository<Company> companies, IRepository<Shop> shops, IRepository<User> user)
        {
            _companies = companies;
            _shop = shops;
            _user = user;
        }

        private readonly IRepository<Company> _companies;
        private readonly IRepository<Shop> _shop;
        private readonly IRepository<User> _user;
    }
}
