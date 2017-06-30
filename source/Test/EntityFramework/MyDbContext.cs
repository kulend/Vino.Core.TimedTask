using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Vino.Core.TimedTask;
using Vino.Core.TimedTask.EntityFramework;

namespace Test.EntityFramework
{
    public class MyDbContext: DbContext, IDbContext, ITimedTaskContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        public DbSet<TimedTask> TimedTasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
