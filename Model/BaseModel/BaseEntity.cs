﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Helpdesk.WebApi.Models.Extensions;
using static Helpdesk.WebApi.Models.EnumType.EnumRepo;

namespace Helpdesk.WebApi.Models.BaseModel
{
    //sealed 不能被继承
    //internal 当前程序集
    //protected 只有在继承的子类中可访问，可以跨程序集
    public class BaseEntity : IEntity<string>
    {
        //[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "主键", Description = "主键")]
        [Required, MaxLength(50)]
        public virtual string ID { get { return _ID.ToString(); } set { _ID = value; } }

        [NotMapped]
        public Object _ID { get; set; } = "-";

        [Display(Name = "状态", Description = "状态")]
        [DefaultValue(1)]
        public virtual UseStatusEnum Status { get; set; } = (UseStatusEnum)1;

        [Display(Name = "新增人ID", Description = "新增人ID")]
        [MaxLength(50)]
        public string CreateUserId { get; set; }

        [Display(Name = "新增人", Description = "新增人")]
        [MaxLength(20)]
        public string CreateUserName { get; set; }

        [Display(Name = "新增时间", Description = "新增时间")]
        public virtual Int64 CreateDate { get; set; } = DateTime.Now.to_Long().Value;

        [Display(Name = "修改人ID", Description = "修改人ID")]
        [MaxLength(50)]
        public string ModifyUserId { get; set; }

        [Display(Name = "修改人", Description = "修改人")]
        [MaxLength(20)]
        public string ModifyUserName { get; set; }

        [Display(Name = "修改时间", Description = "修改时间")]
        public Int64? ModifyDate { get; set; }

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

    public class _BaseEntityDto : _IEntityDto<string>
    {
        [Display(Name = "主键", Description = "主键")]
        public virtual string _ID { get; set; } = "-";

        [Display(Name = "状态", Description = "状态")]
        [DefaultValue(1)]
        public virtual UseStatusEnum _Status { get; set; } = (UseStatusEnum)1;

        [Display(Name = "新增人ID", Description = "新增人ID")]
        [MaxLength(50)]
        public string _CreateUserId { get; set; }

        [Display(Name = "新增人", Description = "新增人")]
        [MaxLength(20)]
        public string _CreateUserName { get; set; }

        [Display(Name = "新增时间", Description = "新增时间")]
        public DateTime _CreateDate { get; set; } = DateTime.Now;

        [Display(Name = "修改人ID", Description = "修改人ID")]
        [MaxLength(50)]
        public string _ModifyUserId { get; set; }

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
