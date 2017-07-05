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
            return self.Entity<TimedTask>(e =>
            {
                e.HasKey(x => x.Id);
            });
        }
    }
}
