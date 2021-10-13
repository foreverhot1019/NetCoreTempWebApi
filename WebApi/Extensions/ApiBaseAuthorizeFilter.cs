using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.WebApi.Models.View_Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Helpdesk.WebApi.Extensions
{
    public class ApiBaseAuthorizeFilter : AuthorizeFilter //IAsyncAuthorizationFilter //IAuthorizationFilter
    {
        public ApiBaseAuthorizeFilter()
        {
        }

        /// <summary>
        /// 授权
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            return base.OnAuthorizationAsync(context);
            //var hd = context.HttpContext.Request.Headers["X-Token"];
            //var token = hd.ToString();
            //if (string.IsNullOrWhiteSpace(token))
            //{
            //    context.Result = new JsonResult(new ActionReturnMessage
            //    {
            //        StatusCode = System.Net.HttpStatusCode.Unauthorized,
            //        IsSuccess = false,
            //        ErrMessage = "账户没有登录"
            //    });
            //}
            //else
            //{

            //}
            //return Task.CompletedTask;
        }
    }
}
