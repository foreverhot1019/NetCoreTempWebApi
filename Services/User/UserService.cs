using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCoreTemp.WebApi.Services.Base;
using NetCoreTemp.WebApi.Models.Entity;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Models.AutoMapper;
using NetCoreTemp.WebApi.Models.DatabaseContext;

namespace NetCoreTemp.WebApi.Services
{
    public class UserService : BaseService<User>, IUserService, IQueryByFilterRules<User>
    {
        private readonly AppDbContext _context;
        private ILogger<UserService> _logger;
        private UserQuery _searchQuery;

        public UserService(AppDbContext appDbContext, ILogger<UserService> logger) :
            base(appDbContext, logger)
        {
            _context = appDbContext;
            _logger = logger;
            _searchQuery = new UserQuery();
        }

        /// <summary>
        /// 获取用户和权限
        /// </summary>
        /// <param name="filterRules">搜索条件</param>
        /// <returns></returns>
        public async Task<User> getUserRoles(string Id,object rangeKey=null)
        {
            _logger.LogInformation($"getUserRoles-{{Id:\"{Id}\",rangeKey:\"{rangeKey}\"}}");
            var User = await Find(Id, rangeKey);
            return User;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="filterRules">动态条件</param>
        /// <returns></returns>
        public IQueryable<User> QueryByFilterRules(IEnumerable<filterRule> filterRules)
        {
           return SearchFilter(_searchQuery.WithFilterRule(filterRules));
        }
    }
}
