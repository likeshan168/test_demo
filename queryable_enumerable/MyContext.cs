using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace queryable_enumerable
{
    internal class MyContext : DbContext
    {
        public static readonly ILoggerFactory MyLoggerFactory
= LoggerFactory.Create(builder =>
{
#if DEBUG
    builder.AddConsole();
#endif
});

        public DbSet<User> Users { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //写完整地路径
            string connString = @"Data Source=E:\sfqd\study\demo\test_demo\queryable_enumerable\testdb.db";

            optionsBuilder.UseSqlite(connString).UseLoggerFactory(MyLoggerFactory);
        }
    }
}
