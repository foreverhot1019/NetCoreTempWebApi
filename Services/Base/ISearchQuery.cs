using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Helpdesk.WebApi.Services.Base
{
    /// <summary>
    /// 自定义搜索接口
    /// </summary>
    /// <typeparam name="TEntity">实体</typeparam>
    public interface ISearchQuery<TEntity> where TEntity : class, Helpdesk.WebApi.Models.BaseModel.IEntity_
    {
        Expression<Func<TEntity, bool>> Query();
        Expression<Func<TEntity, bool>> And(Expression<Func<TEntity, bool>> query);
        Expression<Func<TEntity, bool>> Or(Expression<Func<TEntity, bool>> query);
        Expression<Func<TEntity, bool>> And(ISearchQuery<TEntity> queryObject);
        Expression<Func<TEntity, bool>> Or(ISearchQuery<TEntity> queryObject);
    }

    /// <summary>
    /// 自定义搜索
    /// </summary>
    /// <typeparam name="TEntity">实体</typeparam>
    public abstract class SearchQuery<TEntity> : ISearchQuery<TEntity> where TEntity : class, Helpdesk.WebApi.Models.BaseModel.IEntity_
    {
        private Expression<Func<TEntity, bool>> _query;

        public virtual Expression<Func<TEntity, bool>> Query()
        {
            return _query;
        }

        public Expression<Func<TEntity, bool>> And(Expression<Func<TEntity, bool>> query)
        {
            return _query = _query == null ? query : _query.And(query.Expand());
        }

        public Expression<Func<TEntity, bool>> Or(Expression<Func<TEntity, bool>> query)
        {
            return _query = _query == null ? query : _query.Or(query.Expand());
        }

        public Expression<Func<TEntity, bool>> And(ISearchQuery<TEntity> queryObject)
        {
            return And(queryObject.Query());
        }

        public Expression<Func<TEntity, bool>> Or(ISearchQuery<TEntity> queryObject)
        {
            return Or(queryObject.Query());
        }
    }
}
