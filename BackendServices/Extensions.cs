using BackendServices.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendServices
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns><see cref="TimeSpan"/></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidTimeSpanFormatExecption"/>
        public static TimeSpan ParseTime(this string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                throw new ArgumentNullException($"The argument {nameof(timeString)} must not be null!!");
            // it is a number
            if (long.TryParse(timeString, out long result))
                return TimeSpan.FromSeconds(result);

            var lastCharacter = timeString[^1];
            var remainingCharecters = timeString[0..^1];
            //if (!"sSmMhHdDwW".Any(c => c == lastCharacter))
            //    throw new ArgumentOutOfRangeException($"The argument {nameof(timeString)} must end in either s,S,m,M,h,H,d,D,w or W!!");

            var timeSpan = lastCharacter switch
            {
                's' or 'S' => TimeSpan.FromSeconds(long.Parse(remainingCharecters)), // seconds
                'm' or 'M' => TimeSpan.FromMinutes(long.Parse(remainingCharecters)), // minutes
                'h' or 'H' => TimeSpan.FromHours(long.Parse(remainingCharecters)), // hours
                'd' or 'D' => TimeSpan.FromDays(long.Parse(remainingCharecters)), // days
                'w' or 'W' => TimeSpan.FromDays(long.Parse(remainingCharecters) * 7), // weeks
                _ => throw new InvalidTimeSpanFormatExecption()
            };

            return timeSpan;
        }

        public static DateTime Reset(this DateTime dateTime)
        {
            dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);
            dateTime = dateTime.AddSeconds(-dateTime.Second);
            dateTime = dateTime.AddMinutes(-dateTime.Minute);
            dateTime = dateTime.AddHours(-dateTime.Hour);

            return dateTime;
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> values)
        {
            var list = await values;

            return list.ToList();
        }

        public static async Task<IDictionary<TKey, TItem>> ToDictionaryAsync<TKey, TItem>(this Task<IEnumerable<TItem>> values, Func<TItem,TKey> keySelector)
        {
            var list = await values;

            return list.ToDictionary(keySelector);
        }

        public static bool EqualCaseInsesitive(this string first, string second)
            => string.Equals(first,second,StringComparison.OrdinalIgnoreCase);

        public static async void Await(this Task task, Action<Exception> onError = default)
        {
            try
            {
                await task.ConfigureAwait(false);
            }catch(Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

    }

}
