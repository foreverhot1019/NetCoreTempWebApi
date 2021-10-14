using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using NetCoreTemp.WebApi.Models.Extensions;

namespace NetCoreTemp.WebApi.Models.AutoMapper
{
    public class BaseProfile<TSource, TDestion> : Profile
        where TSource : class, NetCoreTemp.WebApi.Models.BaseModel.IEntity_
        where TDestion : class, NetCoreTemp.WebApi.Models.BaseModel._IEntityDto_
    {
        /// <summary>
        /// Entity到Destion转换表达式
        /// </summary>
        protected IMappingExpression<TSource, TDestion> Src2DesMapperExp;
        /// <summary>
        /// Destion到Entity转换表达式
        /// </summary>
        protected IMappingExpression<TDestion, TSource> Des2SrcMapperExp;

        public BaseProfile()
        {
            ClearPrefixes();//去除前缀
            RecognizePrefixes("_");//识别前缀_
            Src2DesMapperExp = CreateMap<TSource, TDestion>()
                .ForMember(des => des._EntityGuid, opts => opts.Ignore())
                .ForMember(des => des._CreateDate, opts => opts.MapFrom(src => src.CreateDate.to_DateTime().Value))
                .ForMember(des => des._ModifyDate, opts => opts.MapFrom(src => src.ModifyDate.toDateTime()));
            Des2SrcMapperExp = Src2DesMapperExp.ReverseMap()
                .ForMember(src => src.CreateDate, opts => opts.MapFrom(des => des._CreateDate.to_Long()))
                .ForMember(src => src.ModifyDate, opts => opts.MapFrom(des => des._ModifyDate.toLong()));
            //排除相同值的字段不更新，，以便entity字段不设为Modified
            Des2SrcMapperExp.ForAllMembers(opts => opts.Condition((des, src, srcMember, desMember) => srcMember != desMember));

            //SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
            //DestinationMemberNamingConvention = new PascalCaseNamingConvention(); //将CreateMap 等等放在这里
        }
    }

    /// <summary>
    /// 时间与时间戳转换
    /// </summary>
    public class DateTime_Convertert : ITypeConverter<DateTime, long>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        long ITypeConverter<DateTime, long>.Convert(DateTime source, long destination, ResolutionContext context)
        {
            var dLong = source.to_Long();
            if (dLong.HasValue)
                destination = dLong.Value;
            else
                destination = 0;
            return destination;
        }
    }

    /// <summary>
    /// 时间与时间戳转换
    /// </summary>
    public class DateTimeConvertert : ITypeConverter<DateTime?, long?>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        long? ITypeConverter<DateTime?, long?>.Convert(DateTime? source, long? destination, ResolutionContext context)
        {
            destination = source.toLong();
            return destination;
        }
    }
}
