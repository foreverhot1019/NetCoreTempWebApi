using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace RedisHelp
{
    public static class RedisService
    {
        public static IServiceCollection AddRedisMultiplexer(
            this IServiceCollection services, IConfiguration configuration)
        {
            var RedisConnection = configuration.GetSection("RedisConnection")?.Value ?? "localhost:6379,allowAdmin=true,connectTimeout=1000,connectRetry=3";

            // The Redis is a singleton, shared as much as possible.
            services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(RedisConnection));

            return services.AddScoped<RedisHelper>();
        }
    }
}
