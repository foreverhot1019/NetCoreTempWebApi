using NetCoreTemp.WebApi.Models;
using NetCoreTemp.WebApi.Models.Extensions;
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
                //BaseFieldSearch
                filterRules.AddBaseSearchQuery<UserRole>(this);

                foreach (var rule in filterRules)
                {
                    if (string.IsNullOrWhiteSpace(rule.value))
                        continue;
                    else
                    {
                        //去除AutoMapper-Dto前缀 "_"
                        rule.field = rule.field.CleanAutoMapperDtoPrefix();
                    }
                    if (rule.field == "RoleId")
                    {
                        //if (int.TryParse(rule.value, out int val))
                        if (Guid.TryParse(rule.value, out Guid guid))
                            And(x => x.RoleId == guid);
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
