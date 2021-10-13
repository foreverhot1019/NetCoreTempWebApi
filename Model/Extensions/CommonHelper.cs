using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using Helpdesk.WebApi.Models.EnumType;

namespace Helpdesk.WebApi.Models.Extensions
{
    public class CommonHelper
    {
        #region 程序集

        /// <summary>
        /// 获取当前命名空间
        /// </summary>
        private static string GetCurrentNamespace()
        {
            string NameSpaceStr = "";
            //取得当前方法命名空间
            NameSpaceStr = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            return NameSpaceStr;
        }

        /// <summary>
        /// 获取当前类命名空间
        /// </summary>
        private static string GetCurrentNamespace_ClassName()
        {
            string NameSpace_ClassName = "";
            //取得当前方法类全名
            NameSpace_ClassName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName;
            return NameSpace_ClassName;
        }

        /// <summary>
        /// 当前程序集
        /// </summary>
        public static System.Reflection.Assembly Assembly
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly();
            }
        }

        #endregion

        #region 枚举操作

        /// <summary>
        /// 获取Session枚举值
        /// </summary>
        /// <param name="EnumValName">枚举键</param>
        /// <param name="enumName">枚举名称</param>
        /// <returns></returns>
        public static object GeSessionEnumByName(string FieldStr)
        {
            object EnumVal = null;
            EnumVal = GetEnumByName(FieldStr, "SessionNameS");
            return EnumVal;
        }

        /// <summary>
        /// 获取缓存枚举值
        /// </summary>
        /// <param name="EnumValName">枚举键</param>
        /// <param name="enumName">枚举名称</param>
        /// <returns></returns>
        public static object GeCacheEnumByName(string FieldStr)
        {
            object EnumVal = null;
            EnumVal = GetEnumByName(FieldStr, "CacheNameS");
            return EnumVal;
        }

        /// <summary>
        /// 获取枚举值
        /// </summary>
        /// <param name="EnumValName">枚举键</param>
        /// <param name="enumName">枚举名称</param>
        /// <returns></returns>
        public static object GetEnumByName(string FieldStr, string enumName)
        {
            object EnumVal = null;
            try
            {
                Assembly assem = Assembly.GetExecutingAssembly();
                Type type = assem.GetType(GetCurrentNamespace_ClassName() + "+" + enumName);

                try
                {
                    var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
                    foreach (var fi in fields)
                    {
                        if (fi.Name == FieldStr)
                        {
                            DisplayAttribute attr;
                            attr = (DisplayAttribute)fi.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

                            if (attr == null)
                            {
                                EnumVal = (object)FieldStr;
                            }
                            else
                                EnumVal = (object)((attr != null) ? attr.GetName() : String.Empty);
                            break;
                        }
                    }
                }
                catch
                {
                    EnumVal = null;
                }
            }
            catch
            {
                EnumVal = null;
            }
            return EnumVal;
        }

        /// <summary>
        /// Enum 转换成 字典类型
        /// </summary>
        /// <param name="enumName">枚举名称</param>
        /// <returns></returns>
        public static List<EnumModelType> GetEnumToDic(string enumName, string namespaseStr = "", string JoinCalc = "+")
        {
            List<EnumModelType> ArrEnumMember = new List<EnumModelType>();
            try
            {
                //获取Cache实例
                var HttpRuntime = System.Runtime.Caching.MemoryCache.Default;
                Assembly assem = Assembly.GetExecutingAssembly();
                namespaseStr = string.IsNullOrEmpty(namespaseStr) ? GetCurrentNamespace_ClassName() : namespaseStr;
                string CacheEnumName = namespaseStr + JoinCalc + enumName;
                var Cache_Obj = HttpRuntime[CacheEnumName];
                if (Cache_Obj != null)
                {
                    return (List<EnumModelType>)Cache_Obj;
                }
                Type type = assem.GetType(CacheEnumName);
                foreach (string FieldStr in Enum.GetNames(type))
                {
                    EnumModelType o = new EnumModelType();
                    string NameStr = "";
                    string DescptStr = "";
                    var field = type.GetField(FieldStr);
                    if (field != null)
                    {
                        if (field.Name == FieldStr)
                        {
                            DisplayAttribute attr;
                            attr = (DisplayAttribute)field.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

                            if (attr != null)
                            {
                                NameStr = (attr != null) ? attr.GetName() : String.Empty;
                                DescptStr = (attr != null) ? attr.GetDescription() : String.Empty;
                            }
                        }
                    }
                    o.Key = FieldStr;
                    o.Value = (int)Enum.Parse(type, FieldStr);
                    o.DisplayName = NameStr;
                    o.DisplayDescription = DescptStr;

                    if (!string.IsNullOrEmpty(FieldStr))
                    {
                        ArrEnumMember.Add(o);
                    }
                }
                //设置缓存
                HttpRuntime.Set(CacheEnumName, ArrEnumMember, DateTimeOffset.MaxValue);
            }
            catch
            {
                ArrEnumMember = new List<EnumModelType>();
            }
            return ArrEnumMember;
        }

        /// <summary>
        /// 获取枚举
        /// </summary>
        /// <typeparam name="T">枚举</typeparam>
        /// <param name="enumVal">枚举名称/值</param>
        /// <returns></returns>
        public static T GetEnumVal<T>(string enumVal) where T : struct, IConvertible
        {
            int enumIntVal;
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (int.TryParse(enumVal, out enumIntVal))
            {
                T foo = (T)Enum.ToObject(typeof(T), enumIntVal);
                return foo;
            }
            else
            {
                T foo = (T)Enum.Parse(typeof(T), enumVal);
                // the foo.ToString().Contains(",") check is necessary for enumerations marked with an [Flags] attribute
                if (!Enum.IsDefined(typeof(T), foo) && !foo.ToString().Contains(","))
                    throw new InvalidOperationException(enumVal + " is not an underlying value of the YourEnum enumeration.");
                return foo;
            }
        }

        /// <summary>
        /// 根据枚举 键获取值
        /// </summary>
        /// <param name="enumName">枚举名称</param>
        /// <param name="ColumnName"></param>
        /// <returns></returns>
        public static int GetEnumVal(string enumName, string ColumnName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            Type type = assem.GetType(GetCurrentNamespace_ClassName() + "+" + enumName);
            return (int)Enum.Parse(type, ColumnName);
        }

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="EnumObj"></param>
        /// <returns></returns>
        public static string GetEnumDisplay(object EnumObj)
        {
            var type = EnumObj.GetType();
            if (type.IsEnum)
            {
                var EName = Enum.GetName(type, EnumObj);
                foreach (var FieldStr in Enum.GetNames(type))
                {
                    if (FieldStr == EName)
                    {
                        string NameStr = "";
                        string DescptStr = "";
                        var field = type.GetField(FieldStr);
                        if (field != null)
                        {
                            if (field.Name == FieldStr)
                            {
                                DisplayAttribute attr;
                                attr = (DisplayAttribute)field.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

                                if (attr != null)
                                {
                                    NameStr = (attr != null) ? attr.GetName() : String.Empty;
                                    DescptStr = (attr != null) ? attr.GetDescription() : String.Empty;
                                }
                            }
                        }
                        return DescptStr;
                    }
                }
                return "";
            }
            else
                return "";
        }

        #endregion
    }
}
