using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public class HolidaysHelper
    {
        public static bool IsSAHoliday()
        {

            int today = ConcatDay();
            if (Enum.IsDefined(typeof(SouthAfricanHolidays), (SouthAfricanHolidays)today))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static int ConcatDay()
        {
            int todayMonth = DateTime.UtcNow.Month; //e.g returns 4 for april
            int todayDay = DateTime.UtcNow.Day; //e.g returns 17 for 17th of April
            string day = todayMonth.ToString() + todayDay.ToString(); //e.g returns a string 417
            int today = int.Parse(day); //e.g returns an int 417 }

            return today;
        }
    }
}
