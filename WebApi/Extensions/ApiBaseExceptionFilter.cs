using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Helpdesk.WebApi.Models.View_Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Helpdesk.WebApi.Extensions
{
    public class ApiBaseExceptionFilter : ExceptionFilterAttribute
    {
        private ILogger<ApiBaseExceptionFilter> _logger;
        public ApiBaseExceptionFilter(ILogger<ApiBaseExceptionFilter> logger)
        {
            _logger = logger;
            Order = 1;//筛选器顺序
        }

        /// <summary>
        /// 错误信息 抛出时
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(ExceptionContext context)
        {
            if (!context.ExceptionHandled)
            {
                var message = GetExceptionMsg(context.Exception);
                _logger.LogError(context.Exception, "错误：IP-{0}", context.HttpContext.Connection.RemoteIpAddress);

                context.ExceptionHandled = true;//指示错误已经处理
                context.Result = new JsonResult(new ActionReturnMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrMessage = message
                });
            }
            base.OnException(context);
        }

        /// <summary>
        /// 错误信息 抛出时
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task OnExceptionAsync(ExceptionContext context)
        {
            if (!context.ExceptionHandled)
            {
                var message = GetExceptionMsg(context.Exception);
                _logger.LogError(context.Exception, "错误：IP：{IP}", context.HttpContext.Connection.RemoteIpAddress);

                context.ExceptionHandled = true;//指示错误已经处理

                HttpResponse response = context.HttpContext.Response;
                context.Result = new JsonResult(new ActionReturnMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrMessage = message
                });
            }
            return base.OnExceptionAsync(context);
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <param name="Exception">错误</param>
        /// <param name="MaxTime">最大层级</param>
        /// <returns></returns>
        private string GetExceptionMsg(Exception Exception, int MaxTime = 10)
        {
            string message = Exception.Message;
            var Times = 0;
            while (Exception != null && string.IsNullOrEmpty(message))
            {
                message = Exception.Message;
                Exception = Exception.InnerException;
                Times++;
                if (Times >= MaxTime)
                {
                    break;
                }
            }
            message = string.IsNullOrEmpty(message) ? "未知错误" : message;
            return message;
        }
    }
}
