using BackendServices;
using BackendServices.Actions.PrePurchase;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Models.PrePurchase;
using Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;
using PrePurchase.Models.PrePurchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PrePurchase.Models.Inventory;

namespace Infrastructure.Repositories
{
    public class CashToItemActions : ICashToItemActions
    {
        private readonly IRepository<Recharge> _rechargeRepository;
        private readonly IRepository<UserAccount> _userAccountRepository;
        private readonly IRepository<Product> _userItemRepository;
        private readonly IRepository<CashToItem> _cashToItemRepository;
        private readonly ICommon _common;
        private readonly IRechargeAccountActions _rechargeAccount;
        private readonly ILogger<RechargeAccountActions> _logger;

        public CashToItemActions(
            IRepository<Recharge> rechargeRepository,
            IRepository<UserAccount> userAccountRepository,
            IRepository<Product> userItemRepository,
            IRepository<CashToItem> cashToItemRepository,
            ICommon common,
            ILogger<RechargeAccountActions> logger,
            IRechargeAccountActions rechargeAccount)
        {
            _rechargeRepository = rechargeRepository ?? throw new ArgumentNullException(nameof(rechargeRepository));
            _userAccountRepository = userAccountRepository ?? throw new ArgumentNullException(nameof(userAccountRepository));
            _userItemRepository = userItemRepository ?? throw new ArgumentNullException(nameof(userItemRepository));
            _cashToItemRepository = cashToItemRepository ?? throw new ArgumentNullException(nameof(cashToItemRepository));
            _rechargeAccount = rechargeAccount ?? throw new ArgumentNullException(nameof(rechargeAccount));
            _common = common ?? throw new ArgumentNullException(nameof(common));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<Response> GetCashToItems(string userId)
        {
            _logger.LogInformation("Fetching Cash to items for user {UserId}", userId);

            try
            {
                var cashToItems = await _cashToItemRepository.Find(u => u.UserId == ObjectId.Parse(userId));

                if (cashToItems == null || !cashToItems.Any())
                {
                    _logger.LogWarning("No cash to items found for user {UserId}", userId);
                    throw new HttpResponseException($"No cash to items found for {userId}");
                }

                var Dtos = new List<CashToItemDto>();
                foreach (var item in cashToItems)
                {
                    var dto = new CashToItemDto();
                    dto.DtoFromCashToItem(item);
                    Dtos.Add(dto);
                }
                _logger.LogInformation("Cash to item successfully fetched for user {UserId}", userId);
                return new Response<IEnumerable<CashToItemDto>>(Dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recharges for user {UserId}", userId);
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetCashToItem(string id, string? userId = null)
        {
            _logger.LogInformation("Fetching cash to item {id} for user {UserId} ", id, userId);

            try
            {
                var cashToItem = await _cashToItemRepository.FindById(id);

                if (cashToItem == null || cashToItem.UserId != ObjectId.Parse(userId))
                {
                    _logger.LogWarning("Cash to item {id} not found for user {UserId}", id, userId);
                    throw new HttpResponseException("Cash to item not found");
                }
                var dto = new CashToItemDto();
                dto.DtoFromCashToItem(cashToItem);

                _logger.LogInformation("Cash to item {id} successfully fetched for user {UserId}", id, userId);
                return new Response<CashToItemDto>(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cash to item {id} for user {UserId}", id, userId);
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> ConvertCashToItem(CashToItemDto model, string createdBy, string? userId = null)
        {


            var userAccount = await _userAccountRepository.FindOne(x => x.UserId == ObjectId.Parse(userId));
            if (userAccount == null)
            {
                throw new HttpResponseException($"No recharges found. Please recharge first");
            }

            var item = await _userItemRepository.FindOne(x => x.Id == ObjectId.Parse(model.ItemId) && x.DeletedIndicator == false);
            if (item == null)
            {
                throw new HttpResponseException($"Item to purchase nolonger on the system");
            }
            _logger.LogInformation("check if requested items to be purchased are enough with the money provided");

            var cashBalance = userAccount.AmountBalance;
            var numberofItemsToPurchase = model.NumberOfItemsPurchased;
            var itemPrice = item.Price;

            var costUserWishesToPurchase = numberofItemsToPurchase * itemPrice;
            var varience = cashBalance - costUserWishesToPurchase;
            if (varience < 0)
            {
                throw new HttpResponseException($"Insufficient funds, top up with R{-varience} to aleast get {numberofItemsToPurchase}");
            }

            _logger.LogInformation("converting cash to {model.ItemName} for user {UserId}", userId, model.ItemName);

            try
            {
                model.Id = ObjectId.GenerateNewId().ToString();
                model.CreatedBy = userId;
                model.UpdatedBy = userId;
                model.CreatedDate = DateTime.UtcNow;
                model.UpdatedDate = DateTime.UtcNow;
                model.DeletedIndicator = false;
                model.UserId = userId;

                model.PreviousPriceToPurchaseItem = itemPrice;
                model.AmountSpentOnItem = costUserWishesToPurchase;
                model.ItemImage = item.ItemImage;

                CashToItem cashToItem = new();
                cashToItem.DtoToCashToItem(model);

                await _cashToItemRepository.Insert(cashToItem);

                /// Now let us do somereduction to the cash that is equivalent to the price of items bought
                userAccount.UpdatedDate = DateTime.UtcNow;
                userAccount.UpdatedBy = ObjectId.Parse(createdBy);
                userAccount.AmountBalance -= costUserWishesToPurchase;

                ItemBalance itemBalance = new();
                itemBalance.Balance = numberofItemsToPurchase;
                itemBalance.ItemId = ObjectId.Parse(model.ItemId);
                itemBalance.ItemImage = model.ItemImage;
                itemBalance.ItemName = model.ItemName;

                if (userAccount.ItemsBalances == null || userAccount.ItemsBalances.Count == 0)
                {
                    userAccount.ItemsBalances = new List<ItemBalance>();
                    userAccount.ItemsBalances.Add(itemBalance);
                }
                else
                {
                    userAccount.ItemsBalances.Add(itemBalance);
                }
                await _userAccountRepository.Update(userAccount.Id.ToString(), userAccount);

                _logger.LogInformation("Account recharged successfully for user {UserId}", userId);
                return new Response(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting cash to{model.ItemName} account for user {UserId}", userId, model.ItemName);
                throw new HttpResponseException(ex.Message);
            }
        }

        public Task<Response> GetCashToItemForDashboard(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Response> UpdateCashToItem(CashToItemDto cashToItem, string updatedBy, string userId = null)
        {
            throw new NotImplementedException();
        }

        public Task<Response> UndoCashToItem(string id, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
