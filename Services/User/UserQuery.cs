using Helpdesk.WebApi.Models;
using Helpdesk.WebApi.Models.View_Model;
using Helpdesk.WebApi.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpdesk.WebApi.Services
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
