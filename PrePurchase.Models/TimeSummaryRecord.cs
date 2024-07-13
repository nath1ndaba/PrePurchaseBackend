using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using PrePurchase.Models.HistoryModels;

namespace PrePurchase.Models
{
    public record TimeSummaryRecord
    {
        public readonly ObjectId TimeSummaryId;
        public readonly ObjectId CompanyId;
        public readonly ObjectId EmployeeDetailsId;
        public string EmployeeId { get; set; }
        public readonly IEnumerable<ClockData> Clocks;
        public readonly DateTime StartDate;
        public readonly DateTime EndDate;
        public decimal DailyBonusDays;
        public decimal PayBackLoanAsPerAgreement;
        public AllTheLeave TheLeave;
        public AdjustedValuesOnPay AdjustedValuesOnPay;

        public decimal Amount => Clocks.Sum(c => c.Amount);

        internal TimeSummaryRecord(ObjectId timeSummaryId,
                                  ObjectId companyId,
                                  ObjectId employeeDetailsId,
                                  string employeeId,
                                  IEnumerable<ClockData> clockDatas,
                                  DateTime start,
                                  DateTime end,
                                  decimal dailyBonusDays,
                                  decimal payBackLoanAsPerAgreement,
                                  AllTheLeave leave,
                                 AdjustedValuesOnPay adjustedValuesOnPay

            )
        {
            TimeSummaryId = timeSummaryId;
            CompanyId = companyId;
            EmployeeDetailsId = employeeDetailsId;
            EmployeeId = employeeId;
            Clocks = clockDatas;
            StartDate = start;
            EndDate = end;
            DailyBonusDays = dailyBonusDays;
            PayBackLoanAsPerAgreement= payBackLoanAsPerAgreement;
            TheLeave = leave;
            AdjustedValuesOnPay = adjustedValuesOnPay;
        }
    }
}
