using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using NetCoreTemp.WebApi.Models.View_Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCoreTemp.WebApi.Models.AutoMapper;
using NetCoreTemp.WebApi.QuartzJobScheduler;

namespace NetCoreTemp.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly RedisHelp.RedisHelper _redisHelper;
        private readonly JobScheduler _jobScheduler;

        //private readonly MyMapper<Models.Role, Models.AutoMapper.RoleDto>  _myMapper;


        public WeatherForecastController(ILogger<WeatherForecastController> logger, RedisHelp.RedisHelper redisHelper
            //,MyMapper<Models.Role, Models.AutoMapper.RoleDto> myMapper
            , JobScheduler jobScheduler
            )
        {
            _logger = logger;
            _redisHelper = redisHelper;
            //_myMapper = myMapper;
            ////测试 订阅key事件
            //_redisHelper.SubscribeKeyExpire();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var rng = new Random();
            var Arr = await Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray());

            #region autoMapper测试

            //dynamic foo = new ExpandoObject();
            //foo.Type = "TestType222";
            //foo.Name = "TestName222";
            //foo.Sort = 2;
            //foo.Remark = "Test_Remark222";
            //foo.CreateUserId = "CreateUserId222";
            //foo.CreateUserName = "CreateUserName222";
            //var exp = new ExpandoObject();
            //var x = exp as IDictionary<string, Object>;
            //x.Add("Name", "Menu_Name");
            //foo.ArrMenu = new List<ExpandoObject> { exp };

            //var ss = new Dictionary<string, Object>
            //{
            //    {"Type", "TestType222" },
            //    {"Name", "TestName222"},
            //    {"Sort",  2},
            //    {"Remark",  "Test_Remark222"},
            //    { "CreateUserId", "CreateUserId222" },
            //    { "CreateUserName", "CreateUserName222"},
            //    { "ArrMenu",new List<Dictionary<string, object>>{new Dictionary<string, object>{ { "Name", "Menu_Name222" } } } }
            //};
            //var vb = new Models.Role
            //{
            //    //Type = "TestType111",
            //    Name = "TestName111",
            //    Sort = 1,
            //    Remark = "Test_Remark111"
            //};
            //var aa = Models.AutoMapper.UserMapper.Mapper.Map(ss, vb);

            #endregion

            return Ok(Arr);
            return Ok(new ActionReturnMessage
            {
                IsSuccess = true,
                Data = Arr,
                StatusCode = System.Net.HttpStatusCode.OK
            });
        }
        public class Foo
        {
            public int Bar { get; set; }
            public int Baz { get; set; }
            public Foo InnerFoo { get; set; }
        }
    }
}
