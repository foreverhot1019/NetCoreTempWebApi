using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models
{
    public sealed class RoleMenu : BaseModel.BaseEntity
    {
        public RoleMenu()
        {
            //Children = new HashSet<Menu>();
        }

        private string id;

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

        private string Region { get; set; } = "RoleMenu";

        [Display(Name = "类型", Description = "类型")]
        public string Type { get; } = "RoleMenu";

        [Display(Name = "权限Id", Description = "权限Id")]
        [Required, StringLength(20)]
        public string RoleId { get; set; } = "-";

        [Display(Name = "菜单Id", Description = "菜单Id")]
        [Required, StringLength(20)]
        public string MenuId { get; set; } = "-";

        [Display(Name = "描述", Description = "描述")]
        [StringLength(100)]
        public string Remark { get; set; }
    }
}
