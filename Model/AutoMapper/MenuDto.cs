using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using AutoMapper;
using Helpdesk.WebApi.Models.BaseModel;

namespace Helpdesk.WebApi.Models.AutoMapper
{
    public class MenuDto : _BaseEntityDto
    {
        [Display(Name = "类型", Description = "类型")]
        public string _Type { get; } = "Menu";

        [Display(Name = "隐藏菜单", Description = "隐藏菜单")]
        public bool _Hidden { get; set; }

        [Display(Name = "名称", Description = "菜单名称")]
        [Required, StringLength(20)]
        public string _Name { get; set; } = "-";

        [Display(Name = "排序代码", Description = "菜单排序代码（0100开始）")]
        public int _Sort { get; set; }

        [Display(Name = "描述", Description = "菜单描述")]
        [StringLength(100)]
        public string _Remark { get; set; }

        [Display(Name = "Url", Description = "菜单Url")]
        [Required, StringLength(100)]
        public string _Url { get; set; } = "-";

        [Display(Name = "控制器", Description = "控制器")]
        [StringLength(50)]
        public string _Controller { get; set; } = "-";

        [Display(Name = "图标", Description = "菜单图标")]
        [StringLength(50)]
        public string _IconCls { get; set; }

        [Display(Name = "所属资源", Description = "")]
        [Required, StringLength(50)]
        public string _Resource { get; set; } = "-";

        [Display(Name = "Vue组件", Description = "Vue组件路径")]
        [Required, StringLength(100)]
        public string _Component { get; set; } = "-";

        [Display(Name = "上级菜单", Description = "上级菜单")]
        [Required, StringLength(50)]
        public string _ParentMenuId { get; set; }= "-";

        [Display(Name = "权限集合", Description = "权限集合")]
        [StringLength(500)]
        public string _Roles { get; set; }

        [Display(Name = "子菜单", Description = "子菜单")]
        public IEnumerable<MenuDto> _Children { get; set; }
    }

    /// <summary>
    /// 轮廓
    /// </summary>
    public class MenuProfile : BaseProfile<Menu, MenuDto>
    {
        public MenuProfile()
        {
        }
    }

    /// <summary>
    /// 映射
    /// </summary>
    public static class MenuMapper
    {
        static MenuMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<MenuProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        /// <summary>
        /// 映射到ModelDto
        /// </summary>
        /// <param name="model">Menu</param>
        /// <returns></returns>
        public static MenuDto ToDto(this Menu model)
        {
            return Mapper.Map<MenuDto>(model);
        }

        /// <summary>
        /// 映射到EF实体
        /// </summary>
        /// <param name="model">MenuDto</param>
        /// <returns></returns>
        public static Menu ToEntity(this MenuDto model)
        {
            return Mapper.Map<Menu>(model);
        }
    }
}
