using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using NetCoreTemp.WebApi.Models.View_Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCoreTemp.WebApi.QuartzJobScheduler;
using Microsoft.Extensions.DependencyInjection;
using NetCoreTemp.WebApi.Models;
using NetCoreTemp.WebApi.Models.AutoMapper;
using AutoMapper;

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

        private readonly MyMapper<User, UserDto> _myMapper;
        private readonly IMapper _mapper;


        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            RedisHelp.RedisHelper redisHelper,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _redisHelper = redisHelper;
            var mapper = serviceProvider.GetService<IMapper>();
            if (mapper != null)
                _mapper = mapper;

            var myMapper = serviceProvider.GetService<MyMapper<User, UserDto>>();
            if (myMapper != null)
                _myMapper = myMapper;

            ////测试 订阅key过期事件
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
            //foo.Type = "TestType111";
            //foo.Name = "TestName111";
            //foo.Sort = 1;
            //foo.Remark = "Test_Remark111";
            //foo.CreateUserId = "CreateUserId111";
            //foo.CreateUserName = "CreateUserName111";
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
            //var vb = new Role
            //{
            //    //Type = "TestType111",
            //    Name = "TestName111",
            //    Sort = 1,
            //    Remark = "Test_Remark111"
            //};
            //var dt0 = vb.ToDto();
            //var aa = _mapper.Map(ss, vb);
            //var bb = _mapper.Map(foo, vb);

            //UserDto userDto = new UserDto
            //{
            //    _ID = Guid.NewGuid().ToString(),
            //    _Name = "Test",
            //    _Roles = new List<string> { "A", "B", "C" }
            //};
            //var user = _myMapper.ToSource(userDto);

            #endregion

            #region SearchQuery测试

            //var filters = new List<filterRule> {
            //    new filterRule{  field="CreateUserId", value = "aaa"},
            //    new filterRule{  field="CreateUserName", value = "bbb"},
            //    new filterRule{  field="CreateDate", value = "bbb"},
            //    new filterRule{  field="_CreateDate", value = "2021-12-09"},
            //    new filterRule{  field="CreateDate_", value = "2021-12-09"},
            //};
            //var roleQuery = new Services.RoleQuery();
            //roleQuery.WithFilterRule(filters);
            //var q = roleQuery.Query();

            var user = new User
            {
                Name = "asdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasd",
                Roles = "1,2,3",
                CreateUserId = Guid.Empty,
                ModifyUserId = Guid.Empty
            };
            ModelState.Clear();
            TryValidateModel(user);
            var s = ModelState.IsValid;

            #endregion

            return Ok(Arr);
        }

        public class Foo
        {
            public int Bar { get; set; }
            public int Baz { get; set; }
            public Foo InnerFoo { get; set; }
        }
    }
}
