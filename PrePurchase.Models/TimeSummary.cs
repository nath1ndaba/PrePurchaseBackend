using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using PrePurchase.Models.Converters;
using PrePurchase.Models.HistoryModels;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class TimeSummary
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public DateTime CreatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? UpdatedBy { get; set; }
        public bool? DeletedIndicator { get; set; }

        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonRequired]
        public ObjectId EmployeeDetailsId { get; set; }

        [BsonRequired]
        public string EmployeeId { get; set; }

        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonRequired]
        public ObjectId CompanyId { get; set; }

        // used to identify the month and year of this time summary
        [BsonRequired]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }


        public List<ClockData> Clocks { get; set; } = new();

        [BsonIgnore]
        public decimal Amount => Clocks.Sum(c => c.Amount);

        public decimal DailyBonus { get; set; } 

        internal TimeSummary Normalize()
        {
            foreach(var clock in Clocks)
            {
                if (clock.IsProcessed)
                    continue;
                ClockData.Normalize(clock);
            }

            return this;
        }

        /// <summary>
        /// Creates a <see cref="TimeSummaryRecord"/> using an IEnumerable of <see cref="ClockData"/> and
        /// sets its StartDate and EndDate to the given <paramref name="start"/> date and <paramref name="end"/> date.
        /// <para><seealso cref="GenerateTimeSummaryFromClockData(IEnumerable{ClockData}, DateTime, DateTime)"/> is recommended for most cases.</para>
        /// </summary>
        /// <param name="clockDatas"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        internal TimeSummaryRecord GenerateTimeSummaryFromClockData(IEnumerable<ClockData> clockDatas, 
                                                                                    DateTime start,
                                                                                    DateTime end)
        {
            // update the end and start date of the current TimeSummary
            StartDate = end.AddDays(1);
            //EndDate = StartDate.AddDays((end - start).Days);
            decimal tembonus = 0;
            decimal PayBackLoan = 0;
            AllTheLeave theLeave = new();
            AdjustedValuesOnPay adjustedValuesOnPay = new(); 
       
            return new (Id,
                CompanyId,
                EmployeeDetailsId, 
                EmployeeId, 
                clockDatas, 
                start,
                end, 
                tembonus,
                PayBackLoan,
                theLeave,
                adjustedValuesOnPay
                );
        }

        /// <summary>
        /// Generate a <see cref="TimeSummaryRecord"/> using <see cref="ClockData"/> between
        /// <paramref name="start"/> date and <paramref name="end"/> date.
        /// <para>This is the recommended way to generate a <see cref="TimeSummaryRecord"/> using a <see cref="TimeSummary"/> instance.</para>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        internal TimeSummaryRecord GetClockDataForRange(DateTime start, DateTime end)
        {

            List<ClockData> clocksInRange = new();
            Clocks = Clocks.Where(x => x is not null).ToList();

            int i = -1;



            // loop through the Clocks and find those that are within the range
            while (++i < Clocks.Count)
            {
                ClockData clock = Clocks[i];
                TimeSpan shifthours = Clocks[i].Shift.ShiftEndTime - Clocks[i].Shift.ShiftStartTime;
                DateTime systemToleratedOverTime = Clocks[i].ClockIn.AddHours(shifthours.Hours).AddHours(2); //tolerated 2 hours as overtime

                DateTime endDate = clock.ClockOut ?? systemToleratedOverTime.AddHours(-2); 
                if(clock.ClockIn >= start && endDate <= end)
                {
                    if (Clocks[i].ClockOut is null && systemToleratedOverTime >= DateTime.UtcNow) //3)
                        Clocks[i].ClockOut = DateTime.UtcNow;

                    else if (Clocks[i].ClockOut == null && systemToleratedOverTime < DateTime.UtcNow)
                    {
                        string referenceDate = Clocks[i].ClockIn!.Date.ToShortDateString() + " " + Clocks[i].Shift.ShiftEndTime.ToString();


                        Clocks[i].ClockOut = DateTime.Parse(referenceDate);
                    }
                    clock = Clocks[i];
                    clocksInRange.Add(clock);
                    Clocks[i] = null; // take note of this index, it will need to be removed from clocks
                }

            }

            // remove clocks at indexes in indexes array from Clocks List
            Clocks = Clocks.Where(x => x is not null).ToList();

            // create a TimeSummary for the processed ClockData
            return GenerateTimeSummaryFromClockData(clocksInRange, start, end);

        }

    }
}
