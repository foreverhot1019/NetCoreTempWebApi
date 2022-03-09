using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Models.AutoMapper;
using NetCoreTemp.WebApi.Models.Extensions;
using NetCoreTemp.WebApi.Models;
using NetCoreTemp.WebApi.Extensions;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NetCoreTemp.WebApi.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using NetCoreTemp.WebApi.Services.Base;
using Microsoft.EntityFrameworkCore;
using NetCoreTemp.WebApi.Models.DatabaseContext;
using StackExchange.Redis;
using RedisHelp;
using Quartz;
using Quartz.Impl;
using System.Reflection;
using Quartz.Spi;
using NetCoreTemp.WebApi.QuartzJobScheduler;
using NetCoreTemp.WebApi.QuartzJobScheduler.Job;
using NetCoreTemp.WebApi.WXKF;
using NetCoreTemp.WebApi.QuartzScheduler;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using LocalizerCustomValidation;

namespace NetCoreTemp.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebEnv = env;
            string appRoot = env.ContentRootPath;
            Environment.SetEnvironmentVariable("DataDirectory", Path.Combine(appRoot, "App_Data"));
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebEnv { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(opts =>
            {
                opts.Filters.Add<ApiBaseExceptionFilter>();//全局异常处理
                opts.Filters.Add<ApiBaseAuthorizeFilter>();//全局权限认证
                opts.Filters.Add<ApiBaseActionFilter>();//全局Action统一格式返回
                //opts.ModelValidatorProviders.Add(new MyModelValidatorProvider());
            }).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix) //启用razor 引擎的时候起作用
            .AddDataAnnotationsLocalization(opts =>
            {
                opts.DataAnnotationLocalizerProvider = (type, factory) =>
                {
                    Type _type = typeof(CommonLanguage.Language);
                    var asm = type.Assembly;
                    if (type.BaseType == typeof(Models.BaseModel.BaseEntity) || type.BaseType == typeof(Models.BaseModel._BaseEntityDto))
                    {
                        var typename = type.FullName.Replace("NetCoreTemp.WebApi.Models.", "NetCoreTemp.WebApi.Models.Resources.");
                        var _rtype = asm.GetType(typename);
                        if (_rtype != null)
                            _type = _rtype;
                    }
                    if (type.Name.IndexOf("Controller") >= 0)
                    {
                        _type = typeof(CommonLanguage.MVCLang.MvcResources);
                    }
                    return factory.Create(_type);
                };
            }); //不要重写 DataAnnotationLocalizerProvider，否则需要自己管理 资源文件
            //.AddJsonOptions(opts =>
            //{
            //    //驼峰
            //    opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            //    opts.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            //});

            //ModelState 验证错误 统一格式返回
            services.Configure<ApiBehaviorOptions>(ApiBhvOpts =>
            {
                ////关闭默认模型验证
                //ApiBhvOpts.SuppressModelStateInvalidFilter = true;
                ApiBhvOpts.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    //foreach (var modelState in actionContext.ModelState.Values)
                    //{
                    //    foreach (var error in modelState.Errors)
                    //    {
                    //        var err = error.ErrorMessage;
                    //    }
                    //}
                    var ArrError = actionContext.ModelState.Values.Select(x => x.AttemptedValue + string.Join(",", x.Errors.Select(n => n.ErrorMessage)));
                    return new JsonResult(new ActionReturnMessage { StatusCode = System.Net.HttpStatusCode.Forbidden, IsSuccess = false, ErrMessage = "参数格式错误:" + string.Join(";", ArrError) });
                };
            });

            //注入IOption<JwtOption>
            services.Configure<JwtOption>(Configuration.GetSection("JWT"));
            //注册EF上下文
            var dbConnStr = Environment.ExpandEnvironmentVariables(Configuration.GetConnectionString("DefaultConnection"));
            services.AddDbContext<AppDbContext>(opts => opts.UseSqlServer(dbConnStr, sqldbBuilder =>
            {
                sqldbBuilder.CommandTimeout(500);
                sqldbBuilder.MigrationsAssembly("NetCoreTemp.WebApi");
            })); ;
            //增加AutoMapper
            services.AddAutoMapperService();
            //注入scope-自定义Service
            services.AddCustomerServices();
            //认证
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.Challenge = "X-Token";
                var serverSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:ServerSecret"]));
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = serverSecret,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(30),
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:Audience"]
                };
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["X-Token"].ToString();
                        if (string.IsNullOrEmpty(token))
                            token = context.Request.Cookies["X-Token"];
                        if (!string.IsNullOrEmpty(token))
                            context.Token = token;
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = async context =>
                    {
                        ////获取注册的服务
                        //var mapper = context.HttpContext.RequestServices.GetService<AutoMapper.IMapper>();
                        var res = context.Response;
                        var error = context.Exception;
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var actResultMsg = new ActionReturnMessage
                        {
                            StatusCode = HttpStatusCode.Unauthorized,
                            ErrMessage = error?.Message ?? "token 验证失败",
                            IsSuccess = false
                        };
                        if (error != null && error is SecurityTokenException)
                        {
                            switch (error)
                            {
                                case SecurityTokenInvalidIssuerException err when err != null:
                                    actResultMsg.ErrMessage = $"token Issuer验证失败：{err.Message}";
                                    break;
                                case SecurityTokenInvalidAudienceException err when err != null:
                                    actResultMsg.ErrMessage = $"token Audience验证失败：{err.Message}";
                                    break;
                                case SecurityTokenExpiredException err when err != null:
                                    actResultMsg.ErrMessage = $"token 已过期：{err.Message}";
                                    break;
                                default:
                                    actResultMsg.ErrMessage = $"token 验证失败：{error?.Message ?? ""}";
                                    break;
                            }
                        }
                        var opt = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                        await context.Response.WriteAsync(JsonSerializer.Serialize(actResultMsg, opt));
                    },
                    OnTokenValidated = async context =>
                    {
                        //赋值 Identity数据
                        context.HttpContext.User.AddIdentities(context.Principal.Identities);
                        await Task.CompletedTask;
                    }
                };
            });
            //授权
            services.AddAuthorization(x =>
            {
                //多种认证模式（Cookie和JWT同时使用）
                //var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                //    JwtBearerDefaults.AuthenticationScheme,
                //    CookieAuthenticationDefaults.AuthenticationScheme);

                //自定义策略
                x.AddPolicy("Michael", policy =>
                {
                    policy.RequireClaim("Michael");
                });
                //默认策略
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim("Michael");
                x.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });
            //跨域
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    var http = Configuration.GetValue<string>("Cors.Origins") ?? "http://localhost:8080";
                    policy
                    //.AllowAnyOrigin()//前端设置withCredentials时 chrome不允许*
                    .WithOrigins(new string[] {
                        "http://localhost:9526",
                        "http://localhost:9527",
                        http
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    /*前端设置withCredentials=true时，AllowCredentials必须设置,可以配合SetIsOriginAllowed组合使用 */
                    //.AllowCredentials()
                    //.SetIsOriginAllowed(_ => true)
                    /* 浏览器默认响应头
                     * Cache-Control
                     * Content-Language
                     * Content-Type
                     * Expires
                     * Last-Modified
                     * Pragma
                    */
                    .WithExposedHeaders("Content-Disposition", "server-login");
                    // 指定可缓存对预检请求的响应的时间长度
                    policy.SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                });
            });
            services.AddMemoryCache();

            //RedisCache
            services.AddStackExchangeRedisCache(options =>
            {
                var redisConnectionString = Configuration.GetSection("RedisConnection")?.Value ?? "localhost:6379,allowAdmin=true,connectTimeout=1000,connectRetry=3";
                options.Configuration = redisConnectionString;

                var assemblyName = Assembly.GetExecutingAssembly().GetName();
                options.InstanceName = assemblyName.Name;
            });

            #region redis

            services.AddRedisMultiplexer(Configuration);

            #endregion

            #region HttpClientFactory

            //不验证 SSL证书
            System.Net.Http.HttpClientHandler clientHandler = new System.Net.Http.HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            services.AddHttpClient("WXKF", client =>
            {
                var WXKFBaseUrl = Configuration.GetValue<string>("WXKFBaseUrl") ?? "https://qyapi.weixin.qq.com/cgi-bin/kf/";
                client.BaseAddress = new Uri(WXKFBaseUrl);
            }).SetHandlerLifetime(TimeSpan.FromHours(12)).ConfigurePrimaryHttpMessageHandler(_ => clientHandler);
            //微信获取Token
            services.AddHttpClient("WXAccessToken", client =>
            {
                var WXKFBaseUrl = "https://qyapi.weixin.qq.com/cgi-bin/";
                client.BaseAddress = new Uri(WXKFBaseUrl);
            }).SetHandlerLifetime(TimeSpan.FromHours(12)).ConfigurePrimaryHttpMessageHandler(_ => clientHandler);

            #endregion

            #region QuartzJobScheduler

            ////1.自动IJobFactory使用MicrosoftDependencyInjectionJobFactory包实现
            //services.AddQuartzSchedulerServiceAuto();

            ////2.手动实现IJobFactory（必须将IJob先注入DI）
            //services.AddQuartzSchedulerService();

            #endregion

            ////视图显示提供者
            //services.AddSingleton<Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider, My_ModelMetadataProvider>();

            #region 自定义验证器

            //合并到类库
            services.AddMyModelValidatorProvider();

            #endregion

            #region 国际化

            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(actLocalizationOpts);

            #endregion

            #region AspectCore AOP 拦截器

            /*
             * using AspectCore.Extensions.DependencyInjection;
             * using AspectCore.Configuration;
             * program  .UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());//AsceptCore-AOP
             * AspectCore AOP 属性注入,执行时间日志
             * 不是全局注入 无需依赖注入
            */
            //services.AddTransient<AOPLogAttribute>();
            //services.ConfigureDynamicProxy();

            /*
             * 注册全局拦截器
             */
            //services.ConfigureDynamicProxy(config =>
            //{
            //    /*
            //     * 不注册全局拦截器
            //     * NonAspectAttribute 来使得Service或Method不被代理
            //     */
            //    //全局拦截器（特定服务）
            //    config.Interceptors.AddTyped<AOPLogAttribute>(Predicates.ForService("*Service"));
            //    //全局拦截器（Params结尾的方法）
            //    config.Interceptors.AddTyped<AOPLogAttribute>(method => method.Name.EndsWith("Params"));

            //    //App1命名空间下的Service不会被代理
            //    config.NonAspectPredicates.AddNamespace("BI.CPA.Core");

            //    //最后一级为App1的命名空间下的Service不会被代理
            //    config.NonAspectPredicates.AddNamespace("*.Entities");

            //    //ICustomService接口不会被代理
            //    config.NonAspectPredicates.AddService("ICustomService");

            //    //后缀为Service的接口和类不会被代理
            //    config.NonAspectPredicates.AddService("*Service");

            //    //命名为Query的方法不会被代理
            //    config.NonAspectPredicates.AddMethod("Query");

            //    //后缀为Query的方法不会被代理
            //    config.NonAspectPredicates.AddMethod("*Query");
            //});

            #endregion

            services.AddLogging(config => {
                config.AddLog4Net();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            IServiceProvider serviceProvider = app.ApplicationServices;
            //为了 实现ConfigServices里的 serviceProvider的实例
            serviceProvider.GetService<IModelValidatorProvider>();
            //注册静态依赖注入实例,以便Fody和其他地方使用
            Services.AOP.FodyDIService.Configration(serviceProvider);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseHttpsRedirection();

            //跨域
            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseCookiePolicy();

            #region 国际化 必须在 app.UseMvc();前

            var options = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>();
            if (options?.Value != null)
                app.UseRequestLocalization(options.Value);
            else
                app.UseRequestLocalization(actLocalizationOpts);

            #endregion

            #region 设置静态文件

            /*
             * With UseDefaultFiles, requests to a folder will search for:
             * default.htm
             * default.html
             * index.htm
             * index.html
             */
            app.UseDefaultFiles();
            app.UseStaticFiles();
            int cacheMaxAge = env.IsDevelopment() ? 100 : 604800;
            app.UseStaticFiles(new StaticFileOptions
            {
                //设置静态文件缓存时间
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cacheMaxAge}");
                },
                ////自定义 静态文件
                //FileProvider = new PhysicalFileProvider(
                //    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
                //RequestPath = "/Static"
            });
            //允许浏览静态文件
            //app.UseDirectoryBrowser();

            #endregion

            //认证
            app.UseAuthentication();
            //授权
            app.UseAuthorization();

            #region 错误捕获中间件 401错误会被 JwtEvent截获，不会抛出错误

            //app.UseMiddleware(typeof(ExceptionMiddleWare));
            //跳转到错误页
            //app.UseExceptionHandler("/error");
            //自定义错误处理
            //app.UseExceptionHandler(opts =>
            //{
            //    opts.Run(async context => {
            //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //        context.Response.ContentType = "application/json";
            //        var exceptionObject = context.Features.Get<IExceptionHandlerFeature>();
            //        if (null != exceptionObject)
            //        {
            //            var errorMessage = $"Error: {exceptionObject.Error.Message};StackTrace:{exceptionObject.Error.StackTrace}";
            //            var errObj = ActionReturnMessage.Error(errorMessage);
            //            var bytes = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(errObj));
            //            await context.Response.Body.WriteAsync(bytes).ConfigureAwait(false);
            //        }
            //    });
            //});

            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// 国际化配置
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
                        //默认读取 accept-language
                        var result = new ProviderCultureResult(culture.Name);
                        // My custom request culture logic
                        var pvdCultureResult = new ProviderCultureResult(lang);
                        return pvdCultureResult;
                    }
                }
                return null;
            }));
            #region 设置 国际化 Cookie 名称 CookieName=c=ja-jp|uic= 格式 必须是 c={culture}|uic={culture}

            // Find the cookie provider with LINQ
            var cookieProvider = options.RequestCultureProviders.OfType<CookieRequestCultureProvider>().First();
            // Set the new cookie name
            cookieProvider.CookieName = "FinchCulture";

            #endregion
        });
    }
}
