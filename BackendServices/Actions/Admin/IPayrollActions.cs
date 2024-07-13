using BackendServices.Models;
using PrePurchase.Models;
using PrePurchase.Models.HistoryModels;
using PrePurchase.Models.LeaveDays;
using PrePurchase.Models.Requests;
using PrePurchase.Models.StatementsModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin
{
    public interface IPayrollActions
    {
        Task<Response> AmendClockings(string updatedBy, List<AmendClocks> model, string employeeDetailsId, string role, string companyId = null);

        Task<Response> OverrideClockings(string updatedBy, string employeeDetailsId, string role, string companyId = null);

        Task<Response> OverrideAllClockings(string updatedBy, string role, string companyId = null);

        Task<Response> ClockEmployeeViaAdmin(string updatedBy, AdminManualClockings model, string role, string companyId = null);

        Task<Response> TimeSummariesByCompanyId(string role, string companyId);

        Task<Response> TimeSummariesForRangeByCompanyId(string role, TimeSummariesForRangeModel model);

        Task<Response> StoreProcessedPayrollBatch(string createdBy, string updatedBy, string role, BatchRequest model, string AdminWhoProcessed, string companyId = null);

        Task<Response> UpdateProcessedPayrollBatch(string updatedBy, string role, string BatchCode, List<AdjustedValuesOnPay> valuesOnPay, string AdminWhoUpdated, string companyId = null);

        Task<Response> GetProcessedPayrollBatch(string role, string companyId = null);

        Task<Response> GetPayrolBatchHistory(string role, string companyId = null);

        Task<Response> GetprocessedTimesSummaries(string role, string companyId = null);

        Task<Response> GetProcessedTimesSummariesBatch(string role, string batch, string companyId = null);
        Task<Response> UndoProcessedTimesSummariesBatch(string updatedBy, string role, string batch, string companyId = null);

        Task<Response> ApplyLeaveViaAdmin(string createdBy, string updatedBy, RequestLeaveModel model, string employeeId, string companyId, string role);
    }
}
