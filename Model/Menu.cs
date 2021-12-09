using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models
{
    public class Menu : BaseModel.BaseEntity
    {
        public Menu()
        {
            ChildrenMenu = new HashSet<Menu>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "主键", Description = "主键")]
        [Required]
        public override Guid ID
        {
            get;
            set;
        } = Guid.NewGuid();

        #region DynamoDB 主键（string：类型#排序值）

        //private string id;
        //[Display(Name = "主键", Description = "主键")]
        //[Required, MaxLength(50)]
        //public override string ID
        //{
        //    get
        //    {
        //        if (id == "-")
        //            id = $"{Type}#{DateTime.Now.to_Long().ToString()}";
        //        return id;
        //    }
        //    set
        //    {
        //        if (string.IsNullOrEmpty(value) || value == "-")
        //            id = value;
        //        else
        //        {
        //            var profix = Type + "#";
        //            if (value.IndexOf(profix) != 0)
        //                id = Type + "#" + value;
        //            else
        //                id = value;
        //        }
        //    }
        //}

        //private string Region { get; set; } = "Menu";

        //[Display(Name = "类型", Description = "类型")]
        //public string Type { get; } = "Menu";

        #endregion

        [Display(Name = "隐藏菜单", Description = "隐藏菜单")]
        public bool Hidden { get; set; }

        [Display(Name = "菜单名称", Description = "菜单名称")]
        [Required, StringLength(20)]
        public string Name { get; set; } = "-";

        [Display(Name = "排序代码", Description = "菜单排序代码（0100开始）")]
        //[Index("IX_MenuCode", 1, IsUnique = true)]
        public int Sort { get; set; }

        [Display(Name = "菜单描述", Description = "菜单描述")]
        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "菜单Url", Description = "菜单Url")]
        [Required, StringLength(100)]
        //[Index("IX_MenuUrl", 1, IsUnique = false)]
        public string Url { get; set; } = "-";

        [Display(Name = "控制器", Description = "控制器")]
        [StringLength(50)]
        public string Controller { get; set; } = "-";

        [Display(Name = "菜单图标", Description = "菜单图标")]
        [StringLength(50)]
        public string IconCls { get; set; }

        [Display(Name = "所属资源", Description = "")]
        [Required, StringLength(50)]
        public string Resource { get; set; } = "-";

        [Display(Name = "Vue组件", Description = "Vue组件路径")]
        [Required, StringLength(100)]
        public string Component { get; set; } = "-";

        [Display(Name = "上级菜单", Description = "上级菜单")]
        [Required, StringLength(50)]
        public Guid ParentMenuId { get; set; } = Guid.Empty;

        //[Display(Name = "权限集合", Description = "权限集合")]
        //[StringLength(500)]
        //public string Roles { get; set; }

        [Display(Name = "权限集合", Description = "权限集合")]
        public virtual ICollection<Role> ArrRole { get; set; }

        [Display(Name = "上级菜单", Description = "上级菜单")]
        [ForeignKey("ParentMenuId")]
        public Menu ParentMenu { get; set; }

        [Display(Name = "子菜单", Description = "子菜单")]
        public virtual ICollection<Menu> ChildrenMenu { get; set; }
    }
}
