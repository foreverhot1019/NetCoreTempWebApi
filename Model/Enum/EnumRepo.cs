using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NetCoreTemp.WebApi.Models.EnumType
{
    /// <summary>
    /// 枚举类型
    /// </summary>
    public static class EnumRepo
    {
        /// <summary>
        /// 使用状态
        /// </summary>
        public enum UseStatusEnum
        {
            [Display(Name = "草稿", Description = "草稿")]
            Draft = 0,
            [Display(Name = "启用", Description = "启用")]
            Enable = 1,
            [Display(Name = "停用", Description = "停用")]
            Disable = -1
        }

        /// <summary>
        /// 审批状态
        /// </summary>
        public enum AuditStatusEnum
        {
            [Display(Name = "草稿", Description = "草稿")]
            Draft = 0,
            [Display(Name = "审批中", Description = "审批中")]
            Auditing = 1,
            [Display(Name = "审批通过", Description = "审批通过")]
            AuditSuccess = 2,
            [Display(Name = "审批拒绝", Description = "审批拒绝")]
            AuditFail = -1
        }

        /// <summary>
        /// 状态
        /// </summary>
        public enum UseStatusIsOrNoEnum
        {
            [Display(Name = "否", Description = "否")]
            Draft = 0,
            [Display(Name = "是", Description = "是")]
            Enable = 1
        }

        /// <summary>
        /// 数据新增类型
        /// </summary>
        public enum AddType
        {
            [Display(Name = "自动", Description = "自动产生")]
            AutoAdd = 1,
            [Display(Name = "手动", Description = "手动产生")]
            HandAdd
        }

        /// <summary>
        /// 发送状态
        /// </summary>
        public enum SendStatus
        {
            [Display(Name = "正常", Description = "正常")]
            Normal = 1,
            [Display(Name = "已发送", Description = "已发送")]
            Send,
            [Display(Name = "发送成功", Description = "发送成功")]
            SuccessFeedBack,
            [Display(Name = "发送异常", Description = "发送异常")]
            ErrorFeedBack,
        }

        /// <summary>
        /// 比较
        /// </summary>
        public enum FilterOp
        {
            [Display(Name = "等于", Description = "等于")]
            Equal = 0,
            [Display(Name = "不等于", Description = "不等于")]
            NotEqual = 1,
            [Display(Name = "小于等于", Description = "小于等于")]
            LessThanOrEqual = 2,
            [Display(Name = "小于", Description = "小于")]
            LessThan = 3,
            [Display(Name = "大于等于", Description = "大于等于")]
            GreaterThanOrEqual = 4,
            [Display(Name = "大于", Description = "大于")]
            GreaterThan = 5,
            [Display(Name = "不为空", Description = "不为空")]
            IsNotNull = 6,
            [Display(Name = "为空", Description = "为空")]
            IsNull = 7,
            [Display(Name = "含有", Description = "含有")]
            Contains = 8,
            [Display(Name = "不含有", Description = "不含有")]
            NotContains = 9,
            [Display(Name = "起始包含", Description = "起始包含")]
            BeginsWith = 10,
            [Display(Name = "在", Description = "在")]
            In = 11,
            [Display(Name = "介于", Description = "介于")]
            Between = 12
        }

        /// <summary>
        /// filterRule 值类型
        /// </summary>
        public enum FilterValueType
        {
            String=0,
            Number,
            Boolean,
            Datetime
        }
    }

    /// <summary>
    /// 枚举类
    /// </summary>
    public class EnumModelType
    {
        public EnumModelType()
        {

        }

        /// <summary>
        /// Session/Cache 枚举键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Session/Cache 枚举键值 如果有的话
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Session/Cache 枚举键显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Session/Cache 枚举键显示描述
        /// </summary>
        public string DisplayDescription { get; set; }

    }

}
