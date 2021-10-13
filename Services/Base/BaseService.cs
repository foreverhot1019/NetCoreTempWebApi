using Helpdesk.WebApi.Models;
using Helpdesk.WebApi.Models.DatabaseContext;
using Helpdesk.WebApi.Models.View_Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Helpdesk.WebApi.Services.Base
{
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class, Helpdesk.WebApi.Models.BaseModel.IEntity_
    {
        private readonly AppDbContext _context;
        private ILogger<BaseService<TEntity>> _logger;

        public BaseService(AppDbContext  appDbContext, ILogger<BaseService<TEntity>> logger)
        {
            _context = appDbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取 DbContext
        /// </summary>
        /// <returns></returns>
        public AppDbContext GetDBContext()
        {
            return _context;
        }

        /// <summary>
        /// 搜索条件拼接
        /// </summary>
        /// <param name="searchQuery">搜索条件</param>
        /// <returns></returns>
        public IQueryable<TEntity> SearchFilter(ISearchQuery<TEntity> searchQuery)
        {
            var OSet = _context.Set<TEntity>().AsQueryable();
            var q = searchQuery.Query();
            if (q != null)
                OSet = OSet.Where(q);
            return OSet;
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> query)
        {
            var set = _context.Set<TEntity>();
            return set.Where(query);
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="query">条件表达树</param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> QueryList(Expression<Func<TEntity, bool>> query)
        {
            var set = _context.Set<TEntity>();
            return await set.Where(query).ToListAsync();
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="Id">主键集合</param>
        /// <returns></returns>
        public virtual async Task<TEntity> Find(params object[] keyValues)
        {
            return await _context.Set<TEntity>().FindAsync(keyValues);
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="query">条件表达树</param>
        /// <returns></returns>
        public virtual async Task<TEntity> Find(Expression<Func<TEntity, bool>> query)
        {
            return await _context.Set<TEntity>().Where(query).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="OTEntity"></param>
        public virtual async Task Insert(TEntity OTEntity)
        {
            await _context.Set<TEntity>().AddAsync(OTEntity);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="ArrTEntity"></param>
        public virtual async Task InsertRange(IEnumerable<TEntity> ArrTEntity)
        {
            await _context.Set<TEntity>().AddRangeAsync(ArrTEntity);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="OTEntity"></param>
        public virtual void Delete(TEntity OTEntity)
        {
            _logger.LogInformation($"Delete-{OTEntity}");
            _context.Set<TEntity>().Remove(OTEntity);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id">hash键</param>
        /// <param name="rangeKey">range键</param>
        public virtual async Task Delete(object Id)
        {
            _logger.LogInformation($"Delete-{Id}");
            var OTEntity = await _context.Set<TEntity>().FindAsync(Id);
            _context.Remove<TEntity>(OTEntity);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="OTEntity"></param>
        public virtual void Update(TEntity OTEntity)
        {
            _logger.LogInformation($"Update-{OTEntity}");
            _context.Set<TEntity>().Update(OTEntity);
        }
    }
}