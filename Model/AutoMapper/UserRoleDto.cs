using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using AutoMapper;
using NetCoreTemp.WebApi.Models.BaseModel;
using NetCoreTemp.WebApi.Models.Entity;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models.AutoMapper
{
    public class UserRoleDto : _BaseEntityDto
    {
        [Display(Name = "账户Id", Description = "账户Id")]
        [Required, StringLength(20)]
        public Guid _UserId { get; set; } = Guid.Empty;

        [Display(Name = "权限Id", Description = "权限Id")]
        [Required, StringLength(20)]
        public Guid _RoleId { get; set; } = Guid.Empty;

        [Display(Name = "描述", Description = "描述")]
        [StringLength(100)]
        public string _Remark { get; set; }

        [Display(Name = "账户", Description = "账户")]
        public User _OUser { get; set; }

        [Display(Name = "权限", Description = "权限")]
        public Role _ORole { get; set; }
    }

    /// <summary>
    /// 轮廓
    /// </summary>
    public class UserRoleProfile : BaseProfile<UserRole, UserRoleDto>
    {
        public UserRoleProfile()
        {
            this.Src2DesMapperExp
                .ForMember(dto => dto._OUser, opts => opts.MapFrom(src => src.OUser))
                .ForMember(dto => dto._ORole, opts => opts.MapFrom(src => src.ORole));
            this.Des2SrcMapperExp
                .ForMember(src => src.OUser, opts => opts.MapFrom(dto => dto._OUser))
                .ForMember(src => src.ORole, opts => opts.MapFrom(dto => dto._ORole));
        }
    }

    ////AutoMapper.Extensions.Microsoft.DenpendencyInjection方式 不能使用 继承BaseProfile<UserRole, UserRoleDto>方式
    //public class UserRoleProfile : Profile
    //{
    //    /// <summary>
    //    /// Entity到Destion转换表达式
    //    /// </summary>
    //    protected IMappingExpression<UserRole, UserRoleDto> Src2DesMapperExp;
    //    /// <summary>
    //    /// Destion到Entity转换表达式
    //    /// </summary>
    //    protected IMappingExpression<UserRoleDto, UserRole> Des2SrcMapperExp;

    //    public UserRoleProfile()
    //    {
    //        ClearPrefixes();//去除前缀
    //        RecognizePrefixes("_");//识别前缀_
    //        Src2DesMapperExp = CreateMap<UserRole, UserRoleDto>()
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
