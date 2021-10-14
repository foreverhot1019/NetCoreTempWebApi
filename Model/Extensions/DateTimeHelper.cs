using System;
using System.Collections.Generic;
using System.Text;

namespace NetCoreTemp.WebApi.Models.Extensions
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// 起始时间
        /// </summary>
        private static DateTime baseStartDate = new System.DateTime(1970, 1, 1);

        /// <summary>
        /// 时间转换为 时间戳
        /// </summary>
        /// <param name="date">时间</param>
        /// <returns>long-时间戳</returns>
        public static long? toLong(this DateTime? date)
        {
            long? retlong;
            System.DateTime startTime = TimeZoneInfo.ConvertTime(baseStartDate, TimeZoneInfo.Local);
            if (date.HasValue && date.Value > baseStartDate)
            {
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
        /// <returns>long-时间戳</returns>
        public static long? to_Long(this DateTime date)
        {
            return toLong(date);
        }

        /// <summary>
        /// 时间戳转换为 时间
        /// </summary>
        /// <param name="date">long-时间戳</param>
        /// <returns>时间</returns>
        public static DateTime? toDateTime(this long? unixTimeStamp)
        {
            DateTime? retdat;
            ////中八区
            //System.DateTime _startTime = TimeZone.CurrentTimeZone.ToLocalTime(baseStartDate);
            System.DateTime startTime = TimeZoneInfo.ConvertTime(baseStartDate, TimeZoneInfo.Local);

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
        /// <returns>时间</returns>
        public static DateTime? to_DateTime(this long unixTimeStamp)
        {
            return toDateTime(unixTimeStamp);
        }
    }
}
