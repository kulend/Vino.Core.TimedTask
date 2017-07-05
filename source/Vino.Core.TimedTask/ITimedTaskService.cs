using System;
using System.Collections.Generic;
using System.Text;

namespace Vino.Core.TimedTask
{
    public interface ITimedTaskService
    {
        /// <summary>
        /// 定时服务是否有效
        /// </summary>
        bool IsEnable { set; get; }

        /// <summary>
        /// 重置所有数据库定时服务
        /// </summary>
        void ResetDbTask();

        /// <summary>
        /// 重置某一定时服务
        /// </summary>
        void ResetDbTask(string taskId);
    }
}
