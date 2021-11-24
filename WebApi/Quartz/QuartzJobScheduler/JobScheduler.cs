using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Quartz;
using Quartz.Impl;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Microsoft.Extensions.Logging;
using Quartz.Logging;

namespace NetCoreTemp.WebApi.QuartzJobScheduler
{
    public class JobScheduler
    {
        //调度器
        public IScheduler _scheduler;
        //调度器工厂
        public ISchedulerFactory _schedulerFactory;
        private readonly ILogger<JobScheduler> log;

        public JobScheduler(ILogger<JobScheduler> logger, ISchedulerFactory schedulerFactory)
        {
            log = logger;
            _schedulerFactory = schedulerFactory;
            //
            Quartz.Logging.LogProvider.SetCurrentLogProvider(new QuartzLogProvider(log));
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            try
            {
                _scheduler = await _schedulerFactory.GetScheduler();
                log.LogInformation("Quarz调度器启动");
                var name = _scheduler.SchedulerName;
                var arr = _scheduler.GetJobGroupNames();
                await _scheduler.Start();

                //// define the job and tie it to our HelloJob class
                //IJobDetail job = JobBuilder.Create<Job.WXKF2DataBaseJob>()
                //    .WithIdentity("job1", "group1")
                //    .Build();

                //// Trigger the job to run now, and then repeat every 10 seconds
                //ITrigger trigger = TriggerBuilder.Create()
                //    .WithIdentity("trigger1", "group1")
                //    .StartNow()
                //    .WithSimpleSchedule(x => x
                //        .WithIntervalInSeconds(10)
                //        .RepeatForever())
                //    .Build();

                //// Tell quartz to schedule the job using our trigger
                //await _scheduler.ScheduleJob(job, trigger);
            }
            catch (Exception ex)
            {
                log.LogError("Quarz调度器启动", ex);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            await _scheduler?.Shutdown();
            log.LogInformation("Quarz调度器启动");
        }
    }


    public class QuartzLogProvider : ILogProvider
    {
        private readonly ILogger _logger;
        public QuartzLogProvider(ILogger logger)
        {
            _logger = logger;
        }

        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (level >= Quartz.Logging.LogLevel.Info && func != null)
                {
                    string msg = "[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func();
                    //Console.WriteLine(msg, parameters);
                    if (exception == null)
                        _logger.LogInformation(msg, parameters);
                    else
                        _logger.LogError(exception, msg, parameters);
                }
                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            _logger.LogInformation($"OpenNestedContext:{message}");
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            _logger.LogInformation($"OpenMappedContext:{key}-{value}");
            throw new NotImplementedException();
        }
    }
}