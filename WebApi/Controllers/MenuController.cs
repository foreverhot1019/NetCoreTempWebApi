using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCoreTemp.WebApi.Models.Entity;
using NetCoreTemp.WebApi.Models.AutoMapper;
using NetCoreTemp.WebApi.Models.Extensions;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Services;
using NetCoreTemp.WebApi.Services.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NetCoreTemp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private ILogger<MenuController> _logger;
        private MenuService _menuService;
        private RoleService _roleService;
        private RoleMenuService _roleMenuService;

        public MenuController(ILogger<MenuController> logger, MenuService menuService, RoleService roleService, RoleMenuService roleMenuService)
        {
            _logger = logger;
            _menuService = menuService;
            _roleService = roleService;
            _roleMenuService = roleMenuService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterRules">搜索条件</param>
        /// <param name="pageNationToken">分页token</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        // GET: api/<MenuController>
        [HttpGet]
        public async Task<PpagenationResult> Get(string searhFilters = null, int page = 0, int limit = 10)
        {
            List<filterRule> filterRules = new List<filterRule>();
            if (!string.IsNullOrEmpty(searhFilters))
                filterRules = System.Text.Json.JsonSerializer.Deserialize<List<filterRule>>(searhFilters);
            //filterRules = searhFilters;
            if (filterRules == null)
            {
                filterRules = new List<filterRule> {
                    new filterRule{ field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType= Models.EnumType.EnumRepo.FilterValueType.Number }
                };
            }
            if (!filterRules.Any(x => x.field == "Status"))
            {
                filterRules.Add(new filterRule { field = "Status", op = Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType = Models.EnumType.EnumRepo.FilterValueType.Number });
            }
            if (!filterRules.Any(x => x.field == "Type"))
            {
                filterRules.Add(new filterRule { field = "ID", op = Models.EnumType.EnumRepo.FilterOp.BeginsWith, value = "Menu#" });
            }
            IEnumerable<Menu> ArrMenu;
            long rowsCount;
            (ArrMenu, rowsCount) = _menuService.QueryByFilterRules(filterRules).SelectPage(page, limit);
            return new PpagenationResult
            {
                ArrData = ArrMenu.Select(x => x.ToDto()),
                RowsCount = rowsCount
            };
        }

        // GET api/<MenuController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var filterRules = new List<filterRule> {
                    new filterRule{ field="Status", op= (int)Models.EnumType.EnumRepo.FilterOp.Equal, value = "1" },
                    new filterRule{ field="ID", op= (int)Models.EnumType.EnumRepo.FilterOp.Equal, value = id },
                };
            var ArrMenu = await _menuService.QueryByFilterRules(filterRules).ToListAsync();
            if (ArrMenu.Any())
            {
                return Ok(ArrMenu.First());
            }
            return StatusCode(404, "数据不存在");
        }

        // POST api/<MenuController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MenuDto menuDto)
        {
            var menu = menuDto.ToEntity();

            if (menu.ID == Guid.Empty)
            {
                menu.ID = Guid.NewGuid();
            }
            await _menuService.Insert(menu);
            return Ok();
        }

        // PUT api/<MenuController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] MenuDto menuDto)
        {
            var menu = menuDto.ToEntity();
            if (Guid.TryParse(id, out Guid gid))
            {
                if (menu.ID != gid)
                    menu.ID = gid;
                _menuService.Update(menu);
                return Ok();
            }
            else
                return StatusCode(404, "数据不存在");
        }

        // DELETE api/<MenuController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var menus = await _menuService.QueryByFilterRules(new List<filterRule>
            {
                new filterRule{  field="ID", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = id}
            }).ToListAsync();
            if (menus.Any())
            {
                var menu = menus.FirstOrDefault();
                if (menu.ID == Guid.Empty)
                {
                    _menuService.Delete(menu);
                    return Ok();
                }
            }
            return StatusCode(404, "数据不存在");
        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="searchKey">搜索值</param>
        /// <returns></returns>
        [HttpGet("GetMenuItemSelect")]
        public async Task<IActionResult> GetMenuItemSelect(string searchKey)
        {
            var filters = new List<filterRule>();
            if (filters == null)
            {
                filters = new List<filterRule> {
                    new filterRule{ field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType= Models.EnumType.EnumRepo.FilterValueType.Number }
                };
            }
            if (!filters.Any(x => x.field == "Status"))
            {
                filters.Add(new filterRule { field = "Status", op = Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType = Models.EnumType.EnumRepo.FilterValueType.Number });
            }
            if (!filters.Any(x => x.field == "Type"))
            {
                filters.Add(new filterRule { field = "ID", op = Models.EnumType.EnumRepo.FilterOp.BeginsWith, value = "Menu#" });
            }

            if (!string.IsNullOrWhiteSpace(searchKey))
                filters.Add(new filterRule { field = "Name", op = Models.EnumType.EnumRepo.FilterOp.Contains, value = searchKey });
            var menus = await _menuService.QueryByFilterRules(filters).ToListAsync();
            return Ok(menus.Select(x => new
            {
                Key = x.ID,
                DisplayName = x.Name,
                Value = x.ID
            }));
        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="searchKey">搜索值</param>
        /// <returns></returns>
        [HttpGet("getMenuTree")]
        public async Task<IActionResult> getMenuTree(string ParentMenuId)
        {
            var filters = new List<filterRule>{
                new filterRule{ field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType= Models.EnumType.EnumRepo.FilterValueType.Number },
                new filterRule { field = "ID", op = Models.EnumType.EnumRepo.FilterOp.BeginsWith, value = "Menu#" }
            };
            if (string.IsNullOrWhiteSpace(ParentMenuId))
            {
                ParentMenuId = "-";
            }
            filters.Add(new filterRule { field = "ParentMenuId", op = (int)Models.EnumType.EnumRepo.FilterOp.Equal, value = ParentMenuId });
            var menus = await _menuService.QueryByFilterRules(filters).ToListAsync();
            return Ok(menus.Select(x => x.ToDto()));
        }
    }
}
