using BackendServices;
using BackendServices.Actions.PrePurchase;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PrePurchase.Models;
using PrePurchase.Models.PrePurchase;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.PrePurchase
{
    public class UserActions : IUserActions
    {
        private readonly IRepository<User> _user;
        private readonly IRepository<Address> _address;
        private readonly IPasswordManager _passwordManager;
        private readonly ILogger<UserActions> _logger;

        public UserActions(IRepository<User> user, IRepository<Address> address, IPasswordManager passwordManager, ILogger<UserActions> logger)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response> RegisterUser(UserDto model, string createdBy)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            _logger.LogInformation($"Registering user with email: {model.Email}");

            User user = await _user.FindOne(x =>
                x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant() || x.UserName.ToLowerInvariant() == model.UserName.ToLowerInvariant());

            if (user is not null)
            {
                _logger.LogWarning($"Email address or username {model.Email}/{model.UserName} already exists.");
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict,
                    error: "Email address or username is already been used in the system"));
            }
            if (model.Role is not UserRole.Resident && model.ShopId is null)
            {
                _logger.LogWarning($"shopId Not passed by a {model.Role}");
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict,
                    error: $"You need to provide a shop if you are a {model.Role}"));
            }

            var hash = await _passwordManager.Hash(model.Password);
            model.Password = hash;
            model.Id = ObjectId.GenerateNewId().ToString();
            model.CreatedBy = createdBy;
            model.UpdatedBy = createdBy;
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedIndicator = false;

            User newUser = new();
            newUser.ShopId = new List<ObjectId>();
            newUser.DtoToUser(model);

            await _user.Insert(newUser);

            user = await _user.FindOne(x => x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant());

            if (user != null && model.Address != null)
            {
                model.Address.AddressBelongsToId = user.Id;
                await _address.Insert(model.Address);
            }

            return new Response<UserDto>(model, HttpStatusCode.Created);
        }

        public async Task<Response> UpdateUser(UserDto model, string updatedBy)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            _logger.LogInformation($"Updating user with email: {model.Email}");

            User exists = await _user.FindOne(x => x.Email.ToLowerInvariant() == model.Email.ToLowerInvariant());

            if (exists is null)
            {
                _logger.LogWarning($"User with email {model.Email} not found.");
                throw new HttpResponseException(new Response(HttpStatusCode.Conflict,
                    error: $@"A User ""{model.Name}"" is not found!"));
            }

            exists.UpdatedBy = ObjectId.Parse(updatedBy);
            exists.UpdatedDate = DateTime.UtcNow;
            exists.Name = model.Name ?? exists.Name;
            exists.SurName = model.SurName ?? exists.SurName;
            exists.UserName = model.UserName ?? exists.UserName;
            exists.PhoneNumber = model.PhoneNumber ?? exists.PhoneNumber;
            exists.Email = model.Email ?? exists.Email;
            exists.Password = model.Password ?? exists.Password;
            exists.Role = model.Role;
            foreach (var shopId in model.ShopId)
            {
                exists.ShopId.Add(ObjectId.Parse(shopId));
            }

            var userAddress = await _address.FindOne(x => x.AddressBelongsToId == exists.Id);
            if (userAddress is null && model.Address != null)
            {
                model.Address.AddressBelongsToId = exists.Id;
                await _address.Insert(model.Address);
            }
            else if (model.Address != null && userAddress is not null)
            {
                await _address.Update(userAddress.Id.ToString(), model.Address);
            }

            await _user.Update(exists.Id.ToString(), exists);

            return new Response<User>(exists, HttpStatusCode.Accepted);
        }

        public async Task<Response> GetUser(string UserId)
        {
            if (string.IsNullOrEmpty(UserId)) throw new ArgumentNullException(nameof(UserId));

            _logger.LogInformation($"Retrieving user with ID: {UserId}");

            User exists = await _user.FindOne(x => x.Id == ObjectId.Parse(UserId));

            if (exists is null)
            {
                _logger.LogWarning($"User with ID {UserId} not found.");
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                    error: "User provided not found!"));
            }

            return new Response<User>(exists, HttpStatusCode.Found);
        }

        public async Task<Response> GetUsers()
        {
            _logger.LogInformation("Retrieving all users");

            var users = await _user.Find(x => x.DeletedIndicator == false) ?? new List<User>();

            return new Response<IEnumerable<User>>(users, HttpStatusCode.Found);
        }

        public async Task<Response> ArchiveUser(string UserId, string updatedBy)
        {
            if (string.IsNullOrEmpty(UserId)) throw new ArgumentNullException(nameof(UserId));
            if (string.IsNullOrEmpty(updatedBy)) throw new ArgumentNullException(nameof(updatedBy));

            _logger.LogInformation($"Archiving user with ID: {UserId}");

            User user = await _user.FindOne(x => x.Id == ObjectId.Parse(UserId));

            if (user is null)
            {
                _logger.LogWarning($"User with ID {UserId} not found.");
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                    error: "User provided not found!"));
            }

            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = ObjectId.Parse(updatedBy);
            user.DeletedIndicator = true;
            var results = await _user.Update(UserId, user);

            return new Response<User>(user, HttpStatusCode.OK);
        }

        public async Task<Response> RestoreUser(string UserId, string updatedBy)
        {
            if (string.IsNullOrEmpty(UserId)) throw new ArgumentNullException(nameof(UserId));
            if (string.IsNullOrEmpty(updatedBy)) throw new ArgumentNullException(nameof(updatedBy));

            _logger.LogInformation($"Restoring user with ID: {UserId}");

            User user = await _user.FindOne(x => x.Id == ObjectId.Parse(UserId));

            if (user is null)
            {
                _logger.LogWarning($"User with ID {UserId} not found.");
                throw new HttpResponseException(new Response(HttpStatusCode.NotFound,
                    error: "User provided not found!"));
            }

            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = ObjectId.Parse(updatedBy);
            user.DeletedIndicator = false;
            var results = await _user.Update(UserId, user);

            return new Response<User>(user, HttpStatusCode.OK);
        }
    }
}
