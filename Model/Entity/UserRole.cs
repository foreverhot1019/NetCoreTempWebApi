using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models.Entity
{
    public sealed class UserRole : BaseModel.BaseEntity
    {
        public UserRole()
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

        //private string Region { get; set; } = "UserRole";

        //[Display(Name = "类型", Description = "类型")]
        //public string Type { get; } = "UserRole";
        #endregion

        [Display(Name = "账户Id", Description = "账户Id")]
        [Required]
        public Guid UserId { get; set; } = Guid.Empty;

        [Display(Name = "权限Id", Description = "权限Id")]
        [Required]
        public Guid RoleId { get; set; } = Guid.Empty;

        [Display(Name = "描述", Description = "描述")]
        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "账户", Description = "账户")]
        [ForeignKey("UserId")]
        public User OUser { get; set; }

        [Display(Name = "权限", Description = "权限")]
        [ForeignKey("RoleId")]
        public Role ORole { get; set; }
    }
}
