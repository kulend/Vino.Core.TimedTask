using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.EntityFramework;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            IServiceCollection services = new ServiceCollection();
            var constr = "server=localhost;userid=root;pwd=Pw@123456;port=3306;database=test;sslmode=none;";
            services.AddDbContext<MyDbContext>(options => options.UseMySql(constr, b => b.MigrationsAssembly("Test")));

            services.AddTimedTask().AddEntityFrameworkTimedTask<MyDbContext>();

            //构建容器
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.UseTimedTask();

            Console.ReadLine();
        }
    }
}