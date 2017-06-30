using System;
using Microsoft.EntityFrameworkCore;

namespace Vino.Core.TimedTask.EntityFramework
{
    public interface ITimedTaskContext
    {
        DbSet<TimedTask> TimedTasks { get; set; }

        int SaveChanges();
    }
}
