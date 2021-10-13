using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.WebApi.Services.Base
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
    }
}
