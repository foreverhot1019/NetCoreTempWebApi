using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.WebApi.Services.Base
{
    public static class AddServices
    {
        /// <summary>
        /// 注册AutoMapper
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomerServices(this IServiceCollection services)
        {
            var AllType = System.Reflection.Assembly.Load("Helpdesk.WebApi.Services").GetTypes();
            //var type = typeof(BaseService<>);
            // && (x.IsSubclassOf(type) || x.IsInstanceOfType(type) ||x.IsAssignableFrom(type))
            var ArrServiceType = AllType.Where(x => !x.IsGenericType && x.BaseType?.Name.StartsWith("BaseService`1") == true);
            foreach (var service in ArrServiceType)
                services.AddScoped(service);

            return services;
        }
    }
}
