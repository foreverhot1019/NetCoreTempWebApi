using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCoreTemp.WebApi.Models.Entity;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Models.AutoMapper;
using NetCoreTemp.WebApi.Models.DatabaseContext;
using NetCoreTemp.WebApi.Services.Base;

namespace NetCoreTemp.WebApi.Services
{
    public class UserRoleService : BaseService<UserRole>, IUserRoleService, IQueryByFilterRules<UserRole>
    {
        private readonly AppDbContext _context;
        private ILogger<UserRoleService> _logger;
        private UserRoleQuery _searchQuery;

        public UserRoleService(ILogger<UserRoleService> logger,
            AppDbContext appDbContext,
            IServiceProvider serviceProvider) :
            base(serviceProvider, logger)
        {
            _context = appDbContext;
            _logger = logger;
            _searchQuery = new UserRoleQuery();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="filterRules">动态条件</param>
        /// <returns></returns>
        public IQueryable<UserRole> QueryByFilterRules(IEnumerable<filterRule> filterRules)
        {
            return SearchFilter(_searchQuery.WithFilterRule(filterRules));
        }
    }
}
