using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NetCoreTemp.WebApi.WXKF;
using NetCoreTemp.WebApi;

namespace NetCoreTemp.WebApi.QuartzJobScheduler.Job
{
    /// <summary>
    /// 微信客服 Redis数据保存到数据库
    /// 对话和消息数据
    /// PersistJobDataAfterExecution: 执行完Job后保存 JobDataMap 当中固定数据，以便任务在重复执行的时候具有相同的 JobDataMap
    /// DisallowConcurrentExecution：不能同时运行同一作业的多个实例
    /// </summary>
    [PersistJobDataAfterExecution, DisallowConcurrentExecution]
    public class WXKFMsgCleanJob : IJob
    {
        //从DI获取日志组件
        private readonly ILogger<WXKFMsgCleanJob> _logger;

        /// <summary>
        /// Redis缓存
        /// </summary>
        readonly RedisHelp.RedisHelper redisHelper;

        /// <summary>
        /// 微信访问助手
        /// </summary>
        private WXFLHttpClientHelper _WXFLHttpClientHelper;

        #region 清理数据

        /// <summary>
        /// 客服正在接单的数量接待(String)
        /// 0：客服接待人员Id
        /// 1：Open_Kfid
        /// </summary>
        public readonly string ServicerServingRedisKey = WXKFAssignHandler.ServicerServingRedisKey;//"Servicer:ServingNum:{0}:{1}";
        /// <summary>
        /// 48小时内，客服正在接单的接待列表(SortedSet)
        /// 0:客服接单人员
        /// </summary>
        public readonly string InServingRedisKey = WXKFAssignHandler.InServingRedisKey;//"Servicer:InServing:{0}";
        /// <summary>
        /// 所有消息Redis-Store建
        /// </summary>
        public readonly string ALLMsgListRedisKey = WXKFAssignHandler.ALLMsgListRedisKey;//"WXKFMsgList:ALLMsgList";
        /// <summary>
        /// 已处理消息Redis-Store建
        /// </summary>
        public readonly string MsgAnalysisedListRedisKey = WXKFAssignHandler.MsgAnalysisedListRedisKey;//"WXKFMsgList:MsgAnalysisedList";
        /// <summary>
        /// 会话记录(SortedSet)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        readonly string WXDialogRedisKey = WXKFAssignHandler.WXDialogRedisKey;//"WXKFDialog:{0}:{1}";
        /// <summary>
        /// 会话消息ID对照(Hash)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        readonly string WXMessageDialogRedisKey = WXKFAssignHandler.WXMessageDialogRedisKey;//"WXKFMessageDialog:{0}:{1}";

        /// <summary>
        /// 未结束的会话,自动结束执行记录
        /// 0:当天日期
        /// 每天0-1点，执行一次
        /// </summary>
        readonly string cleanKey = WXKFAssignHandler.cleanKey;//"WXKF_CleanData:{0}";
        readonly string AllcleanKey = WXKFAssignHandler.AllcleanKey;//"WXKF_CleanData:ALL";

        #endregion

        public WXKFMsgCleanJob(RedisHelp.RedisHelper _redisHelper,
            //IHttpClientFactory httpClientFactory, 
            WXFLHttpClientHelper wXFLHttpClientHelper, ILogger<WXKFMsgCleanJob> logger)
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
            await CleanWXKFMsgByRedis();
        }

        /// <summary>
        /// 清理微信客服Redis数据
        /// 0点-1点 清除一次未结束的会话
        /// </summary>
        /// <returns></returns>
        public async Task CleanWXKFMsgByRedis()
        {
            var nowDate = DateTime.Now.Date;
            var end24TimeSpan = nowDate.AddHours(-24).to_Long() ?? 0;
            var end48TimeSpan = nowDate.AddHours(-48).to_Long() ?? 0;
            //所有消息 当天0点往前48小时
            await redisHelper.SortedSetRemoveRangeByScoreAsync(ALLMsgListRedisKey, double.NegativeInfinity, end48TimeSpan);
            //已处理消息 现在往前24小时
            var end24TimeSpan4now = DateTime.Now.AddHours(-24).to_Long() ?? 0;
            await redisHelper.SortedSetRemoveRangeByScoreAsync(MsgAnalysisedListRedisKey, double.NegativeInfinity, end24TimeSpan4now);
            //未结束的会话,自动结束执行记录 现在往前24小时
            await redisHelper.SortedSetRemoveRangeByScoreAsync(AllcleanKey, double.NegativeInfinity, end24TimeSpan4now);

            //所有微信客服接待人员
            var WXKFServicerRes = await _WXFLHttpClientHelper.GetWXKFAccountServicerList();
            var ArrWXKFServicer = WXKFServicerRes?.account_list;
            foreach (var servicer in ArrWXKFServicer)
            {
                //当天0点往前48小时，客服正在接单的接待列表(SortedSet)
                var newInServingRedisKey = string.Format(InServingRedisKey, servicer.userid);
                await redisHelper.SortedSetRemoveRangeByScoreAsync(newInServingRedisKey, double.NegativeInfinity, end48TimeSpan);
            }
            #region 每晚00 清空未结束会话的数据

            var MaxHour = 1;
#if DEBUG
            MaxHour = DateTime.Now.Hour;
#endif
            var newcleanKey = string.Format(cleanKey, DateTime.Now.Date.ToString("yyyyMMdd"));
            var cleanDate = await redisHelper.StringGetAsync(newcleanKey);
            //错过执行时间后，执行一次
            if (DateTime.Now.Hour <= MaxHour || (DateTime.Now.Hour > MaxHour && string.IsNullOrWhiteSpace(cleanDate)))
            {
                if (string.IsNullOrWhiteSpace(cleanDate))
                {
                    var tf = await redisHelper.StringSetAsync(newcleanKey, DateTime.Now.ToString("yyyyMMdd HH:mm:ss"), TimeSpan.FromHours(24));
                    if (tf)
                    {
                        tf = await redisHelper.SortedSetAddAsync(AllcleanKey, newcleanKey, DateTime.Now.to_Long() ?? 0);
                        if (tf)
                        {
                            #region 关闭未结束的会话

                            //结束的会话
                            var DialogueState = "finished";
                            var lastDay = DateTime.Now.Date;//到今天0点为止的，没有结束的会话
                            
                            #endregion

                            #region 需要清除 WXDialogRedisKey & WXMessageDialogRedisKey

                            //48小时前的数据
                            var befor48 = DateTime.Now.AddHours(-48);

                            #endregion
                        }
                    }
                    else
                    {
                        _logger.LogError($"清空未结束会话的数据-失败：{cleanKey}");
                    }
                }
            }

            #endregion
        }
    }
}