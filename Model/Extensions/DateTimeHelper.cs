using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// 字符串转时间
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="ArrFormat"></param>
        /// <returns></returns>
        public static bool Parse2DateTime(this string dateStr, out DateTime? retDate, IEnumerable<string> ArrFormat = null)
        {
            bool ret = false;
            retDate = null;
            if (ArrFormat?.Any() != true)
            {
                ArrFormat = new string[]{
                   "yyyy/M/d",
                   "yyyy/M/d h:mm",
                   "yyyy/M/d hh:mm",
                   "yyyy/M/d HH:mm",
                   "yyyy/M/d h:mm:ss",
                   "yyyy/M/d hh:mm:ss",
                   "yyyy/M/d HH:mm:ss",
                   "yyyy/M/d HH:mm:ss:ff",
                   "yyyy/M/d HH:mm:ss:fff",
                   "yyyy/M/dTHHmmssfff",
                   "yyyy/MM/dd",
                   "yyyy/MM/dd h:mm",
                   "yyyy/MM/dd hh:mm",
                   "yyyy/MM/dd HH:mm",
                   "yyyy/MM/dd h:mm:ss",
                   "yyyy/MM/dd hh:mm:ss",
                   "yyyy/MM/dd HH:mm:ss",
                   "yyyy/MM/dd HH:mm:ss:ff",
                   "yyyy/MM/dd HH:mm:ss:fff",
                   "yyyy/MM/ddTHHmmssfff"
                };
            }
            if (dateStr.IndexOf("-") > 0)
            {
                //dateStr = Regex.Replace(dateStr, "/-/g", "/", RegexOptions.None);
                dateStr = dateStr.Replace("-", "/");
            }
            if (DateTime.TryParseExact(dateStr, ArrFormat.ToArray(), new System.Globalization.CultureInfo("zh-CN"), System.Globalization.DateTimeStyles.None, out DateTime dateVal))
            //if (DateTime.TryParseExact(dateStr, ArrFormat, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out dateVal))
            {
                retDate = dateVal;
                ret = true;
            }

            return ret;
        }
    }
}
