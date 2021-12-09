using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetCoreTemp.WebApi.Models.Extensions
{
    /// <summary>
    /// .Net-Guid&Oracle-Guid转换
    /// </summary>
    public static class GuidConvertHelper
    {
        #region .Net和Oracle的Guid转换

        /// <summary>
        /// .Net-Guid转换为 Oracle-Guid
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string DotNetToOracle(string text)
        {
            Guid guid = new Guid(text);
            return DotNetToOracle(guid);
        }

        /// <summary>
        /// .Net-Guid转换为 Oracle-Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string DotNetToOracle(Guid guid)
        {
            return BitConverter.ToString(guid.ToByteArray()).Replace("-", "");
        }

        /// <summary>
        /// Oracle-Guid转换为 .Net-Guid
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Guid OracleToDotNetGuid(string text)
        {
            byte[] bytes = ParseHex(text);
            Guid guid = new Guid(bytes);
            return guid;
        }

        /// <summary>
        /// Oracle-Guid转换为 .Net-Guid
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string OracleToDotNet(string text)
        {
            Guid guid = OracleToDotNetGuid(text);
            return guid.ToString().ToUpperInvariant();
        }

        /// <summary>
        /// Guid转Hex
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] ParseHex(string text)
        {
            byte[] ret = new byte[text.Length / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
            }
            return ret;
        }

        #endregion

        /// <summary>
        /// 转换为Guid
        /// </summary>
        /// <param name="guidStr"></param>
        /// <returns></returns>
        public static Guid? parse2Guid(this string guidStr)
        {
            Guid? retGid = null;
            if (Guid.TryParse(guidStr,out Guid guid))
            {
                retGid = guid;
            }
            return retGid;
        }
    }
}
