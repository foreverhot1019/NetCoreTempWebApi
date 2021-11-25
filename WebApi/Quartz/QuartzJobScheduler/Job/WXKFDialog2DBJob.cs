using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NetCoreTemp.WebApi.WXKF;

namespace NetCoreTemp.WebApi.QuartzJobScheduler.Job
{
    /// <summary>
    /// 微信客服会话数据 Redis数据保存到数据库
    /// 开始和结束的会话 数据处理
    /// PersistJobDataAfterExecution: 执行完Job后保存 JobDataMap 当中固定数据，以便任务在重复执行的时候具有相同的 JobDataMap
    /// DisallowConcurrentExecution：不能同时运行同一作业的多个实例
    /// </summary>
    [PersistJobDataAfterExecution, DisallowConcurrentExecution]
    public class WXKFDialog2DBJob : IJob
    {
        //从DI获取日志组件
        private readonly ILogger<WXKFDialog2DBJob> _logger;

        /// <summary>
        /// Redis缓存
        /// </summary>
        readonly RedisHelp.RedisHelper _redisHelper;

        /// <summary>
        /// 微信访问助手
        /// </summary>
        private WXFLHttpClientHelper _WXFLHttpClientHelper;

        #region redis-keys

        /// <summary>
        /// 所有会话记录(SortedSet)
        /// </summary>
        readonly string ALLWXDialogRedisKey = WXKFAssignHandler.ALLWXDialogRedisKey;//"WXKFDialog:ALL";
        /// <summary>
        /// 会话记录(SortedSet)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        readonly string WXDialogRedisKey = WXKFAssignHandler.WXDialogRedisKey;//"WXKFDialog:{0}:{1}";

        /// <summary>
        /// 会话等待(String，30分钟过期)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        readonly string WXDialogWaitingRedisKey = WXKFAssignHandler.WXDialogWaitingRedisKey;//"WXKFDialog:{0}:{1}:Waiting";

        /// <summary>
        /// 所有结束的会话(SortedSet)
        /// </summary>
        readonly string ALLWXDialogEndingRedisKey = WXKFAssignHandler.ALLWXDialogEndingRedisKey;//"WXKFDialog:ALL:Ending";
        /// <summary>
        /// 结束的会话(Hash)
        /// 0:Open_Kfid
        /// 1:外部客户id
        /// </summary>
        readonly string WXDialogEndingRedisKey = WXKFAssignHandler.WXDialogEndingRedisKey;//"WXKFDialog:{0}:{1}:Ending";

        #endregion

        public WXKFDialog2DBJob(RedisHelp.RedisHelper redisHelper,
            //IHttpClientFactory httpClientFactory, 
            WXFLHttpClientHelper wXFLHttpClientHelper, ILogger<WXKFDialog2DBJob> logger)
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
            var ArrJobData = context.JobDetail.JobDataMap.ToList();
            foreach (var jdata in ArrJobData)
            {
                _logger.LogInformation($"--------JobDataMap:{jdata.Key}-{jdata.Value}");
            }
            DateTime? startDate = null;
            var QArrJobData = ArrJobData.Where(x => x.Key == "start");
            if (QArrJobData.Any())
            {
                startDate = context.JobDetail.JobDataMap.GetDateTime(QArrJobData.FirstOrDefault().Key);
            }

            if (startDate < DateTime.Now)
            {
                await InsertWXKFDialog2DB();
            }
            else
            {
                await Task.FromResult(1);
                _logger.LogInformation($"--------Execute:NoneFunc-{DateTime.Now.ToString()}");
            }
        }

        /// <summary>
        /// 新增会话状态到数据库
        /// </summary>
        /// <returns></returns>
        public async Task InsertWXKFDialog2DB()
        {
            try
            {
                _logger.LogInformation($"--------Execute:{DateTime.Now.ToString()}");
                #region 获取所有WXKF 会话

                var ArrWXDialog = await _redisHelper.SortedSetRangeByRankAsync<WXKFDialog>(ALLWXDialogRedisKey);
                if (ArrWXDialog.Any())
                {
                    //所有微信客服接待人员
                    var WXKFServicerRes = await _WXFLHttpClientHelper.GetWXKFAccountServicerList();
                    var ArrWXKFServicer = WXKFServicerRes?.account_list;
                    
                }

                #endregion

                #region 获取所有WXKF 结束的会话

                var ALLWXDialogEnding = await _redisHelper.SortedSetRangeByRankAsync<WXKFDialog>(ALLWXDialogEndingRedisKey);
                if (ALLWXDialogEnding.Any())
                {
                    var retUpdate = 1;
                    if (retUpdate > 0)
                    {
                        //清除已处理的数据
                        foreach (var _wxkfDialogEnding in ALLWXDialogEnding)
                        {
                            var remove_tf = await _redisHelper.SortedSetRemoveAsync(ALLWXDialogEndingRedisKey, _wxkfDialogEnding);
                            if (!remove_tf)
                                _logger.LogError($"删除所有微信结束的会话数据错误：{JsonConvert.SerializeObject(_wxkfDialogEnding)}");

                            #region 需要清除 WXDialogRedisKey

                            #endregion
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError($"处理会话状态到数据库-错误：{ex.Message ?? ex.InnerException?.Message}", ex);
            }
        }
    }
}