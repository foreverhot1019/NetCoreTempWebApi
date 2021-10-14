﻿using NetCoreTemp.WebApi.Models;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreTemp.WebApi.Services
{
    public class UserRoleQuery : SearchQuery<UserRole>
    {
        /// <summary>
        /// 搜索条件拼接
        /// </summary>
        /// <param name="filterRules">搜索条件</param>
        /// <returns></returns>
        public UserRoleQuery WithFilterRule(IEnumerable<filterRule> filterRules)
        {
            if (filterRules != null && filterRules.Any())
            {
                foreach (var rule in filterRules)
                {
                    if (string.IsNullOrWhiteSpace(rule.value))
                        continue;
                    if (rule.field == "ID")
                    {
                        //if (int.TryParse(rule.value, out int val))
                            And(x => x.ID == rule.value);
                    }
                    if (rule.field == "Remark")
                    {
                        And(x => x.Remark.Contains(rule.value));
                    }
                }
            }
            return this;
        }
    }
}
