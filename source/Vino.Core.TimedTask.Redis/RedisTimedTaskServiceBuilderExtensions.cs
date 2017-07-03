using System;
using StackExchange.Redis;
using Vino.Core.TimedTask.Database;
using Vino.Core.TimedTask.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisTimedTaskServiceBuilderExtensions
    {
        public static IServiceCollection AddRedisTimedTask(this IServiceCollection self)
        {
            return self.AddScoped<ITimedTaskProvider, RedisTimedTaskProvider>();
        }
    }
}


namespace Microsoft.AspNetCore.Builder
{
    public static class TimedTaskExtensions
    {
        public static IApplicationBuilder UseRedisTimedTask(this IApplicationBuilder self, IDatabase database)
        {
            RedisTimedTaskProvider._db = database;
            return self;
        }
    }
}