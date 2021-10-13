﻿using System;
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
    public class RoleService : BaseService<Role>, IRoleService, IQueryByFilterRules<Role>
    {
        private readonly AppDbContext _context;
        private ILogger<RoleService> _logger;
        private RoleQuery _searchQuery;

        public RoleService(ILogger<RoleService> logger,
            AppDbContext appDbContext) :
            base(appDbContext, logger)
        {
            _context = appDbContext;
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
