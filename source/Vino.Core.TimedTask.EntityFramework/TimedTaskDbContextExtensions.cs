using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Vino.Core.TimedTask.EntityFramework
{
    public static class TimedTaskDbContextExtensions
    {
        public static ModelBuilder SetupTimedTask(this ModelBuilder self)
        {
            self.Entity<TimedTaskLog>(e =>
            {
                e.HasIndex(x=>x.TaskId);
            });

            return self.Entity<TimedTask>(e =>
            {
                e.HasKey(x => x.Id);
            });
        }
    }
}
