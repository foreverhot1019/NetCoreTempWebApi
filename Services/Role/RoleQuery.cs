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
    public class RoleQuery : SearchQuery<Role>
    {
        /// <summary>
        /// 搜索条件拼接
        /// </summary>
        /// <param name="filterRules">搜索条件</param>
        /// <returns></returns>
        public RoleQuery WithFilterRule(IEnumerable<filterRule> filterRules)
        {
            if (filterRules?.Any() == true)
            {
                //BaseFieldSearch
                SearchQuery<Role> ref_searchQuery = this;
                filterRules.AddBaseSearchQuery<Role>(ref ref_searchQuery);

                foreach (var rule in filterRules)
                {
                    if (string.IsNullOrWhiteSpace(rule.value))
                        continue;
                    else {
                        //去除AutoMapper-Dto前缀 "_"
                        rule.field = rule.field.CleanAutoMapperDtoPrefix();
                    }
                    if (rule.field == "Name")
                    {
                        And(x => x.Name.StartsWith(rule.value));
                    }
                    #region BaseFieldSearch

                    //if (rule.field == "ID")
                    //{
                    //    //if (int.TryParse(rule.value, out int val))
                    //    if (Guid.TryParse(rule.value, out Guid guid))// && guid != Guid.Empty
                    //        And(x => x.ID == guid);
                    //}
                    //if (rule.field == "CreateUserId")
                    //{
                    //    if (Guid.TryParse(rule.value, out Guid guid))// && guid != Guid.Empty
                    //        And(x => x.CreateUserId == guid);
                    //}
                    //if (rule.field == "CreateUserName")
                    //{
                    //    And(x => x.CreateUserName.StartsWith(rule.value));
                    //}
                    //if (rule.field == "CreateDate")
                    //{
                    //    if (long.TryParse(rule.value, out long val))
                    //        And(x => x.CreateDate == val);
                    //}
                    //if (rule.field == "_CreateDate")
                    //{
                    //    if (long.TryParse(rule.value, out long val))
                    //        And(x => x.CreateDate >= val);
                    //    else if (rule.value.Parse2DateTime(out DateTime? date))
                    //    {
                    //        var timespan = date.toLong();
                    //        And(x => x.CreateDate >= timespan);
                    //    }
                    //}
                    //if (rule.field == "CreateDate_")
                    //{
                    //    if (long.TryParse(rule.value, out long val))
                    //        And(x => x.CreateDate <= val);
                    //    else if (rule.value.Parse2DateTime(out DateTime? date))
                    //    {
                    //        var timespan = date.toLong();
                    //        And(x => x.CreateDate <= timespan);
                    //    }
                    //}
                    //if (rule.field == "ModifyUserId")
                    //{
                    //    if (Guid.TryParse(rule.value, out Guid guid))// && guid != Guid.Empty
                    //        And(x => x.ModifyUserId == guid);
                    //}
                    //if (rule.field == "ModifyUserName")
                    //{
                    //    And(x => x.ModifyUserName.StartsWith(rule.value));
                    //}
                    //if (rule.field == "ModifyDate")
                    //{
                    //    if (long.TryParse(rule.value, out long val))
                    //        And(x => x.ModifyDate == val);
                    //}
                    //if (rule.field == "_ModifyDate")
                    //{
                    //    if (long.TryParse(rule.value, out long val))
                    //        And(x => x.ModifyDate >= val);
                    //    else if (rule.value.Parse2DateTime(out DateTime? date))
                    //    {
                    //        var timespan = date.toLong();
                    //        And(x => x.ModifyDate >= timespan);
                    //    }
                    //}
                    //if (rule.field == "ModifyDate_")
                    //{
                    //    if (long.TryParse(rule.value, out long val))
                    //        And(x => x.ModifyDate <= val);
                    //    else if (rule.value.Parse2DateTime(out DateTime? date))
                    //    {
                    //        var timespan = date.toLong();
                    //        And(x => x.ModifyDate <= timespan);
                    //    }
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
