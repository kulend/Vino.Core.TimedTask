using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Test.EntityFramework
{
    public class MyDbContextFactory : IDbContextFactory<MyDbContext>
    {
        public MyDbContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
            var constr = "server=localhost;userid=root;pwd=Pw@123456;port=3306;database=test;sslmode=none;";
            optionsBuilder.UseMySql(constr);
            return new MyDbContext(optionsBuilder.Options);
        }
    }
}
