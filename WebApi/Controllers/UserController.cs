using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetCoreTemp.WebApi.Models.Entity;
using Microsoft.Extensions.Logging;
using NetCoreTemp.WebApi.Models.AutoMapper;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Models.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NetCoreTemp.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using NetCoreTemp.WebApi.Services.Base;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NetCoreTemp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private ILogger<UserController> _logger;
        private UserService _userService;
        private UserRoleService _userRoleService;
        private RoleService _roleService;
        private readonly IConfiguration _Configuration;

        public UserController(ILogger<UserController> logger,
            UserService userService,
            UserRoleService userRoleService,
            RoleService roleService, 
            IConfiguration Configuration)
        {
            _logger = logger;
            _userService = userService;
            _userRoleService = userRoleService;
            _roleService = roleService;
            _Configuration = Configuration;
        }

        /// <summary>
        /// 获取账户列表
        /// </summary>
        /// <param name="searhFilters">搜索条件</param>
        /// <param name="pageNationToken">分页token</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        // GET: api/<UserController>
        [HttpGet]
        public async Task<PagenationResult> Get(string searhFilters = null, int page = 0, int limit = 10)
        {
            List<filterRule> filterRules = new List<filterRule>();
            if (!string.IsNullOrEmpty(searhFilters))
                filterRules = System.Text.Json.JsonSerializer.Deserialize<List<filterRule>>(searhFilters);
            //filterRules = searhFilters;
            if (filterRules == null)
            {
                filterRules = new List<filterRule> {
                    new filterRule{ field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1" , valType= Models.EnumType.EnumRepo.FilterValueType.Number}
                };
            }
            if (!filterRules.Any(x => x.field == "Status"))
            {
                filterRules.Add(new filterRule { field = "Status", op = Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType = Models.EnumType.EnumRepo.FilterValueType.Number });
            }
            var name = User.Identity.Name;
            var role = User.IsInRole("admin");
            var sub = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault()?.Value;
            var TmpUser = _userService.QueryByFilterRules(filterRules).SelectPage(page,limit);
            var ArrUserDto = TmpUser.ArrEntity.Select(x => x.ToDto()).ToList();
            return new PagenationResult
            {
                ArrData = ArrUserDto,
                RowsCount = TmpUser.rowsCount
            };
        }

        /// <summary>
        /// 获取{id}账户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rangkey"></param>
        /// <returns></returns>
        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, [FromQuery] long? rangkey = null)
        {
            if (rangkey == null || rangkey <= 0)
            {
                var users = await _userService.QueryByFilterRules(new List<filterRule> {
                    new filterRule{  field="ID",op= Models.EnumType.EnumRepo.FilterOp.Equal, value=id}
                }).ToListAsync();
                if (users.Any())
                {
                    rangkey = users.FirstOrDefault().CreateDate;
                }
                else
                    return StatusCode(404, "数据不存在");
            }
            var user = await _userService.Find(id, rangkey);
            return Ok(user.ToDto());
        }

        /// <summary>
        /// 新增账户
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        // POST api/<UserController>
        [HttpPost]
        public async Task Post([FromBody] UserDto userDto)
        {
            //转换为实体
            var user = userDto.ToEntity();
            if (userDto._Roles != null && userDto._Roles.Any())
            {
                var roles = await _roleService.QueryByFilterRules(new List<filterRule> {
                    new filterRule { field="Name", op= Models.EnumType.EnumRepo.FilterOp.In, value= user.Roles}
                }).ToListAsync();
                foreach (var role in roles)
                {
                    var userRole = new UserRole
                    {
                        ID = Guid.NewGuid(),//DateTime.Now.to_Long().ToString(),
                        CreateDate = DateTime.Now.to_Long().Value,
                        CreateUserId = Guid.Empty,//User.Identity.Name,
                        CreateUserName = User.Identity.Name,
                        RoleId = role.ID,
                        UserId = user.ID
                    };
                    await _userRoleService.Insert(userRole);
                }
            }
            await _userService.Insert(user);
        }

        /// <summary>
        /// 删除账户权限
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task DeleteUserRole(string userId)
        {
            var userRoles = await _userRoleService.QueryByFilterRules(new List<filterRule> {
                new filterRule { field="UserId", op= Models.EnumType.EnumRepo.FilterOp.Equal, value= userId}
            }).ToListAsync();
            if (userRoles.Any())
            {
                foreach (var userrole in userRoles)
                    _userRoleService.Delete(userrole);
            }
        }

        /// <summary>
        /// 修改账户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userDto"></param>
        /// <returns></returns>
        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public async Task Put(string id, [FromBody] UserDto userDto)
        {
            var user = await _userService.Find(id, userDto._CreateDate.to_Long());
            var data_userDto = user.ToDto();
            data_userDto._ModifyDate = DateTime.Now;
            data_userDto._ModifyUserId = Guid.Empty;// "Michael_ID";
            data_userDto._ModifyUserName = "Michael";
            data_userDto._Name = userDto._Name;
            data_userDto._Email = userDto._Email;
            data_userDto._Password = userDto._Password;
            data_userDto._Status = userDto._Status; 
            if (userDto._Roles != null)
            {
                data_userDto._Roles = userDto._Roles;
                if (userDto._Roles.Any())
                {
                    var roles = await _roleService.QueryByFilterRules(new List<filterRule> {
                        new filterRule { field="Name", op= Models.EnumType.EnumRepo.FilterOp.In, value= user.Roles}
                    }).ToListAsync();

                    await DeleteUserRole(user.ID.ToString());

                    foreach (var role in roles)
                    {
                        var userRole = new UserRole
                        {
                            ID = Guid.NewGuid(),//DateTime.Now.to_Long().ToString(),
                            CreateDate = DateTime.Now.to_Long().Value,
                            CreateUserId = Guid.Empty,//User.Identity.Name,
                            CreateUserName = User.Identity.Name,
                            RoleId = role.ID,
                            UserId = user.ID
                        };
                        await _userRoleService.Insert(userRole);
                    }
                }
            }
            _userService.Update(data_userDto.ToEntity());
        }

        /// <summary>
        /// 删除账户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var users = await _userService.QueryByFilterRules(new List<filterRule> {
                new filterRule { field="ID", op= Models.EnumType.EnumRepo.FilterOp.Equal, value= id}
            }).ToListAsync();
            if (users.Any())
            {
                await DeleteUserRole(id);
                await _userService.Delete(id);
                return Ok();
            }
            else
            {
                return BadRequest("数据不存在");
            }
        }

        #region MyRegion

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost, Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserDto user)
        {
            string username = user._Name;
            string password = user._Password;
            //var token = "editor-token";
            //if (username == "admin")
            //{
            //    token = "admin-token";
            //}
            var ArrUser = await _userService.QueryByFilterRules((new List<filterRule> {
                new filterRule {  field= "Name", op= Models.EnumType.EnumRepo.FilterOp.Equal, value= username },
                new filterRule {  field= "Password", op= Models.EnumType.EnumRepo.FilterOp.Equal, value= password }
            })).ToListAsync();
            if (!ArrUser.Any() || ArrUser.Count() > 1)
            {
                return StatusCode(400, "账户/密码错误");
            }
            var OUser = ArrUser.FirstOrDefault();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuration["JWT:ServerSecret"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("Email", OUser.Email??""),
                new Claim("UserName", OUser.Name??""),
                new Claim("Roles", OUser.Roles??""),
                new Claim("RangKey", OUser.CreateDate.ToString()),
                new Claim("Michael", OUser.Email??""),
                new Claim(JwtRegisteredClaimNames.Sid, OUser.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, OUser.Name),
                new Claim(ClaimsIdentity.DefaultNameClaimType,OUser.Name),
                new Claim(ClaimsIdentity.DefaultRoleClaimType,OUser.Roles)
            };
            var tokenModel = new JwtSecurityToken(
                _Configuration["JWT:Issuer"],
                _Configuration["JWT:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: credentials
            );
            var token = new JwtSecurityTokenHandler().WriteToken(tokenModel);
            return StatusCode(200, new { Token = token });
        }

        /// <summary>
        /// 获取用户权限和信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Info")]
        public async Task<IActionResult> Info()
        {
            var Id = User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Sid)?.First()?.Value;
            var rangeKeyStr = User.Claims.Where(x => x.Type == "RangKey")?.First()?.Value;
            long.TryParse(rangeKeyStr, out long rangeKey);
            var OUser = await _userService.Find(Id, rangeKey);
            if (OUser == null || OUser.ID==Guid.Empty)
            {
                return StatusCode(403, "账户不存在");
            }
            else
            {
                var _roles = OUser.Roles.Split(",");
                return Ok(new
                {
                    roles = _roles,
                    introduction = "I am a super administrator",
                    avatar = "https://wpimg.wallstcn.com/f778738c-e4f8-4870-b634-56703b4acafe.gif",
                    name = OUser.Name
                });
            }

            //var roles = new List<string>() { "editor" };
            //if (token == "admin-token")
            //{
            //    roles.Add("admin");
            //}
            //var retObj = new
            //{
            //    code = 20000,
            //    data = new { roles = roles, introduction = "I am a super administrator", avatar = "https://wpimg.wallstcn.com/f778738c-e4f8-4870-b634-56703b4acafe.gif", name = "Super Admin" }
            //};
            //var rst = new JsonResult(retObj);
            //return await Task.FromResult(rst);
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            return await Task.FromResult(Ok());
            //var retObj = new
            //{
            //    code = 20000,
            //    data = "success"
            //};
            //var rst = new JsonResult(retObj);
            //return await Task.FromResult(rst);
        }

        #endregion
    }
}
