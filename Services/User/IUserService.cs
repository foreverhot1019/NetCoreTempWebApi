using Helpdesk.WebApi.Models;
using Helpdesk.WebApi.Models.View_Model;
using Helpdesk.WebApi.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpdesk.WebApi.Services
{
    public interface IUserService : IBaseService<User>
    {
        /// <summary>
        /// 获取用户和权限
        /// </summary>
        /// <param name="filterRules">搜索条件</param>
        /// <returns></returns>
        Task<User> getUserRoles(string Id, object rangeKey = null);
    }
}
