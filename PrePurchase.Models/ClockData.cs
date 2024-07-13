using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Net.Http.Headers;
using PrePurchase.Models.Converters;
using PrePurchase.Models.HistoryModels;

namespace PrePurchase.Models
{
    [BsonIgnoreExtraElements]
    public class ClockData
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }


        [BsonRequired]
        public Shift Shift { get; set; }

        [BsonRequired]
        public Rate Rate { get; set; }


        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public RateType RateType { get; set; }


        [BsonIgnore]
        //public double TotalHours => Math.Abs((Normalized.ClockOut!.Value - ClockIn).TotalHours);

        public double TotalHours => AdjustTotalHours();

        [BsonIgnore]
        [JsonIgnore]
        internal double NormalShiftHours => Math.Abs((Shift.ShiftEndTime - Shift.ShiftStartTime).TotalHours);

        [BsonIgnore]
        public double OverTimeHours
        {
            get
            {
                // hours worked - shift total hours = over time

                var overTime = TotalHours - NormalShiftHours;

                return Math.Max(0, overTime);
            }
        }

        [BsonIgnore]
        public decimal Amount
        {
            get
            {
                var rate = RateType switch
                {
                    RateType.Standard => Rate.StandardDaysRate,
                    RateType.PublicHoliday => Rate.PublicHolidaysRate,
                    RateType.Sunday => Rate.SundaysRate,
                    RateType.Saturday => Rate.SaturdaysRate,
                    _ => Rate.StandardDaysRate
                };

                //Overtime * OverTimeRate + (TotalHours-Overtime) * rate = Amount
                var normalHours = TotalHours - OverTimeHours;
                var normalAmount = (decimal)normalHours * rate;
                var overTimeAmount = (decimal)OverTimeHours * Rate.OverTimeRate;
                return overTimeAmount + normalAmount;
            }
        }
        // => get data (data with information of how manny hours worked/payment amount, User)

        /// <summary>
        /// Indicates whether this clockData can be changed. eg
        /// 
        /// </summary>
        /// 
        public bool IsAdminClocking { get; set; }

        public bool IsProcessed { get; set; }

        public bool IsAnnualLeaveDays { get; set; }
        public bool IsSickLeaveDays { get; set; }
        public bool IsFamilyLeaveDays { get; set; }


        public bool IsClockOutAdjusted { get; set; }
        public bool IsClockInAdjusted { get; set; }
        public DateTime OldClockInValue { get; set; }
        public DateTime OldClockOutValue { get; set; }
        public List<RestTime> RestTimes { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        internal ClockData Normalized => Normalize(this);

        internal bool CanClockOut()
            => IsProcessed is false && ClockOut is null;

        public static ClockData Normalize(ClockData clockData)
        {
            if (clockData.CanClockOut() is false) return clockData;

            if (clockData.ClockOut == null && clockData.ClockIn.Date == DateTime.UtcNow.Date)
            {
                clockData.ClockOut = DateTime.UtcNow;
            }
            else if (clockData.ClockOut == null && clockData.ClockIn.Date < DateTime.UtcNow.Date)
            {
                // Assuming ShiftStartTime and ShiftEndTime are TimeOnly
                TimeSpan shiftEnd = clockData.Shift.ShiftEndTime.ToTimeSpan();

                // Adjust ClockOut to the end time of the shift for the day of ClockIn
                DateTime shiftEndDateTime = clockData.ClockIn.Date + shiftEnd;
                clockData.ClockOut = shiftEndDateTime;

            }
            return clockData;
        }

        private double AdjustTotalHours()
        {
            TimeSpan totalWorkHours = TimeSpan.Zero;

            if (ClockOut == null && ClockIn.Date == DateTime.UtcNow.Date)
            {
                totalWorkHours = DateTime.UtcNow - ClockIn;
                ClockOut = DateTime.UtcNow;
            }
            else if (ClockOut == null && ClockIn.Date < DateTime.UtcNow.Date)
            {
                // Assuming ShiftStartTime and ShiftEndTime are TimeOnly
                TimeSpan shiftEnd = Shift.ShiftEndTime.ToTimeSpan();

                // Adjust ClockOut to the end time of the shift for the day of ClockIn
                DateTime shiftEndDateTime = (ClockIn.Date + shiftEnd).AddHours(-2);
                ClockOut = shiftEndDateTime;

                // Calculate total hours based on adjusted ClockOut
                totalWorkHours = shiftEndDateTime - ClockIn;
            }
            //clock out has a value
            else if (ClockOut.HasValue)
            {
                totalWorkHours = ClockOut.Value - ClockIn;
            }
            return totalWorkHours.TotalHours;
        }

    }
    public class RestTime
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
