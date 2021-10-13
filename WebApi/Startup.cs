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
using Helpdesk.WebApi.Models.View_Model;
using Helpdesk.WebApi.Models.AutoMapper;
using Helpdesk.WebApi.Models.Extensions;
using Helpdesk.WebApi.Models;
using Helpdesk.WebApi.Extensions;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Helpdesk.WebApi.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Helpdesk.WebApi.Services.Base;
using Microsoft.EntityFrameworkCore;
using Helpdesk.WebApi.Models.DatabaseContext;

namespace Helpdesk.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebEnv = env;
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
            });
            //    .AddJsonOptions(opts =>
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
            services.AddDbContext<AppDbContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), sqldbBuilder =>
            {
                sqldbBuilder.CommandTimeout(500);
            }));
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
                //自定义策略
                x.AddPolicy("Michael", policy =>
                {
                    policy.RequireClaim("Michael");
                });
                //默认策略
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder("Bearer")
                .RequireAuthenticatedUser()
                .RequireClaim("Michael");
                x.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });
            //跨域
            services.AddCors(options => {
                options.AddPolicy("localhost", policy => {
                    policy.WithOrigins("http://publics3.gileadchina.cn", "http://localhost:9526", "http://localhost:9527", "http://localhost:5000", "https://localhost:5001").AllowAnyHeader().AllowAnyMethod();
                });
            });
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            loggerFactory.AddLog4Net();

            //app.UseHttpsRedirection();

            //跨域
            app.UseCors("localhost");
            app.UseRouting();

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
    }
}
