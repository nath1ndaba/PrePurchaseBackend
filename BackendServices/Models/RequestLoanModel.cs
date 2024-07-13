using PrePurchase.Models;
using System.ComponentModel.DataAnnotations;

namespace BackendServices.Models
{
    public record RequestLoanModel
        ([Required] decimal LoanAmount,
        [Required] decimal LoanDurationInMonths,
        [Required] string Reason);

#nullable enable
    public record QueryLoanModel(
        string? CompanyId,
        decimal? LoanAmount,
        string? EmployeeId,
        decimal? LoanDurationInMonths,
        LoanStatus? Status);

    public record CompanyUpdateLoan(
        decimal? Payment,
        LoanStatus Status, string updatedBy);

#nullable disable
}
