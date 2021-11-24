using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreTemp.WebApi;
using NetCoreTemp.WebApi.WXKF;
using Newtonsoft.Json;
using Quartz;

namespace NetCoreTemp.WebApi.QuartzJobScheduler.Job
{
    /// <summary>
    /// 微信客服消息存储到对应的会话到数据库中
    /// PersistJobDataAfterExecution: 执行完Job后保存 JobDataMap 当中固定数据，以便任务在重复执行的时候具有相同的 JobDataMap
    /// DisallowConcurrentExecution：不能同时运行同一作业的多个实例
    /// </summary>
    [PersistJobDataAfterExecution, DisallowConcurrentExecution]
    public class WXKFDialogMsg2DBJob : IJob
    {
        //从DI获取日志组件
        private readonly ILogger<WXKFDialogMsg2DBJob> _logger;

        /// <summary>
        /// Redis缓存
        /// </summary>
        readonly RedisHelp.RedisHelper redisHelper;

        /// <summary>
        /// 微信访问助手
        /// </summary>
        private WXFLHttpClientHelper _WXFLHttpClientHelper;

        #region redis-keys

        /// <summary>
        /// 所有消息Redis-Store建
        /// </summary>
        readonly string ALLMsgListRedisKey = WXKFAssignHandler.ALLMsgListRedisKey;//"WXKFMsgList:ALLMsgList";
        /// <summary>
        /// 已处理消息Redis-Store建
        /// </summary>
        readonly string MsgAnalysisedListRedisKey = WXKFAssignHandler.MsgAnalysisedListRedisKey;//"WXKFMsgList:MsgAnalysisedList";
        /// <summary>
        /// 未结束的会话,自动结束执行记录
        /// 0:当天日期
        /// 每天0-1点，执行一次
        /// </summary>
        readonly string cleanKey = WXKFAssignHandler.cleanKey;//"WXKF_CleanData:{0}";
        readonly string AllcleanKey = WXKFAssignHandler.AllcleanKey;//"WXKF_CleanData:ALL";

        /// <summary>
        /// 消息会话ID对照(Hash)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        readonly string WXMessageDialogRedisKey = WXKFAssignHandler.WXMessageDialogRedisKey;//"WXKFMessageDialog:{0}:{1}"

        #endregion

        /// <summary>
        /// 获取消息发送类型
        /// </summary>
        Func<int, string> funcGetSenderType = new Func<int, string>(orign =>
        {
            //orign: 3 - 微信客户发送的消息 4 - 系统推送的事件消息 5 - 接待人员在企业微信客户端发送的消息
            var result = "System";
            switch (orign)
            {
                case 3:
                    result = "User";
                    break;
                case 5:
                    result = "Staff";
                    break;
                case 4:
                default:
                    result = "System";
                    break;
            }
            return result;
        });

        /// <summary>
        /// 获取消息id和发送时间
        /// </summary>
        Func<string, (string msgId, int sendTime)> funcGetMsg = new Func<string, (string msgId, int sendTime)>(msgStr =>
        {
            int result = 0;
            string msgid = msgStr;
            var idx = msgStr.LastIndexOf(":");
            if (idx > 0)
            {
                if (int.TryParse(msgStr.Substring(idx + 1), out result))
                {
                    msgid = msgStr.Substring(0, idx);
                }
            }
            return (msgid, result);
        });

        public WXKFDialogMsg2DBJob(RedisHelp.RedisHelper _redisHelper,
            //IHttpClientFactory httpClientFactory, 
            WXFLHttpClientHelper wXFLHttpClientHelper, ILogger<WXKFDialogMsg2DBJob> logger)
        {
            _redisHelper = redisHelper;
            _WXFLHttpClientHelper = wXFLHttpClientHelper;
            _logger = logger;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="context">job上下文</param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"--------Execute:{DateTime.Now.ToString()}");
            await WXKFDialogMsg2DB();
        }

        /// <summary>
        /// 微信客服会话消息数据入库
        /// </summary>
        /// <returns></returns>
        public async Task WXKFDialogMsg2DB()
        {
            var startDate = DateTime.Now.Date;
            if (DateTime.Now.Hour == 8) //测试服务器 22-8停止运行，用于修复 22-8间 发送的消息无法入库
            {
                var WebSiteUrl = System.Configuration.ConfigurationManager.AppSettings["WebSiteUrl"] ?? "";
                if (WebSiteUrl.IndexOf("test.") > 0)
                {
                    startDate = startDate.AddDays(-1);
                }
            }
            var endDate = DateTime.Now;
            //0点到现在的会话
            
        }

        /// <summary>
        /// 获取会话消息
        /// </summary>
        /// <param name="wX_DIALOGUE"></param>
        /// <param name="newWXMessageDialogRedisKey">消息会话列表</param>
        /// <returns></returns>
        private async Task<List<WXKFMessage>> GetXKFMessage(dynamic wxDialogue, string newWXMessageDialogRedisKey)
        {
            var retWXKFMessage = new List<WXKFMessage>();
            if (wxDialogue != null)
            {
                //记录已结束的且没有消息的会话
                var NoMsg_EndedDialo = $"NoMsg_EndedDialog:{wxDialogue.WXKF_DialogId}";
                var date = await redisHelper.StringGetAsync(NoMsg_EndedDialo);
                if (string.IsNullOrEmpty(date))
                {
                    var DictMsgDialog = await redisHelper.HashGetAllAsync<string, WXKFDialog>(newWXMessageDialogRedisKey);
                    var ArrMsg = DictMsgDialog.Where(x => x.Value.dialogId == wxDialogue.WXKF_DialogId).Select(x => new { key = x.Key, msg = funcGetMsg(x.Key) });
                    var QArrMsg = ArrMsg.Where(x => x.msg.sendTime > 0);
                    if (QArrMsg.Any())
                    {
                        var minSendTime = (long?)QArrMsg?.Min(x => x.msg.sendTime) ?? DateTime.Now.AddHours(12).to_Long();
                        var maxSendTime = (long?)QArrMsg?.Max(x => x.msg.sendTime) ?? DateTime.Now.to_Long();
                        var ArrWXKFMessage = await redisHelper.SortedSetRangeByScoreAsync<WXKFMessage>(ALLMsgListRedisKey, minSendTime ?? 0, maxSendTime ?? 0);
                        var ArrMsgId = ArrMsg.Select(x => x.msg.msgId);
                        retWXKFMessage = ArrWXKFMessage.Where(x => ArrMsgId.Contains(x.msgid)).OrderBy(x => x.send_time).ToList();
                        #region 记录已结束的且没有消息的会话 1小时过期

                        if (!retWXKFMessage.Any() && wxDialogue.DialogueState == "finished")
                        {
                            await redisHelper.StringSetAsync(NoMsg_EndedDialo, DateTime.Now, TimeSpan.FromHours(1));
                        }

                        #endregion
                    }
                }
            }

            return retWXKFMessage;
        }

    }
}