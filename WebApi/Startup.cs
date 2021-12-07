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

namespace NetCoreTemp.WebApi
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
                opts.Filters.Add<ApiBaseExceptionFilter>();//ȫ���쳣����
                opts.Filters.Add<ApiBaseAuthorizeFilter>();//ȫ��Ȩ����֤
                opts.Filters.Add<ApiBaseActionFilter>();//ȫ��Actionͳһ��ʽ����
            });
            //    .AddJsonOptions(opts =>
            //{
            //    //�շ�
            //    opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            //    opts.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            //});
            //ModelState ��֤���� ͳһ��ʽ����
            services.Configure<ApiBehaviorOptions>(ApiBhvOpts =>
            {
                ////�ر�Ĭ��ģ����֤
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
                    return new JsonResult(new ActionReturnMessage { StatusCode = System.Net.HttpStatusCode.Forbidden, IsSuccess = false, ErrMessage = "������ʽ����:" + string.Join(";", ArrError) });
                };
            });
            //ע��IOption<JwtOption>
            services.Configure<JwtOption>(Configuration.GetSection("JWT"));
            //ע��EF������
            services.AddDbContext<AppDbContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), sqldbBuilder =>
            {
                sqldbBuilder.CommandTimeout(500);
            }));
            //����AutoMapper
            services.AddAutoMapperService();
            //ע��scope-�Զ���Service
            services.AddCustomerServices();
            //��֤
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
                        ////��ȡע��ķ���
                        //var mapper = context.HttpContext.RequestServices.GetService<AutoMapper.IMapper>();
                        var res = context.Response;
                        var error = context.Exception;
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var actResultMsg = new ActionReturnMessage
                        {
                            StatusCode = HttpStatusCode.Unauthorized,
                            ErrMessage = error?.Message ?? "token ��֤ʧ��",
                            IsSuccess = false
                        };
                        if (error != null && error is SecurityTokenException)
                        {
                            switch (error)
                            {
                                case SecurityTokenInvalidIssuerException err when err != null:
                                    actResultMsg.ErrMessage = $"token Issuer��֤ʧ�ܣ�{err.Message}";
                                    break;
                                case SecurityTokenInvalidAudienceException err when err != null:
                                    actResultMsg.ErrMessage = $"token Audience��֤ʧ�ܣ�{err.Message}";
                                    break;
                                case SecurityTokenExpiredException err when err != null:
                                    actResultMsg.ErrMessage = $"token �ѹ��ڣ�{err.Message}";
                                    break;
                                default:
                                    actResultMsg.ErrMessage = $"token ��֤ʧ�ܣ�{error?.Message ?? ""}";
                                    break;
                            }
                        }
                        var opt = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                        await context.Response.WriteAsync(JsonSerializer.Serialize(actResultMsg, opt));
                    },
                    OnTokenValidated = async context =>
                    {
                        //��ֵ Identity����
                        context.HttpContext.User.AddIdentities(context.Principal.Identities);
                        await Task.CompletedTask;
                    }
                };
            });
            //��Ȩ
            services.AddAuthorization(x =>
            {
                //�Զ������
                x.AddPolicy("Michael", policy =>
                {
                    policy.RequireClaim("Michael");
                });
                //Ĭ�ϲ���
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder("Bearer")
                .RequireAuthenticatedUser()
                .RequireClaim("Michael");
                x.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });
            //����
            services.AddCors(options =>
            {
                options.AddPolicy("localhost", policy =>
                {
                    var http = Configuration.GetValue<string>("Kestrel.Endpoints.Http") ?? "http://localhost:5000";
                    var tls = Configuration.GetValue<string>("Kestrel.Endpoints.Https") ?? "https://localhost:5001";
                    policy.WithOrigins(new string[] {
                        "http://localhost:9526",
                        "http://localhost:9527",
                        http,
                        tls
                    }).AllowAnyHeader().AllowAnyMethod();
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

            #region Polly

            //����֤ SSL֤��
            System.Net.Http.HttpClientHandler clientHandler = new System.Net.Http.HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            services.AddHttpClient("WXKF", client =>
            {
                var WXKFBaseUrl = Configuration.GetValue<string>("WXKFBaseUrl") ?? "https://qyapi.weixin.qq.com/cgi-bin/kf/";
                client.BaseAddress = new Uri(WXKFBaseUrl);
            }).SetHandlerLifetime(TimeSpan.FromHours(12)).ConfigurePrimaryHttpMessageHandler(_ => clientHandler);
            //΢�Ż�ȡToken
            services.AddHttpClient("WXAccessToken", client =>
            {
                var WXKFBaseUrl = "https://qyapi.weixin.qq.com/cgi-bin/";
                client.BaseAddress = new Uri(WXKFBaseUrl);
            }).SetHandlerLifetime(TimeSpan.FromHours(12)).ConfigurePrimaryHttpMessageHandler(_ => clientHandler);

            #endregion

            services.AddScoped<WXFLHttpClientHelper>();
            services.AddScoped<WXKFAssignHandler>();

            #region QuartzJobScheduler

            //1.�Զ�IJobFactory
            //services.AddQuartzHostedService(x =>
            //{
            //    x.WaitForJobsToComplete = true;
            //});
            //services.AddQuartz(q =>
            //{
            //    q.SchedulerName = "MyQuartzScheduler";
            //    //ʹ��jobs�����ļ�
            //    q.UseXmlSchedulingConfiguration(x => { 
            //        x.Files = new [] { "~/Quartz/quartz_jobs.xml" };
            //        x.ScanInterval = TimeSpan.FromMinutes(1);
            //        x.FailOnFileNotFound = true;
            //        x.FailOnSchedulingError = true;
            //    });
            //    q.UseMicrosoftDependencyInjectionJobFactory();
            //});
            //services.AddQuartzServer(option =>
            //{
            //    option.WaitForJobsToComplete = true;
            //});

            //2.�ֶ�ʵ��IJobFactory�����뽫IJob��ע��DI��
            //services.AddQuartzSchedulerService();

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            loggerFactory.AddLog4Net();

            app.UseHttpsRedirection();

            //����
            app.UseCors("localhost");
            app.UseRouting();

            #region ���þ�̬�ļ�

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
                //���þ�̬�ļ�����ʱ��
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cacheMaxAge}");
                },
                ////�Զ��� ��̬�ļ�
                //FileProvider = new PhysicalFileProvider(
                //    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
                //RequestPath = "/Static"
            });
            //���������̬�ļ�
            //app.UseDirectoryBrowser();

            #endregion

            //��֤
            app.UseAuthentication();
            //��Ȩ
            app.UseAuthorization();

            #region ���󲶻��м�� 401����ᱻ JwtEvent�ػ񣬲����׳�����

            //app.UseMiddleware(typeof(ExceptionMiddleWare));
            //��ת������ҳ
            //app.UseExceptionHandler("/error");
            //�Զ��������
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
