using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static NetCoreTemp.WebApi.Models.EnumType.EnumRepo;

namespace NetCoreTemp.WebApi.Models.BaseModel
{
    /// <summary>
    /// Entity接口
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey> : IEntity_
    {
        /// <summary>
        /// 主键
        /// </summary>
        TKey ID { 
            get { return (TKey)_ID; } 
            set { _ID = value; }
        }

        /// <summary>
        /// 新增人ID
        /// </summary>
        TKey CreateUserId { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        TKey ModifyUserId { get; set; }
    }

    /// <summary>
    ///  Entity-base接口
    /// </summary>
    public interface IEntity_
    {
        Object _ID { get; set; }

        [NotMapped]
        Guid EntityGuid { get; set; }

        /// <summary>
        /// 新增人
        /// </summary>
        string CreateUserName { get; set; }

        /// <summary>
        /// 新增时间
        /// </summary>
        long CreateDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        string ModifyUserName { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        long? ModifyDate { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        UseStatusEnum Status { get; set; }
    }

    /// <summary>
    /// Dto接口
    /// 给AutoMapper使用
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface _IEntityDto<TKey> : _IEntityDto_
    {
        /// <summary>
        /// 主键
        /// </summary>
        TKey _ID { get; set; }

        /// <summary>
        /// 新增人ID
        /// </summary>
        TKey _CreateUserId { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        TKey _ModifyUserId { get; set; }
    }

    /// <summary>
    /// Dto-base接口
    /// </summary>
    public interface _IEntityDto_
    {
        [NotMapped]
        Guid _EntityGuid { get; set; }

        /// <summary>
        /// 新增人
        /// </summary>
        string _CreateUserName { get; set; }

        /// <summary>
        /// 新增时间
        /// </summary>
        DateTime _CreateDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        string _ModifyUserName { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        DateTime? _ModifyDate { get; set; }
    }
}
