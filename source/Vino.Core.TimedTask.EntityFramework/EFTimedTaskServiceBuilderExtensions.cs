using System;
using System.Collections.Generic;
using System.Text;
using Vino.Core.TimedTask.Database;
using Vino.Core.TimedTask.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EFTimedTaskServiceBuilderExtensions
    {
        public static IServiceCollection AddEntityFrameworkTimedTask<TContext>(this IServiceCollection self)
            where TContext : ITimedTaskContext
        {
            return self.AddTransient<ITimedTaskProvider, EFTimedTaskProvider<TContext>>();
        }
    }
}
