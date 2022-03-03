using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace NetCoreTemp.WebApi.Services
{
    /// <summary>
    /// Aspect AOP属性注入
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, Inherited = false)]
    public class AOPLogAttribute : AbstractInterceptorAttribute
    {
        //作为全局拦截器时
        //ps : 只有使用 config.Interceptors.AddTyped<CustomInterceptorAttribute>(); 时，属性注入才生效， 
        //     不能使用以下这种方式 services.AddSingleton<CustomInterceptorAttribute>(); + services.ConfigureDynamicProxy(config => { config.Interceptors.AddServiced<CustomInterceptorAttribute>(); });
        //ps : services.AddSingleton<CustomInterceptorAttribute>(); + services.ConfigureDynamicProxy(config => { config.Interceptors.AddServiced<CustomInterceptorAttribute>(); });时，使用如下注入
        //public AOPLogAttribute(ILogger<AOPLogAttribute> logger, ILogger<IUnitOfWork> unitofwork, IJobLogService jobLogService)
        [FromServiceContext]
        private ILogger<AOPLogAttribute> _logger { get; set; }

        /// <summary>
        /// 模块名称
        /// 作为全局拦截器时，无法读取到值，必须使用反射获取
        /// </summary>
        public string ModualName { get; set; }

        /// <summary>
        /// 事件描述
        /// 作为全局拦截器时，无法读取到值，必须使用反射获取
        /// </summary>
        public string Description { get; set; }

        ///// <summary>
        ///// 构造函数依赖注入
        ///// services.AddSingleton<CustomInterceptorAttribute>(); + services.ConfigureDynamicProxy(config => { config.Interceptors.AddServiced<CustomInterceptorAttribute>(); });
        ///// https://github.com/dotnetcore/AspectCore-Framework/blob/master/docs/1.%E4%BD%BF%E7%94%A8%E6%8C%87%E5%8D%97.md
        ///// </summary>
        ///// <param name="logger"></param>
        ///// <param name="jobLogService"></param>
        //public AOPLogAttribute(ILogger<AOPLogAttribute> logger)
        //    :this()
        //{
        //    _logger = logger;
        //}

        /// <summary>
        /// 执行代理方法
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            DateTime dtElapsed = DateTime.Now;
            var _ModualName = ModualName;
            var _Description = Description;
            AOPLogAttribute AOPLogattr = null;

            try
            {
                //全局注入时，反射读取 属性
                AOPLogattr = getAOPLogAttribute(context);
                if (string.IsNullOrEmpty(ModualName) && AOPLogattr != null)
                {
                    _ModualName = AOPLogattr.ModualName ?? context.ImplementationMethod?.DeclaringType?.Name;
                }
                //全局注入时，反射读取 属性
                if (string.IsNullOrEmpty(Description) && AOPLogattr != null)
                {
                    _Description = AOPLogattr.Description ?? context.ImplementationMethod?.Name;
                }
                var MsgStr = $"{_ModualName}- {_Description} Start Excute";
                _logger.LogInformation(MsgStr);
                await next(context);
            }
            catch (Exception ex)
            {
                //全局注入时，反射读取 属性
                if (AOPLogattr == null)
                    AOPLogattr = getAOPLogAttribute(context);
                if (string.IsNullOrEmpty(ModualName) && AOPLogattr != null)
                {
                    _ModualName = AOPLogattr.ModualName ?? context.ImplementationMethod?.DeclaringType?.Name;
                }
                //全局注入时，反射读取 属性
                if (string.IsNullOrEmpty(Description) && AOPLogattr != null)
                {
                    _Description = AOPLogattr.Description ?? context.ImplementationMethod?.Name;
                }
                _logger.LogError(ex, $"{_ModualName}- {_Description} Excute Exception：{ex.InnerException?.Message ?? ex.Message}");
                throw;
            }
            finally
            {
                //全局注入时，反射读取 属性
                if (AOPLogattr == null)
                    AOPLogattr = getAOPLogAttribute(context);
                if (string.IsNullOrEmpty(ModualName) && AOPLogattr != null)
                {
                    _ModualName = AOPLogattr.ModualName ?? context.ImplementationMethod?.DeclaringType?.Name;
                }
                if (string.IsNullOrEmpty(Description) && AOPLogattr != null)
                {
                    _Description = AOPLogattr.Description ?? context.ImplementationMethod?.Name;
                }
                var MsgStr = $"{_ModualName}- {_Description} End Excute(Elapsed:{DateTime.Now.Subtract(dtElapsed).TotalSeconds}s)";
                _logger.LogInformation(MsgStr);
            }
        }

        /// <summary>
        /// 获取AopLog属性
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private AOPLogAttribute getAOPLogAttribute(AspectContext context)
        {
            AOPLogAttribute AOPLogattr = null;
            if (string.IsNullOrEmpty(ModualName) || string.IsNullOrEmpty(Description))
            {
                if (AOPLogattr == null)
                    //全局注入时，反射读取 属性
                    AOPLogattr = (AOPLogAttribute)context.ImplementationMethod?.GetCustomAttributes(typeof(AOPLogAttribute), false).FirstOrDefault();//全局注入时，反射读取 属性

            }
            return AOPLogattr;
        }
    }
}
