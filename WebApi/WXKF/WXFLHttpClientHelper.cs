using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.Logging;

namespace NetCoreTemp.WebApi.WXKF
{
    /// <summary>
    /// 微信Api 访问助手
    /// </summary>
    public class WXFLHttpClientHelper
    {
        /// <summary>
        /// 微信客服Url
        /// </summary>
        public string WXKFBaseUrl;

        /// <summary>
        /// 通过IHttpClientFactory获取
        /// global里polly设置了重试策略
        /// </summary>
        private HttpClient httpClient;

        /// <summary>
        /// DI中获取IHttpClientFactory服务
        /// </summary>
        private IHttpClientFactory httpClientFactory;

        /// <summary>
        /// 本机缓存
        /// </summary>
        private MemoryCache _cache = MemoryCache.Default;

        /// <summary>
        /// 微信客服数据
        /// </summary>
        private string WXKF_CorpID;
        private string WXKF_Secret;
        //记录 企业微信accessToken
        public string accessToken;

        private ILogger _logger;

        public WXFLHttpClientHelper(IHttpClientFactory _httpClientFactory, ILogger<WXFLHttpClientHelper> logger)
        {
            _logger = logger;
            //从DI中获取注册的服务
            httpClientFactory = _httpClientFactory;
            httpClient = httpClientFactory.CreateClient("WXKF");
            if (httpClient.BaseAddress == null)
            {
                //获取WXKF-baseUrl
                WXKFBaseUrl = System.Configuration.ConfigurationManager.AppSettings["WXKFBaseUrl"] ?? "https://qyapi.weixin.qq.com/cgi-bin/kf/";
                httpClient.BaseAddress = new Uri(WXKFBaseUrl);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = TimeSpan.FromSeconds(100);//100秒超时
            }

            //微信客服数据
            WXKF_CorpID = System.Configuration.ConfigurationManager.AppSettings["WXKF_CorpID"] ?? "";
            WXKF_Secret = System.Configuration.ConfigurationManager.AppSettings["WXKF_Secret"] ?? "";
            //初始化 token
            accessToken = Task.Run(async () =>
            {
                var res = await GetToken();
                return res.access_token;
            })?.Result;
        }

        /// <summary>
        /// Polly重试&401重新获取token策略
        /// </summary>
        /// <param name="MethodName">方法名</param>
        /// <returns></returns>
        private AsyncPolicyWrap<HttpResponseMessage> CreateRetryPolicy(string MethodName = null)
        {
            // 为每个重试定义超时策略
            AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(100);
            //重试策略
            var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(res =>
            {
                MethodName = MethodName ?? res.RequestMessage.RequestUri.ToString();
                //微信错误码：https://work.weixin.qq.com/api/doc/90000/90139/90313
                res.Headers.TryGetValues("Error-Code", out IEnumerable<string> ArrValues);
                if (ArrValues != null && ArrValues.Any(x => x != "0"))
                {
                    //WXKFResponseBase resObj = null;
                    //var resJsonStr = await res.Content.ReadAsStringAsync();
                    //resObj = JsonConvert.DeserializeObject<WXKFResponseBase>(resJsonStr);

                    res.Headers.TryGetValues("Error-Msg", out IEnumerable<string> ArrErrMsg);
                    _logger.LogError($"{MethodName}-Error:{(ArrErrMsg?.Any() == true ? string.Join(";", ArrErrMsg) : "")}");
                    //缺少access_token参数 或者access_token过期  会话已经结束(分配状态)
                    if (ArrValues.Any(x => x == "41001" || x == "42001"))
                        return true;
                    else
                    {
                        return false;
                    }
                }
                else
                    return false;
            })
            //.Or<TimeoutRejectedException>() // 若超时则抛出此异常
            .Or<UnauthorizedAccessException>() //若超时则抛出此异常
            .WaitAndRetryAsync(new[] { //重试3次
                 TimeSpan.FromSeconds(1),//第一次重试间隔 1秒
                 TimeSpan.FromSeconds(2),//第二次重试间隔 2秒
                 TimeSpan.FromSeconds(5)//第三次重试间隔 5秒
            }, async (res, timeSpan, ctx) =>
            {
                var reGetToken = false;
                //微信错误码：https://work.weixin.qq.com/api/doc/90000/90139/90313
                res.Result.Headers.TryGetValues("Error-Code", out IEnumerable<string> ArrValues);
                //缺少access_token参数 或者access_token过期
                if (ArrValues != null && ArrValues.Any(x => x == "41001" || x == "42001"))
                    reGetToken = true;
                else
                    reGetToken = false;
                if (res.Exception is UnauthorizedAccessException || reGetToken)
                {
                    accessToken = "";
                    _logger.LogInformation("获取微信Token");
                    await GetToken();
                }
                _logger.LogInformation($"重试{ctx.PolicyKey}-{timeSpan}-{res.Exception ?? res.Exception?.InnerException}");
            }).WrapAsync(timeoutPolicy);
            return retryPolicy;
        }

