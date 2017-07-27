using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Vino.Core.TimedTask.Database;

namespace Vino.Core.TimedTask.EntityFramework
{
    public class EFTimedTaskProvider<TContext> : ITimedTaskProvider
        where TContext : ITimedTaskContext
    {
        private TContext _db { get; set; }
        private IServiceProvider _services;

        public EFTimedTaskProvider(TContext db, IServiceProvider services)
        {
            _db = db;
            this._services = services;
        }

        public IList<TimedTask> GetTasks()
        {
            return _db.TimedTasks
                .Where(x => x.IsEnabled)
                //.Select(x => new TimedTask
                //{
                //    Id = x.Id,
                //    BeginTime = x.BeginTime,
                //    Interval = x.Interval,
                //    IsEnabled = x.IsEnabled
                //})
                .ToList();
        }

        public TimedTask GetTaskById(string id)
        {
            return _db.TimedTasks.SingleOrDefault(x => x.Id.Equals(id));
        }

        public void AddLog(TimedTaskLog log)
        {
            _db.TimedTaskLogs.Add(log);
            _db.SaveChanges();
        }
    }
}
