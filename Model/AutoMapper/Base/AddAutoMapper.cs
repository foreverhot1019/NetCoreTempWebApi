using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace NetCoreTemp.WebApi.Models.AutoMapper
{
    public static class AddAutoMapper
    {
        /// <summary>
        /// 注册AutoMapper
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAutoMapperService(this IServiceCollection services)
        {
            var AllType = System.Reflection.Assembly.Load("NetCoreTemp.WebApi.Models").GetTypes();
            var ArrProfileType = AllType.Where(x => !x.IsGenericType && x.IsSubclassOf(typeof(Profile)));
            services.AddAutoMapper(ArrProfileType.ToArray());
            //services.AddAutoMapper(typeof(UserProfile), typeof(RoleProfile));
            //services.AddSingleton(typeof(MyMapper<,>));

            return services;
        }
    }
}
