using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Helpdesk.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private ILogger<ArticleController> _logger;
        public ArticleController(ILogger<ArticleController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> list(int page = 0, int limit = 10)
        {
            var items = new List<object> {
                new { id = 1+(page*limit), timestamp = 181808339032, author = "Michelle", reviewer = "Dorothy", title = "Qhjye Nwc Eewxnwab Ukbeiy Xvckvaus Mvsiey Jkp Qdzrpqjfl Xujkinu", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=21.54,importance=2,type = "US", status = "published", display_time = "1985-06-08 11:10:29", comment_disabled=true,pageviews=3783,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}},
                new { id = 2+(page*limit), timestamp = 606144328512, author = "Susan", reviewer = "Paul", title = "Oerwvxh Jupxq Lfwvtq Bcsjl Axtrdkg Zhv Jefczes Sayg", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=45.88,importance=2,type = "US", status = "draft", display_time = "1978-07-29 14:21:22", comment_disabled=true,pageviews=4274,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}},
                new { id = 3+(page*limit), timestamp = 1527982804470, author = "Kevin", reviewer = "Edward", title = "Hwhe Qxiibpcbs Uvn Mjzcn Bsskuhst Lmuwyna", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=19.02,importance=2,type = "EU", status = "published", display_time = "2008-02-25 22:11:16", comment_disabled=true,pageviews=1475,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}},
                new { id = 4+(page*limit), timestamp = 562338121862, author = "Cynthia", reviewer = "Laura", title = "Znxufvp Mzdujxw Zjqkmqiqkh Kdtbcsqlu Ajrhfrsk Cupwwjorz Hbbj Owxbhptzl", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=84.37,importance=3,type = "US", status = "published", display_time = "1978-05-21 13:03:07", comment_disabled=true,pageviews=1438,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}},
                new { id = 5+(page*limit), timestamp = 1442597693995, author = "Brenda", reviewer = "Nancy", title = "Xdbgxyoyko Wfw Vkjnja Culgzo Rbmknowcv Wbqvvbkb Tnun Dvkwwvsxq Ljuwxlgsv", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=77.55,importance=3,type = "US", status = "draft", display_time = "2016-11-01 21:39:04", comment_disabled=true,pageviews=3671,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}},
                new { id = 6+(page*limit), timestamp = 994474806087, author = "Brian", reviewer = "Jennifer", title = "Rfpn Rhcbvhqt Ddzx Tvlmaojwl Emoyxk Qdyhw Eorxsy", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=56.42,importance=1,type = "JP", status = "published", display_time = "2018-12-09 01:56:53", comment_disabled=true,pageviews=2984,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}},
                new { id = 7+(page*limit), timestamp = 1233485076403, author = "Eric", reviewer = "Michael", title = "Zwnkye Wjrofjjg Mohad Hkktmse Dlvzyl Wjonmemba Vydxwfemh Rfbddf Nediffrsor", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=31.13,importance=3,type = "US", status = "published", display_time = "1987-01-28 15:07:26", comment_disabled=true,pageviews=4310,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}},
                new { id = 8+(page*limit), timestamp = 1546716294318, author = "James", reviewer = "Donald", title = "Gtysmxdbu Bclllys Hfmn Hoqqxggo Rujvs Xepy Fxesltdre Ffdh", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=79.53,importance=2,type = "EU", status = "published", display_time = "2003-02-21 20:45:34", comment_disabled=true,pageviews=1426,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}},
                new { id = 9+(page*limit), timestamp = 1609793974902, author = "Eric", reviewer = "Donna", title = "Emxttsrkxi Bpklein Susvfwg Kzmllc Otvb Npwkdq Mqafkdyn", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=41.53,importance=2,type = "US", status = "draft", display_time = "2001-01-05 20:34:14", comment_disabled=true,pageviews=2945,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}},
                new { id = 10+(page*limit), timestamp = 7241671826, author = "Mark", reviewer = "Mary", title = "Wgsc Qbuewk Jwt Qdrqbljky Bxf Gyuprkfq Cartbbx Rjmj", content_short = "mock data", content = "<p>I am testing data, I am testing data.</p><p><img src=\"https://wpimg.wallstcn.com/4c69009c-0fd4-4153-b112-6cb53d1cf943\"></p>", forecast=67.88,importance=2,type = "US", status = "draft", display_time = "1996-03-30 05:15:32", comment_disabled=true,pageviews=4048,image_uri = "https://wpimg.wallstcn.com/e4558086-631c-425c-9430-56ffb46e70b3", platforms = new string[]{"a-platform"}}
            };
            if (limit > 10)
            {
                items = items.Concat(items.Take(limit - 10)).ToList();
            }
            else
                items = items.Take(10 - limit).ToList();
            //int s = (page * limit) + 1;

            var ret = new { code = 20000, data = new { total = 100, items = items } };
            return await Task.FromResult(new JsonResult(ret));
        }
    }
}
