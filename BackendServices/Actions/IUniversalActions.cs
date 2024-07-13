using BackendServices.Models;
using PrePurchase.Models;
using PrePurchase.Models.LeaveDays;
using PrePurchase.Models.Requests;
using PrePurchase.Models.StatementsModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendServices.Actions
{
    /// <summary>
    /// This interface defines the contract of the actions a company is able to perform on our system
    /// </summary>
#nullable enable
    public interface IUniversalActions
    {

        Task<Response> Login(UniversalLoginModel model);

        Task<Response> ChangeEmployeePassword(string employeeDetailsId, string password);
        Task<Response> ChangeEmployeePin(ChangePin changePin);

        Task<Response> UniversalClockings(UniversalClockingModel model, string role, string companyId = null);

        Task<Response> GetPositionsInfo(string role, string companyId = null);

    }

#nullable disable
}
