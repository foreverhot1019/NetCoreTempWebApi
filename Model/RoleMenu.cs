using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models
{
    public sealed class RoleMenu : BaseModel.BaseEntity
    {
        public RoleMenu()
        {
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

        //private string Region { get; set; } = "RoleMenu";

        //[Display(Name = "类型", Description = "类型")]
        //public string Type { get; } = "RoleMenu";

        #endregion

        [Display(Name = "权限Id", Description = "权限Id")]
        [Required]
        public Guid RoleId { get; set; } = Guid.Empty;

        [Display(Name = "菜单Id", Description = "菜单Id")]
        [Required]
        public Guid MenuId { get; set; } = Guid.Empty;

        [Display(Name = "描述", Description = "描述")]
        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name ="权限")]
        [ForeignKey("RoleId")]
        public Role ORole { get; set; }

        [Display(Name = "菜单")]
        [ForeignKey("MenuId")]
        public Menu OMenu { get; set; }
    }
}
