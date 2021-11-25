using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreTemp.WebApi.QuartzJobScheduler;
using NetCoreTemp.WebApi.QuartzJobScheduler.Job;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace NetCoreTemp.WebApi.QuartzScheduler
{
    public static class QuartzSchedulerService
    {
        /// <summary>
        /// 注入 Quartz所需服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzSchedulerService(this IServiceCollection services)
        {
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
            services.AddSingleton<IJobFactory, MyJobFactory>(x => { 
                var logger = x.GetService<ILogger<MyJobFactory>>();
                return new MyJobFactory(x, logger); 
            }) ;
            //QuartzJobScheduler调度器
            services.AddSingleton<QuartzJobScheduler.JobScheduler>(x =>
            {
                var schedulerfactory = x.GetService<ISchedulerFactory>();
                var jobfactory = x.GetService<IJobFactory>();
                var log = x.GetService<ILogger<QuartzJobScheduler.JobScheduler>>();
                var job = new QuartzJobScheduler.JobScheduler(log, schedulerfactory, jobfactory);
                Task.Run(() => job.Start());
                return job;
            });
            return services;
        }
    }
}
