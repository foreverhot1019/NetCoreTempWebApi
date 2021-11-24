using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetCoreTemp.WebApi.WXKF
{
    /// <summary>
    /// 利用DI注入的单例模式
    /// </summary>
    public class WXKFAssignHandler
    {
        #region 属性

        ////锁 加入数组数据
        //public static object lockInsertArrMsgObj = new object();

        /// <summary>
        /// 微信客服Url
        /// </summary>
        public string WXKFBaseUrl;

        /// <summary>
        /// 静态化，重复使用
        /// </summary>
        //private HttpClient httpClient;

        /// <summary>
        /// DI中获取IHttpClientFactory服务
        /// </summary>
        //private IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// 微信访问助手
        /// </summary>
        private WXFLHttpClientHelper _WXFLHttpClientHelper;

        /// <summary>
        /// 本机缓存
        /// </summary>
        private MemoryCache _cache = MemoryCache.Default;

        /// <summary>
        /// redisCache
        /// </summary>
        IDistributedCache redisCache;

        /// <summary>
        /// Redis缓存
        /// </summary>
        RedisHelp.RedisHelper redisHelper;

        /// <summary>
        /// 记录消息数据
        /// </summary>
        List<WXKFMessage> ArrMsg = new List<WXKFMessage>();

        /// <summary>
        /// 所有消息(SortedSet)
        /// </summary>
        public readonly static string ALLMsgListRedisKey = "WXKFMsgList:ALLMsgList";
        /// <summary>
        /// 已处理消息(SortedSet)
        /// </summary>
        public readonly static string MsgAnalysisedListRedisKey = "WXKFMsgList:MsgAnalysisedList";

        /// <summary>
        /// 客服正在接单的数量接待(String)
        /// 0：客服接待人员Id
        /// 1：Open_Kfid
        /// </summary>
        public readonly static string ServicerServingRedisKey = "Servicer:ServingNum:{0}:{1}";
        public readonly static string ServicerUserMappingRedisKey = "Servicer:" + WXKFCacheKey.kfid_servicerId_userid_mapping.ToString() + ":{0}";

        #region 会话数据

        /// <summary>
        /// 48小时内，客服正在接单的接待列表(SortedSet)
        /// 0:客服接单人员
        /// </summary>
        public readonly static string InServingRedisKey = "Servicer:InServing:{0}";

        /// <summary>
        /// 所有会话记录(SortedSet)
        /// </summary>
        public readonly static string ALLWXDialogRedisKey = "WXKFDialog:ALL";
        /// <summary>
        /// 会话记录(SortedSet)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        public readonly static string WXDialogRedisKey = "WXKFDialog:{0}:{1}";

        /// <summary>
        /// 会话等待(String，30分钟过期)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        public readonly static string WXDialogWaitingRedisKey = "WXKFDialog:Waiting:{0}:{1}";

        /// <summary>
        /// 所有结束的会话(SortedSet)
        /// </summary>
        public readonly static string ALLWXDialogEndingRedisKey = "WXKFDialog:Ending:ALL";
        /// <summary>
        /// 结束的会话(Hash)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        public readonly static string WXDialogEndingRedisKey = "WXKFDialog:Ending:{0}:{1}";

        /// <summary>
        /// 所有会话消息(SortedSet)
        /// </summary>
        public readonly static string ALLWXMessageDialogRedisKey = "WXKFMessageDialog:ALL";
        /// <summary>
        /// 会话消息ID对照(Hash)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        public readonly static string WXMessageDialogRedisKey = "WXKFMessageDialog:{0}:{1}";

        /// <summary>
        /// 每天0-1点清除未结束的会话
        /// 0:当天日期
        /// 每晚执行一次
        /// </summary>
        public readonly static string cleanKey = "WXKF_CleanData:{0}";
        public readonly static string AllcleanKey = "WXKF_CleanData:ALL";

        #endregion

        /// <summary>
        /// 微信客服最大接待人数
        /// </summary>
        public int WXKFServicerMaxServingNum = 10;

        /// <summary>
        /// 处理最近微信客服消息的分钟数
        /// </summary>
        public int WXKFMsgAnalysisClose2Minutes = 5;

        #endregion

        private ILogger _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WXKFAssignHandler(RedisHelp.RedisHelper _redisHelper,
            //IHttpClientFactory httpClientFactory, 
            WXFLHttpClientHelper wXFLHttpClientHelper,
            IDistributedCache distributedCache, ILogger<WXKFAssignHandler> logger)
        {
            redisHelper = _redisHelper;
            //_httpClientFactory = httpClientFactory;
            _WXFLHttpClientHelper = wXFLHttpClientHelper;
            redisCache = distributedCache;
            _logger = logger;

            var WXKFServicerMaxServingNumStr = System.Configuration.ConfigurationManager.AppSettings["WXKFServicerMaxServingNum"] ?? "10";
            int.TryParse(WXKFServicerMaxServingNumStr, out WXKFServicerMaxServingNum);

            var WXKFMsgAnalysisClose2MinutesStr = System.Configuration.ConfigurationManager.AppSettings["WXKFMsgAnalysisClose2Minutes"] ?? "5";
            int.TryParse(WXKFMsgAnalysisClose2MinutesStr, out WXKFMsgAnalysisClose2Minutes);
        }

        /// <summary>
        /// 新增消息数据处理
        /// 只处理最后5分钟内的消息（通过Redis进行管理是否处理过）
        /// </summary>
        /// <param name="OriginArr">原始消息列表（所有消息集合）</param>
        /// <param name="addArr">最近拉取的消息列表</param>
        /// <param name="newMsg">需要处理的消息列表</param>
        /// <param name="_redisHelper">redis帮助类</param>
        /// <param name="_WXKFMsgAnalysisClose2Minutes">处理最近微信客服消息的分钟数</param>
        Action<List<WXKFMessage>, IEnumerable<WXKFMessage>, List<WXKFMessage>, RedisHelp.RedisHelper, int> fucInsertMsg = new Action<List<WXKFMessage>, IEnumerable<WXKFMessage>, List<WXKFMessage>, RedisHelp.RedisHelper, int>((OriginArr, addArr, newMsg, _redisHelper, _WXKFMsgAnalysisClose2Minutes) =>
        {
            var nowUnixTimeStamp = DateTime.Now.AddMinutes(-_WXKFMsgAnalysisClose2Minutes).to_Long();
            //lock (lockInsertArrMsgObj)
            //{
            var last = OriginArr.FirstOrDefault();
            //数组正序
            addArr = addArr.OrderBy(x => x.send_time);
            var startAdd = false;
            //没有与上一次最后一条数据重复时，直接插入所有
            if (string.IsNullOrEmpty(last?.msgid) || !addArr.Any(x => x.msgid == last.msgid))
            {
                startAdd = true;
            }
            foreach (var item in addArr)
            {
                //插入所有序集合
                var tfAll = _redisHelper.SortedSetAdd(ALLMsgListRedisKey, item, Convert.ToDouble(item.send_time));
                if (!startAdd && item.msgid == last?.msgid)
                {
                    startAdd = true;
                }
                //插入成功（不重复）
                if (tfAll)
                    OriginArr.Insert(0, item);
                if (startAdd && item.send_time > (last?.send_time ?? 0))
                {
                    //插入已处理有序集合
                    if (item.send_time <= nowUnixTimeStamp)
                    {
                        bool tf = _redisHelper.SortedSetAdd(MsgAnalysisedListRedisKey, item, Convert.ToDouble(item.send_time));
                    }
                    //插入成功（不重复）
                    if (tfAll)
                    {
                        OriginArr.Insert(0, item);
                        newMsg.Add(item);
                    }
                }
            }
            //}
        });

        /// <summary>
        /// 分析微信客服消息
        /// </summary>
        /// <param name="WXKFEventXML"></param>
        public async Task AnalysisWXKFMessage(string WXKFEventXML)
        {
            //反序列化XML
            _logger.LogInformation($"AnalysisWXKFMessage Begin:{ WXKFEventXML}");
            WXKF_EventResponse WXKFRes = null;
            using (StringReader sr = new StringReader(WXKFEventXML))
            {
                System.Xml.Serialization.XmlSerializer xmlser;

                var _type = typeof(WXKF_EventResponse);
                xmlser = new System.Xml.Serialization.XmlSerializer(_type);
                WXKFRes = (WXKF_EventResponse)xmlser.Deserialize(sr);
            }
            _logger.LogInformation($"AnalysisWXKFMessage:{ JsonConvert.SerializeObject(WXKFRes)}");
            if (!string.IsNullOrEmpty(WXKFRes?.Token))
            {
                #region 获取消息列表

                //最后一次 消息位置Cache
                var cacheMsgListNext_Cursor = $"WXKFMsgList:Next_Cursor:{WXKFRes.ToUserName}";
                //
                var cacheMsgList = $"MsgList:{WXKFRes.ToUserName}";
                //新消息
                List<WXKFMessage> newXKFMessages = new List<WXKFMessage>();

                //设置最后一次 消息位置
                string next_cursor = "";
                next_cursor = await redisHelper.StringGetAsync(cacheMsgListNext_Cursor);
                //next_cursor = (_cache.Get(cacheMsgListNext_Cursor) ?? "").ToString();
                var WXKFMsgResObj = await _WXFLHttpClientHelper.GetWXKFMessageByEventToken(WXKFRes.Token, next_cursor);
                if (WXKFMsgResObj.msg_list.Any())
                {
                    //新增消息数据
                    fucInsertMsg(ArrMsg, WXKFMsgResObj.msg_list, newXKFMessages, redisHelper, WXKFMsgAnalysisClose2Minutes);
                }
                _logger.LogInformation($"AnalysisWXKFMessage has_more:{ WXKFMsgResObj.has_more}");
                //不能通过判断msg_list是否空来停止拉取，可能会出现has_more为1，而msg_list为空的情况
                while (WXKFMsgResObj.has_more) //获取最后一条消息
                {
                    next_cursor = WXKFMsgResObj.next_cursor;
                    WXKFMsgResObj = await _WXFLHttpClientHelper.GetWXKFMessageByEventToken(WXKFRes?.Token, next_cursor);
                    if (WXKFMsgResObj.msg_list.Any())
                    {
                        //新增消息数据
                        fucInsertMsg(ArrMsg, WXKFMsgResObj.msg_list, newXKFMessages, redisHelper, WXKFMsgAnalysisClose2Minutes);
                    }
                    //设置最后一次 消息位置
                    await redisHelper.StringSetAsync(cacheMsgListNext_Cursor, WXKFMsgResObj.next_cursor);
                    //int MaxMinutes = 30;
                    //_cache.Set(cacheMsgListNext_Cursor, WXKFMsgResObj.next_cursor, DateTimeOffset.Now.AddMinutes(MaxMinutes));
                }
                ////数组顺序反转
                //ArrMsg = ArrMsg.Distinct().OrderByDescending(x => x.send_time).ToList();
                _logger.LogInformation($"ArrMsg:{ArrMsg.Count()}");
                //设置消息未分析
                var Set_isAnalysis = ArrMsg.Where(x => x.send_time > DateTime.Now.AddMinutes(-WXKFMsgAnalysisClose2Minutes).to_Long());
                //_logger.LogInformation($"Set_isAnalysis:{Set_isAnalysis.Count()}-------------------------------");
                foreach (var item in Set_isAnalysis)
                {
                    item.isAnalysis = false;
                }

                #endregion
                ////设置最新的消息数据
                //_cache.Set(cacheMsgList, ArrMsg, DateTimeOffset.MaxValue);
                if (newXKFMessages.Any())
                {
                    var unixTimestamp = DateTime.Now.AddMinutes(-WXKFMsgAnalysisClose2Minutes).to_Long();
                    //设置消息未分析
                    var newSet_isAnalysis = newXKFMessages.Where(x => x.send_time > unixTimestamp);
                    //_logger.LogInformation($"newSet_isAnalysis:{newSet_isAnalysis.Count()}----------------------");
                    foreach (var item in newSet_isAnalysis)
                    {
                        item.isAnalysis = false;
                        //_logger.LogInformation($"newXKFMessages:{JsonConvert.SerializeObject(item)}+++++++++++++++++++++");
                    }
                    //只处理最近5分钟内的 消息
                    IEnumerable<WXKFMessage> ArrMsgisAnalysis = newXKFMessages.Where(x => !x.isAnalysis);
                    ////验证是否是第一次
                    //if (_ArrMsgObj == null)
                    //    ArrMsgisAnalysis = ArrMsg.Where(x => !x.isAnalysis);
                    //_logger.LogInformation($"ArrMsg:{ArrMsg.Count()},ArrMsgisAnalysis:{ArrMsgisAnalysis.Count()}");
                    foreach (var item in ArrMsgisAnalysis)
                    {
                        #region 克隆一个，得到原始数据 利用Redis判断是否处理过

                        var orign = item.Clone();
                        orign.isAnalysis = true;
                        //插入已处理有序集合
                        bool tf = await redisHelper.SortedSetAddAsync(MsgAnalysisedListRedisKey, orign, Convert.ToDouble(item.send_time));
                        if (!tf)
                            continue;

                        #endregion

                        //origin 3-微信客户发送的消息 4-系统推送的事件消息 5-接待人员在企业微信客户端发送的消息
                        WXKFMessage MsgObj = item;
                        _logger.LogInformation($"MsgObj:{JsonConvert.SerializeObject(MsgObj)}");
                        //origin:3-微信客户发送的消息
                        if (MsgObj.origin == 3)
                        {
                            #region 用户 发送消息 有接待客服在线直接分配 没有接待客服在线提醒用户

                            //有接待客服在线直接分配
                            //没有接待客服在线提醒用户
                            await CheckAndChangeServiceStatus(MsgObj);
                            MsgObj.isAnalysis = true;

                            #endregion
                        }
                        //origin:4-系统推送的事件消息
                        if (MsgObj.msgtype == "event")
                        {
                            //处理事件消息
                            await HandleEventMsg(MsgObj);
                            //设置消息已分析
                            MsgObj.isAnalysis = true;
                        }
                        //origin:4-接待人员在企业微信客户端发送的消息
                        if (MsgObj.origin == 5)
                        {
                            #region 插入会话消息数据

                            await AddWXMessageDialogRedisKey(MsgObj);
                            ////会话列表
                            //var newWXDialogRedisKey = string.Format(WXDialogRedisKey, MsgObj.open_kfid, MsgObj.external_userid);
                            ////消息会话列表
                            //var newWXMessageDialogRedisKey = string.Format(WXMessageDialogRedisKey, MsgObj.open_kfid, MsgObj.external_userid);
                            ////获取最后一个会话
                            //var wxDialog = redisHelper.SortedSetRangeByScore<WXKFDialog>(key: newWXDialogRedisKey, take: 1)?.FirstOrDefault();
                            //if (wxDialog.dialogId != Guid.Empty && !string.IsNullOrEmpty(wxDialog.servicerId))
                            //{
                            //    //加入消息会话列表
                            //    await redisHelper.HashSetAsync(newWXMessageDialogRedisKey, $"{MsgObj.msgid}:{MsgObj.send_time}", wxDialog);
                            //}

                            #endregion
                        }
                        _logger.LogInformation($"---------MsgObj:{JsonConvert.SerializeObject(MsgObj)}");
                    }
                }
            }
            else
                _logger.LogInformation($"WXKF数据token为空：\n" + WXKFEventXML);
        }

        /// <summary>
        /// 处理事件消息-微信回调事件
        /// </summary>
        /// <param name="MsgObj"></param>
        /// <returns></returns>
        public async Task HandleEventMsg(WXKFMessage MsgObj)
        {
            var eventMsg = MsgObj.@event;
            _logger.LogInformation($"HandleEventMsg:{JsonConvert.SerializeObject(MsgObj)}");
            switch (eventMsg.event_type)
            {
                case "enter_session":
                    #region enter_session：用户进入会话事件

                    // 进入会话的场景值，获取客服帐号链接开发者自定义的场景值
                    //public string scene { get; set; }

                    //有接待客服在线直接分配
                    //没有接待客服在线提醒用户
                    await CheckAndChangeServiceStatus(MsgObj);

                    #endregion
                    break;
                case "msg_send_fail":
                    #region msg_send_fail: 消息发送失败事件

                    // 发送失败的消息msgid
                    //public string fail_msgid { get; set; }
                    // 失败类型。0-未知原因 1-客服账号已删除 2-应用已关闭 4-会话已过期，超过48小时 5-会话已关闭 6-超过5条限制 7-未绑定视频号 8-主体未验证 9-未绑定视频号且主体未验证 10-用户拒收
                    //public int fail_type { get; set; }

                    #endregion
                    break;
                case "servicer_status_change":
                    #region servicer_status_change: 客服人员接待状态变更事件

                    // 客服人员userid
                    //public string servicer_userid { get; set; }
                    // 状态类型。1-接待中 2-停止接待
                    //public int status { get; set; }

                    //强制刷新客服接待人员
                    await _WXFLHttpClientHelper.GetWXKFAccountServicerList(forceUpdate: true);

                    #endregion
                    break;
                case "session_status_change":
                    #region session_status_change：会话状态变更事件（只有在客户端的操作才会触发，API的操作不会触发）

                    // 变更类型。1-从接待池接入会话 2-转接会话 3-结束会话
                    //public int change_type { get; set; }
                    //老的客服人员userid。仅change_type为2和3有值
                    //public string old_servicer_userid { get; set; }
                    // 新的客服人员userid。仅change_type为1和2有值
                    //public string new_servicer_userid { get; set; }

                    var newkey = string.Format(ServicerServingRedisKey, eventMsg.new_servicer_userid, eventMsg.open_kfid);
                    var oldkey = string.Format(ServicerServingRedisKey, eventMsg.old_servicer_userid, eventMsg.open_kfid);

                    //会话列表 
                    var newWXDialogRedisKey = string.Format(WXDialogRedisKey, eventMsg.open_kfid, eventMsg.external_userid);
                    //会话等待列表
                    var newWXDialogWaitingRedisKey = string.Format(WXDialogWaitingRedisKey, eventMsg.open_kfid, eventMsg.external_userid);
                    //会话结束列表
                    var newWXDialogEndingRedisKey = string.Format(WXDialogEndingRedisKey, eventMsg.open_kfid, eventMsg.external_userid);
                    //消息会话列表
                    var newWXMessageDialogRedisKey = string.Format(WXMessageDialogRedisKey, eventMsg.open_kfid, eventMsg.external_userid);
                    switch (eventMsg.change_type)
                    {
                        case 1: //从接待池接入会话
                            _logger.LogInformation($"从接待池接入会话");
                            break;
                        case 2: //转接会话
                            #region 转接会话
                            _logger.LogInformation($"转接会话");
                            if (!string.IsNullOrWhiteSpace(eventMsg.old_servicer_userid))
                            {
                                //记录接待人数到redis-1
                                await redisHelper.StringDecrementAsync(oldkey);
                            }
                            //记录接待人数到redis+1
                            await StringIncrementAsync(newkey);

                            await _WXFLHttpClientHelper.SendWXKFMsg(eventMsg.new_servicer_userid, eventMsg.open_kfid, "测试发送客户名称");

                            //会话等待列表 （30分钟过期）
                            var wxDialog = await redisHelper.StringGetAsync<WXKFDialog>(newWXDialogWaitingRedisKey);
                            if (wxDialog == null || wxDialog.dialogId == Guid.Empty)
                            {
                                //会话等待过期 重新产生一个会话
                                wxDialog = new WXKFDialog
                                {
                                    dialogId = Guid.NewGuid(),
                                    StartTime = MsgObj.send_time,
                                    external_userid = eventMsg.external_userid,
                                    open_kfid = eventMsg.open_kfid
                                };
                            }
                            //加入会话列表
                            wxDialog.servicerId = eventMsg.new_servicer_userid;
                            wxDialog.servicer_pickup_time = MsgObj.send_time;
                            //加入会话列表
                            await AddWXDialog(newWXDialogRedisKey, wxDialog);
                            ////所有会话
                            //await redisHelper.SortedSetAddAsync(ALLWXDialogRedisKey, wxDialog, wxDialog.StartTime);
                            ////加入会话列表
                            //await redisHelper.SortedSetAddAsync(newWXDialogRedisKey, wxDialog, wxDialog.StartTime);

                            //加入消息会话列表
                            //await redisHelper.HashSetAsync(newWXMessageDialogRedisKey, $"{MsgObj.msgid}:{MsgObj.send_time}", wxDialog);
                            await AddWXMessageDialogRedisKey(MsgObj, wxDialog);

                            #endregion
                            break;
                        case 3: //结束会话
                            #region 结束会话

                            //记录接待人数到redis-1
                            await redisHelper.StringDecrementAsync(oldkey);
                            //获取最后一个会话
                            var ArrDialog = redisHelper.SortedSetRangeByScore<WXKFDialog>(key: newWXDialogRedisKey, take: 1);
                            //long DialogNum = await redisHelper.SortedSetLengthAsync(newWXDialogRedisKey);
                            //var ArrDialog = await redisHelper.SortedSetRangeByRankWithScoresAsync<WXKFDialog>(newWXDialogRedisKey, DialogNum - 1, DialogNum);
                            ////结束的会话 从会话列表删除
                            //WXKFDialog removeDialog = null;
                            if (ArrDialog.Any())
                            {
                                var OWXKFDialog = ArrDialog?.FirstOrDefault();
                                //var OWXKFDialog = ArrDialog.OrderByDescending(x => x.Score)?.FirstOrDefault().Element;
                                //removeDialog = OWXKFDialog.Clone();
                                OWXKFDialog.EndTime = MsgObj.send_time;
                                //记录会话结束时间
                                await AddWXDialogEnding(newWXDialogEndingRedisKey, OWXKFDialog);
                                //await redisHelper.HashSetAsync(newWXDialogEndingRedisKey, OWXKFDialog.dialogId.ToString(), OWXKFDialog);

                                //加入消息会话列表
                                //await redisHelper.HashSetAsync(newWXMessageDialogRedisKey, $"{MsgObj.msgid}:{MsgObj.send_time}", OWXKFDialog);
                                await AddWXMessageDialogRedisKey(MsgObj, OWXKFDialog);

                                _logger.LogInformation($"开始发送满意度消息 begin：{MsgObj.@event.msg_code}");
                                //发送满意度评价消息
                                //await _WXFLHttpClientHelper.SendWXKFMsgOnEvent(MsgObj.@event.msg_code, WXKFSendMsgType.Endsession);
                                ////从消息会话列表 删除已借宿的会话
                                //await redisHelper.SortedSetRemoveAsync(newWXDialogRedisKey, removeDialog);
                            }

                            //在此处关闭

                            #endregion
                            break;
                    }
                    #endregion
                    break;
            }
        }

        /// <summary>
        /// 检测是否有客服在线 分配客服 &发送消息 并发送欢迎消息通知
        /// </summary>
        /// <param name="MsgObj"></param>
        /// <returns></returns>
        public async Task CheckAndChangeServiceStatus(WXKFMessage MsgObj)
        {
            _logger.LogInformation($"检测是否有客服在线 分配客服 &发送消息begin:{JsonConvert.SerializeObject(MsgObj)}");
            string open_kfid = "", external_userid = "";
            switch (MsgObj.msgtype)
            {
                case "event":
                    open_kfid = MsgObj.@event.open_kfid;
                    external_userid = MsgObj.@event.external_userid;
                    break;
                case "text":
                case "image":
                case "voice":
                case "video":
                case "file":
                case "location":
                case "link":
                case "business_card":
                case "miniprogram":
                default:
                    open_kfid = MsgObj.open_kfid;
                    external_userid = MsgObj.external_userid;
                    break;
            }
            _logger.LogInformation($"检测是否有客服在线 获取kfgourpname begin:open_kfid：{open_kfid} external_userid：{external_userid}");
            //检查是否有客服在线，如果没有在线客服，提醒用户没有接待客服在线
            (bool NoServicerInline, List<WXKFServicer> ArrWXKFServicer, WXKFServicerResponse GetWXKFSessionRes) = await CheckHasServicerInline_Reminder(MsgObj.msgid, open_kfid, external_userid);
            if (NoServicerInline)
            {
                return;
            }

            //消息会话列表
            var newWXMessageDialogRedisKey = string.Format(WXMessageDialogRedisKey, open_kfid, external_userid);
            //会话列表
            var newWXDialogRedisKey = string.Format(WXDialogRedisKey, open_kfid, external_userid);

            #region 查询状态&（0：未处理，1：由智能接待）转入接待池
            //如果会话状态没有再去 微信取一次
            if (GetWXKFSessionRes == null)
            {
                //service_state：0.未处理。1.直接用API自动回复消息。2.放进待接入池等待接待人员接待。3.指定接待人员进行接待。4.已结束
                GetWXKFSessionRes = await _WXFLHttpClientHelper.GetKFSessionStatus(open_kfid, external_userid);
            }
            //判断是否已经有人接待了
            if (GetWXKFSessionRes.service_state != 3)
            {
                //是事件才分配客服
                if (MsgObj.msgtype == "event" || (MsgObj.origin == 3 && GetWXKFSessionRes.service_state == 0))
                {
                    //微信会话
                    var wxDialog = new WXKFDialog { external_userid = external_userid, open_kfid = open_kfid, dialogId = Guid.NewGuid(), StartTime = MsgObj.send_time };
                    var wxDialogHistory = new List<WXKFDialog>() { };
                    //客服接待人员
                    var QArrWXKFServicer = ArrWXKFServicer.Where(x => x.status == 0);
                    if (QArrWXKFServicer.Any())
                    {
                        #region 分配客服

                        var dict = new Dictionary<string, int>();
                        foreach (var servicer in QArrWXKFServicer)
                        {
                            var _newkey = string.Format(ServicerServingRedisKey, servicer.userid, open_kfid);
                            //获取客服正在接待的人数
                            var val = await redisHelper.StringGetAsync(_newkey);
                            int.TryParse(val, out int ServingNum);
                            if (ServingNum < WXKFServicerMaxServingNum)
                                dict.Add(servicer.userid, ServingNum);
                        }
                        string servicerId = "";
                        if (dict.Any())
                        {
                            //如果没有合适的客服再找到接待人数最少的一个
                            #region 分配客服逻辑

                            if (string.IsNullOrWhiteSpace(servicerId))
                            {
                                #region 分配给接待人数最少的

                                //自动分配给最小接待人数客服，相同的 随机一个
                                var first = dict.OrderByDescending(x => x.Value).FirstOrDefault();
                                var leastServicing = dict.Where(x => x.Value == first.Value).ToList();
                                servicerId = first.Key;
                                var num = leastServicing.Count();
                                if (num > 1)
                                {
                                    var radNum = (new Random()).Next(num);
                                    servicerId = leastServicing[radNum].Key;
                                }

                                #endregion
                            }
                            //分配到指定客服
                            var WXKFSessionChangeRes = await _WXFLHttpClientHelper.ChangKFSessionStatus(open_kfid, external_userid, 3, servicerId);
                            //分配客服出错了，放到智能助手中
                            if (WXKFSessionChangeRes.errcode != 0)
                            {
                                _logger.LogError($"ChangKFSessionStatus-分配到指定客服-Error：{servicerId}-{WXKFSessionChangeRes.errmsg}");
                                //95013  会话已结束
                                List<int> errorcode = new List<int> { 95013 };
                                if (errorcode.ToArray().Contains(WXKFSessionChangeRes.errcode))
                                {
                                    _logger.LogInformation($"ChangKFSessionStatus-变更为未处理状态 {external_userid}{servicerId}");
                                    WXKFSessionChangeRes = await _WXFLHttpClientHelper.ChangKFSessionStatus(open_kfid, external_userid, 0, servicerId);
                                    if (WXKFSessionChangeRes.errcode != 0)
                                    {
                                        _logger.LogError($"ChangKFSessionStatus-再次变更为未处理状态-Error：{servicerId}-{WXKFSessionChangeRes.errmsg}");
                                    }
                                }
                                var MsgRes = await _WXFLHttpClientHelper.SendWXKFMsg(external_userid, open_kfid, $"分配到指定客服-发生错误，请联系IT {open_kfid}{external_userid}{servicerId}");
                                if (MsgRes.errcode != 0)
                                {
                                    _logger.LogError($"SendWXKFMsg-Error：{MsgRes.errmsg}");
                                }
                            }
                            else
                            {
                                try
                                {
                                    //分配成功后 记录当前客服接待人员 接待数量+1
                                    _logger.LogInformation($"ChangKFSessionStatus-分配到指定客服 成功，记录到Redis：{servicerId}");
                                    //客服48小时内接单记录
                                    var newInServingRedisKey = string.Format(InServingRedisKey, servicerId);
                                    wxDialog.servicerId = servicerId;
                                    wxDialog.servicer_pickup_time = MsgObj.send_time;
                                    await redisHelper.SortedSetAddAsync<WXKFDialog>(newInServingRedisKey, wxDialog, MsgObj.send_time);
                                    //记录客服和用户的关系，以及最后一次更新时间
                                    wxDialog.external_userid = external_userid;
                                    if (wxDialogHistory == null || wxDialogHistory.Count <= 0)
                                    {
                                        wxDialogHistory = new List<WXKFDialog>();
                                    }

                                    wxDialogHistory.Add(wxDialog);
                                    _logger.LogInformation($"ChangKFSessionStatus-Add wxDialog end");
                                    if (wxDialogHistory.Count > 10)
                                    {
                                        wxDialogHistory.RemoveAt(0);
                                    }
                                    _logger.LogInformation($"分配到指定客服{servicerId}成功");

                                    //客服接待人数当天0点，清除
                                    var newkey = string.Format(ServicerServingRedisKey, servicerId, open_kfid);
                                    //记录接待人数到redis
                                    await StringIncrementAsync(newkey);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"分配到指定客服{servicerId}错误", ex);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region 都超过最大接待人数 分配到排队队列

                            //分配到排队队列
                            var WXKFSessionChangeRes = await _WXFLHttpClientHelper.ChangKFSessionStatus(open_kfid, external_userid, 2);
                            if (WXKFSessionChangeRes.errcode != 0)
                            {
                                _logger.LogError($"ArrWXKFServicer-分配到排队队列-Error：{open_kfid}-{external_userid}-{WXKFSessionChangeRes.errmsg}");
                            }

                            #endregion
                        }

                        #endregion
                    }
                    else
                    {
                        _logger.LogError($"分配客服-Error：ArrWXKFServicer为空");
                        //分配到排队队列
                        var WXKFSessionChangeRes = await _WXFLHttpClientHelper.ChangKFSessionStatus(open_kfid, external_userid, 2);
                        if (WXKFSessionChangeRes.errcode != 0)
                        {
                            _logger.LogError($"ArrWXKFServicer-分配到排队队列-Error：{open_kfid}-{external_userid}-{WXKFSessionChangeRes.errmsg}");
                        }
                    }
                    if (string.IsNullOrWhiteSpace(wxDialog.servicerId))
                    {
                        //加入等待列表
                        var newWXDialogWaitingRedisKey = newWXDialogRedisKey + ":Waiting";
                        await redisHelper.StringSetAsync<WXKFDialog>(newWXDialogWaitingRedisKey, wxDialog, TimeSpan.FromMinutes(30));
                    }
                    else
                    {
                        //加入会话列表
                        await AddWXDialog(newWXDialogRedisKey, wxDialog);
                        //await redisHelper.SortedSetAddAsync<WXKFDialog>(newWXDialogRedisKey, wxDialog, MsgObj.send_time);
                    }
                    //加入消息会话列表
                    //await redisHelper.HashSetAsync(newWXMessageDialogRedisKey, $"{MsgObj.msgid}:{MsgObj.send_time}", wxDialog);
                    await AddWXMessageDialogRedisKey(MsgObj, wxDialog);
                }
            }
            else //已由 指定接待人员进行接待
            {
                /*
                 * 发送测试数据
                 *只有处于 0:未处理 或 1:由智能助手接待 才可以推送消息
                */
                //await _WXFLHttpClientHelper.SendWXKFMsg(GetWXKFSessionRes.servicer_userid, open_kfid, "测试发送客户名称");
            }

            #endregion

        }

        /// <summary>
        /// 检测是否有客服在线&发送消息
        /// </summary>
        /// <param name="msgid">消息ID</param>
        /// <param name="open_kfid">客服ID</param>
        /// <param name="external_userid">外部客户ID</param>
        public async Task<(bool, List<WXKFServicer>, WXKFServicerResponse)> CheckHasServicerInline_Reminder(string msgid, string open_kfid, string external_userid)
        {
            var cacheReminderMaxMinutes = 5;//记录 发送过提醒信息最大分钟数
            List<WXKFServicer> ArrWXKFServicer;
            var reminderUserNoServicerInline = false;
            var cacheItem = $"reminderUserNoServicerInline:{external_userid}-{open_kfid}";
            var lastCacheTimeStamp = await redisCache.GetStringAsync(cacheItem);
            WXKFServicerResponse wXKFServicerResponse = null;

            #region cacheReminderMaxMinutes 分钟内未发送过提醒 才检测是否有客服在线

            var WXKFServicerRes = await _WXFLHttpClientHelper.GetServicers(open_kfid);
            ArrWXKFServicer = WXKFServicerRes?.servicer_list;
            //status 0:接待中,1:停止接待
            if (ArrWXKFServicer?.Any(x => x.status == 0) == false)
            {
                reminderUserNoServicerInline = true;
            }

            if (reminderUserNoServicerInline)
            {
                if (string.IsNullOrEmpty(lastCacheTimeStamp))
                {
                    //service_state：\
                    //0.未处理。新会话接入（客户发信咨询）。可选择：1.直接用API自动回复消息。2.放进待接入池等待接待人员接待。3.指定接待人员（接待人员须处于“正在接待”中，下同）进行接待
                    //1.由智能助手接待  可使用API回复消息。可选择转入待接入池或者指定接待人员处理。
                    //2.待接入池排队中   放进待接入池等待接待人员接待。
                    //3.人工接待中    可选择转接给其他接待人员处理或者结束会话。
                    //4.已结束    会话已经结束或未开始。不允许变更会话状态，客户重新发信咨询后会话状态变为“未处理”
                    wXKFServicerResponse = await _WXFLHttpClientHelper.GetKFSessionStatus(open_kfid, external_userid);
                    if (wXKFServicerResponse.service_state != 1)
                    {
                        /*
                         * 分配到智能助手接待
                         * 只有处于 1:由智能助手接待 或 3:由人工接待 才可以推送消息
                         * service_state：0.未处理。1.直接用API自动回复消息。2.放进待接入池等待接待人员接待。3.指定接待人员进行接待。4.已结束
                        */
                        var WXKFSessionChangeRes = await _WXFLHttpClientHelper.ChangKFSessionStatus(open_kfid, external_userid, 1);
                    }
                    var NoServicerInlineRemindStr = ConfigurationManager.AppSettings["NoServicerInlineRemindStr"] ?? "当前无客服在线，请在工作时间内联系或通过其他途径联系，谢谢。";
                    var MsgRes = await _WXFLHttpClientHelper.SendWXKFMsg(external_userid, open_kfid, NoServicerInlineRemindStr);
                    if (MsgRes.errcode != 0)
                    {
                        _logger.LogError($"SendWXKFMsg-Error：{MsgRes.errmsg}");
                    }
                    //设置缓存不再 发送
                    await redisCache.SetStringAsync(cacheItem, DateTime.Now.to_Long().ToString(), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheReminderMaxMinutes) });
                }
            }

            #endregion

            return (reminderUserNoServicerInline, ArrWXKFServicer, wXKFServicerResponse);
        }

        /// <summary>
        /// 设置stringKey增长val（默认：1）
        /// Key不存在，自动创建key并增长val（默认：1）
        /// </summary>
        /// <param name="newKey"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public async Task<bool> StringIncrementAsync(string newKey, double val = 1)
        {
            var retTF = false;
            if (!string.IsNullOrEmpty(newKey))
            {
                var isExists = await redisHelper.KeyExistsAsync(newKey);
                //记录接待人数到redis
                var retDb = await redisHelper.StringIncrementAsync(newKey, val);
                if (retDb > 0)
                    retTF = true;
                if (!isExists)
                {
                    var ExpireDate = (DateTime?)DateTime.Now.Date.AddDays(1);
                    //设置过期时间 当天0点 过期
                    await redisHelper.KeyExpireAsync(newKey, ExpireDate);
                }
            }
            return retTF;
        }

        /// <summary>
        /// 增加会话到会话列表
        /// 同步增加到所有 会话列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wXKFDialog"></param>
        /// <returns></returns>
        public async Task<bool> AddWXDialog(string newWXDialogRedisKey, WXKFDialog wXKFDialog)
        {
            //加入会话列表
            var tf = await redisHelper.SortedSetAddAsync(newWXDialogRedisKey, wXKFDialog, wXKFDialog.StartTime);
            if (tf)
            {
                //所有会话
                tf = await redisHelper.SortedSetAddAsync(ALLWXDialogRedisKey, wXKFDialog, wXKFDialog.StartTime);
            }
            return tf;
        }

        /// <summary>
        /// 增加 会话结束列表
        /// 同步增加到所有 会话结束列表
        /// </summary>
        /// <param name="newWXDialogEndingRedisKey"></param>
        /// <param name="wXKFDialogEnding"></param>
        /// <returns></returns>
        public async Task<bool> AddWXDialogEnding(string newWXDialogEndingRedisKey, WXKFDialog wXKFDialogEnding)
        {
            if (wXKFDialogEnding.EndTime <= 0)
            {
                _logger.LogError($"会话结束,没有结束时间:{JsonConvert.SerializeObject(wXKFDialogEnding)}");
            }
            bool tf = false;
            ////记录会话结束时间
            //tf = await redisHelper.HashSetAsync(newWXDialogEndingRedisKey, wXKFDialogEnding.dialogId.ToString(), wXKFDialogEnding);
            //if (tf)
            //{
            //所有会话结束时间
            tf = await redisHelper.SortedSetAddAsync(ALLWXDialogEndingRedisKey, wXKFDialogEnding, wXKFDialogEnding.EndTime);
            //}

            return tf;
        }

        /// <summary>
        /// 增加 会话消息列表
        /// 同步增加到所有 会话消息
        /// </summary>
        /// <param name="wXKFMessage"></param>
        /// <param name="wXKFDialog">null时自动获取最后一个会话</param>
        /// <returns></returns>
        public async Task<bool> AddWXMessageDialogRedisKey(WXKFMessage MsgObj, WXKFDialog wXKFDialog = null)
        {
            var result = false;
            if (MsgObj.origin == 4)
            {
                return true;
            }
            //会话列表
            var newWXDialogRedisKey = string.Format(WXDialogRedisKey, MsgObj.open_kfid, MsgObj.external_userid);
            //消息会话列表
            var newWXMessageDialogRedisKey = string.Format(WXMessageDialogRedisKey, MsgObj.open_kfid, MsgObj.external_userid);
            WXKFDialog wxDialog = wXKFDialog;
            if (wxDialog == null)
            {
                //获取最后一个会话
                wxDialog = redisHelper.SortedSetRangeByScore<WXKFDialog>(key: newWXDialogRedisKey, take: 1)?.FirstOrDefault();
            }
            if (wxDialog.dialogId != Guid.Empty && !string.IsNullOrEmpty(wxDialog.servicerId))
            {
                //加入消息会话列表
                result = await redisHelper.HashSetAsync(newWXMessageDialogRedisKey, $"{MsgObj.msgid}:{MsgObj.send_time}", wxDialog);
                //if (result)
                //{
                //    //加入所有会话
                //    var tf = await redisHelper.SortedSetAddAsync(WXDialogEndingRedisKey, wXKFDialogEnding, wXKFDialogEnding.EndTime);
                //}
            }
            else
            {
                _logger.LogError($"增加 会话消息列表-错误：会话Id为空或没有分配客服接待人员---MsgObj:{JsonConvert.SerializeObject(MsgObj)};wxDialog:{JsonConvert.SerializeObject(wxDialog)}");
            }
            return result;
        }

        /// <summary>
        /// 获取KFid对应的客服组名称
        /// </summary>
        /// <param name="kfid">kfid</param>
        /// <returns></returns>
        public async Task<string> GetKFGroupName(string kfid)
        {
            //[{ "open_kfid":"wkMrwzBgAAd-8Ic7GeOXhjBiCm1uyJJg",
            //"name":"Finance服务","avatar":"https://wwcdn.weixin.qq.com/node/wework/images/avatar2.96df991a19.png"},
            //{ "open_kfid":"wkMrwzBgAAYmMa-GOOxsDANo1-DsirKg",
            //"name":"IT服务","avatar":"https://wwcdn.weixin.qq.com/node/wework/images/mini_customer_defualt_head_yellow_female.efca167d70.png"},
            //{ "open_kfid":"wkMrwzBgAAFVG2rNLoAdf4BqwgM8B_JA",
            //"name":"吉利德吉客天下客服","avatar":"https://wwcdn.weixin.qq.com/node/wework/images/kf_head_image_url_2.png"},
            //{ "open_kfid":"wkMrwzBgAAwRn0--BfQ9rLlZ5gDRqsPA","name":"测试","avatar":"https://wwcdn.weixin.qq.com/node/wework/images/avatar2.96df991a19.png"},{ "open_kfid":"wkMrwzBgAAI_TDHklcne3fG1gek34CAQ",
            //"name":"HR服务","avatar":"https://wwcdn.weixin.qq.com/node/wework/images/mini_customer_defualt_head_male.ffa7e6927a.png"}]}

            var servicelist = await _WXFLHttpClientHelper.GetWXKFAccountServicerList();
            _logger.LogInformation($"获取kfgourpname function:open_kfid：{kfid} servicelist: {JsonConvert.SerializeObject(servicelist)} ");
            if (servicelist != null && servicelist.account_list.Any())
            {
                string groupname = servicelist.account_list.Where(s => s.open_kfid == kfid).FirstOrDefault().name;
                _logger.LogInformation($"获取kfgourpname function:open_kfid：{kfid} groupname: {groupname} ");
                return groupname;
            }
            return "";
        }
    }
}