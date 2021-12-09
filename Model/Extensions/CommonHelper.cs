using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using NetCoreTemp.WebApi.Models.EnumType;
using NetCoreTemp.WebApi.Models.View_Model;

namespace NetCoreTemp.WebApi.Models.Extensions
{
    public static class CommonHelper
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

        #region 获取属性DisplayName

        /// <summary>
        /// 获取类的中文名
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetDisplayName(Type dataType, string fieldName)
        {
            // First look into attributes on a type and it's parents
            DisplayAttribute attr;
            attr = (DisplayAttribute)dataType.GetProperty(fieldName).GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

            if (attr == null)
            {
                return String.Empty;
            }
            else
                return (attr != null) ? attr.GetName() : String.Empty;
        }

        /// <summary>
        /// 获取类的Metadata中设置的Display中文名
        /// 先取 类中的 Display然后去 Metadata类中的Display
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="fieldName"></param>
        /// <returns>有MetadataType取Metadata,没有取类中的</returns>
        public static string GetMetaDataDisplayName(Type dataType, string fieldName)
        {
            // First look into attributes on a type and it's parents
            DisplayAttribute attr = null;
            attr = (DisplayAttribute)dataType.GetProperty(fieldName).GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

            MetadataTypeAttribute metadataType = (MetadataTypeAttribute)dataType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
            if (metadataType != null)
            {
                var property = metadataType.MetadataClassType.GetProperty(fieldName);
                if (property != null)
                {
                    var MetaAttr = (DisplayAttribute)property.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();
                    if (!(MetaAttr == null || MetaAttr.Name.ToUpper() == fieldName.ToUpper()))
                        attr = MetaAttr;
                }
            }

            return (attr != null) ? attr.Name : String.Empty;
        }

        /// <summary>
        /// 获取类的Metadata中设置的Display中文名
        /// 先取 类中的 Display然后去 Metadata类中的Display
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="fieldName"></param>
        /// <returns>有MetadataType取Metadata,没有取类中的</returns>
        public static string GetOnlyMetaDataDisplayName(Type dataType, string fieldName)
        {
            // First look into attributes on a type and it's parents
            DisplayAttribute attr = null;
            // Look for [MetadataType] attribute in type hierarchy
            // http://stackoverflow.com/questions/1910532/attribute-isdefined-doesnt-see-attributes-applied-with-metadatatype-class

            MetadataTypeAttribute metadataType = (MetadataTypeAttribute)dataType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).OfType<MetadataTypeAttribute>().FirstOrDefault();
            if (metadataType != null)
            {
                var property = metadataType.MetadataClassType.GetProperty(fieldName);
                if (property != null)
                {
                    var ss = property.GetCustomAttributes();
                    attr = (DisplayAttribute)property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), true).SingleOrDefault();
                }
            }

            return (attr != null) ? attr.Name : String.Empty;
        }

        #endregion

        /// <summary>
        /// 根据文件头判断上传的文件类型
        /// </summary>
        /// <param name="bytedata">文件的数据流 </param>
        /// <param name="len">读取文件流字节数 </param>
        /// <returns>返回true或false</returns>
        public static string GetByteType(byte[] bytedata, int len =2)
        {
            try
            {
                string FileType = "";

                StringBuilder strb = new StringBuilder();
                int Num = 0;
                while (Num < 2)
                {
                    byte buffer = bytedata[Num];
                    string str = buffer.ToString();
                    strb.Append(str);
                    Num++;
                }
                string fileClass = strb.ToString();

                //255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar 
                switch (fileClass)
                {
                    case "255216":
                        FileType = "jpg";
                        break;
                    case "7173":
                        FileType = "gif";
                        break;
                    case "13780":
                        FileType = "PNG";
                        break;
                    case "6677":
                        FileType = "BMP";
                        break;
                    case "7790":
                        FileType = "exe";
                        break;
                    case "8297":
                        FileType = "rar";
                        break;
                }

                #region 文件byte流 前2位 数据 图片 测试 正常 文件 rar 测试正常 office 文件无法判断
                // 
                /*文件扩展名说明
                 * 255216 jpg
                 * 208207 doc xls ppt wps
                 * 8075 docx pptx xlsx zip
                 * 5150 txt
                 * 8297 rar
                 * 7790 exe
                 * 3780 pdf      
                 * 
                 * 4946/104116 txt
                 * 7173        gif 
                 * 255216      jpg
                 * 13780       png
                 * 6677        bmp
                 * 239187      txt,aspx,asp,sql
                 * 208207      xls.doc.ppt
                 * 6063        xml
                 * 6033        htm,html
                 * 4742        js
                 * 8075        xlsx,zip,pptx,mmap,zip
                 * 8297        rar   
                 * 01          accdb,mdb
                 * 7790        exe,dll
                 * 5666        psd 
                 * 255254      rdp 
                 * 10056       bt种子 
                 * 64101       bat 
                 * 4059        sgf    
                 */
                //* JPG = 255216,
                //* GIF = 7173,
                //* BMP = 6677,
                //* PNG = 13780,
                //* COM = 7790,
                //* EXE = 7790,
                //* DLL = 7790,
                //* RAR = 8297,
                //* ZIP = 8075,
                //* XML = 6063,
                //* HTML = 6033,
                //* ASPX = 239187,
                //* CS = 117115,
                //* JS = 119105,
                //* TXT = 210187,
                //* SQL = 255254,
                //* BAT = 64101,
                //* BTSEED = 10056,
                //* RDP = 255254,
                //* PSD = 5666,
                //* PDF = 3780,
                //* CHM = 7384,
                //* LOG = 70105,
                //* REG = 8269,
                //* HLP = 6395,
                //* DOC = 208207,
                //* XLS = 208207,
                //* DOCX = 208207,
                //* XLSX = 208207

                #endregion

                return FileType;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// String转换成Bool
        /// 如果是Int >0 为True
        /// 如果是String =true 为True
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        public static bool? Switch2Bool(this string Str)
        {
            bool? IsTrue = null;
            int Num = 0;
            if (int.TryParse(Str, out Num))
            {
                if (Num >= 1)
                    IsTrue = true;
            }
            else
            {
                if (bool.TryParse(Str, out bool tf))
                    IsTrue = tf;
            }
            return IsTrue;
        }

        /// <summary>
        /// 是否枚举
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static (bool TF, TEnum EnumVal) IsEnum<TEnum>(string val) where TEnum : struct, Enum
        {
            bool ret = false;
            var type = typeof(TEnum);
            TEnum A = default(TEnum);
            if (Enum.TryParse(val, out A))
                if (Enum.IsDefined(type, A) | val.ToString().Contains(","))
                    ret = true;
            return (ret, A);
        }

        /// <summary>
        /// 去除AutoMapper-Dto前缀
        /// </summary>
        /// <param name="field">字段名</param>
        /// <param name="prefix">Dto前缀（默认"_"）</param>
        /// <returns></returns>
        public static string CleanAutoMapperDtoPrefix(this string field, string prefix = "_")
        {
            if (field.Substring(0, 1) == prefix)
            {
                #region 去除AutoMapper-Dto前缀 "_"

                if (field.IndexOf("Date") > 0)
                {
                    //_在filterRule里 是开始时间默认前缀
                    if (field.StartsWith(prefix+ prefix))
                        //去除AutoMapper-Dto前缀 "_"
                        field = field.Substring(1);
                }
                else
                    field = field.Substring(1);

                #endregion
            }
            return field;
        }

    }
}
