using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NetCoreTemp.WebApi.Models.View_Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace NetCoreTemp.WebApi.Extensions
{
    public class ApiBaseActionFilter : ActionFilterAttribute
    {

        /*
         * * 3xx 重定向-------------------------------
         * 300 Multiple Choices
         * 被请求的资源有一系列可供选择的回馈信息，每个都有自己特定的地址和浏览器驱动的商议信息。用户或浏览器能够自行选择一个首选的地址进行重定向。[17]
         * 除非这是一个HEAD请求，否则该响应应当包括一个资源特性及地址的列表的实体，以便用户或浏览器从中选择最合适的重定向地址。这个实体的格式由Content-Type定义的格式所决定。浏览器可能根据响应的格式以及浏览器自身能力，自动作出最合适的选择。当然，RFC 2616规范并没有规定这样的自动选择该如何进行。
         * 如果服务器本身已经有了首选的回馈选择，那么在Location中应当指明这个回馈的URI；浏览器可能会将这个Location值作为自动重定向的地址。此外，除非额外指定，否则这个响应也是可缓存的。
         * 301 Moved Permanently
         * 被请求的资源已永久移动到新位置，并且将来任何对此资源的引用都应该使用本响应返回的若干个URI之一。如果可能，拥有链接编辑功能的客户端应当自动把请求的地址修改为从服务器反馈回来的地址。[18]除非额外指定，否则这个响应也是可缓存的。
         * 新的永久性的URI应当在响应的Location域中返回。除非这是一个HEAD请求，否则响应的实体中应当包含指向新的URI的超链接及简短说明。
         * 如果这不是一个GET或者HEAD请求，那么浏览器禁止自动进行重定向，除非得到用户的确认，因为请求的条件可能因此发生变化。
         * 注意：对于某些使用HTTP/1.0协议的浏览器，当它们发送的POST请求得到了一个301响应的话，接下来的重定向请求将会变成GET方式。
         * 302 Found
         * 要求客户端执行临时重定向（原始描述短语为“Moved Temporarily”）。[19]由于这样的重定向是临时的，客户端应当继续向原有地址发送以后的请求。只有在Cache-Control或Expires中进行了指定的情况下，这个响应才是可缓存的。
         * 新的临时性的URI应当在响应的Location域中返回。除非这是一个HEAD请求，否则响应的实体中应当包含指向新的URI的超链接及简短说明。
         * 如果这不是一个GET或者HEAD请求，那么浏览器禁止自动进行重定向，除非得到用户的确认，因为请求的条件可能因此发生变化。
         * 注意：虽然RFC 1945和RFC 2068规范不允许客户端在重定向时改变请求的方法，但是很多现存的浏览器将302响应视作为303响应，并且使用GET方式访问在Location中规定的URI，而无视原先请求的方法。[20]因此状态码303和307被添加了进来，用以明确服务器期待客户端进行何种反应。[21]
         * * 4xx 客户端错误-------------------------------
         * 400 Bad Request 1、语义有误，当前请求无法被服务器理解。除非进行修改，否则客户端不应该重复提交这个请求。2、请求参数有误。
         * 401 Unauthorized “未认证”，即用户没有必要的凭据
         * 403 Forbidden 服务器已经理解请求，但是拒绝执行它。
         * 404 Not Found 请求失败，请求所希望得到的资源未被在服务器上发现，但允许用户的后续请求。
         * 405 Method Not Allowed 请求行中指定的请求方法不能被用于请求相应的资源。该响应必须返回一个Allow头信息用以表示出当前资源能够接受的请求方法的列表。
         * 408 Request Timeout 请求超时。根据HTTP规范，客户端没有在服务器预备等待的时间内完成一个请求的发送，客户端可以随时再次提交这一请求而无需进行任何更改。
         * 415 Unsupported Media Type 对于当前请求的方法和所请求的资源，请求中提交的互联网媒体类型并不是服务器中所支持的格式，因此请求被拒绝。
         * * 5xx 服务端错误-------------------------------
         * 500 Internal Server Error 通用错误消息，服务器遇到了一个未曾预料的状况，导致了它无法完成对请求的处理。没有给出具体错误信息。
         * 502 Bad Gateway  作为网关或者代理工作的服务器尝试执行请求时，从上游服务器接收到无效的响应。
         * 503 Service Unavailable 由于临时的服务器维护或者过载，服务器当前无法处理请求。这个状况是暂时的，并且将在一段时间以后恢复。
         * 504 Gateway Timeout 作为网关或者代理工作的服务器尝试执行请求时，未能及时从上游服务器（URI标识出的服务器，例如HTTP、FTP、LDAP）或者辅助服务器（例如DNS）收到响应。
         */

        private ILogger<ApiBaseActionFilter> _logger;

        public ApiBaseActionFilter(ILogger<ApiBaseActionFilter> logger)
        {
            _logger = logger;
            Order = 1;//筛选器顺序
        }

        /// <summary>
        /// 操作处理时
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var code = context.HttpContext.Response.StatusCode;
            _logger.LogInformation($"控制器：{context.ActionDescriptor.DisplayName}-{context.ActionArguments}");
            base.OnActionExecuting(context);
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var code = context.HttpContext.Response.StatusCode;
            return base.OnActionExecutionAsync(context, next);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            var code = context.HttpContext.Response.StatusCode;
            base.OnResultExecuted(context);
        }

        /// <summary>
        /// 结果返回时
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            int StatusCode = context.HttpContext.Response.StatusCode;//无效一直是200
            var result = context?.Result as ObjectResult;
            if (result != null && result?.StatusCode != null)
                StatusCode = result.StatusCode.Value;
            //模型验证成功(重定向 不处理)
            if ((StatusCode < 300 || StatusCode >= 400) && context.ModelState.IsValid)
            {
                if (result != null)
                {
                    if (!(result?.Value is ActionReturnMessage))
                    {
                        context.Result = new JsonResult(new ActionReturnMessage
                        {
                            StatusCode = (HttpStatusCode)StatusCode,
                            //客户端错误和服务端错误 返回错误信息是string时，直接填充 错误信息
                            ErrMessage = StatusCode >= 400 && result?.Value is string ? result?.Value.ToString() : "",
                            IsSuccess = StatusCode < 400,
                            Data = result?.Value
                        });
                    }
                }
                else
                {
                    if (!(context.Result is FileResult))
                    {
                        context.Result = new JsonResult(new ActionReturnMessage
                        {
                            StatusCode = (HttpStatusCode)context.HttpContext.Response.StatusCode,
                            ErrMessage = "",
                            IsSuccess = true,
                            Data = null
                        });
                    }
                }
            }
            return base.OnResultExecutionAsync(context, next);
        }
    }
}
