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
    public class MenuQuery : SearchQuery<Menu>
    {
        /// <summary>
        /// 搜索条件拼接
        /// </summary>
        /// <param name="filterRules">搜索条件</param>
        /// <returns></returns>
        public MenuQuery WithFilterRule(IEnumerable<filterRule> filterRules)
        {
            if (filterRules?.Any() == true)
            {
                //BaseFieldSearch
                SearchQuery<Menu> ref_searchQuery = this;
                filterRules.AddBaseSearchQuery<Menu>(ref ref_searchQuery);

                foreach (var rule in filterRules)
                {
                    if (string.IsNullOrWhiteSpace(rule.value))
                        continue;
                    else
                    {
                        //去除AutoMapper-Dto前缀 "_"
                        rule.field = rule.field.CleanAutoMapperDtoPrefix();
                    }
                    if (rule.field == "Hidden")
                    {
                        if (bool.TryParse(rule.value, out bool val))
                            And(x => x.Hidden == val);
                    }
                    if (rule.field == "ParentMenuId")
                    {
                        //if (int.TryParse(rule.value, out int val))
                        if (Guid.TryParse(rule.value, out Guid guid))// && guid != Guid.Empty
                            And(x => x.ParentMenuId == guid);
                    }
                    if (rule.field == "Name")
                    {
                        And(x => x.Name.StartsWith(rule.value));
                    }
                    if (rule.field == "Remark")
                    {
                        And(x => x.Remark.StartsWith(rule.value));
                    }

                    #region BaseFieldSearch

                    //if (rule.field == "ID")
                    //{
                    //    //if (int.TryParse(rule.value, out int val))
                    //    if (Guid.TryParse(rule.value, out Guid guid))// && guid != Guid.Empty
                    //        And(x => x.ID == guid);
                    //}
                    //if (rule.field == "Status")
                    //{
                    //    if (int.TryParse(rule.value, out int intVal))
                    //        And(x => x.Status == (Models.EnumType.EnumRepo.UseStatusEnum)intVal);
                    //}

                    #endregion
                }
            }
            return this;
        }
    }
}
