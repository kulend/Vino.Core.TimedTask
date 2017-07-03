using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;
using Vino.Core.TimedTask.Database;

namespace Vino.Core.TimedTask.Redis
{
    public class RedisTimedTaskProvider: ITimedTaskProvider
    {
        public static IDatabase _db;

        public RedisTimedTaskProvider()
        {
        }

        public IList<TimedTask> GetTasks()
        {
            if (_db == null)
            {
                return new List<TimedTask>();
            }
            return _db.ListRange("TimedTaskList")
                .Select(x => JsonConvert.DeserializeObject<TimedTask>(x))
                .Where(x => x.IsEnabled)
                .ToList();
        }
    }
}