        /// <summary>
        /// 获取Token字符串
        /// </summary>
        /// <param name="corpid"></param>
        /// <param name="corpsecret"></param>
        /// <returns></returns>
        public async Task<GetTokenRes> GetToken(string corpid = "", string corpsecret = "")
        {
            GetTokenRes tokenRes;

            #region Polly 重试策略

            var _httpRequestPolicy = Policy.HandleResult<HttpResponseMessage>(
               r => r.StatusCode == HttpStatusCode.Unauthorized)
            .Or<TimeoutException>()
            .WaitAndRetryAsync(new[] { //重试3次
                 TimeSpan.FromSeconds(1),//第一次重试间隔 1秒
                 TimeSpan.FromSeconds(2),//第二次重试间隔 2秒
                 TimeSpan.FromSeconds(3)//第三次重试间隔 3秒
            }, (res, timeSpan) =>
            {
                _logger.LogInformation($"重试WX-gettoken-{res.Exception ?? res.Exception?.InnerException}");
            })
            .ExecuteAsync(async () =>
            {
                corpid = string.IsNullOrEmpty(corpid) ? WXKF_CorpID : corpid;
                corpsecret = string.IsNullOrEmpty(corpsecret) ? WXKF_Secret : corpsecret;
                string url = "gettoken?corpid={0}&corpsecret={1}";
                url = string.Format(url, corpid, corpsecret);
                var _httpClient = httpClientFactory.CreateClient("WXAccessToken");
                return await _httpClient.GetAsync(url);
            });
            var _httpRes = await _httpRequestPolicy;

            #endregion

            var json = await _httpRes.Content.ReadAsStringAsync();
            tokenRes = JsonConvert.DeserializeObject<GetTokenRes>(json);
            accessToken = tokenRes.access_token;
            return tokenRes;
        }

        /// <summary>
        /// 根据微信客服事件返回的token，获取消息
        /// </summary>
        /// <param name="eventToken">WXKF事件返回的token</param>
        /// <param name="cursor">上一次搜索位置</param>
        /// <param name="limit">搜索条数</param>
        /// <returns></returns>
        public async Task<WXKFMessageResponse> GetWXKFMessageByEventToken(string eventToken, string cursor = "", int limit = 30)
        {
            #region 获取消息列表

            //errcode: 95007 不合法的msgtoken
            var _httpRequestPolicy = CreateRetryPolicy("GetWXKFMessageByEventToken");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    cursor = cursor,//上一次搜索位置
                    token = eventToken,
                    limit = limit
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                var resWXKFMsgRes = await httpClient.PostAsync($"sync_msg?access_token={accessToken}", content);

                return resWXKFMsgRes;
            });
            var resStr = await resWXKFMsg.Content.ReadAsStringAsync();
            _logger.LogInformation("GetWXKFMessageByEventToken:--" + resStr);
            return JsonConvert.DeserializeObject<WXKFMessageResponse>(resStr);

