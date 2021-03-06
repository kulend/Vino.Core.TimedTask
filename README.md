# Vino.Core.TimedTask
基于.NET CORE的一个定时任务运行管理组件。

* 版本发布
    <p>[2017.07.04] 版本 1.0.1.2</p>
    <p>    Invoke标注添加Name属性，TimedTask添加Name属性。</p>

    <p>[2017.07.03] 版本 1.0.1.1</p>
    <p>    VinoTimedTask标注改为TimedTask。</p>

* 安装方法
Nuget：Install-Package Vino.Core.TimedTask

* 使用方法
    1. 在Startup的ConfigureServices方法中
    ```c#
        services.AddTimedTask();
    ```

    2. 在Startup的Configure方法中
    ```c#
        app.UseTimedTask();
    ```

    3. 创建一个TestTask类，添加TimedTask标注，添加一个Run方法，添加Invoke标注
    ```c#
        [TimedTask]
        public class TestTask
        {
            [Invoke(Interval = 5000)]
            [SingleTask]
            public void Run()
            {
                Debug.WriteLine(DateTime.Now + " TestTask Run invoke...");
            }

            ...
        }
    ```
        Invoke标注有以下属性
            IsEnabled：是否有效，默认为true
            AutoReset：设置是执行一次（false）还是一直执行(true)，默认为true
            Interval：运行间隔，默认一分钟
            BeginTime：有效开始时间
            ExpireTime：失效时间

        SingleTask标注代表如果上次运行还未结束，则本次将跳过运行。


# Vino.Core.TimedTask.EntityFramework
使用EntityFramework从数据库获取定时任务配置信息。

* 安装方法
Nuget：Install-Package Vino.Core.TimedTask.EntityFramework

* 使用方法
    1. 创建MyDbContext
    ```c#
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
    ```

    2. 在Startup的ConfigureServices方法中
    ```c#
        services.AddTimedTask().AddEntityFrameworkTimedTask<MyDbContext>();
    ```
    3. 进行dotnet ef migrations add xxx, dotnet ef database update，在表中配置事务，其中
        Identifier设置为：类名.方法名，如Test.TestTask.RunForDb
    
    4. 创建一个TestTask类，添加VinoTimedTask标注，添加一个RunForDb方法
    ```c#
        [VinoTimedTask]
        public class TestTask
        {
            [SingleTask]
            public void RunForDb()
            {
                Debug.WriteLine(DateTime.Now + " TestTask RunForDb invoke...");
            }

            ...
        }
    ```
    5. 自定义数据表名
        <p>默认的表名为timed_task，如果需要自定义表名，则在MyDbContext的OnModelCreating方法中设置</p>
    ```c#
        modelBuilder.Entity<TimedTask>().ToTable("tablename");
    ```
        创建表sql（mysql）：
    ```sql
        CREATE TABLE `tablename` (
        `Id` varchar(32) NOT NULL,
        `Name` varchar(64) DEFAULT NULL,
        `AutoReset` bit(1) NOT NULL,
        `BeginTime` datetime(6) NOT NULL,
        `ExpireTime` datetime(6) NOT NULL,
        `Identifier` varchar(256) DEFAULT NULL,
        `Interval` int(11) NOT NULL,
        `IsEnabled` bit(1) NOT NULL,
        PRIMARY KEY (`Id`)
        )
    ```
本项目部分代码参照并引用了Pomelo.AspNetCore.TimedJob相关代码