using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using AutoMapper;
using NetCoreTemp.WebApi.Models.BaseModel;

namespace NetCoreTemp.WebApi.Models.AutoMapper
{
    public class UserRoleDto : _BaseEntityDto
    {
        [Display(Name = "类型", Description = "类型")]
        public string _Type { get; } = "UserRole";

        [Display(Name = "账户Id", Description = "账户Id")]
        [Required, StringLength(20)]
        public string _UserId { get; set; } = "-";

        [Display(Name = "权限Id", Description = "权限Id")]
        [Required, StringLength(20)]
        public string _RoleId { get; set; } = "-";

        [Display(Name = "描述", Description = "描述")]
        [StringLength(100)]
        public string _Remark { get; set; }
    }

    /// <summary>
    /// 轮廓
    /// </summary>
    public class UserRoleProfile : BaseProfile<UserRole, UserRoleDto>
    {
        public UserRoleProfile()
        {
        }
    }

    /// <summary>
    /// 映射
    /// </summary>
    public static class UserRoleMapper
    {
        static UserRoleMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserRoleProfile>())
                .CreateMapper();
        }

        //只有在同一程序集的文件中，内部类型或成员才可访问
        internal static IMapper Mapper { get; }

        /// <summary>
        /// 映射到ModelDto
        /// </summary>
        /// <param name="model">UserRole</param>
        /// <returns></returns>
        public static UserRoleDto ToDto(this UserRole model)
        {
            return Mapper.Map<UserRoleDto>(model);
        }

        /// <summary>
        /// 映射到EF实体
        /// </summary>
        /// <param name="model">UserRoleDto</param>
        /// <returns></returns>
        public static UserRole ToEntity(this UserRoleDto model)
        {
            return Mapper.Map<UserRole>(model);
        }
    }
}
