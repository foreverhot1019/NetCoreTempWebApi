using AutoMapper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NetCoreTemp.WebApi.QuartzJobScheduler;
using NetCoreTemp.WebApi.Models.AutoMapper;
using NetCoreTemp.WebApi.Models.Entity;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Models.DatabaseContext;
using NetCoreTemp.WebApi.Services;

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
        private readonly AppDbContext _dbContext;
        private readonly RoleService _roleService;

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

            _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
            _roleService = serviceProvider.GetRequiredService<RoleService>();

            ////测试 订阅key过期事件
            //_redisHelper.SubscribeKeyExpire();

        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("开始测试");

            #region Fody测试

            Services.FodyTest.SampleA sample = new Services.FodyTest.SampleA
            {
                A = "A",
                AA = "AA",
                B = 1,
                BB = 2,
                C = 1.123m,
                CC = 1.223m,
                D = true,
                DD = false
            }; 
            sample.PropertyChanged += (obj, args) =>
            {
                _logger.LogInformation("sample.PropertyChanged:{0}", args.PropertyName);
            };
            sample.A += "_";
            sample.B += 10;
            sample.C += 10.999m;
            sample.D = false;
            sample.MethodA();
            sample.MethodTime("TTTTTime");

            Services.FodyTest.SampleB sampleB = new Services.FodyTest.SampleB
            {
                A = "B",
                AA = "BB",
                B = 2,
                BB = 22,
                C = 2.123m,
                CC = 2.223m,
                D = true,
                DD = false
            };
            sampleB.PropertyChanged += (obj, args) =>
            {
                _logger.LogInformation("sample.PropertyChanged:{0}", args.PropertyName);
            };
            sampleB.A += "_";
            sampleB.B += 20;
            sampleB.C += 20.999m;
            sampleB.D = false;
            sampleB.MethodB();
            sampleB.MethodTimeB("BBBTime");

            #endregion

            var rng = new Random();
            var Arr = await Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToList());

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
            //    new filterRule{  field="Name", value = "aaa"},
            //    new filterRule{  field="CreateUserId", value = "aaa"},
            //    new filterRule{  field="CreateUserName", value = "bbb"},
            //    new filterRule{  field="CreateDate", value = "bbb"},
            //    new filterRule{  field="_CreateDate", value = "2021-12-09"},
            //    new filterRule{  field="CreateDate_", value = "2021-12-09"},
            //};
            ////var roleQuery = new Services.RoleQuery();
            //var qf = _roleService.QueryByFilterRules(filters);

            #endregion

            var user = new User
            {
                Name = "asdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasdasd",
                Roles = "1,2,3",
                Email = "aqqqqq.com",
                CreateUserId = Guid.Empty,
                ModifyUserId = Guid.Empty
            };

            //var tf = await TryUpdateModelAsync<User>(user);

            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var uiculture = System.Globalization.CultureInfo.CurrentUICulture;
            ModelState.Clear();
            TryValidateModel(user);
            var s = ModelState.IsValid;
            if (!s)
            {
                var errs = ModelState.Select(x => x.Key + ":" + string.Join(" . ", x.Value?.Errors.Select((n, i) => $" {i + 1}. {n.ErrorMessage}").ToArray()));
                Arr.Add(new WeatherForecast
                {
                    Date = DateTime.Now,
                    Summary = string.Join(";", errs),
                    TemperatureC = 1
                });
            }
            _dbContext.User.ToList();

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
