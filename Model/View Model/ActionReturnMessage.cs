using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Helpdesk.WebApi.Models.View_Model
{
    /// <summary>
    /// Action返回数据
    /// </summary>
    public class ActionReturnMessage
    {
        public ActionReturnMessage()
        {

        }

        /// <summary>
        /// http状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrMessage { get; set; }

        /// <summary>
        /// 返回数据集
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="StatusCode"></param>
        /// <returns></returns>
        public static ActionReturnMessage Error(string ErrMsg,int StatusCode=500)
        {
            return new ActionReturnMessage
            {
                StatusCode = (HttpStatusCode)StatusCode,
                ErrMessage = ErrMsg,
                IsSuccess = false,
                Data = null
            };
        }
    }
}
