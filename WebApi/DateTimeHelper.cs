using System;
using System.Collections.Generic;
using System.Text;

namespace NetCoreTemp.WebApi
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// UTC起始时间 1970-1-1
        /// </summary>
        private static DateTime baseStartDate = new System.DateTime(1970, 1, 1);

        /// <summary>
        /// 本地时间 1970-1-1
        /// </summary>
        private static System.DateTime localStartDate = TimeZone.CurrentTimeZone.ToLocalTime(baseStartDate);

        /// <summary>
        /// 时间转换为 时间戳
        /// </summary>
        /// <param name="date">时间</param>
        /// <param name="isLocal">本地时间</param>
        public static long? toLong(this DateTime? date, bool isLocal = true)
        {
            long? retlong;
            System.DateTime startTime = TimeZoneInfo.ConvertTime(baseStartDate, TimeZoneInfo.Local);
            if (date.HasValue && date.Value > baseStartDate)
            {
                if (isLocal)
                    date = date.Value.ToUniversalTime();
                // 当地时区
                retlong = (long)(date - startTime).Value.TotalSeconds;
            }
            else
                retlong = null;
            return retlong;
        }

        /// <summary>
        /// 时间转换为 时间戳
        /// </summary>
        /// <param name="date">时间</param>
        /// <param name="isLocal">本地时间</param>
        public static long? to_Long(this DateTime date, bool isLocal = true)
        {
            return toLong(date, isLocal);
        }

        /// <summary>
        /// 时间戳转换为 时间
        /// </summary>
        /// <param name="date">long-时间戳</param>
        /// <param name="isLocal">本地时间</param>
        /// <returns>时间</returns>
        public static DateTime? toDateTime(this long? unixTimeStamp, bool isLocal = true)
        {
            DateTime? retdat;
            //中八区
            //System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(baseStartDate);
            //System.DateTime startTime = TimeZoneInfo.ConvertTime(baseStartDate, TimeZoneInfo.Local);
            System.DateTime startTime = localStartDate;
            if (!isLocal)
                startTime = TimeZoneInfo.ConvertTime(baseStartDate, TimeZoneInfo.Local);

            if (unixTimeStamp.HasValue && unixTimeStamp > 0)
            {
                // 当地时区
                retdat = startTime.AddSeconds(unixTimeStamp.Value);
            }
            else
                retdat = null;
            return retdat;
        }

        /// <summary>
        /// 时间戳转换为 时间
        /// </summary>
        /// <param name="date">long-时间戳</param>
        /// <param name="isLocal">本地时间</param>
        /// <returns>时间</returns>
        public static DateTime? to_DateTime(this long unixTimeStamp, bool isLocal = true)
        {
            return toDateTime(unixTimeStamp, isLocal);
        }
    }
}
