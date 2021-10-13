using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpdesk.WebApi.Models.View_Model;
using Microsoft.AspNetCore.Http;

namespace Helpdesk.WebApi.Extensions
{
    public class ExceptionMiddleWare : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                //抛给下一个中间件
                await next(context);
            }
            catch (Exception ex)
            {
                await WriteExceptionAsync(context, ex);
            }
            finally
            {
                await WriteExceptionAsync(context, null);
            }
        }

        /// <summary>
        /// 写错误
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private async Task WriteExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception != null)
            {
                var response = context.Response;
                var message = exception.InnerException == null ? exception.Message : exception.InnerException.Message;
                response.ContentType = "application/json";
                await response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(ActionReturnMessage.Error(message, 500))).ConfigureAwait(false);
            }
            else
            {
                var code = context.Response.StatusCode;
                switch (code)
                {
                    case 200:
                        return;
                    case 204:
                        return;
                    case 401:
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(ActionReturnMessage.Error("token已过期,请重新登录.", code))).ConfigureAwait(false);
                        break;
                    default:
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(ActionReturnMessage.Error("未知错误", code))).ConfigureAwait(false);
                        break;
                }
            }
        }
    }
}
