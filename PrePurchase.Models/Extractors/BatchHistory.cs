using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models;
using System.Linq;

namespace PrePurchase.Models.Extractors
{
    [BsonIgnoreExtraElements]
    public class BatchHistory : SimpleHistory
    {
        public SimplePaySlip PaySlip { get; set; }
        internal static BatchHistory FromHistory(History history)
        {
            SimplePaySlip _paySlip = history.PaySlip;
            _paySlip.WorkPeriod = _paySlip.WorkPeriod.Where(x => x.Type == PaymentInfo.PaymentType.Earning).ToList();
            return new()
            {
                Id = history.Id,
                CreatedBy = history.CreatedBy,
                CreatedDate = history.CreatedDate,
                UpdatedBy = history.UpdatedBy,
                UpdatedDate = history.UpdatedDate,
                DeletedIndicator = history.DeletedIndicator,
                Name = history.Name,
                Surname = history.Surname,
                TimeSummaryId = history.TimeSummaryId,
                EmployeeDetailsId = history.EmployeeDetailsId,
                EmployeeId = history.EmployeeId,
                CompanyId = history.CompanyId,
                PaySlip = _paySlip,
                IsPaid = history.IsPaid,
            };
        }
    }
}
