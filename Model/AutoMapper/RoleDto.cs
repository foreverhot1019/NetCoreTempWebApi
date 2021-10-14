using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NetCoreTemp.WebApi.Models.BaseModel;
using NetCoreTemp.WebApi.Models.EnumType;
using AutoMapper;

namespace NetCoreTemp.WebApi.Models.AutoMapper
{
    public class RoleDto : _BaseEntityDto
    {
        [Display(Name = "类型", Description = "类型")]
        public string _Type { get; } = "Role";

        [Display(Name = "权限名称", Description = "权限名称")]
        [Required, MaxLength(50)]
        public string _Name { get; set; }

        [Display(Name = "序号", Description = "序号")]
        [Required, Range(0, int.MaxValue)]
        public int _Sort { get; set; }

        [Display(Name = "权限描述", Description = "权限描述")]
        [StringLength(100)]
        public string _Remark { get; set; }

        [Display(Name = "菜单集合", Description = "菜单集合")]
        public IEnumerable<MenuDto> _ArrMenu { get; set; }

    }

    /// <summary>
    /// 轮廓
    /// </summary>
    public class RoleProfile : BaseProfile<Role, RoleDto>
    {
        public RoleProfile()
        {
            Src2DesMapperExp.ForMember(des => des._ArrMenu, opts => opts.MapFrom(src => src.ArrMenu));
        }
    }

    /// <summary>
    /// 映射
    /// </summary>
    public static class RoleMapper
    {
        static RoleMapper()
        {
            Mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RoleProfile>();
                cfg.AddProfile<MenuProfile>();
            }).CreateMapper();
        }

        public static IMapper Mapper { get; }

        /// <summary>
        /// 映射到ModelDto
        /// </summary>
        /// <param name="Model">Role</param>
        /// <returns></returns>
        public static RoleDto ToDto(this Role Model)
        {
            return Mapper.Map<RoleDto>(Model);
        }

        /// <summary>
        /// 映射到EF实体
        /// </summary>
        /// <param name="model">RoleDto</param>
        /// <returns></returns>
        public static Role ToEntity(this RoleDto model)
        {
            return Mapper.Map<Role>(model);
        }
    }
}
