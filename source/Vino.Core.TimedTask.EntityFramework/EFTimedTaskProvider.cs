using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vino.Core.TimedTask.Database;

namespace Vino.Core.TimedTask.EntityFramework
{
    public class EFTimedTaskProvider<TContext> : ITimedTaskProvider
        where TContext : ITimedTaskContext
    {
        private TContext _db { get; set; }

        public EFTimedTaskProvider(TContext db)
        {
            _db = db;
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
    }
}
