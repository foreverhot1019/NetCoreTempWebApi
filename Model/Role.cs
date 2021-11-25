using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NetCoreTemp.WebApi.Models.BaseModel;
using NetCoreTemp.WebApi.Models.EnumType;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models
{
    public sealed class Role : BaseEntity
    {
        private string id = "-";

        [Display(Name = "主键", Description = "主键")]
        [Required, MaxLength(50)]
        public override string ID
        {
            get
            {
                if (id == "-")
                    id = $"{Type}#{DateTime.Now.to_Long().ToString()}";
                return id;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || value == "-")
                    id = value;
                else
                {
                    var profix = Type + "#";
                    if (value.IndexOf(profix) != 0)
                        id = Type + "#" + value;
                    else
                        id = value;
                }
            }
        }

        private string Region { get; set; } = "Role";
        
        [Display(Name = "类型", Description = "类型")]
        public string Type { get; } = "Role";

        [Display(Name = "权限名称", Description = "权限名称")]
        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Display(Name = "序号", Description = "序号")]
        [Required, Range(0, int.MaxValue)]
        public int Sort { get; set; }

        [Display(Name = "权限描述", Description = "权限描述")]
        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "菜单集合", Description = "菜单集合")]
        public IEnumerable<Menu> ArrMenu { get; set; }

    }
}
