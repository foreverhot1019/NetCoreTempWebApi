using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using AutoMapper;
using NetCoreTemp.WebApi.Models;
using NetCoreTemp.WebApi.Models.BaseModel;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models.AutoMapper
{
    /// <summary>
    /// 账户Dto
    /// 字段增加前缀_
    /// </summary>
    public class UserDto : _BaseEntityDto
    {
        [Display(Name = "账户名称", Description = "账户名称")]
        [Required, MaxLength(50)]
        public string _Name { get; set; }

        [Display(Name = "邮件", Description = "邮件")]
        [Required, MaxLength(100)]
        public string _Email { get; set; }

        [Display(Name = "密码", Description = "密码")]
        [Required, MaxLength(100)]
        public string _Password { get; set; }

        [Display(Name = "账户权限", Description = "账户权限")]
        public List<string> _Roles { get; set; }

        [Display(Name = "账户权限", Description = "账户权限")]
        public IEnumerable<Role> _ArrRole { get; set; }
    }

    /// <summary>
    /// AutoMapper轮廓
    /// AutoMapper.Extensions.Microsoft.DenpendencyInjection方式 不能使用 继承BaseProfile<UserRole, UserRoleDto>方式
    /// </summary>
    public class UserProfile : BaseProfile<User, UserDto>
    {
        Func<string, IEnumerable<string>> string2List = new Func<string, IEnumerable<string>>(str =>
        {
            if (string.IsNullOrEmpty(str))
                return new List<string>();
            else
                return str.Split(",").ToList();
        });
        public UserProfile()
        {
            this.Src2DesMapperExp
                .ForMember(dto => dto._Roles, opts => opts.MapFrom(src => string2List(src.Roles)))
                .ForMember(dto => dto._ArrRole, opts => opts.MapFrom(src => src.ArrRole));
            this.Des2SrcMapperExp
                .ForMember(src => src.Roles, opts => opts.MapFrom(dto => dto._Roles == null ? "" : string.Join(",", dto._Roles)))
                .ForMember(src => src.ArrRole, opts => opts.MapFrom(dto => dto._ArrRole));

            //ClearPrefixes();
            //RecognizePrefixes("_");//去除前缀_
            //var mapper = CreateMap<User, UserDto>()
            //    .ForMember(des => des._EntityGuid, opts => opts.Ignore())
            //    .ForMember(des => des._CreateDate, opts => opts.MapFrom(src => src.CreateDate.to_DateTime().Value))
            //    .ForMember(des => des._ModifyDate, opts => opts.MapFrom(src => src.ModifyDate.toDateTime()))
            //    .ForMember(des => des._Roles, opts => opts.MapFrom(src => string2List(src.Roles)))
            //    .ReverseMap()
            //    .ForMember(src => src.CreateDate, opts => opts.MapFrom(des => des._CreateDate.to_Long()))
            //    .ForMember(src => src.ModifyDate, opts => opts.MapFrom(des => des._ModifyDate.toLong()))
            //    .ForMember(src => src.Roles, opts => opts.MapFrom(des => des._Roles == null ? "" : string.Join(",", des._Roles)));
            ////SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
            ////DestinationMemberNamingConvention = new PascalCaseNamingConvention(); //将CreateMap 等等放在这里
        }
    }

    public static class UserMapper
    {
        static UserMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>())
                .CreateMapper();
        }

        //只有在同一程序集的文件中，内部类型或成员才可访问
        internal static IMapper Mapper { get; }

        /// <summary>
        /// 映射到ModelDto
        /// </summary>
        /// <param name="IdsModel">IdentityServer4.Models.Client</param>
        /// <returns></returns>
        public static UserDto ToDto(this User Model)
        {
            return Mapper.Map<UserDto>(Model);
        }

        /// <summary>
        /// 映射到EF实体
        /// </summary>
        /// <param name="model">Client ViewModel</param>
        /// <returns></returns>
        public static User ToEntity(this UserDto model)
        {
            return Mapper.Map<User>(model);
        }
    }
}
