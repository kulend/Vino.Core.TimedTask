using System;
using System.Collections.Generic;
using System.Text;

namespace Vino.Core.TimedTask.Database
{
    public interface ITimedTaskProvider
    {
        /// <summary>
        /// 获取数据库中的有效事务
        /// </summary>
        /// <returns></returns>
        IList<TimedTask> GetTasks();

        TimedTask GetTaskById(string id);
    }
}
