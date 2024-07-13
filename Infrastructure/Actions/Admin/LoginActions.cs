using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.Models;
using MongoDB.Bson;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin
{
    public class LoginActions : ILoginActions
    {
        public async Task<LoginResponse> Login(AdminLoginModel model)
        {
            string email = model.Email.Trim().ToLowerInvariant();
            User user = _users.FindOne(x => x.Email == email).Result;
            if (user is null)
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid Email😒"));

            bool matches = await _passwordManager.IsMatch(model.Password, user.Password);
            if (!matches)
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid Password😒"));

            IEnumerable<CompanyEmployee> companyEmployee =
                await _companyEmployees.Find(x => x.EmployeeId == user.EmployeeId);
            List<ObjectId> companyIds = companyEmployee.Select(x => x.CompanyId).ToList();

            IQueryBuilder<Company> _query = _queryBuilderProvider.For<Company>().New().In(x => x.Id, companyIds);
            IEnumerable<Company> companies = await _companies.Find(_query) ?? new List<Company>();

            CompanyEmployee? userResponse = companyEmployee.FirstOrDefault();
            if (userResponse is null)
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound, error: "User Not Found😒"));

            LoginResponse loginResponse = new()
            {
                Id = userResponse.Id.ToString(),
                Name = userResponse.Name,
                SurName = userResponse.Surname,
                NickName = userResponse.NickName,
                EmployeeId = userResponse.EmployeeId,
                LoggedUserEmailAddress = user.Email,
                IsSuperAdmin = user.IsSuperAdmin,
                Companies = companies.ToList()
            };
            if (companies.Any(x => x.LicenseExpiryDate < DateTime.UtcNow && x.Id == userResponse.CompanyId))
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                    error: "Services Discontinued😒"));
            return loginResponse;
        }


        public LoginActions(IRepository<Company> companies, IRepository<User> users,
            IRepository<CompanyEmployee> companyEmployees, IQueryBuilderProvider queryBuilderProvider,
            IPasswordManager passwordManager)
        {
            _companies = companies;
            _companyEmployees = companyEmployees;
            _users = users;
            _passwordManager = passwordManager;
            _queryBuilderProvider = queryBuilderProvider;
        }

        private readonly IRepository<Company> _companies;
        private readonly IRepository<CompanyEmployee> _companyEmployees;
        private readonly IQueryBuilderProvider _queryBuilderProvider;
        private readonly IPasswordManager _passwordManager;
        private readonly IRepository<User> _users;
    }
}