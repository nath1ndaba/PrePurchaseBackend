using BackendServices;
using BackendServices.Models;
using MongoDB.Bson;
using PrePurchase.Models;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Helpers
{
    public static class ModelQueryHelpers
    {
        public static IQueryBuilder<Loan> ToQuery(this QueryLoanModel model, IQueryBuilderProvider provider)
        {
            return provider.For<Loan>().FromModel(model);
        }

        public static IQueryBuilder<Loan> FromModel(this IQueryBuilder<Loan> queryBuilder, QueryLoanModel model)
        {
            return queryBuilder.IgnoreNulls(true)
                .Eq(x => x.CompanyId, ObjectId.Parse(model.CompanyId))
                .And(x => x.LoanAmount, model.LoanAmount)
                .And(x => x.EmployeeSummary.EmployeeId, model.EmployeeId)
                .And(x => x.LoanDurationInMonths, model.LoanDurationInMonths)
                .And(x => x.LoanStatus, model.Status);
        }

        public static IQueryBuilder<Leave> ToQuery(this QueryLeaveModel model, IQueryBuilderProvider provider)
        {
            return provider.For<Leave>().FromModel(model);
        }

        public static IQueryBuilder<Leave> FromModel(this IQueryBuilder<Leave> queryBuilder, QueryLeaveModel model)
        {
            return queryBuilder.IgnoreNulls(true)
                .Eq(x => x.CompanyId, ObjectId.Parse(model.CompanyId))
                .And(x => x.TypeOfLeave, model.TypeOfLeave)
                .And(x => x.EmployeeSummary.EmployeeId, model.EmployeeId)
                .And(x => x.LeaveStartDate, model.LeaveStartDate)
                .And(x => x.LeaveEndDate, model.LeaveEndDate)
                .And(x => x.Status, model.Status);
        }
        public static IEnumerable<(RateType ratetype, IEnumerable<ClockData> clocks)> GetAmounts(this IEnumerable<TimeSummary> dtos)
        {
            return dtos.SelectMany(x => x.Clocks).GroupBy(x => x.RateType).Select(x => (x.Key, x.AsEnumerable()));
        }

    }
}
