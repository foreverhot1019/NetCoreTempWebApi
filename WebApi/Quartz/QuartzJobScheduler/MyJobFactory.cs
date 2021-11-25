using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace NetCoreTemp.WebApi.QuartzJobScheduler
{
    public class MyJobFactory : IJobFactory
    {
        protected readonly ConcurrentDictionary<IJob, IServiceScope> _scopes = new ConcurrentDictionary<IJob, IServiceScope>();

        public MyJobFactory(IServiceProvider serviceProvider, ILogger<MyJobFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MyJobFactory> _logger;

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobType = bundle.JobDetail.JobType;
            IJob job = null;
            var scope = _serviceProvider.CreateScope();
            try
            {
                job = scope.ServiceProvider.GetRequiredService(jobType) as IJob;
            }
            catch (Exception ex)
            {
                // Failed to create the job -> ensure scope gets disposed
                scope.Dispose();
                _logger?.LogError("获取DI-Scope，错误", ex);
            }
            // Add scope to dictionary so we can dispose it once the job finishes
            if (!_scopes.TryAdd(job, scope))
            {
                // Failed to track DI scope -> ensure scope gets disposed
                scope.Dispose();
                _logger?.LogError("增加DI-Scope和job，错误");
            }

            return job;
        }

        public void ReturnJob(IJob job)
        {
            if (_scopes.TryRemove(job, out var scope))
            {
                // The Dispose() method ends the scope lifetime.
                // Once Dispose is called, any scoped services that have been resolved from ServiceProvider will be disposed.
                scope.Dispose();
            }
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}
