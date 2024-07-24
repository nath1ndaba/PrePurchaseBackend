using BackendServices;
using BackendServices.Actions.PrePurchase;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Models.Inventory;
using BackendServices.Models.PrePurchase;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using PrePurchase.Models;
using PrePurchase.Models.Inventory;
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
        private readonly IRepository<Product> _product;
        private readonly IQueryBuilderProvider _queryBuilderProvider;
        private readonly IPasswordManager _passwordManager;
        private readonly ILogger<UserLoginActions> _logger;

        public UserLoginActions(
            IRepository<Address> address,
            IRepository<User> users,
            IRepository<Shop> shop,
            IRepository<Product> product,
            IQueryBuilderProvider queryBuilderProvider,
            IPasswordManager passwordManager,
            ILogger<UserLoginActions> logger)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _shop = shop ?? throw new ArgumentNullException(nameof(shop));
            _product = product ?? throw new ArgumentNullException(nameof(product));
            _queryBuilderProvider = queryBuilderProvider ?? throw new ArgumentNullException(nameof(queryBuilderProvider));
            _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserLoginResponse> UserLogin(LoginModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            string userName = model.Username.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning(" username is null or empty.");
                throw new HttpResponseException(new Response(HttpStatusCode.BadRequest, error: "Email or username is required."));
            }

            User user = await _users.FindOne(x => x.Email.Trim().ToLowerInvariant() == userName || x.SurName.Trim().ToLowerInvariant() == userName);

            if (user == null)
            {
                _logger.LogWarning("User not found for username: {UserName}", userName);
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid username 😒"));
            }

            bool matches = await _passwordManager.IsMatch(model.Password, user.Password);
            if (!matches)
            {
                _logger.LogWarning("Password mismatch for user with username: {userName}", userName);
                throw new HttpResponseException(new Response(HttpStatusCode.Unauthorized, error: "Invalid Password 😒"));
            }

            _logger.LogInformation("User successfully authenticated with email: {userName}", userName);

            var userInfo = new UserDto();
            userInfo.ShopId = [];
            userInfo.DtoFromUser(user);
            userInfo.Address = await _address.FindOne(x => x.AddressBelongsToId == user.Id) ?? new Address();

            var products = await _product.Find(x => user.ShopId.Contains(x.ShopId));
            var productsResults = new List<ProductDto>();
            foreach (var product in products)
            {
                var dto = new ProductDto();
                dto.DtoFromProduct(product);
                productsResults.Add(dto);
            }

            var loginResponse = new UserLoginResponse
            {
                User = userInfo,
                Products = productsResults,
                Shop = [],
            };
            if (user.Role != UserRole.Resident)
            {
                // Fetch all shops related to the user in a single query
                var shops = await _shop.Find(x => user.ShopId.Contains(x.Id));
                if (shops != null && shops.Any())
                {
                    _logger.LogInformation("Found {ShopCount} shops for user with username: {userName}", shops.Count(), userName);

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
                    _logger.LogInformation("No shops found for user with username: {userName}", userName);
                    throw new HttpResponseException(new Response(HttpStatusCode.NoContent, error: "You do not belong to any shop?? 😒"));

                }
            }
            return loginResponse;
        }
    }
}
