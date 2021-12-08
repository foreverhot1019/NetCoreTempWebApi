using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NetCoreTemp.WebApi.Models.BaseModel;
using NetCoreTemp.WebApi.Models.EnumType;
using AutoMapper;
using NetCoreTemp.WebApi.Models.Extensions;

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
            Des2SrcMapperExp.ForMember(des => des.ArrMenu, opts => opts.MapFrom(src => src._ArrMenu));
        }
    }

    ////AutoMapper.Extensions.Microsoft.DenpendencyInjection方式 不能使用 继承BaseProfile<UserRole, UserRoleDto>方式
    //public class RoleProfile : Profile
    //{
    //    /// <summary>
    //    /// Entity到Destion转换表达式
    //    /// </summary>
    //    protected IMappingExpression<Role, RoleDto> Src2DesMapperExp;
    //    /// <summary>
    //    /// Destion到Entity转换表达式
    //    /// </summary>
    //    protected IMappingExpression<RoleDto, Role> Des2SrcMapperExp;

    //    public RoleProfile()
    //    {
    //        ClearPrefixes();//去除前缀
    //        RecognizePrefixes("_");//识别前缀_
    //        Src2DesMapperExp = CreateMap<Role, RoleDto>()
    //            .ForMember(des => des._EntityGuid, opts => opts.Ignore())
    //            .ForMember(des => des._CreateDate, opts => opts.MapFrom(src => src.CreateDate.to_DateTime().Value))
    //            .ForMember(des => des._ModifyDate, opts => opts.MapFrom(src => src.ModifyDate.toDateTime()))
    //            .ForMember(des => des._ArrMenu, opts => opts.MapFrom(src => src.ArrMenu));
    //        Des2SrcMapperExp = Src2DesMapperExp.ReverseMap()
    //            .ForMember(src => src.CreateDate, opts => opts.MapFrom(des => des._CreateDate.to_Long()))
    //            .ForMember(src => src.ModifyDate, opts => opts.MapFrom(des => des._ModifyDate.toLong()))
    //            .ForMember(des => des.ArrMenu, opts => opts.MapFrom(src => src._ArrMenu));
    //        //排除相同值的字段不更新，，以便entity字段不设为Modified
    //        Des2SrcMapperExp.ForAllMembers(opts => opts.Condition((des, src, srcMember, desMember) => srcMember != desMember));

    //        //SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
    //        //DestinationMemberNamingConvention = new PascalCaseNamingConvention(); //将CreateMap 等等放在这里
    //    }
    //}

    /// <summary>
    /// 映射
    /// </summary>
    public static class RoleMapper
    {
        //只有在同一程序集的文件中，内部类型或成员才可访问
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
