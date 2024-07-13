using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrePurchase.Models.Extractors;
using System;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;

namespace PrePurchase.Models
{
    // generated when payment is being made
    [BsonIgnoreExtraElements]
    public class History : SimpleHistory
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? TransactionId { get; set; }
        [BsonRequired]
        public PaySlip PaySlip { get; set; }
        [BsonIgnore]
        public new string Month => Timestamp.ToMonthName();
        [BsonIgnore]
        public decimal TotalAmount => Math.Round(PaySlip.TotalAmount, 2);
        [BsonIgnore]
        public decimal TotalDeduction => Math.Round(PaySlip.TotalDeductions, 2);
        [BsonIgnore]
        public decimal TotalNet => Math.Round(TotalAmount - TotalDeduction, 2);
        [BsonIgnore]
        public decimal Bonus { get; set; }

        /// <summary>
        /// Used to generate a payslip for a given <see cref="TimeSummaryRecord"/>.
        /// <see cref="TimeSummaryRecord"/> is generated using <see cref="TimeSummary.GetClockDataForRange(DateTime, DateTime)"/>
        /// </summary>
        /// <param name="record"></param>
        public History GeneratePaySlip(TimeSummaryRecord record)
        {

            PaySlip = new()
            {
                Id = Id,
                //CreatedBy = record.CreatedBy,
                //CreatedDate = record.CreatedDate,
                //UpdatedBy = record.updatedBy,
                //UpdatedDate = record.updatedDate,
                //DeletedIndicator = record.DeletedIndicator,
                Leave = record.TheLeave,
                PayBackLoanAsPerAgreement = record.PayBackLoanAsPerAgreement
            };

            TimeSummaryId = record.TimeSummaryId;
            CompanyId = record.CompanyId;
            EmployeeDetailsId = record.EmployeeDetailsId;
            EmployeeId = record.EmployeeId;

            Bonus = record.DailyBonusDays;

            DateTime? startDate = null;
            DateTime? endDate = null;
            foreach (var data in record.Clocks)
            {
                PaymentInfo info = new();

                info.Id = data!.Id;
                info.Amount = data!.Amount;
                info.RateType = data!.RateType;
                info.StartTime = data!.ClockIn;


                TimeSpan shifthours = TimeSpan.Zero;

                shifthours = data!.Shift.ShiftEndTime - data!.Shift.ShiftStartTime;
                info.EndTime = data!.ClockOut ?? data!.ClockIn.AddHours(shifthours.Hours);

                PaySlip.WorkPeriod.Add(info);
                //PaySlip.WorkPeriod.Add(data);
                startDate ??= data!.ClockIn;
                endDate ??= data!.ClockOut;
                if (startDate is not null)
                {
                    startDate = data!.ClockIn < startDate ? data!.ClockIn : startDate;
                    endDate = data!.ClockOut > endDate ? data!.ClockOut : endDate;
                }

                endDate ??= data!.ClockIn.AddHours(data!.NormalShiftHours);

            }

            PaySlip.StartDate = startDate.Value;
            PaySlip.EndDate = endDate.Value;
            // added bonus if any
            //AdjustedValuesOnPay valuesOnPay = new()
            //{
            //    TotalBasic = PaySlip.WorkPeriod.Sum(x => x.Amount),
            //    AdjustedBonus = 0,
            //    UIFDeduction = PaySlip.WorkPeriod.FirstOrDefault(x => x.Type == PaymentInfo.PaymentType.UIF)?.Amount ?? 0m,
            //    TaxDeduction = PaySlip.WorkPeriod.FirstOrDefault(x => x.Type == PaymentInfo.PaymentType.TAX)?.Amount ?? 0m,
            //    AdjustedNet = 0,
            //    InitialBonus = 0
            //};

            if (Bonus > 0)
            {
                PaySlip.WorkPeriod.Add(
                    new PaymentInfo
                    {
                        Amount = Bonus,
                        StartTime = PaySlip.StartDate,
                        EndTime = PaySlip.EndDate,
                        Description = "Bonus"

                    });
                //valuesOnPay.InitialBonus = Bonus;
            }

            //valuesOnPay.OriginalNet = valuesOnPay.TotalBasic - valuesOnPay.UIFDeduction - valuesOnPay.TaxDeduction + valuesOnPay.InitialBonus;

            // PaySlip.AdjustedValuesOnPay = valuesOnPay;

            return this;
        }

    }
}