            #endregion
        }

        #region 接待人员

        /// <summary>
        /// 添加接待人员
        /// </summary>
        /// <param name="open_kfid">客服帐号ID</param>
        /// <param name="userid_list">接待人员userid列表。第三方应用填密文userid，即open_userid 可填充个数：1 ~100。超过100个需分批调用</param>
        /// <returns></returns>
        public async Task<WXKFServicerResponse> AddServicers(string open_kfid, IEnumerable<string> userid_list)
        {
            WXKFServicerResponse WXKFServicerResponseObj = null;
            var _httpRequestPolicy = CreateRetryPolicy("AddServicers");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    open_kfid = open_kfid,// 客服帐号ID
                    userid_list = userid_list,// 接待人员userid列表。第三方应用填密文userid，即open_userid 可填充个数：1 ~100。超过100个需分批调用。
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"servicer/add?access_token={accessToken}", content);
            });
            var WXKFListResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFListResStr))
            {
                WXKFServicerResponseObj = JsonConvert.DeserializeObject<WXKFServicerResponse>(WXKFListResStr);
            }
            _logger.LogInformation("AddServicers:--" + WXKFListResStr);
            return WXKFServicerResponseObj;
        }

        /// <summary>
        /// 删除接待人员
        /// </summary>
        /// <param name="open_kfid">客服帐号ID</param>
        /// <param name="userid_list">接待人员userid列表。第三方应用填密文userid，即open_userid 可填充个数：1 ~100。超过100个需分批调用</param>
        /// <returns></returns>
        public async Task<WXKFServicerResponse> DelServicers(string open_kfid, IEnumerable<string> userid_list)
        {
            WXKFServicerResponse WXKFServicerResponseObj = null;
            var _httpRequestPolicy = CreateRetryPolicy("DelServicers");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    open_kfid = open_kfid,// 客服帐号ID
                    userid_list = userid_list,// 接待人员userid列表。第三方应用填密文userid，即open_userid 可填充个数：1 ~100。超过100个需分批调用。
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"servicer/del?access_token={accessToken}", content);
            });
            var WXKFListResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFListResStr))
            {
                WXKFServicerResponseObj = JsonConvert.DeserializeObject<WXKFServicerResponse>(WXKFListResStr);
            }
            _logger.LogInformation("DelServicers:--" + WXKFListResStr);
            return WXKFServicerResponseObj;
        }

        /// <summary>
        /// 获取接待人员列表
        /// </summary>
        /// <param name="open_kfid">客服帐号ID</param>
        /// <returns></returns>
        public async Task<WXKFServicerResponse> GetServicers(string open_kfid)
        {
            WXKFServicerResponse WXKFServicerResponseObj = null;
            var _httpRequestPolicy = CreateRetryPolicy("GetServicers");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                return await httpClient.GetAsync($"servicer/list?access_token={accessToken}&open_kfid={open_kfid}");
            });
            var WXKFListResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFListResStr))
            {
                WXKFServicerResponseObj = JsonConvert.DeserializeObject<WXKFServicerResponse>(WXKFListResStr);
            }
            //_logger.LogInformation("GetServicers:--" + WXKFListResStr);
            return WXKFServicerResponseObj;
        }

        #endregion

        /// <summary>
        /// 获取客服会话状态
        /// </summary>
        /// <param name="open_kfid">客服帐号ID</param>
        /// <param name="external_userid">微信客户ID</param>
        /// <returns></returns>
        public async Task<WXKFServicerResponse> GetKFSessionStatus(string open_kfid, string external_userid)
        {
            WXKFServicerResponse WXKFServicerResponseObj = null;
            var _httpRequestPolicy = CreateRetryPolicy("GetKFSessionStatus");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    open_kfid = open_kfid,
                    external_userid = external_userid
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"service_state/get?access_token={accessToken}", content);
            });
            var WXKFListResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFListResStr))
            {
                WXKFServicerResponseObj = JsonConvert.DeserializeObject<WXKFServicerResponse>(WXKFListResStr);
            }
            _logger.LogInformation($"GetKFSessionStatus:--" + WXKFListResStr);
            return WXKFServicerResponseObj;
        }

        /// <summary>
        /// 变更客服会话状态
        /// </summary>
        /// <param name="open_kfid">客服帐号ID</param>
        /// <param name="external_userid">微信客户ID</param>
        /// <param name="service_state">变更的目标状态:1.直接用API自动回复消息。2.放进待接入池等待接待人员接待。3.指定接待人员进行接待。4.结束</param>
        /// <param name="servicer_userid">接待人员ID</param>
        /// <returns></returns>
        public async Task<WXKFServicerResponse> ChangKFSessionStatus(string open_kfid, string external_userid, int service_state = 1, string servicer_userid = "")
        {
            //1.直接用API自动回复消息。2.放进待接入池等待接待人员接待。3.指定接待人员进行接待。4.结束
            WXKFServicerResponse WXKFServicerResponseObj = null;
            //Policy.HandleResult<HttpResponseMessage>(res =>
            //{
            //    res.Headers.TryGetValues("Error-Code", out IEnumerable<string> ArrValues);
            //    //会话已经结束(分配状态)
            //    if (ArrValues.Any(x => x == "95013"))
            //        return true;
            //    else
            //        return false;
            //}).RetryAsync(async (res, timeSpan, ctx) => {
            //    //微信错误码：https://work.weixin.qq.com/api/doc/90000/90139/90313
            //    res.Result.Headers.TryGetValues("Error-Code", out IEnumerable<string> ArrValues);
            //    //会话已经结束
            //    if(ArrValues!=null && ArrValues.Any(x=>x == "95013"))
            //    {

            //    }
            //});

            var _httpRequestPolicy = CreateRetryPolicy("ChangKFSessionStatus");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    open_kfid = open_kfid,// 客服帐号ID
                    external_userid = external_userid,// 微信客户的external_userid
                    service_state = service_state,// 变更的目标状态，状态定义和所允许的变更可参考概述中的流程图和表格
                    servicer_userid = servicer_userid // 接待人员的userid，当state = 3时要求必填
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"service_state/trans?access_token={accessToken}", content);
            });
            var WXKFListResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFListResStr))
            {
                WXKFServicerResponseObj = JsonConvert.DeserializeObject<WXKFServicerResponse>(WXKFListResStr);
            }
            _logger.LogInformation($"ChangKFSessionStatus:--" + WXKFListResStr);
            return WXKFServicerResponseObj;
        }

        /// <summary>
        /// 发送微信客服消息
        /// 当微信客户处于“新接入待处理”或“由智能助手接待”状态下，可调用该接口给用户发送消息。
        /// 注意仅当微信客户在主动发送消息给客服后的48小时内，企业可发送消息给客户，最多可发送5条消息；若用户继续发送消息，企业可再次下发消息。
        /// </summary>
        /// <param name="touser">接收消息的客户UserID</param>
        /// <param name="open_kfid">发送消息的客服帐号ID</param>
        /// <param name="MsgStr">文本消息</param>
        /// <returns></returns>
        public async Task<WXKFMsgResponse> SendWXKFMsg(string touser, string open_kfid, string MsgStr)
        {
            //1.直接用API自动回复消息。2.放进待接入池等待接待人员接待。3.指定接待人员进行接待
            WXKFMsgResponse WXKFMsgResponseObj = null;
            var _httpRequestPolicy = CreateRetryPolicy("SendWXKFMsg");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    touser = touser,// 指定接收消息的客户UserID
                    open_kfid = open_kfid,// 指定发送消息的客服帐号ID
                    //msgid = "",// 指定消息ID
                    msgtype = "text", // 消息类型
                    text = new textMsg // 文本消息
                    {
                        content = MsgStr
                    }
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"send_msg?access_token={accessToken}", content);
            });
            var WXKFMsgResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFMsgResStr))
            {
                WXKFMsgResponseObj = JsonConvert.DeserializeObject<WXKFMsgResponse>(WXKFMsgResStr);
            }
            _logger.LogInformation($"SendWXKFMsg:--" + WXKFMsgResStr);
            return WXKFMsgResponseObj;
        }

        /// <summary>
        /// 发送事件响应消息 （欢迎语、排队提示语、发送非工作时间的提示语或超时未回复的提示语、发送结束会话提示语或满意度评价等）
        /// 开发者可以此code为凭证，调用该接口给用户发送相应事件场景下的消息，如客服欢迎语、客服提示语和会话结束语等
        /// 除"用户进入会话事件"以外，响应消息仅支持会话处于获取该code的会话状态时发送，如将会话转入待接入池时获得的code仅能在会话状态为"待接入池排队中"时发送
        /// </summary>
        /// <param name="code">事件回调时携带的code</param>
        /// <param name="msgType">要发送消息的类型</param>
        /// <returns></returns>
        public async Task<WXKFMsgResponse> SendWXKFMsgOnEvent(string code, WXKFSendMsgType msgType)
        {
            //1.直接用API自动回复消息。2.放进待接入池等待接待人员接待。3.指定接待人员进行接待
            WXKFMsgResponse WXKFMsgResponseObj = null;
            var _httpRequestPolicy = CreateRetryPolicy("SendWXKFMsg");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var content = new StringContent("");
                switch (msgType)
                {
                    case WXKFSendMsgType.Userenter:
                        break;
                    case WXKFSendMsgType.Endsession:
                        var obj = new
                        {
                            code = code,// 指定code
                            msgid = "",
                            msgtype = "msgmenu", // 消息类型
                            msgmenu = new
                            {
                                head_content = "您对本次服务是否满意",
                                list = new List<MenuMsg> // 菜单消息
                                {
                                    new MenuMsg(){ id="FeedBack01",content="满意" },
                                    new MenuMsg(){ id="FeedBack02",content="一般" },
                                    new MenuMsg(){ id="FeedBack03",content="不满意" }
                                },
                                tail_content = "如有问题，随时联系小吉在线。"
                            }
                        };
                        _logger.LogInformation($"发送满意度消息 StringContent begin：{JsonConvert.SerializeObject(obj)}");
                        content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                        break;
                    default:
                        break;
                }
                _logger.LogInformation($"发送满意度消息 begin：{JsonConvert.SerializeObject(content)}");
                return await httpClient.PostAsync($"send_msg_on_event?access_token={accessToken}", content);
            });
            var WXKFMsgResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFMsgResStr))
            {
                WXKFMsgResponseObj = JsonConvert.DeserializeObject<WXKFMsgResponse>(WXKFMsgResStr);
            }
            _logger.LogInformation($"SendWXKFMsgOnEvent:--" + WXKFMsgResStr);
            return WXKFMsgResponseObj;
        }

        /// <summary>
        /// 获取客户基本信息
        /// </summary>
        /// <typeparam name="WXKFServicerResponse"></typeparam>
        /// <param name="external_userid_list">外部客户Id</param>
        /// <returns></returns>
        public async Task<WXKFServicerResponse> GetExternalUserMsg(IEnumerable<string> external_userid_list)
        {
            WXKFServicerResponse WXKFServicerResponseObj = null;
            var _httpRequestPolicy = CreateRetryPolicy("GetExternalUserMsg");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    external_userid_list = external_userid_list
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"customer/batchget?access_token={accessToken}", content);
            });
            var WXKFListResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFListResStr))
            {
                WXKFServicerResponseObj = JsonConvert.DeserializeObject<WXKFServicerResponse>(WXKFListResStr);
            }
            _logger.LogInformation($"GetExternalUserMsg:--" + WXKFListResStr);
            return WXKFServicerResponseObj;
        }

        /// <summary>
        /// 添加媒体文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public async Task<WXKFMediaResponse> AddMedia(string filePath)
        {
            //type 图片（image）、语音（voice）、视频（video），普通文件（file）
            var type = "file";
            FileInfo fileInfo = new FileInfo(filePath);
            switch (fileInfo.Extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".icon":
                case ".png":
                    type = "image";
                    break;
                case ".mp3":
                case ".m4a":
                    type = "voice";
                    break;
                case ".mp4":
                    type = "video";
                    break;
                default:
                    type = "file";
                    break;
            }
            // https://qyapi.weixin.qq.com/cgi-bin/media/upload?access_token=ACCESS_TOKEN&type=TYPE
            WXKFMediaResponse ResObj = null;
            var _httpRequestPolicy = CreateRetryPolicy("AddMedia");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var streamContent = new StreamContent(fileInfo.OpenRead());
                var content = new MultipartFormDataContent();
                content.Add(streamContent, "media", fileInfo.Name);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                return await httpClient.PostAsync($"media/upload?access_token={accessToken}&type={type}", content);
            });
            var WXKFMsgResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFMsgResStr))
            {
                ResObj = JsonConvert.DeserializeObject<WXKFMediaResponse>(WXKFMsgResStr);
            }
            _logger.LogInformation($"AddMedia:--" + WXKFMsgResStr);
            return ResObj;
        }

        #region 客服账号

        /// <summary>
        /// 添加客服账号
        /// </summary>
        /// <param name="KFName">客服账号名称</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public async Task<WXKFResponseBase> AddWXKFAccount(string KFName, string filePath)
        {
            WXKFMediaResponse mediaRes = null;
            if (string.IsNullOrEmpty(filePath))
            {
                mediaRes = await AddMedia(filePath);
            }
            var _httpRequestPolicy = CreateRetryPolicy("AddWXKFAccount");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    name = KFName,// 客服名称不多于16个字符
                    media_id = mediaRes?.media_id// 客服头像临时素材。
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"account/add?access_token={accessToken}", content);
            });
            WXKFResponseBase WXKFAccountRes = new WXKFResponseBase();
            var WXKFMsgResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFMsgResStr))
            {
                WXKFAccountRes = JsonConvert.DeserializeObject<WXKFResponseBase>(WXKFMsgResStr);
            }
            _logger.LogInformation($"AddWXKFAccount:--" + WXKFMsgResStr);
            return WXKFAccountRes;
        }

        /// <summary>
        /// 删除客服账号
        /// </summary>
        /// <param name="open_kfid">客服帐号ID。不多于64字节</param>
        /// <returns></returns>
        public async Task<WXKFResponseBase> DelWXKFAccount(string open_kfid)
        {
            var _httpRequestPolicy = CreateRetryPolicy("DelWXKFAccount");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    open_kfid = open_kfid,// 客服帐号ID。不多于64字节
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"account/del?access_token={accessToken}", content);
            });
            WXKFResponseBase WXKFAccountRes = new WXKFResponseBase();
            var WXKFMsgResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFMsgResStr))
            {
                WXKFAccountRes = JsonConvert.DeserializeObject<WXKFResponseBase>(WXKFMsgResStr);
            }
            _logger.LogInformation($"DelWXKFAccount:--" + WXKFMsgResStr);
            return WXKFAccountRes;
        }

        /// <summary>
        /// 修改客服账号
        /// </summary>
        /// <param name="open_kfid">客服帐号ID。不多于64字节</param>
        /// <param name="newKFName">客服账号名称</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public async Task<WXKFResponseBase> UpdateWXKFAccount(string open_kfid, string newKFName, string filePath)
        {
            WXKFMediaResponse mediaRes = null;
            if (string.IsNullOrEmpty(filePath))
            {
                mediaRes = await AddMedia(filePath);
            }
            var _httpRequestPolicy = CreateRetryPolicy("UpdateWXKFAccount");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    open_kfid = open_kfid,// 要修改的客服帐号ID。不多于64字节
                    name = newKFName,// 客服名称不多于16个字符
                    media_id = mediaRes?.media_id// 客服头像临时素材。
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"account/update?access_token={accessToken}", content);
            });
            WXKFResponseBase WXKFAccountRes = new WXKFResponseBase();
            var WXKFMsgResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFMsgResStr))
            {
                WXKFAccountRes = JsonConvert.DeserializeObject<WXKFResponseBase>(WXKFMsgResStr);
            }
            _logger.LogInformation($"UpdateWXKFAccount:--" + WXKFMsgResStr);
            return WXKFAccountRes;
        }

        /// <summary>
        /// 获取客服帐号列表
        /// </summary>
        /// <returns></returns>
        public async Task<WXKFServicerResponse> GetWXKFAccountList()
        {
            WXKFServicerResponse WXKFServicerResponseObj = null;
            var _httpRequestPolicy = CreateRetryPolicy("GetWXKFAccountList");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                //获取客服帐号列表
                return await httpClient.GetAsync($"account/list?access_token={accessToken}");
            });
            var WXKFListResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFListResStr))
            {
                WXKFServicerResponseObj = JsonConvert.DeserializeObject<WXKFServicerResponse>(WXKFListResStr);
            }
            _logger.LogInformation($"GetWXKFAccountList:--" + WXKFListResStr);
            return WXKFServicerResponseObj;
        }

        /// <summary>
        /// 获取所有客服接待人员信息
        /// </summary>
        /// <param name="cacheMinutes">缓存时间（分钟）</param>
        /// <param name="forceUpdate">强制刷新</param>
        /// <returns></returns>
        public async Task<WXKFServicerResponse> GetWXKFAccountServicerList(int cacheMinutes = 720,bool forceUpdate = false)
        {
            if (cacheMinutes < 0)
                cacheMinutes = 720;
            var cacheName = WXKFCacheKey.GetWXKFAccountServicerList.ToString();
            var obj = _cache.Get(cacheName);
            if (obj != null && !forceUpdate)
            {
                return (WXKFServicerResponse)obj;
            }
            else
            {
                WXKFServicerResponse WXKFServicerResponseObj = await GetWXKFAccountList();
                //var _httpRequestPolicy = CreateRetryPolicy("GetWXKFAccountList");
                //var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
                //{
                //    //获取客服帐号列表
                //    return await httpClient.GetAsync($"account/list?access_token={accessToken}");
                //});
                //var WXKFListResStr = await resWXKFMsg.Content.ReadAsStringAsync();
                //if (!string.IsNullOrEmpty(WXKFListResStr))
                //{
                //    WXKFServicerResponseObj = JsonConvert.DeserializeObject<WXKFServicerResponse>(WXKFListResStr);
                //}
                //客服帐号列表
                if (WXKFServicerResponseObj.account_list?.Any() == true)
                {
                    var ArrAccountList = new List<WXKFServicer>();
                    foreach (var item in WXKFServicerResponseObj.account_list)
                    {
                        //获取客服帐号-接单人员列表
                        var ServicerRes = await GetServicers(item.open_kfid);
                        if (ServicerRes?.servicer_list?.Any() == true)
                        {
                            ServicerRes.servicer_list.ForEach(x =>
                            {
                                x.open_kfid = item.open_kfid;
                                x.name = item.name;
                                x.avatar = item.avatar;
                            });
                            ArrAccountList.AddRange(ServicerRes.servicer_list);
                        }
                    }
                    WXKFServicerResponseObj.account_list = ArrAccountList;
                }
                _cache.Set(cacheName, WXKFServicerResponseObj, new DateTimeOffset().AddMinutes(cacheMinutes));
                return WXKFServicerResponseObj;
            }
        }

        /// <summary>
        /// 获取客服帐号链接
        /// </summary>
        /// <param name="open_kfid">客服帐号ID。不多于64字节</param>
        /// <param name="scene">场景值，字符串类型，由开发者自定义。</param>
        /// <returns></returns>
        public async Task<string> GetWXKFAccountLink(string open_kfid, string scene)
        {
            var link = "";
            var _httpRequestPolicy = CreateRetryPolicy("GetWXKFAccountLink");
            var resWXKFMsg = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                var obj = new
                {
                    open_kfid = open_kfid,// 客服帐号ID。不多于64字节
                    scene = scene // 场景值，字符串类型，由开发者自定义。不多于32字节字符串取值范围(正则表达式)：[0-9a-zA-Z_-]*
                };
                var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                return await httpClient.PostAsync($"add_contact_way?access_token={accessToken}", content);
            });
            WXKFResponseBase WXKFAccountRes = new WXKFResponseBase();
            var WXKFMsgResStr = await resWXKFMsg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(WXKFMsgResStr))
            {
                WXKFAccountRes = JsonConvert.DeserializeObject<WXKFResponseBase>(WXKFMsgResStr);
                if (WXKFAccountRes.errcode == 0)
                {
                    var key = "\"url\":";
                    var idx = WXKFMsgResStr.LastIndexOf(key);
                    if (idx > 0)
                        link = WXKFMsgResStr.Substring(idx + key.Length);
                }
            }
            _logger.LogInformation($"GetWXKFAccountLink:--" + WXKFMsgResStr);
            return link;
        }

        #endregion
    }
}