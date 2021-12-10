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
    public class UserQuery : SearchQuery<User>
    {
        /// <summary>
        /// 搜索条件拼接
        /// </summary>
        /// <param name="filterRules">搜索条件</param>
        /// <returns></returns>
        public UserQuery WithFilterRule(IEnumerable<filterRule> filterRules)
        {
            if (filterRules?.Any() == true)
            {
                //BaseFieldSearch
                filterRules.AddBaseSearchQuery<User>(this);

                foreach (var rule in filterRules)
                {
                    if (string.IsNullOrWhiteSpace(rule.value))
                        continue;
                    else
                    {
                        //去除AutoMapper-Dto前缀 "_"
                        rule.field = rule.field.CleanAutoMapperDtoPrefix();
                    }
                    
                    if (rule.field == "Name")
                    {
                        And(x => x.Name.StartsWith(rule.value));
                    }
                    if (rule.field == "Email")
                    {
                        And(x => x.Email.StartsWith(rule.value));
                    }
                }
            }
            return this;
        }
    }
}
