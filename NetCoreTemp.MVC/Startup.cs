using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace NetCoreTemp.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(x => x.ResourcesPath = "Resources");
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix) //����razor �����ʱ��������
                .AddDataAnnotationsLocalization(); //��Ҫ��д DataAnnotationLocalizerProvider��������Ҫ�Լ����� ��Դ�ļ�
            services.AddMemoryCache();

            //��ͼ��ʾ�ṩ�� ������д Asp.NetCore Localizar ��ʵ��
            //services.AddSingleton<Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider, My_ModelMetadataProvider>();

            #region �Զ�����֤��

            IServiceProvider serviceProvider = null;
            services.AddSingleton<IModelValidatorProvider, MyModelValidatorProvider>(sp =>
            {
                serviceProvider = sp;
                var memoryCache = sp.GetService<IMemoryCache>();
                var stringLocalizer = sp.GetService<Microsoft.Extensions.Localization.IStringLocalizer>();
                var sharedLocalizer = sp.GetService<Microsoft.Extensions.Localization.IStringLocalizer<CommonLanguage.Language>>();
                return new MyModelValidatorProvider(memoryCache, stringLocalizer, sharedLocalizer);
            });

            services.Configure<MvcOptions>(opts =>
            {
                var Arr = serviceProvider?.GetServices<IModelValidatorProvider>();
                var defaultProviders = opts.ModelValidatorProviders.OfType<IModelValidatorProvider>();
                opts.ModelValidatorProviders.Clear();
                opts.ModelValidatorProviders.Add(Arr?.FirstOrDefault());
            });

            #endregion
            services.Configure<RequestLocalizationOptions>(actLocalizationOpts);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            //Ϊ�� ʵ��ConfigServices��� serviceProvider��ʵ��
            serviceProvider.GetService<IModelValidatorProvider>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            #region ���ʻ�

            var options = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>();
            if (options?.Value != null)
                app.UseRequestLocalization(options.Value);
            else
                app.UseRequestLocalization(actLocalizationOpts);

            #endregion

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// ���ʻ�����
        /// </summary>
        Action<RequestLocalizationOptions> actLocalizationOpts = new Action<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("zh-cn"),
                new CultureInfo("en-US"),
                new CultureInfo("zh-tw"),
                new CultureInfo("ja-jp")
            };

            options.DefaultRequestCulture = new RequestCulture(culture: supportedCultures[0], uiCulture: supportedCultures[0]);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.AddInitialRequestCultureProvider(new CustomRequestCultureProvider(async context =>
            {
                var lang = context.Request.Headers["Content-Language"].FirstOrDefault();

                if (!string.IsNullOrEmpty(lang))
                {
                    var culture = new CultureInfo(lang);
                    if (culture != null && supportedCultures.Any(x => x.Equals(culture)))
                    {
                        //Ĭ�϶�ȡ accept-language
                        var result = new ProviderCultureResult(culture.Name);
                        // My custom request culture logic
                        return new ProviderCultureResult(lang);
                    }
                }
                return null;
            }));
            #region ���� ���ʻ� Cookie ���� CookieName=c=ja-jp|uic= ��ʽ ������ c={culture}|uic={culture}

            // Find the cookie provider with LINQ
            var cookieProvider = options.RequestCultureProviders.OfType<CookieRequestCultureProvider>().First();
            // Set the new cookie name
            cookieProvider.CookieName = "FinchCulture";

            #endregion
        });
    }
}
