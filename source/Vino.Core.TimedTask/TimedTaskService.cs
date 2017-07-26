using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vino.Core.TimedTask.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vino.Core.TimedTask.Attribute;
using Vino.Core.TimedTask.Database;

namespace Vino.Core.TimedTask
{
    public class TimedTaskService: ITimedTaskService
    {
        /// <summary>
        /// 语言
        /// </summary>
        private static readonly bool IsZh = "zh-cn".Equals(System.Globalization.CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase) 
                                   || "zh-tw".Equals(System.Globalization.CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase)
                                   || "zh-hk".Equals(System.Globalization.CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase);

        private ILogger logger { get; set; }

        private IAssemblyLocator locator { get; set; }
        private IServiceProvider services { get; set; }

        private ITimedTaskProvider timedTaskProvider { get; set; }

        private List<TypeInfo> JobTypeCollection { get; set; } = new List<TypeInfo>();

        public static Dictionary<string, bool> TaskStatus { get; private set; } = new Dictionary<string, bool>();
        public static Dictionary<string, Timer> StaticTimers { get; private set; } = new Dictionary<string, Timer>();
        public static Dictionary<string, Timer> DbTimers { get; private set; } = new Dictionary<string, Timer>();

        /// <summary>
        /// 设置定时服务是否可用
        /// </summary>
        public bool IsEnable { set; get; } = true;

        public TimedTaskService(IAssemblyLocator locator, IServiceProvider services)
        {
            this.locator = locator;
            this.services = services;
            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();
            this.logger = loggerFactory.CreateLogger<TimedTaskService>();
            //this.logger = services.GetService<ILogger>();
            this.timedTaskProvider = services.GetService<ITimedTaskProvider>();
            var asm = locator.GetAssemblies();
            foreach (var x in asm)
            {
                //查找带有TimedTaskAttribute的类
                var types = x.DefinedTypes.Where(y => y.GetCustomAttribute(typeof(TimedTaskAttribute), true) != null);
                foreach (var type in types)
                {
                    JobTypeCollection.Add(type);
                }
            }

            //取得所有方法
            foreach (var clazz in JobTypeCollection)
            {
                var clazzTimedTaskAttr = clazz.GetCustomAttribute<TimedTaskAttribute>();
                var clazzName = string.IsNullOrEmpty(clazzTimedTaskAttr.Name) ? clazz.Name : clazzTimedTaskAttr.Name;
                foreach (var method in 
                    clazz.DeclaredMethods.Where(x=>x.GetCustomAttributes<InvokeAttribute>(true).Any()))
                {
                    //取得所有Invoke配置
                    var invokes = method.GetCustomAttributes<InvokeAttribute>(true);
                    foreach (var invoke in invokes.Where(i=>i.IsEnabled 
                        && (i.ExpireTime >= DateTime.Now || i.ExpireTime == default(DateTime))))
                    {
                        //需要延时的时间
                        int delta = 0;
                        if (invoke.BeginTime == default(DateTime))
                        {
                            invoke.BeginTime = DateTime.Now;
                        }
                        else
                        {
                            delta = Convert.ToInt32((invoke.BeginTime - DateTime.Now).TotalMilliseconds);
                        }
                        if (delta < 0)
                        {
                            delta = delta % invoke.Interval;
                            if (delta < 0)
                                delta += invoke.Interval;
                        }
                        Task.Factory.StartNew(() =>
                        {
                            var invokeName = string.IsNullOrEmpty(invoke.Name) ? method.Name : invoke.Name;
                            var task = new TimedTask();
                            task.Id = Guid.NewGuid().ToString("N");
                            task.Name = clazzName + "." + invokeName;
                            task.Identifier = clazz.FullName + "." + method.Name;
                            task.BeginTime = invoke.BeginTime;
                            task.ExpireTime = invoke.ExpireTime;
                            task.Interval = invoke.Interval;
                            task.AutoReset = invoke.AutoReset;
                            task.IsEnabled = invoke.IsEnabled;
                            var timer = new Timer(t =>
                            {
                                Execute(task, clazz, method);
                            }, null, delta, invoke.AutoReset ? invoke.Interval : 0);
                            StaticTimers.Add(task.Id, timer);
                        });
                    }
                }
            }

            ExecuteDbTask();
        }

