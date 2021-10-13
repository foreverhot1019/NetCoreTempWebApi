using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Helpdesk.WebApi.Models.AutoMapper
{
    /// <summary>
    /// 统一转换
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public class MyMapper<TSource, TDestination>
    {
        protected readonly IMapper _Mapper; 
        public MyMapper(IMapper mapper)
        {
            _Mapper = mapper;
        }
        public TSource ToSource(TDestination entity)
        {
            return _Mapper.Map<TSource>(entity);
        }
        public TDestination ToDestination(TSource model)
        {
            return _Mapper.Map<TDestination>(model);
        }
    }
}
