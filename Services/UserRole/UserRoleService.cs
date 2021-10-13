using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.WebApi.Services.Base;
using Helpdesk.WebApi.Models;
using Helpdesk.WebApi.Models.View_Model;
using Helpdesk.WebApi.Models.AutoMapper;
using Helpdesk.WebApi.Models.DatabaseContext;

namespace Helpdesk.WebApi.Services
{
    public class UserRoleService : BaseService<UserRole>, IUserRoleService, IQueryByFilterRules<UserRole>
    {
        private readonly AppDbContext _context;
        private ILogger<UserRoleService> _logger;
        private UserRoleQuery _searchQuery;

        public UserRoleService(ILogger<UserRoleService> logger,
            AppDbContext appDbContext) :
            base(appDbContext, logger)
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
