using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCoreTemp.WebApi.Models.Entity;
using NetCoreTemp.WebApi.Models.DatabaseContext;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Services.Base;
using Microsoft.Extensions.Logging;

namespace NetCoreTemp.WebApi.Services
{
    public class RoleService : BaseService<Role>, IRoleService, IQueryByFilterRules<Role>
    {
        private readonly AppDbContext _context;
        private ILogger<RoleService> _logger;
        private RoleQuery _searchQuery;

        public RoleService(ILogger<RoleService> logger,
            IServiceProvider serviceProvider) :
            base(serviceProvider, logger)
        {
            _context = base.GetDBContext();
            _logger = logger;
            _searchQuery = new RoleQuery();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="filterRules">动态条件</param>
        /// <returns></returns>
        public IQueryable<Role> QueryByFilterRules(IEnumerable<filterRule> filterRules)
        {
            return SearchFilter(_searchQuery.WithFilterRule(filterRules));
        }
    }
}
