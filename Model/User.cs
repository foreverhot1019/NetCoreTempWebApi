using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetCoreTemp.WebApi.Models.BaseModel;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models
{
    public partial class User : BaseEntity
    {
        public User()
        {
            ArrRole = new HashSet<Role>();
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
        //            id = $"User#{DateTime.Now.to_Long().ToString()}";
        //        return id;
        //    }
        //    set
        //    {
        //        if (string.IsNullOrEmpty(value) || value == "-")
        //            id = value;
        //        else
        //        {
        //            var profix = "User#";
        //            if (value.IndexOf(profix) != 0)
        //                id = profix + value;
        //            else
        //                id = value;
        //        }
        //    }
        //}

        #endregion

        [Display(Name = "账户名称", Description = "账户名称")]
        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Display(Name = "邮件", Description = "邮件")]
        [Required, MaxLength(100)]
        public string Email { get; set; }

        [Display(Name = "密码", Description = "密码")]
        [Required, MaxLength(100)]
        public string Password { get; set; }

        [Display(Name = "账户权限", Description = "账户权限")]
        public string Roles { get; set; }

        [Display(Name = "账户权限", Description = "账户权限")]
        public virtual ICollection<Role> ArrRole { get; set; }
    }

    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
    }
    public partial class UserMetadata
    {

    }
}
