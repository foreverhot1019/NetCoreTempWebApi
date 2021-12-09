using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreTemp.WebApi.QuartzJobScheduler;
using NetCoreTemp.WebApi.QuartzJobScheduler.Job;
using NetCoreTemp.WebApi.WXKF;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace NetCoreTemp.WebApi.QuartzScheduler
{
    public static class QuartzSchedulerService
    {
        /// <summary>
        /// 注入 Quartz所需服务
        /// 手动实现IJobFactory（必须将IJob服务先注入DI）
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerService(this IServiceCollection services, bool AutoStart = true)
        {
            services.AddScoped<WXFLHttpClientHelper>();
            services.AddScoped<WXKFAssignHandler>();
            // Add Quartz services
            services.AddSingleton<ISchedulerFactory>(x =>
            {
                var factory = new StdSchedulerFactory();
                return factory;
            });
            services.AddScoped<WXKFDialog2DBJob>();
            services.AddScoped<WXKFDialogMsg2DBJob>();
            services.AddScoped<WXKFMsgCleanJob>();
            //获取Job（主要为了 依赖注入）Scoped的Job 在Singleton里必须 CreateScope()后获取
            services.AddSingleton<IJobFactory, MyJobFactory>(x =>
            {
                var logger = x.GetService<ILogger<MyJobFactory>>();
                return new MyJobFactory(x, logger);
            });
            //QuartzJobScheduler调度器
            services.AddSingleton<QuartzJobScheduler.JobScheduler>(x =>
            {
                var schedulerfactory = x.GetService<ISchedulerFactory>();
                var jobfactory = x.GetService<IJobFactory>();
                var log = x.GetService<ILogger<QuartzJobScheduler.JobScheduler>>();
                var job = new QuartzJobScheduler.JobScheduler(log, schedulerfactory, jobfactory);
                return job;
            });

            if (AutoStart)
                services.BuildServiceProvider().GetService<JobScheduler>().Start();

            return services;
        }

        /// <summary>
        /// 注入 Quartz所需服务（自动）
        /// MicrosoftDependencyInjectionJobFactory包实现IJobFactory
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerServiceAuto(this IServiceCollection services)
        {
            services.AddScoped<WXFLHttpClientHelper>();
            services.AddScoped<WXKFAssignHandler>();
            //1.自动IJobFactory使用MicrosoftDependencyInjectionJobFactory包实现
            services.AddQuartzHostedService(x =>
            {
                x.WaitForJobsToComplete = true;
            });
            services.AddQuartz(q =>
            {
                q.SchedulerName = "MyQuartzScheduler";
                //使用jobs配置文件
                q.UseXmlSchedulingConfiguration(x =>
                {
                    x.Files = new[] { "~/Quartz/quartz_jobs.xml" };
                    x.ScanInterval = TimeSpan.FromMinutes(1);
                    x.FailOnFileNotFound = true;
                    x.FailOnSchedulingError = true;
                });
                q.UseMicrosoftDependencyInjectionJobFactory();
            });
            services.AddQuartzServer(option =>
            {
                option.WaitForJobsToComplete = true;
            });
            return services;
        }
    }
}
