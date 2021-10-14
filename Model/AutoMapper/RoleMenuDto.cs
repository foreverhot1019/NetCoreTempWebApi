using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using AutoMapper;
using NetCoreTemp.WebApi.Models.BaseModel;

namespace NetCoreTemp.WebApi.Models.AutoMapper
{
    public class RoleMenuDto : _BaseEntityDto
    {
        [Display(Name = "类型", Description = "类型")]
        public string _Type { get; } = "RoleMenu";

        [Display(Name = "权限Id", Description = "权限Id")]
        [Required, StringLength(20)]
        public string _RoleId { get; set; } = "-";

        [Display(Name = "菜单Id", Description = "菜单Id")]
        [Required, StringLength(20)]
        public string _MenuId { get; set; } = "-";

        [Display(Name = "描述", Description = "描述")]
        [StringLength(100)]
        public string _Remark { get; set; }
    }

    /// <summary>
    /// 轮廓
    /// </summary>
    public class RoleMenuProfile : BaseProfile<RoleMenu, RoleMenuDto>
    {
        public RoleMenuProfile()
        {
        }
    }

    /// <summary>
    /// 映射
    /// </summary>
    public static class RoleMenuMapper
    {
        static RoleMenuMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<RoleMenuProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        /// <summary>
        /// 映射到ModelDto
        /// </summary>
        /// <param name="model">RoleMenu</param>
        /// <returns></returns>
        public static RoleMenuDto ToDto(this RoleMenu model)
        {
            return Mapper.Map<RoleMenuDto>(model);
        }

        /// <summary>
        /// 映射到EF实体
        /// </summary>
        /// <param name="model">RoleMenuDto</param>
        /// <returns></returns>
        public static RoleMenu ToEntity(this RoleMenuDto model)
        {
            return Mapper.Map<RoleMenu>(model);
        }
    }
}
