using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using AutoMapper;
using NetCoreTemp.WebApi.Models.BaseModel;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models.AutoMapper
{
    public class RoleMenuDto : _BaseEntityDto
    {
        [Display(Name = "权限Id", Description = "权限Id")]
        [Required]
        public Guid _RoleId { get; set; } = Guid.Empty;

        [Display(Name = "菜单Id", Description = "菜单Id")]
        [Required]
        public Guid _MenuId { get; set; } = Guid.Empty;

        [Display(Name = "描述", Description = "描述")]
        [StringLength(100)]
        public string _Remark { get; set; }

        [Display(Name = "权限")]
        public Role _ORole { get; set; }

        [Display(Name = "菜单")]
        public Menu _OMenu { get; set; }
    }

    /// <summary>
    /// 轮廓
    /// </summary>
    public class RoleMenuProfile : BaseProfile<RoleMenu, RoleMenuDto>
    {
        public RoleMenuProfile()
        {
            Src2DesMapperExp
                .ForMember(dto => dto._ORole, opts => opts.MapFrom(src => src.ORole))
                .ForMember(dto => dto._OMenu, opts => opts.MapFrom(src => src.OMenu));
            Des2SrcMapperExp
                .ForMember(src => src.ORole, opts => opts.MapFrom(dto => dto._ORole))
                .ForMember(src => src.OMenu, opts => opts.MapFrom(dto => dto._OMenu));
        }
    }

    ////AutoMapper.Extensions.Microsoft.DenpendencyInjection方式 不能使用 继承BaseProfile<UserRole, UserRoleDto>方式
    //public class RoleMenuProfile : Profile
    //{
    //    /// <summary>
    //    /// Entity到Destion转换表达式
    //    /// </summary>
    //    protected IMappingExpression<RoleMenu, RoleMenuDto> Src2DesMapperExp;
    //    /// <summary>
    //    /// Destion到Entity转换表达式
    //    /// </summary>
    //    protected IMappingExpression<RoleMenuDto, RoleMenu> Des2SrcMapperExp;

    //    public RoleMenuProfile()
    //    {
    //        ClearPrefixes();//去除前缀
    //        RecognizePrefixes("_");//识别前缀_
    //        Src2DesMapperExp = CreateMap<RoleMenu, RoleMenuDto>()
    //            .ForMember(des => des._EntityGuid, opts => opts.Ignore())
    //            .ForMember(des => des._CreateDate, opts => opts.MapFrom(src => src.CreateDate.to_DateTime().Value))
    //            .ForMember(des => des._ModifyDate, opts => opts.MapFrom(src => src.ModifyDate.toDateTime()));
    //        Des2SrcMapperExp = Src2DesMapperExp.ReverseMap()
    //            .ForMember(src => src.CreateDate, opts => opts.MapFrom(des => des._CreateDate.to_Long()))
    //            .ForMember(src => src.ModifyDate, opts => opts.MapFrom(des => des._ModifyDate.toLong()));
    //        //排除相同值的字段不更新，，以便entity字段不设为Modified
    //        Des2SrcMapperExp.ForAllMembers(opts => opts.Condition((des, src, srcMember, desMember) => srcMember != desMember));

    //        //SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
    //        //DestinationMemberNamingConvention = new PascalCaseNamingConvention(); //将CreateMap 等等放在这里
    //    }
    //}

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

        //只有在同一程序集的文件中，内部类型或成员才可访问
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
