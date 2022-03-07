using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace NetCoreTemp.WebApi.Services.AOP
{
    public static class FodyDIService
    {
        private static IServiceProvider _serviceProvider;

        public static void Configration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取BaseService
        /// </summary>
        /// <typeparam name="TEntity">Entity类</typeparam>
        /// <returns></returns>
        public static Base.BaseService<TEntity> GetBaseService<TEntity>() where TEntity : class, NetCoreTemp.WebApi.Models.BaseModel.IEntity_
        {
            return _serviceProvider.GetService<Base.BaseService<TEntity>>();
        }

        /// <summary>
        /// 获取Entity服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService GetIService<TService>() where TService : class, Base.IEntityService
        {
            return _serviceProvider.GetService<TService>();
        }

        /// <summary>
        /// 获取日志服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static ILogger<T> GetILogger<T>() where T : class, new()
        {
            return _serviceProvider.GetService<ILogger<T>>();
        }
    }
}
