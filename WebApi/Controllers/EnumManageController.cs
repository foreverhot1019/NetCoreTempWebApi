using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Helpdesk.WebApi.Models.EnumType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Helpdesk.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnumManageController : ControllerBase
    {
        private IMemoryCache _cache;
        public EnumManageController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// 获取枚举数据
        /// </summary>
        /// <remarks>
        /// GET: api/<EnumManage>
        /// </remarks>
        /// <param name="enumName">枚举名称</param>
        /// <param name="Searhfilter">搜索条件</param>
        /// <returns>
        /// [{Key: '枚举名称', Value: 0, DisplayName: '显示名称', DisplayDescription: '描述'}]
        /// </returns>
        [HttpGet("{enumName}")]
        public IEnumerable<EnumModelType> Get(string enumName, string searhFilters="")
        {
            List<EnumModelType> ArrEnumModelType = new List<EnumModelType>();
            try
            {
                var namespaseStr = "Helpdesk.WebApi.Models.EnumType.EnumRepo";
                var EnumFullName = namespaseStr + "+" + enumName;
                //获取缓存
                if (!_cache.TryGetValue(EnumFullName, out ArrEnumModelType))
                {
                    ArrEnumModelType = ArrEnumModelType ?? new List<EnumModelType>();
                    var AssEnum = Assembly.GetAssembly(typeof(EnumRepo));
                    var etype = AssEnum.GetType($"Helpdesk.WebApi.Models.EnumType.EnumRepo+{enumName}");
                    Type type = AssEnum.GetType(EnumFullName);
                    if (type == null)
                        type = AssEnum.GetType(EnumFullName.Replace("+" + enumName, "." + enumName));
                    if (type != null)
                    {
                        foreach (string FieldStr in Enum.GetNames(type))
                        {
                            EnumModelType enumModelType = new EnumModelType();
                            string NameStr = "";//枚举显示名（Display）
                            string DescptStr = "";//枚举描述（Display）
                            var field = type.GetField(FieldStr);
                            if (field != null)
                            {
                                if (field.Name == FieldStr)
                                {
                                    DisplayAttribute attr;
                                    attr = (DisplayAttribute)field.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

                                    NameStr = attr?.GetName() ?? String.Empty;
                                    DescptStr = attr?.GetDescription() ?? String.Empty;
                                }
                            }
                            enumModelType.Key = FieldStr;
                            enumModelType.Value = (int)Enum.Parse(type, FieldStr);
                            enumModelType.DisplayName = NameStr;
                            enumModelType.DisplayDescription = DescptStr;

                            if (!string.IsNullOrEmpty(FieldStr))
                            {
                                ArrEnumModelType.Add(enumModelType);
                            }
                        }
                        //设置缓存
                        _cache.Set(EnumFullName, ArrEnumModelType);
                    }
                }
            }
            catch
            {
                ArrEnumModelType = new List<EnumModelType>();
            }
            return ArrEnumModelType;
        }

        /// <summary>
        /// 获取枚举Status数据
        /// </summary>
        /// <returns>
        /// [{Key: '枚举名称', Value: 0, DisplayName: '显示名称', DisplayDescription: '描述'}]
        /// </returns>
        [HttpGet("GetStatus")]
        public ActionResult GetEnumStatus()
        {
            var data = this.Get("UseStatusEnum");
            return Ok(data);
        }
    }
}
