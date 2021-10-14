using System;
using System.Collections.Generic;
using System.Text;
using NetCoreTemp.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace NetCoreTemp.WebApi.Models.DatabaseContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
               : base(options)
        {
        }

        #region 表

        public DbSet<User> User { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<Role> Role { get; set; }

        #endregion

        /// <summary>
        /// 模型创建时
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ////判断当前数据库是Oracle 需要手动添加Schema(DBA提供的数据库账号名称)
            //if(this.Database.IsOracle())
            //{
            //    modelBuilder.HasDefaultSchema("NETCORE");
            //}

            ////创建索引
            //modelBuilder.Entity<Menu>(entity =>
            //{
            //    entity.HasIndex(e => new { e.Controller, e.Resource })
            //        .HasName("IX_MenuItemUnique").IsUnique(true);
            //});

            base.OnModelCreating(modelBuilder);
        }
    }
}
