using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NetCoreTemp.WebApi.Models.Extensions;
using static NetCoreTemp.WebApi.Models.EnumType.EnumRepo;

namespace NetCoreTemp.WebApi.Models.BaseModel
{
    //sealed 不能被继承
    //internal 当前程序集
    //protected 只有在继承的子类中可访问，可以跨程序集
    public class BaseEntity : IEntity<Guid>
    {
        //[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "主键", Description = "主键")]
        [Required]
        //[MaxLength(50)]
        public virtual Guid ID { get { return (Guid)ID_; } set { ID_ = value; } }

        [NotMapped]
        public Object ID_ { get; set; } = Guid.NewGuid();

        [Display(Name = "状态", Description = "状态")]
        [DefaultValue(1)]
        public virtual UseStatusEnum Status { get; set; } = (UseStatusEnum)1;

        [Display(Name = "新增人ID", Description = "新增人ID")]
        [Required]
        //[MaxLength(50)]
        public Guid CreateUserId { get; set; }

        [Display(Name = "新增人", Description = "新增人")]
        [MaxLength(20)]
        public string CreateUserName { get; set; }

        [Display(Name = "新增时间", Description = "新增时间")]
        public virtual long CreateDate { get; set; } = DateTime.Now.to_Long().Value;

        [Display(Name = "修改人ID", Description = "修改人ID")]
        [Required]
        //[MaxLength(50)]
        public Guid ModifyUserId { get; set; }

        [Display(Name = "修改人", Description = "修改人")]
        [MaxLength(20)]
        public string ModifyUserName { get; set; }

        [Display(Name = "修改时间", Description = "修改时间")]
        public long? ModifyDate { get; set; }

        [NotMapped]
        public Guid EntityGuid { get; set; }
    }

    /// <summary>
    /// 带Scope范围
    /// </summary>
    public class BaseEntityScope : BaseEntity
    {
        [Display(Name = "范围", Description = "范围")]
        [MaxLength(100)]
        public virtual string Scope { get; set; } = "-";
    }

    #region BaseEntityDto

    public class _BaseEntityDto : _IEntityDto<Guid>
    {
        [Display(Name = "主键", Description = "主键")]
        public virtual Guid _ID { get; set; } = Guid.Empty;

        [Display(Name = "状态", Description = "状态")]
        [DefaultValue(1)]
        public virtual UseStatusEnum _Status { get; set; } = (UseStatusEnum)1;

        [Display(Name = "新增人ID", Description = "新增人ID")]
        [MaxLength(50)]
        public Guid _CreateUserId { get; set; }

        [Display(Name = "新增人", Description = "新增人")]
        [MaxLength(20)]
        public string _CreateUserName { get; set; }

        [Display(Name = "新增时间", Description = "新增时间")]
        public DateTime _CreateDate { get; set; } = DateTime.Now;

        [Display(Name = "修改人ID", Description = "修改人ID")]
        [MaxLength(50)]
        public Guid _ModifyUserId { get; set; }

        [Display(Name = "修改人", Description = "修改人")]
        [MaxLength(20)]
        public string _ModifyUserName { get; set; }

        [Display(Name = "修改时间", Description = "修改时间")]
        public DateTime? _ModifyDate { get; set; }

        [NotMapped]
        public Guid _EntityGuid { get; set; }
    }

    #endregion
}
