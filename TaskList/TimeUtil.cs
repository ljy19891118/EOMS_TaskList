using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskList
{
    class TimeUtil
    {
        public static DateTime FromUnixTimeStamp(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
    }
}
