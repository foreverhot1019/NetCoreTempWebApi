using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static NetCoreTemp.WebApi.Models.EnumType.EnumRepo;

namespace NetCoreTemp.WebApi.Models.View_Model
{
    /// <summary>
    /// 搜索类
    /// </summary>
    public sealed class filterRule
    {
        [Display(Name = "字段", Description = "字段")]
        public string field { get; set; }

        [Display(Name = "比较符号", Description = "比较符号")]
        public int op { get; set; }//FilterOp

        [Display(Name = "值", Description = "值")]
        public string value { get; set; }

        [Display(Name = "值类型", Description = "值类型")]
        public FilterValueType valType { get; set; } = 0;
    }
}
