using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCoreTemp.WebApi.Models;
using NetCoreTemp.WebApi.Models.AutoMapper;
using NetCoreTemp.WebApi.Models.Extensions;
using NetCoreTemp.WebApi.Models.View_Model;
using NetCoreTemp.WebApi.Services;
using NetCoreTemp.WebApi.Services.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NetCoreTemp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private ILogger<RoleController> _logger;
        private MenuService _menuService;
        private RoleService _roleService;
        private RoleMenuService _roleMenuService;

        public RoleController(ILogger<RoleController> logger, MenuService menuService, RoleService roleService, RoleMenuService roleMenuService)
        {
            _logger = logger;
            _menuService = menuService;
            _roleService = roleService;
            _roleMenuService = roleMenuService;
        }

        /// <summary>
        /// 获取无限自己
        /// </summary>
        /// <param name="AllMenu">所有的Menu（排除已用过的MenuId过后的数据）</param>
        /// <param name="ArrUseId">已用过的MenuId</param>
        /// <param name="ParentId">父级</param>
        /// <returns></returns>
        private IEnumerable<Menu> GetMenus(IEnumerable<Menu> AllMenu, string ParentId = "-")
        {
            Guid.TryParse(ParentId, out Guid gid);
            //第一层
            var retMenus = AllMenu.Where(x => x.ParentMenuId == gid).Select(x => x);
            //排除已使用
            var _AllMenu = AllMenu.Where(x => x.ParentMenuId != gid).Select(x => x);

            foreach (var menu in retMenus)
            {
                if (AllMenu.Any(x => x.ParentMenuId == menu.ID))
                {
                    //ArrUseId.Add(menu.ID);
                    menu.ChildrenMenu = GetMenus(_AllMenu, menu.ID.ToString()).ToArray();
                }
            }

            return retMenus;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="filterRules">搜索条件</param>
        /// <param name="pageNationToken">分页token</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PpagenationResult> Get(string searhFilters = null, int page = 0, int limit = 10)
        {
            List<filterRule> filterRules = new List<filterRule>();
            if (!string.IsNullOrEmpty(searhFilters))
                filterRules = System.Text.Json.JsonSerializer.Deserialize<List<filterRule>>(searhFilters);
            //filterRules = searhFilters;
            filterRules = filterRules ?? new List<filterRule>();
            if (!filterRules.Any(x => x.field == "Status"))
            {
                filterRules.Add(new filterRule { field = "Status", op = Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType = Models.EnumType.EnumRepo.FilterValueType.Number });
            }
            if (!filterRules.Any(x => x.field == "Type"))
            {
                filterRules.Add(new filterRule { field = "ID", op = Models.EnumType.EnumRepo.FilterOp.BeginsWith, value = "Role#" });
            }
            IEnumerable<Role> ArrRole = new List<Role>();
            long rowsCount = 0;
            string _paginationToken = "";
            (ArrRole, rowsCount) = _roleService.QueryByFilterRules(filterRules).SelectPage(page, limit);
            var roleMunuFilters = new List<filterRule> {
                new filterRule{ field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType= Models.EnumType.EnumRepo.FilterValueType.Number },
            };
            if (ArrRole?.Any() == true)
                roleMunuFilters.Add(new filterRule { field = "RoleId", op = Models.EnumType.EnumRepo.FilterOp.In, value = string.Join(",", ArrRole.Select(x => x.ID)) });
            IEnumerable<RoleMenu> ArrRoleMenu = new List<RoleMenu>();
            ArrRoleMenu = await _roleMenuService.QueryByFilterRules(roleMunuFilters).ToListAsync();
            //ArrRoleMenu = tmp.ArrTEntity;
            var menuFilters = new List<filterRule> {
                new filterRule{ field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1" }
            };

            var ArrMenu = await _menuService.QueryByFilterRules(menuFilters).ToListAsync();

            foreach (var role in ArrRole)
            {
                var roleAllMenu = ArrMenu.Where(x => ArrRoleMenu.Where(n => n.RoleId == role.ID).Select(n => n.MenuId).Contains(x.ID));
                role.ArrMenu = GetMenus(roleAllMenu).ToList();
            }

            return new PpagenationResult
            {
                ArrData = ArrRole.Select(x => x.ToDto()),
                PageNationToken = _paginationToken,
                RowsCount = rowsCount
            };

            #region MyRegion

            //var obj = new
            //{
            //    code = 20000,
            //    data = new List<Object> {
            //        new {
            //            key="admin",name="admin",description="Super Administrator. Have access to view all pages.",
            //            routes= new List<Object> {
            //                new { path="/redirect",component="layout/Layout",hidden=true,children=new List<Object> { new{ path="/redirect/:path*",component="views/redirect/index"}}},
            //                new { path="/login",component="views/login/index",hidden=true},
            //                new { path="/auth-redirect",component="views/login/auth-redirect",hidden=true},
            //                new { path="/404",component="views/error-page/404",hidden=true},
            //                new { path="/401",component="views/error-page/401",hidden=true},
            //                new { path="",component="layout/Layout",redirect="dashboard",children=new List<Object> { new{ path="dashboard",component="views/dashboard/index",name="Dashboard",meta= new { title="dashboard",icon="dashboard",affix=true} }}},
            //                new { path="/documentation",component="layout/Layout",children=new List<Object> { new{ path="index",component="views/documentation/index",name="Documentation",meta= new { title="documentation",icon="documentation",affix=true} }}},
            //                new { path="/guide",component="layout/Layout",redirect="/guide/index",children=new List<Object> { new{ path="index",component="views/guide/index",name="Guide",meta= new { title="guide",icon="guide",noCache=true} }}},
            //                new { path="/permission",component="layout/Layout",redirect="/permission/index",alwaysShow=true,meta= new { title="permission",icon="lock",roles=new string[]{"admin","editor" } },children=new List<Object> { new{ path="page",component="views/permission/page",name="PagePermission",meta= new { title="pagePermission",roles=new string[]{"admin" } } },
            //                new { path="directive",component="views/permission/directive",name="DirectivePermission",meta= new { title="directivePermission"} },
            //                new { path="role",component="views/permission/role",name="RolePermission",meta= new { title="rolePermission",roles=new string[]{"admin" } } }}},
            //                new { path="/icon",component="layout/Layout",children=new List<Object> { new{ path="index",component="views/icons/index",name="Icons",meta= new { title="icons",icon="icon",noCache=true} }}},
            //                new { path="/components",component="layout/Layout",redirect="noRedirect",name="ComponentDemo",meta= new { title="components",icon="component"},children=new List<Object> { new{ path="tinymce",component="views/components-demo/tinymce",name="TinymceDemo",meta= new { title="tinymce"} },
            //                new { path="markdown",component="views/components-demo/markdown",name="MarkdownDemo",meta= new { title="markdown"} },
            //                new { path="json-editor",component="views/components-demo/json-editor",name="JsonEditorDemo",meta= new { title="jsonEditor"} },
            //                new { path="split-pane",component="views/components-demo/split-pane",name="SplitpaneDemo",meta= new { title="splitPane"} },
            //                new { path="avatar-upload",component="views/components-demo/avatar-upload",name="AvatarUploadDemo",meta= new { title="avatarUpload"} },
            //                new { path="dropzone",component="views/components-demo/dropzone",name="DropzoneDemo",meta= new { title="dropzone"} },
            //                new { path="sticky",component="views/components-demo/sticky",name="StickyDemo",meta= new { title="sticky"} },
            //                new { path="count-to",component="views/components-demo/count-to",name="CountToDemo",meta= new { title="countTo"} },
            //                new { path="mixin",component="views/components-demo/mixin",name="ComponentMixinDemo",meta= new { title="componentMixin"} },
            //                new { path="back-to-top",component="views/components-demo/back-to-top",name="BackToTopDemo",meta= new { title="backToTop"} },
            //                new { path="drag-dialog",component="views/components-demo/drag-dialog",name="DragDialogDemo",meta= new { title="dragDialog"} },
            //                new { path="drag-select",component="views/components-demo/drag-select",name="DragSelectDemo",meta= new { title="dragSelect"} },
            //                new { path="dnd-list",component="views/components-demo/dnd-list",name="DndListDemo",meta= new { title="dndList"} },
            //                new { path="drag-kanban",component="views/components-demo/drag-kanban",name="DragKanbanDemo",meta= new { title="dragKanban"} }}},
            //                new { path="/charts",component="layout/Layout",redirect="noRedirect",name="Charts",meta= new { title="charts",icon="chart"},children=new List<Object> { new{ path="keyboard",component="views/charts/keyboard",name="KeyboardChart",meta= new { title="keyboardChart",noCache=true} },
            //                new { path="line",component="views/charts/line",name="LineChart",meta= new { title="lineChart",noCache=true} },
            //                new { path="mixchart",component="views/charts/mixChart",name="MixChart",meta= new { title="mixChart",noCache=true} }}},
            //                new { path="/nested",component="layout/Layout",redirect="/nested/menu1/menu1-1",name="Nested",meta= new { title="nested",icon="nested"},children=new List<Object> { new{ path="menu1",component="views/nested/menu1/index",name="Menu1",meta= new { title="menu1"},redirect="/nested/menu1/menu1-1",children=new List<Object> { new{ path="menu1-1",component="views/nested/menu1/menu1-1",name="Menu1-1",meta= new { title="menu1-1"} },
            //                new { path="menu1-2",component="views/nested/menu1/menu1-2",name="Menu1-2",redirect="/nested/menu1/menu1-2/menu1-2-1",meta= new { title="menu1-2"},children=new List<Object> { new{ path="menu1-2-1",component="views/nested/menu1/menu1-2/menu1-2-1",name="Menu1-2-1",meta= new { title="menu1-2-1"} },
            //                new { path="menu1-2-2",component="views/nested/menu1/menu1-2/menu1-2-2",name="Menu1-2-2",meta= new { title="menu1-2-2"} }}},
            //                new { path="menu1-3",component="views/nested/menu1/menu1-3",name="Menu1-3",meta= new { title="menu1-3"} }}},
            //                new { path="menu2",name="Menu2",component="views/nested/menu2/index",meta= new { title="menu2"} }}},
            //                new { path="/example",component="layout/Layout",redirect="/example/list",name="Example",meta= new { title="example",icon="example"},children=new List<Object> { new{ path="create",component="views/example/create",name="CreateArticle",meta= new { title="createArticle",icon="edit"} },
            //                new { path="edit/:id(\\d+)",component="views/example/edit",name="EditArticle",meta= new { title="editArticle",noCache=true},hidden=true},
            //                new { path="list",component="views/example/list",name="ArticleList",meta= new { title="articleList",icon="list"} }}},
            //                new { path="/tab",component="layout/Layout",children=new List<Object> { new{ path="index",component="views/tab/index",name="Tab",meta= new { title="tab",icon="tab"} }}},
            //                new { path="/error",component="layout/Layout",redirect="noRedirect",name="ErrorPages",meta= new { title="errorPages",icon="404"},children=new List<Object> { new{ path="401",component="views/error-page/401",name="Page401",meta= new { title="page401",noCache=true} },
            //                new { path="404",component="views/error-page/404",name="Page404",meta= new { title="page404",noCache=true} }}},
            //                new { path="/error-log",component="layout/Layout",redirect="noRedirect",children=new List<Object> { new{ path="log",component="views/error-log/index",name="ErrorLog",meta= new { title="errorLog",icon="bug"} }}},
            //                new { path="/excel",component="layout/Layout",redirect="/excel/export-excel",name="Excel",meta= new { title="excel",icon="excel"},children=new List<Object> { new{ path="export-excel",component="views/excel/export-excel",name="ExportExcel",meta= new { title="exportExcel"} },
            //                new { path="export-selected-excel",component="views/excel/select-excel",name="SelectExcel",meta= new { title="selectExcel"} },
            //                new { path="export-merge-header",component="views/excel/merge-header",name="MergeHeader",meta= new { title="mergeHeader"} },
            //                new { path="upload-excel",component="views/excel/upload-excel",name="UploadExcel",meta= new { title="uploadExcel"} }}},
            //                new { path="/zip",component="layout/Layout",redirect="/zip/download",alwaysShow=true,meta= new { title="zip",icon="zip"},children=new List<Object> { new{ path="download",component="views/zip/index",name="ExportZip",meta= new { title="exportZip"} }}},
            //                new { path="/pdf",component="layout/Layout",redirect="/pdf/index",children=new List<Object> { new{ path="index",component="views/pdf/index",name="PDF",meta= new { title="pdf",icon="pdf"} }}},
            //                new { path="/pdf/download",component="views/pdf/download",hidden=true},
            //                new { path="/theme",component="layout/Layout",redirect="noRedirect",children=new List<Object> { new{ path="index",component="views/theme/index",name="Theme",meta= new { title="theme",icon="theme"} }}},
            //                new { path="/clipboard",component="layout/Layout",redirect="noRedirect",children=new List<Object> { new{ path="index",component="views/clipboard/index",name="ClipboardDemo",meta= new { title="clipboardDemo",icon="clipboard"} }}},
            //                new { path="/i18n",component="layout/Layout",children=new List<Object> { new{ path="index",component="views/i18n-demo/index",name="I18n",meta= new { title="i18n",icon="international"} }}},
            //                new { path="external-link",component="layout/Layout",children=new List<Object> { new{ path="https://github.com/PanJiaChen/vue-element-admin",meta= new { title="externalLink",icon="link"} }}},
            //                new { path="*",redirect="/404",hidden=true}
            //            }
            //        },
            //        new {
            //            key="editor",name="editor",description="Normal Editor. Can see all pages except permission page",
            //            routes = new List<Object> {
            //                new{ path="/redirect",component="layout/Layout",hidden=true,children=new List<Object> { new{ path="/redirect/:path*",component="views/redirect/index"}}},
            //                new { path="/login",component="views/login/index",hidden=true},
            //                new { path="/auth-redirect",component="views/login/auth-redirect",hidden=true},
            //                new { path="/404",component="views/error-page/404",hidden=true},
            //                new { path="/401",component="views/error-page/401",hidden=true},
            //                new { path="",component="layout/Layout",redirect="dashboard",children=new List<Object> { new{ path="dashboard",component="views/dashboard/index",name="Dashboard",meta= new { title="dashboard",icon="dashboard",affix=true} }}},
            //                new { path="/documentation",component="layout/Layout",children=new List<Object> { new{ path="index",component="views/documentation/index",name="Documentation",meta= new { title="documentation",icon="documentation",affix=true} }}},
            //                new { path="/guide",component="layout/Layout",redirect="/guide/index",children=new List<Object> { new{ path="index",component="views/guide/index",name="Guide",meta= new { title="guide",icon="guide",noCache=true} }}},
            //                new { path="/icon",component="layout/Layout",children=new List<Object> { new{ path="index",component="views/icons/index",name="Icons",meta= new { title="icons",icon="icon",noCache=true} }}},
            //                new { path="/components",component="layout/Layout",redirect="noRedirect",name="ComponentDemo",meta= new { title="components",icon="component"},children=new List<Object> { new{ path="tinymce",component="views/components-demo/tinymce",name="TinymceDemo",meta= new { title="tinymce"} },
            //                new { path="markdown",component="views/components-demo/markdown",name="MarkdownDemo",meta= new { title="markdown"} },
            //                new { path="json-editor",component="views/components-demo/json-editor",name="JsonEditorDemo",meta= new { title="jsonEditor"} },
            //                new { path="split-pane",component="views/components-demo/split-pane",name="SplitpaneDemo",meta= new { title="splitPane"} },
            //                new { path="avatar-upload",component="views/components-demo/avatar-upload",name="AvatarUploadDemo",meta= new { title="avatarUpload"} },
            //                new { path="dropzone",component="views/components-demo/dropzone",name="DropzoneDemo",meta= new { title="dropzone"} },
            //                new { path="sticky",component="views/components-demo/sticky",name="StickyDemo",meta= new { title="sticky"} },
            //                new { path="count-to",component="views/components-demo/count-to",name="CountToDemo",meta= new { title="countTo"} },
            //                new { path="mixin",component="views/components-demo/mixin",name="ComponentMixinDemo",meta= new { title="componentMixin"} },
            //                new { path="back-to-top",component="views/components-demo/back-to-top",name="BackToTopDemo",meta= new { title="backToTop"} },
            //                new { path="drag-dialog",component="views/components-demo/drag-dialog",name="DragDialogDemo",meta= new { title="dragDialog"} },
            //                new { path="drag-select",component="views/components-demo/drag-select",name="DragSelectDemo",meta= new { title="dragSelect"} },
            //                new { path="dnd-list",component="views/components-demo/dnd-list",name="DndListDemo",meta= new { title="dndList"} },
            //                new { path="drag-kanban",component="views/components-demo/drag-kanban",name="DragKanbanDemo",meta= new { title="dragKanban"} }}},
            //                new { path="/charts",component="layout/Layout",redirect="noRedirect",name="Charts",meta= new { title="charts",icon="chart"},children=new List<Object> { new{ path="keyboard",component="views/charts/keyboard",name="KeyboardChart",meta= new { title="keyboardChart",noCache=true} },
            //                new { path="line",component="views/charts/line",name="LineChart",meta= new { title="lineChart",noCache=true} },
            //                new { path="mixchart",component="views/charts/mixChart",name="MixChart",meta= new { title="mixChart",noCache=true} }}},
            //                new { path="/nested",component="layout/Layout",redirect="/nested/menu1/menu1-1",name="Nested",meta= new { title="nested",icon="nested"},children=new List<Object> { new{ path="menu1",component="views/nested/menu1/index",name="Menu1",meta= new { title="menu1"},redirect="/nested/menu1/menu1-1",children=new List<Object> { new{ path="menu1-1",component="views/nested/menu1/menu1-1",name="Menu1-1",meta= new { title="menu1-1"} },
            //                new { path="menu1-2",component="views/nested/menu1/menu1-2",name="Menu1-2",redirect="/nested/menu1/menu1-2/menu1-2-1",meta= new { title="menu1-2"},children=new List<Object> { new{ path="menu1-2-1",component="views/nested/menu1/menu1-2/menu1-2-1",name="Menu1-2-1",meta= new { title="menu1-2-1"} },
            //                new { path="menu1-2-2",component="views/nested/menu1/menu1-2/menu1-2-2",name="Menu1-2-2",meta= new { title="menu1-2-2"} }}},
            //                new { path="menu1-3",component="views/nested/menu1/menu1-3",name="Menu1-3",meta= new { title="menu1-3"} }}},
            //                new { path="menu2",name="Menu2",component="views/nested/menu2/index",meta= new { title="menu2"} }}},
            //                new { path="/example",component="layout/Layout",redirect="/example/list",name="Example",meta= new { title="example",icon="example"},children=new List<Object> { new{ path="create",component="views/example/create",name="CreateArticle",meta= new { title="createArticle",icon="edit"} },
            //                new { path="edit/:id(\\d+)",component="views/example/edit",name="EditArticle",meta= new { title="editArticle",noCache=true},hidden=true},
            //                new { path="list",component="views/example/list",name="ArticleList",meta= new { title="articleList",icon="list"} }}},
            //                new { path="/tab",component="layout/Layout",children=new List<Object> { new{ path="index",component="views/tab/index",name="Tab",meta= new { title="tab",icon="tab"} }}},
            //                new { path="/error",component="layout/Layout",redirect="noRedirect",name="ErrorPages",meta= new { title="errorPages",icon="404"},children=new List<Object> { new{ path="401",component="views/error-page/401",name="Page401",meta= new { title="page401",noCache=true} },
            //                new { path="404",component="views/error-page/404",name="Page404",meta= new { title="page404",noCache=true} }}},
            //                new { path="/error-log",component="layout/Layout",redirect="noRedirect",children=new List<Object> { new{ path="log",component="views/error-log/index",name="ErrorLog",meta= new { title="errorLog",icon="bug"} }}},
            //                new { path="/excel",component="layout/Layout",redirect="/excel/export-excel",name="Excel",meta= new { title="excel",icon="excel"},children=new List<Object> { new{ path="export-excel",component="views/excel/export-excel",name="ExportExcel",meta= new { title="exportExcel"} },
            //                new { path="export-selected-excel",component="views/excel/select-excel",name="SelectExcel",meta= new { title="selectExcel"} },
            //                new { path="export-merge-header",component="views/excel/merge-header",name="MergeHeader",meta= new { title="mergeHeader"} },
            //                new { path="upload-excel",component="views/excel/upload-excel",name="UploadExcel",meta= new { title="uploadExcel"} }}},
            //                new { path="/zip",component="layout/Layout",redirect="/zip/download",alwaysShow=true,meta= new { title="zip",icon="zip"},children=new List<Object> { new{ path="download",component="views/zip/index",name="ExportZip",meta= new { title="exportZip"} }}},
            //                new { path = "/pdf",component = "layout/Layout",redirect = "/pdf/index",children = new List<Object> { new { path = "index", component = "views/pdf/index", name = "PDF", meta = new{ title = "pdf", icon = "pdf" } }}},
            //                new { path="/pdf/download",component="views/pdf/download",hidden=true},
            //                new { path = "/theme",component = "layout/Layout",redirect = "noRedirect",children = new List<Object> { new { path = "index", component = "views/theme/index", name = "Theme", meta = new{ title = "theme", icon = "theme" } }}},
            //                new { path = "/clipboard",component = "layout/Layout",redirect = "noRedirect",children = new List<Object> { new { path = "index", component = "views/clipboard/index", name = "ClipboardDemo", meta = new{ title = "clipboardDemo", icon = "clipboard" } }}},
            //                new { path = "/i18n",component = "layout/Layout",children = new List<Object> { new { path = "index", component = "views/i18n-demo/index", name = "I18n", meta = new{ title = "i18n", icon = "international" } }}},
            //                new { path = "external-link",component = "layout/Layout",children = new List<Object> { new { path = "https://github.com/PanJiaChen/vue-element-admin", meta = new{ title = "externalLink", icon = "link" } }}},
            //                new { path="*",redirect="/404",hidden=true}
            //            }
            //        },
            //        new {
            //            key = "visitor",name = "visitor",description = "Just a visitor. Can only see the home page and the document page",
            //            routes = new List<Object> {
            //                new { path = "", redirect = "dashboard", children = new List<Object> { new { path = "dashboard", name = "Dashboard", meta = new { title = "dashboard", icon = "dashboard" } } } }
            //            }
            //        }
            //    }
            //};
            //var Jret = new JsonResult(obj);
            //return await Task.FromResult(Jret);

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var roles = await _roleService.QueryByFilterRules(new List<filterRule>
            {
                new filterRule{  field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType= Models.EnumType.EnumRepo.FilterValueType.Number },
                new filterRule{  field="ID", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = id}
            }).ToListAsync();
            if (roles.Any())
            {
                var role = roles.FirstOrDefault();

                if (role.ID != Guid.Empty)
                {
                    return Ok(role);
                }
            }
            return StatusCode(404, "数据不存在");
        }

        /// <summary>
        /// 新增数据
        /// </summary>
        /// <param name="roleDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(RoleDto roleDto)
        {
            var role = roleDto.ToEntity();
            if (role.ID == Guid.Empty)
                role.ID = Guid.NewGuid();
            await _roleService.Insert(role);
            if (roleDto._ArrMenu != null && roleDto._ArrMenu.Any())
            {
                var roleMenufilter = new List<filterRule>
                {
                    new filterRule { field = "ID", op = Models.EnumType.EnumRepo.FilterOp.BeginsWith, value = "RoleMenu#" },
                    new filterRule{ field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1" },
                    new filterRule{ field="RoleId", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = role.ID.ToString() }
                };
                var roleMenus = await _roleMenuService.QueryByFilterRules(roleMenufilter).ToListAsync();

                #region 删除

                var newMenuIds = roleDto._ArrMenu.Select(n => n._ID);
                var Deletes = roleMenus.Where(x => !newMenuIds.Contains(x.MenuId));
                if (Deletes.Any())
                {
                    foreach (var delete in Deletes)
                        _roleMenuService.Delete(delete);
                }

                #endregion

                #region 新增

                var inMenuIds = roleMenus.Select(x => x.MenuId);
                var Inserts = roleDto._ArrMenu.Where(x => !inMenuIds.Contains(x._ID)).Select(x => new RoleMenu
                {
                    ID = Guid.NewGuid(),//DateTime.Now.to_Long().ToString() + new Random().Next(0, 999).ToString("000"),
                    RoleId = role.ID,
                    MenuId = x._ID,
                    CreateDate = DateTime.Now.to_Long().Value,
                    CreateUserId = Guid.Empty,//User.Identity.Name,
                    CreateUserName = User.Identity.Name
                });
                if (Inserts.Any())
                {
                    await _roleMenuService.InsertRange(Inserts);
                }

                #endregion
            }
            return Ok();
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="roleDto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string Id, [FromBody] RoleDto roleDto)
        {
            return await Post(roleDto);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="roleDto"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var roles = await _roleService.QueryByFilterRules(new List<filterRule>
            {
                new filterRule{  field="ID", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = id}
            }).ToListAsync();
            if (roles.Any())
            {
                var role = roles.FirstOrDefault();
                if (role.ID !=Guid.Empty)
                {
                    var roleMenufilter = new List<filterRule>
                    {
                        //new filterRule{ field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1" },
                        new filterRule { field = "ID", op = Models.EnumType.EnumRepo.FilterOp.BeginsWith, value = "RoleMenu#" },
                        new filterRule{ field="RoleId", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = role.ID.ToString() }
                    };
                    var roleMenus = await _roleMenuService.QueryByFilterRules(roleMenufilter).ToListAsync();
                    if (roleMenus.Any())
                    {
                        foreach (var roleMenu in roleMenus)
                            _roleMenuService.Delete(roleMenu);
                    }
                    _roleService.Delete(role);
                    return Ok();
                }
            }
            return StatusCode(404, "数据不存在");
        }

        /// <summary>
        /// 根据账户获取
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<IAsyncResult> getRolesByUserId(string Id)
        {
            return Task.FromResult(Ok());
        }

        /// <summary>
        /// 根据权限获取菜单
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        [HttpGet("getRoleMenu")]
        public async Task<IActionResult> getMenuByRoleId(string RoleId)
        {
            var roleMenus = await _roleMenuService.QueryByFilterRules(new List<filterRule>
            {
                new filterRule{  field="Status", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = "1", valType= Models.EnumType.EnumRepo.FilterValueType.Number },
                new filterRule { field = "ID", op = Models.EnumType.EnumRepo.FilterOp.BeginsWith, value = "RoleMenu#" },
                new filterRule{  field="RoleId", op= Models.EnumType.EnumRepo.FilterOp.Equal, value = RoleId}
            }).ToListAsync();
            if (roleMenus.Any())
            {
                return Ok(roleMenus.Select(x => x.ToDto()));
            }
            return StatusCode(404, "数据不存在");
        }
    }
}
