using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Helpdesk.WebApi.Models.BaseModel
{
    public interface IEntity<TKey> : IEntity_
    {
        /// <summary>
        /// 主键
        /// </summary>
        TKey ID { 
            get { return (TKey)_ID; } 
            set { _ID = value; } 
        }
    }

    public interface IEntity_
    {
        Object _ID { get; set; }

        [NotMapped]
        Guid EntityGuid { get; set; }

        /// <summary>
        /// 新增时间
        /// </summary>
        Int64 CreateDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        Int64? ModifyDate { get; set; }
    }

    /// <summary>
    /// 给AutoMapper使用
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface _IEntityDto<TKey> : _IEntityDto_
    {
        /// <summary>
        /// 主键
        /// </summary>
        TKey _ID { get; set; }
    }
    public interface _IEntityDto_
    {
        [NotMapped]
        Guid _EntityGuid { get; set; }

        /// <summary>
        /// 新增时间
        /// </summary>
        DateTime _CreateDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        DateTime? _ModifyDate { get; set; }
    }
}