        private void ExecuteDbTask()
        {
            //清除所有DB事务
            if (DbTimers.Count > 0)
            {
                foreach (var dbTimer in DbTimers)
                {
                    dbTimer.Value.Dispose();
                    TaskStatus[dbTimer.Key] = false;
                }
                DbTimers.Clear();
            }
            //取得数据库配置的事务
            if (timedTaskProvider != null)
            {
                var tasks = timedTaskProvider.GetTasks();
                foreach (var task in tasks)
                {
                    if (string.IsNullOrEmpty(task.Identifier))
                    {
                        continue;
                    }
                    var clazzName = task.Identifier.Substring(0, task.Identifier.LastIndexOf('.'));
                    var functionName = task.Identifier.Substring(task.Identifier.LastIndexOf('.') + 1);
                    var clazz = JobTypeCollection.SingleOrDefault(x => x.FullName == clazzName);
                    if (clazz == null)
                    {
                        continue;
                    }
                    var method = clazz.GetMethod(functionName);
                    if (method == null)
                    {
                        continue;
                    }

                    //需要延时的时间
                    int delta = 0;
                    if (task.BeginTime == default(DateTime))
                    {
                        task.BeginTime = DateTime.Now;
                    }
                    else
                    {
                        delta = Convert.ToInt32((task.BeginTime - DateTime.Now).TotalMilliseconds);
                    }
                    if (delta < 0)
                    {
                        delta = delta % task.Interval;
                        if (delta < 0)
                            delta += task.Interval;
                    }
                    Task.Factory.StartNew(() =>
                    {
                        var timer = new Timer(t =>
                        {
                            Execute(task, clazz, method);
                        }, null, delta, task.AutoReset ? task.Interval : 0);
                        DbTimers.Add(task.Id, timer);
                    });
                }
            }
        }

        private bool Execute(TimedTask task, TypeInfo clazz, MethodInfo method)
        {
            var identifier = task.Identifier;

            if (task.ExpireTime != default(DateTime)
                && task.ExpireTime <= DateTime.Now)
            {
                //已过期失效
                StaticTimers[task.Id].Dispose();
                TaskStatus[task.Id] = false;
                StaticTimers.Remove(task.Id);
                TaskStatus.Remove(task.Id);
                return false;
            }
            var argtypes = clazz.GetConstructors()
                .First()
                .GetParameters()
                .Select(x =>
                {
                    if (x.ParameterType == typeof(IServiceProvider))
                        return services;
                    else
                        return services.GetService(x.ParameterType);
                }).ToArray();

            var instance = Activator.CreateInstance(clazz.AsType(), argtypes);
            var paramtypes = method.GetParameters().Select(x => services.GetService(x.ParameterType)).ToArray();
            var singleTaskAttr = method.GetCustomAttribute<SingleTaskAttribute>(true);
            lock (this)
            {
                if (!IsEnable)
                {
                    return false;
                }
                if (singleTaskAttr != null && singleTaskAttr.IsSingleTask 
                    && TaskStatus.ContainsKey(task.Id) && TaskStatus[task.Id])
                {
                    return false;
                }
                TaskStatus[task.Id] = true;
            }
            try
            {
                if (IsZh)
                {
                    logger?.LogInformation($"[事务]{task.Name} 开始执行...");
                }
                else
                {
                    logger?.LogInformation($"[Task]{task.Name} Invoking...");
                }
                Stopwatch sw = new Stopwatch();
                sw.Start();
                method.Invoke(instance, paramtypes);
                sw.Stop();
                if (IsZh)
                {
                    logger?.LogInformation($"[事务]{task.Name} 执行结束，耗时{sw.ElapsedMilliseconds}毫秒。");
                }
                else
                {
                    logger?.LogInformation($"[Task]{task.Name} Finish, takes {sw.ElapsedMilliseconds} milliseconds.");
                }
                Debug.WriteLine($"[Task]{task.Name} Finish, takes {sw.ElapsedMilliseconds} milliseconds.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
            }
            TaskStatus[task.Id] = false;
            return true;
        }

        /// <summary>
        /// 重置所有数据库定时服务
        /// </summary>
        public void ResetDbTask()
        {
            ExecuteDbTask();
        }

        /// <summary>
        /// 重置某一定时服务
        /// </summary>
        /// <param name="taskId"></param>
        public void ResetDbTask(string taskId)
        {
            if (DbTimers.ContainsKey(taskId))
            {
                DbTimers[taskId].Dispose();
                DbTimers.Remove(taskId);
            }
            TaskStatus[taskId] = false;

            if (timedTaskProvider != null)
            {
                var task = timedTaskProvider.GetTaskById(taskId);
                if (task != null && task.IsEnabled)
                {
                    if (string.IsNullOrEmpty(task.Identifier))
                    {
                        return;
                    }
                    var clazzName = task.Identifier.Substring(0, task.Identifier.LastIndexOf('.'));
                    var functionName = task.Identifier.Substring(task.Identifier.LastIndexOf('.') + 1);
                    var clazz = JobTypeCollection.SingleOrDefault(x => x.FullName == clazzName);
                    if (clazz == null)
                    {
                        return;
                    }
                    var method = clazz.GetMethod(functionName);
                    if (method == null)
                    {
                        return;
                    }

                    //需要延时的时间
                    int delta = 0;
                    if (task.BeginTime == default(DateTime))
                    {
                        task.BeginTime = DateTime.Now;
                    }
                    else
                    {
                        delta = Convert.ToInt32((task.BeginTime - DateTime.Now).TotalMilliseconds);
                    }
                    if (delta < 0)
                    {
                        delta = delta % task.Interval;
                        if (delta < 0)
                            delta += task.Interval;
                    }
                    Task.Factory.StartNew(() =>
                    {
                        var timer = new Timer(t =>
                        {
                            Execute(task, clazz, method);
                        }, null, delta, task.AutoReset ? task.Interval : 0);
                        DbTimers.Add(task.Id, timer);
                    });
                }
            }
        }

    }
}
