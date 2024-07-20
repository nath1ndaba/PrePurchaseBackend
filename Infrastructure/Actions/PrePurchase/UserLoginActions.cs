using BackendServices;
using BackendServices.Actions.PrePurchase;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using PrePurchase.Models;
using PrePurchase.Models.PrePurchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Actions.PrePurchase
{
    public class UserLoginActions : IUserLoginActions
    {
        private readonly IRepository<Address> _address;
        private readonly IRepository<Shop> _shop;
        private readonly IRepository<User> _users;
        private readonly IQueryBuilderProvider _queryBuilderProvider;
        private readonly IPasswordManager _passwordManager;
        private readonly ILogger<UserLoginActions> _logger;

        public UserLoginActions(
            IRepository<Address> address,
            IRepository<User> users,
            IRepository<Shop> shop,
            IQueryBuilderProvider queryBuilderProvider,
            IPasswordManager passwordManager,
            ILogger<UserLoginActions> logger)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _shop = shop ?? throw new ArgumentNullException(nameof(shop));
            _queryBuilderProvider = queryBuilderProvider ?? throw new ArgumentNullException(nameof(queryBuilderProvider));
            _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserLoginResponse> UserLogin(LoginModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            string email = model.Email?.Trim().ToLowerInvariant();
            string userName = model.UserName?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("Both email and username are null or empty.");
                throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "Email or username is required."));
            }

            User? user = null;

            if (!string.IsNullOrEmpty(email))
            {
                user = await _users.FindOne(x => x.Email.Trim().ToLowerInvariant() == email);
                if (user != null)
                {
                    _logger.LogInformation("User found by email: {Email}", email);
                }
            }

            if (user == null && !string.IsNullOrEmpty(userName))
            {
                user = await _users.FindOne(x => x.UserName.Trim().ToLowerInvariant() == userName);
                if (user != null)
                {
                    _logger.LogInformation("User found by username: {UserName}", userName);
                }
            }

            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email} and username: {UserName}", email, userName);
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid username or email 😒"));
            }

            bool matches = await _passwordManager.IsMatch(model.Password, user.Password);
            if (!matches)
            {
                _logger.LogWarning("Password mismatch for user with email: {Email}", email);
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid Password 😒"));
            }

            _logger.LogInformation("User successfully authenticated with email: {Email}", email);

            var userInfo = new UserDto();
            userInfo.ShopId = new List<string>();
            userInfo.DtoFromUser(user);
            userInfo.Address = await _address.FindOne(x => x.AddressBelongsToId == user.Id) ?? new Address();

            var loginResponse = new UserLoginResponse
            {
                User = userInfo,
                Shop = new List<ShopDto>()
            };
            if (user.Role != UserRole.Resident)
            {
                // Fetch all shops related to the user in a single query
                var shops = await _shop.Find(x => user.ShopId.Contains(x.Id));
                if (shops != null && shops.Any())
                {
                    _logger.LogInformation("Found {ShopCount} shops for user with email: {Email}", shops.Count(), email);

                    // Fetch all addresses in a single query
                    var shopIds = shops.Select(x => x.Id).ToList();
                    var addresses = await _address.Find(x => shopIds.Contains(x.AddressBelongsToId));

                    var addressDictionary = addresses.ToDictionary(a => a.AddressBelongsToId, a => a);

                    foreach (var shop in shops)
                    {
                        var shopDto = new ShopDto();
                        shopDto.DtoFromShop(shop);

                        if (addressDictionary.TryGetValue(shop.Id, out var shopAddress))
                        {
                            shopDto.Address = shopAddress;
                        }

                        loginResponse.Shop.Add(shopDto);
                    }
                }
                else
                {
                    _logger.LogInformation("No shops found for user with email: {Email}", email);
                    throw new HttpResponseException(new Response(HttpStatusCode.NoContent, error: "You do not belong to any shop?? 😒"));

                }
            }
            return loginResponse;
        }
    }
}
