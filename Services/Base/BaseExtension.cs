using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCoreTemp.WebApi.Models.Extensions;
using NetCoreTemp.WebApi.Models.View_Model;

namespace NetCoreTemp.WebApi.Services.Base
{
    public static class BaseExtension
    {
        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="IQuery"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static (IEnumerable<TEntity> ArrEntity, int rowsCount) SelectPage<TEntity>(this IQueryable<TEntity> IQuery, int page, int pageSize) where TEntity : class, Models.BaseModel.IEntity_
        {
            int totalCount = 0;
            List<TEntity> arr = new List<TEntity>();
            if (page >= 0)
            {
                var skipNum = (page - 1) * pageSize;
                totalCount = IQuery.Count();
                arr = IQuery.Skip(skipNum < 0 ? 0 : skipNum).Take(pageSize).ToList();
            }
            return (arr, totalCount);
        }

        /// <summary>
        /// 异步分页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="IQuery"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<(IEnumerable<TEntity> ArrEntity, int rowsCount)> SelectPageAsync<TEntity>(this IQueryable<TEntity> IQuery, int page, int pageSize) where TEntity : class, Models.BaseModel.IEntity_
        {
            int totalCount = 0;
            List<TEntity> arr = new List<TEntity>();
            if (page >= 0)
            {
                var skipNum = (page - 1) * pageSize;
                totalCount = IQuery.Count();
                arr = await IQuery.Skip(skipNum < 0 ? 0 : skipNum).Take(pageSize).ToListAsync();
            }
            return (arr, totalCount);
        }

        /// <summary>
        /// 增加基础搜索字段
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="filterRules"></param>
        /// <param name="searchQuery"></param>
        public static void AddBaseSearchQuery<TEntity>(this IEnumerable<filterRule> filterRules, ref SearchQuery<TEntity> searchQuery)
            where TEntity : class, NetCoreTemp.WebApi.Models.BaseModel.IEntity<Guid>
        {
            #region BaseFieldSearch

            foreach (var rule in filterRules)
            {
                if (string.IsNullOrWhiteSpace(rule.value))
                    continue;
                else
                {
                    //去除AutoMapper-Dto前缀 "_"
                    rule.field = rule.field.CleanAutoMapperDtoPrefix();
                }

                if (rule.field == "ID")
                {
                    //if (int.TryParse(rule.value, out int val))
                    if (Guid.TryParse(rule.value, out Guid guid))// && guid != Guid.Empty
                        searchQuery.And(x => x.ID == guid);
                }
                if (rule.field == "CreateUserId")
                {
                    if (Guid.TryParse(rule.value, out Guid guid))// && guid != Guid.Empty
                        searchQuery.And(x => x.CreateUserId == guid);
                }
                if (rule.field == "CreateUserName")
                {
                    searchQuery.And(x => x.CreateUserName.StartsWith(rule.value));
                }
                if (rule.field == "CreateDate")
                {
                    if (long.TryParse(rule.value, out long val))
                        searchQuery.And(x => x.CreateDate == val);
                }
                if (rule.field == "_CreateDate")
                {
                    if (long.TryParse(rule.value, out long val))
                        searchQuery.And(x => x.CreateDate >= val);
                    else if (rule.value.Parse2DateTime(out DateTime? date))
                    {
                        var timespan = date.toLong();
                        searchQuery.And(x => x.CreateDate >= timespan);
                    }
                }
                if (rule.field == "CreateDate_")
                {
                    if (long.TryParse(rule.value, out long val))
                        searchQuery.And(x => x.CreateDate <= val);
                    else if (rule.value.Parse2DateTime(out DateTime? date))
                    {
                        var timespan = date.toLong();
                        searchQuery.And(x => x.CreateDate <= timespan);
                    }
                }
                if (rule.field == "ModifyUserId")
                {
                    if (Guid.TryParse(rule.value, out Guid guid))// && guid != Guid.Empty
                        searchQuery.And(x => x.ModifyUserId == guid);
                }
                if (rule.field == "ModifyUserName")
                {
                    searchQuery.And(x => x.ModifyUserName.StartsWith(rule.value));
                }
                if (rule.field == "ModifyDate")
                {
                    if (long.TryParse(rule.value, out long val))
                        searchQuery.And(x => x.ModifyDate == val);
                }
                if (rule.field == "_ModifyDate")
                {
                    if (long.TryParse(rule.value, out long val))
                        searchQuery.And(x => x.ModifyDate >= val);
                    else if (rule.value.Parse2DateTime(out DateTime? date))
                    {
                        var timespan = date.toLong();
                        searchQuery.And(x => x.ModifyDate >= timespan);
                    }
                }
                if (rule.field == "ModifyDate_")
                {
                    if (long.TryParse(rule.value, out long val))
                        searchQuery.And(x => x.ModifyDate <= val);
                    else if (rule.value.Parse2DateTime(out DateTime? date))
                    {
                        var timespan = date.toLong();
                        searchQuery.And(x => x.ModifyDate <= timespan);
                    }
                }
                if (rule.field == "Status")
                {
                    if (int.TryParse(rule.value, out int intVal))
                        searchQuery.And(x => x.Status == (Models.EnumType.EnumRepo.UseStatusEnum)intVal);
                }
            }

            #endregion
        }
    }
}
