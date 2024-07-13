using BackendServices;
using BackendServices.Actions.Admin;
using BackendServices.Exceptions;
using BackendServices.JWT;
using BackendServices.Models;
using Infrastructure.Helpers;
using MongoDB.Bson;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin
{
    public class UserActions : IUserActions
    {
        public async Task<Response> RegisterUser(User model)
        {
            model.Email = model.Email.Trim().ToLowerInvariant();
            User exists = _users.FindOne(x => x.Email == model.Email).Result;
            if (exists is not null)
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict, error: $@"A User with email ""{model.Email}"" already exists!"));

            model.Password = await _passwordManager.Hash(model.Password);

            await _users.Insert(model);

            return new Response<User>(model, HttpStatusCode.Created);
        }
        public async Task<Response> GetUser(string email)
        {
            IEnumerable<User> user = await _users.Find(x => x.Email == email);
            return new Response<IEnumerable<User>>(user);
        }

        public async Task<Response> ChangePassword(string companyId, string userId, ChangePasswordModel model)
        {
            var (currentPassword, newPassword) = model;

            ObjectId userID = ObjectId.Parse(userId);
            var user = _users.FindOne(x => x.Id == userID).Result;
            var isOldPasswordMatch = await _passwordManager.IsMatch(currentPassword, user.Password);

            if (isOldPasswordMatch is false)
                throw new HttpResponseException("Current password does not match!");

            var hash = await _passwordManager.Hash(newPassword);

            user.Password = hash;

            await _users.Update(userId, user);
            return new Response(HttpStatusCode.OK, message: "Password changed!");
        }

        public UserActions(IRepository<User> users, IPasswordManager passwordManager)
        {
            _users = users;
            _passwordManager = passwordManager;
        }
        private readonly IPasswordManager _passwordManager;
        private readonly IRepository<User> _users;

    }
}
