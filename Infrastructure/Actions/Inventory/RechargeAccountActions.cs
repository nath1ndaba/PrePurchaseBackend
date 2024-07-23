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
        private readonly ICommon _common;
        private readonly ILogger<RechargeAccountActions> _logger;

        public RechargeAccountActions(
            IRepository<Recharge> rechargeRepository,
            IRepository<UserAccount> userAccountRepository,
            ICommon common,
            ILogger<RechargeAccountActions> logger)
        {
            _rechargeRepository = rechargeRepository ?? throw new ArgumentNullException(nameof(rechargeRepository));
            _userAccountRepository = userAccountRepository ?? throw new ArgumentNullException(nameof(userAccountRepository));
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

                _logger.LogInformation("Recharges successfully fetched for user {UserId}", userId);
                return new Response<IEnumerable<Recharge>>(recharges);
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

                _logger.LogInformation("Recharge {RechargeId} successfully fetched for user {UserId}", id, userId);
                return new Response<Recharge>(recharge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recharge {RechargeId} for user {UserId}", id, userId);
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
