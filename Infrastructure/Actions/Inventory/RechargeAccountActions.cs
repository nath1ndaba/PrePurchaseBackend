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

namespace Infrastructure.Repositories
{
    public class RechargeAccountActions : IRechargeAccountActions
    {
        private readonly IRepository<Recharge> _rechargeRepository;
        private readonly IRepository<UserAccount> _userAccountRepository;
        private readonly IRepository<Item> _userItemRepository;
        private readonly ICommon _common;
        private readonly ILogger<RechargeAccountActions> _logger;

        public RechargeAccountActions(
            IRepository<Recharge> rechargeRepository,
            IRepository<UserAccount> userAccountRepository,
            IRepository<Item> userItemRepository,
            ICommon common,
            ILogger<RechargeAccountActions> logger)
        {
            _rechargeRepository = rechargeRepository ?? throw new ArgumentNullException(nameof(rechargeRepository));
            _userAccountRepository = userAccountRepository ?? throw new ArgumentNullException(nameof(userAccountRepository));
            _userItemRepository = userItemRepository ?? throw new ArgumentNullException(nameof(userItemRepository));
            _common = common ?? throw new ArgumentNullException(nameof(common));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response> GetRecharges(string userId)
        {
            _logger.LogInformation("Fetching recharges for user {UserId}", userId);

            try
            {
                IEnumerable<Recharge> recharges = await _rechargeRepository.Find(u => u.UserId == ObjectId.Parse(userId));

                if (recharges == null || !recharges.Any())
                {
                    _logger.LogWarning("No recharges found for user {UserId}", userId);
                    throw new HttpResponseException($"No recharges found for {userId}");
                }

                var Dtos = new List<RechargeDto>();
                foreach (var item in recharges)
                {
                    var dto = new RechargeDto();
                    dto.DtoFromRecharge(item);
                    Dtos.Add(dto);
                }
                _logger.LogInformation("Recharges successfully fetched for user {UserId}", userId);
                return new Response<IEnumerable<RechargeDto>>(Dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recharges for user {UserId}", userId);
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> RechargeAccount(RechargeDto model, string createdBy, string? userId = null)
        {
            _logger.LogInformation("Recharging account for user {UserId}", userId);

            try
            {
                model.Id = ObjectId.GenerateNewId().ToString();
                model.CreatedBy = userId;
                model.UpdatedBy = userId;
                model.CreatedDate = DateTime.UtcNow;
                model.UpdatedDate = DateTime.UtcNow;
                model.DeletedIndicator = false;
                model.UserId = userId;

                Recharge recharge = new();
                recharge.DtoToRecharge(model);

                await _rechargeRepository.Insert(recharge);

                await HandleUserAccountUpdate(userId, model.Amount, createdBy);

                _logger.LogInformation("Account recharged successfully for user {UserId}", userId);
                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recharging account for user {UserId}", userId);
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetRecharge(string id, string? userId = null)
        {
            _logger.LogInformation("Fetching recharge {RechargeId} for user {UserId} ", id, userId);

            try
            {
                Recharge recharge = await _rechargeRepository.FindById(id);

                if (recharge == null || recharge.UserId != ObjectId.Parse(userId))
                {
                    _logger.LogWarning("Recharge {RechargeId} not found for user {UserId}", id, userId);
                    throw new HttpResponseException("Recharge not found");
                }
                var dto = new RechargeDto();
                dto.DtoFromRecharge(recharge);

                _logger.LogInformation("Recharge {RechargeId} successfully fetched for user {UserId}", id, userId);
                return new Response<RechargeDto>(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recharge {RechargeId} for user {UserId}", id, userId);
                throw new HttpResponseException(ex.Message);
            }
        }

        public async Task<Response> GetUserAccountBalance(string userId)
        {

            try
            {
                var accountBalance = await _userAccountRepository.FindOne(x => x.UserId == ObjectId.Parse(userId));

                if (accountBalance == null)
                {
                    throw new HttpResponseException("User account not found");
                }
                var dto = new UserAccountDto();
                dto.DtoFromUserAccount(accountBalance);

                _logger.LogInformation("Account for user {UserId} successfully fetched", userId);
                return new Response<UserAccountDto>(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching userAccount for user {UserId}", userId);
                throw new HttpResponseException(ex.Message);
            }
        }


        public async Task<Response> GetDashboardData(string userId)
        {
            try
            {
                var accountBalance = await _userAccountRepository.FindOne(x => x.UserId == ObjectId.Parse(userId));

                if (accountBalance == null)
                {
                    throw new HttpResponseException("User account not found");
                }

                var itemIds = accountBalance.ItemsBalances.Select(ib => ib.ItemId).ToList();
                var items = await _userItemRepository.Find(x => itemIds.Contains(x.Id));
                var itemsDictionary = items.ToDictionary(item => item.Id);

                var itemDtos = accountBalance.ItemsBalances
                                .Where(ib => itemsDictionary.ContainsKey(ib.ItemId))
                                .Select(ib =>
                                {
                                    var item = itemsDictionary[ib.ItemId];
                                    var itemDto = new ItemDto();
                                    itemDto.DtoFromItem(item);
                                    itemDto.RechargeBalance = ib.Balance;
                                    return itemDto;
                                })
                                .ToList();

                var dashboardData = new DashboardData
                {
                    UserId = accountBalance.UserId.ToString(),
                    AmountBalance = accountBalance.AmountBalance,
                    Items = itemDtos
                };

                _logger.LogInformation("Dashboard data for user {UserId} successfully fetched", userId);
                return new Response<DashboardData>(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user account for user {UserId}", userId);
                throw new HttpResponseException(ex.Message);
            }
        }

        private async Task HandleUserAccountUpdate(string userId, decimal amount, string createdBy)
        {
            UserAccount userAccount = await _userAccountRepository.FindOne(x => x.UserId == ObjectId.Parse(userId));

            if (userAccount == null)
            {
                _logger.LogInformation("Creating new user account for user {userId}", userId);

                UserAccountDto dto = new()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = userId,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    DeletedIndicator = false,
                    AmountBalance = amount,
                    ItemsBalances = []
                };

                userAccount = new UserAccount();
                userAccount.DtoToUserAccount(dto);
                await _userAccountRepository.Insert(userAccount);
            }
            else
            {
                _logger.LogInformation("Updating user account balance for user {UserId}", userId);
                userAccount.AmountBalance += amount;
                await _userAccountRepository.Update(userAccount.Id.ToString(), userAccount);
            }
        }

        public async Task<Response> UpdateUserAccountBalance(decimal amount, string createdBy, string? userId = null)
        {
            _logger.LogInformation("Updating balance for user account {UserId} with amount {Amount}", userId, amount);

            try
            {
                UserAccount userAccount = await _userAccountRepository.FindOne(x => x.UserId == ObjectId.Parse(userId));

                if (userAccount == null)
                {
                    _logger.LogWarning("User account not found for user {UserId}", userId);
                    throw new HttpResponseException("User account not found");
                }

                userAccount.AmountBalance += amount;
                await _userAccountRepository.Update(userAccount.Id.ToString(), userAccount);

                _logger.LogInformation("User account balance updated successfully for user {UserId}", userId);
                return new Response(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating balance for user account {UserId}", userId);
                throw new HttpResponseException(ex.Message);
            }
        }

    }
}
