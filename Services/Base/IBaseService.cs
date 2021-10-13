using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Helpdesk.WebApi.Models.DatabaseContext;
using Helpdesk.WebApi.Models.View_Model;

namespace Helpdesk.WebApi.Services.Base
{
    public interface IBaseService<TEntity> where TEntity : class, Helpdesk.WebApi.Models.BaseModel.IEntity_
    {
        /// <summary>
        /// 返回DynamoDBRepository 仓库
        /// </summary>
        /// <returns></returns>
        AppDbContext GetDBContext();

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        IQueryable<TEntity> SearchFilter(ISearchQuery<TEntity> searchQuery);

        /// <summary>
        /// 搜索IQueryable
        /// </summary>
        /// <param name="query">条件表达树</param>
        /// <returns></returns>
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> query);

        /// <summary>
        /// 搜索数据集
        /// </summary>
        /// <param name="query">条件表达树</param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> QueryList(Expression<Func<TEntity, bool>> query);

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="keyValues">主键集合</param>
        /// <returns></returns>
        Task<TEntity> Find(params object[] keyValues);

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="query">条件表达树</param>
        /// <returns></returns>
        Task<TEntity> Find(Expression<Func<TEntity, bool>> query);

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="OTEntity"></param>
        Task Insert(TEntity OTEntity);

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="ArrTEntity"></param>
        Task InsertRange(IEnumerable<TEntity> ArrTEntity);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="OTEntity"></param>
        void Delete(TEntity OTEntity);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id">主键</param>
        Task Delete(object Id);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="OTEntity"></param>
        void Update(TEntity OTEntity);

    }

    /// <summary>
    /// 动态搜索接口
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IQueryByFilterRules<TEntity> where TEntity : class, Helpdesk.WebApi.Models.BaseModel.IEntity_
    {
        IQueryable<TEntity> QueryByFilterRules(IEnumerable<filterRule> filterRules);
    }
}
