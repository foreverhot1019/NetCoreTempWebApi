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
    public class MenuService : BaseService<Menu>, IMenuService, IQueryByFilterRules<Menu>
    {
        private readonly AppDbContext _context;
        private ILogger<MenuService> _logger;
        private MenuQuery _searchQuery;

        public MenuService(ILogger<MenuService> logger,
            AppDbContext appDbContext) :
            base(appDbContext, logger)
        {
            _context = appDbContext;
            _logger = logger;
            _searchQuery = new MenuQuery();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="filterRules">动态条件</param>
        /// <returns></returns>
        public IQueryable<Menu> QueryByFilterRules(IEnumerable<filterRule> filterRules)
        {
            return SearchFilter(_searchQuery.WithFilterRule(filterRules));
        }
    }
}
