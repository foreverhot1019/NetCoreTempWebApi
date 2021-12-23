using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace LocalizerCustomValidation
{
    public static class CustomValidation
    {
        /// <summary>
        /// 增加自定义IModelValidatorProvider验证器提供者
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMyModelValidatorProvider(this IServiceCollection services)
        {
            services.AddSingleton<IModelValidatorProvider, MyModelValidatorProvider>(sp =>
            {
                var memoryCache = sp.GetService<IMemoryCache>();
                var stringLocalizer = sp.GetService<Microsoft.Extensions.Localization.IStringLocalizer<CommonLanguage.Language>>();
                var sharedLocalizer = sp.GetService<Microsoft.Extensions.Localization.IStringLocalizerFactory>();
                return new MyModelValidatorProvider(memoryCache, stringLocalizer, sharedLocalizer);
            }).AddMyValidatorProvider2MvcConfig();
            return services;
        }

        /// <summary>
        /// MvcOptions增加自定义IModelValidatorProvider到ModelValidatorProviders
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static IServiceCollection AddMyValidatorProvider2MvcConfig(this IServiceCollection services)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            services.Configure<MvcOptions>(opts =>
            {
                var Arr = serviceProvider?.GetServices<IModelValidatorProvider>();
                //var defaultProviders = opts.ModelValidatorProviders.OfType<IModelValidatorProvider>();
                //opts.ModelValidatorProviders.Clear();
                opts.ModelValidatorProviders.Add(Arr?.FirstOrDefault());
            });
            return services;
        }
    }
}
