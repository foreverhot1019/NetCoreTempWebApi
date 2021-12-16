using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

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
            //services.AddRazorPages()
            //.AddMvcOptions(options =>
            //{
            //    options.MaxModelValidationErrors = 50;
            //    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            //        _ => "字段 {0} 是必须的.");
            //});
            services.AddRazorPages()
                .AddMvcOptions(options =>
                {
                    options.MaxModelValidationErrors = 50;
                    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                        _ => "The field is required.");
                });

            services.AddSingleton<IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider>();
            services.AddControllersWithViews(opts =>
            {
                //opts.ModelValidatorProviders.
                opts.MaxModelValidationErrors = 50;
                opts.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
                    _ => "字段 {0} 是必须的.");
                //opts.ModelValidatorProviders.Add(CustomValidationAttributeAdapterProvider);
            });
            //services.AddRazorPages()
            //.AddMvcOptions(options =>
            //{
            //    options.MaxModelValidationErrors = 50;
            //    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            //        _ => "字段 {0} 是必须的.");
            //}); ;
            //services.AddSingleton<Microsoft.AspNetCore.Mvc.DataAnnotations.IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class CustomValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private readonly IValidationAttributeAdapterProvider baseProvider =
            new ValidationAttributeAdapterProvider();

        public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute,
            IStringLocalizer stringLocalizer)
        {
            //if (attribute is ClassicMovieAttribute classicMovieAttribute)
            //{
            //    return new ClassicMovieAttributeAdapter(classicMovieAttribute, stringLocalizer);
            //}

            return baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
        }
    }

    public class CustomValidateProvider
        : ValidationAttributeAdapterProvider, IValidationAttributeAdapterProvider
    {
        public CustomValidateProvider()
        {

        }

        IAttributeAdapter IValidationAttributeAdapterProvider.GetAttributeAdapter(
            ValidationAttribute attribute,
            IStringLocalizer stringLocalizer)
        {
            IAttributeAdapter adapter;
            switch (attribute)
            {
                case EmailAddressAttribute emailAttr:
                    attribute.ErrorMessage = "{0}邮箱格式不正确.";
                    adapter = base.GetAttributeAdapter(attribute, stringLocalizer);
                    break;
                case RequiredAttribute rqAttr:
                    attribute.ErrorMessage = "字段 {0} 是必填项.";
                    adapter = base.GetAttributeAdapter(attribute, stringLocalizer);
                    break;

                case StringLengthAttribute slenAttr:
                case MaxLengthAttribute mlenAttr:
                    attribute.ErrorMessage = "字段 {0} 是必填 介于{1}-{2}之间.";
                    adapter = base.GetAttributeAdapter(attribute, stringLocalizer);
                    break;
                case CompareAttribute cmpAttr:
                //attribute.ErrorMessageResourceName = "InvalidCompare";
                //attribute.ErrorMessageResourceType = typeof(System.Resources.ValidationMessages);
                //adapter = new Compa CompareAttributeAdapter(cmpAttr, stringLocalizer);
                //break;
                default:
                    adapter = base.GetAttributeAdapter(attribute, stringLocalizer);
                    break;
            }

            return adapter;
        }
    }



    public class IPAddressOrHostnameAttributeAdapter : AttributeAdapterBase<RequiredAttribute>
    {
        public IPAddressOrHostnameAttributeAdapter(RequiredAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer)
        { }

        public override void AddValidation(ClientModelValidationContext context) { }

        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
        }
    }

    public class IPAddressOrHostnameAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private readonly IValidationAttributeAdapterProvider fallback = new ValidationAttributeAdapterProvider();

        public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer stringLocalizer)
        {
            var attr = attribute as RequiredAttribute;
            return attr == null ?
                this.fallback.GetAttributeAdapter(attribute, stringLocalizer) :
                new IPAddressOrHostnameAttributeAdapter(attr, stringLocalizer);
        }
    }

    public partial class Test
    {
        public int a { get; set; }
        public string b { get; set; }
        public string c { get; set; }
    }

    [ModelMetadataType(typeof(TestMetadata))]
    public partial class Test { }

    public class TestMetadata
    {
        [Range(0,9)]
        public int a { get; set; }
     
        [Required]
        public string b { get; set; }

        [MaxLength(10)]
        public string c { get; set; }

    }
}
