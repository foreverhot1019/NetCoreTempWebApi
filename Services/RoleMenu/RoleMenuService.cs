using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.WebApi.Models;
using Helpdesk.WebApi.Models.DatabaseContext;
using Helpdesk.WebApi.Models.View_Model;
using Helpdesk.WebApi.Services.Base;
using Microsoft.Extensions.Logging;

namespace Helpdesk.WebApi.Services
{
    public class RoleMenuService : BaseService<RoleMenu>, IRoleMenuService, IQueryByFilterRules<RoleMenu>
    {
        private readonly AppDbContext _context;
        private ILogger<RoleMenuService> _logger;
        private RoleMenuQuery _searchQuery;

        public RoleMenuService(ILogger<RoleMenuService> logger,
            AppDbContext appDbContext) :
            base(appDbContext, logger)
        {
            _context = appDbContext;
            _logger = logger;
            _searchQuery = new RoleMenuQuery();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="filterRules">动态条件</param>
        /// <returns></returns>
        public IQueryable<RoleMenu> QueryByFilterRules(IEnumerable<filterRule> filterRules)
        {
            return SearchFilter(_searchQuery.WithFilterRule(filterRules));
        }
    }
}
