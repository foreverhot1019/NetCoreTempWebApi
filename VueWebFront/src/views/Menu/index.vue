<template>
  <div id="div_UserManage">
    <AutoCRUD :controller-name="controllerName" :fields="Fields" />
  </div>
</template>
<script>
import AutoCRUD from '@/components/AutoCRUD' // AutoCRUD组件
// 自定义列数据(覆盖BaseArrField-ArrField行值)
// CustomerFields.Currency = {
//   Name: 'Currency', //名称
//   DisplayName:  '授权币制',//显示名称
//   IsKey: true, //主键v-on:-默认：false
//   Editable: true, //可编辑-默认：true
//   Required: true, //必填-默认：false
//   Type: 'string', //'datetime/number/string/boolean';//类型-默认: string
//   inputType: 'text', //'password/datetime/text/tagedit/tabedit';//form中的input类型-默认: text
//   IsForeignKey: true, //外键渲染为Select
//   multiple: true, //select多选
//   ForeignKeyGetListUrl: '/api/GetPagerPARA_CURR_FromCache', //获取外键数据Url
//   isEmail: true, //邮件格式
//   FormOrder: 1, //Form排序
//   FormShow: true, //Form中展示-默认：true
//   ListOrder: 1, //列表排序
//   ListShow: true, //列表展示-默认：true
//   MaxLength:  50, //最大长度
//   MinLength: 10, //最小长度
//   Precision: 2 //小数位位数 //Type为number时，可设置小数位
//   SearchOrder: 1, //搜索排序
//   SearchShow: true, //搜索中展示-默认：false
//   Sortable: true, //是否可排序-默认：false
//   Width_List: '120', //列表-列宽度 <=0 默认*，>0 此宽度为准
//   Width_input: '178', //Form-input宽度 <=0 默认*，>0 此宽度为准
// }

// 枚举类型字段
export default {
  name: 'UserManage', // 页面名称（当组件引用时用到）
  components: { AutoCRUD }, // AutoCRUD组件
  data() {
    // 渲染CRUD字段数据集
    const SetArrField = [
      { Name: '_ID', DisplayName: 'Id', Type: 'string', IsKey: true },
      { Name: '_Name', DisplayName: '菜单名称', Required: true, MaxLength: 20, SearchShow: true },
      { Name: '_Hidden', DisplayName: '隐藏', MaxLength: 100, Type: 'boolean' },
      { Name: '_Remark', DisplayName: '菜单描述', MaxLength: 100 },
      { Name: '_Sort', DisplayName: '排序代码', Type: 'number' },
      { Name: '_Url', DisplayName: '菜单Url', Required: true, MaxLength: 100 },
      { Name: '_Controller', DisplayName: '控制器', MaxLength: 50 },
      { Name: '_IconCls', DisplayName: '菜单图标', MaxLength: 50 },
      { Name: '_Resource', DisplayName: '所属资源', Required: false, MaxLength: 50 },
      { Name: '_Component', DisplayName: 'Vue组件', Required: true, MaxLength: 100, SearchShow: true },
      {
        Name: '_ParentMenuId', DisplayName: '上级菜单', Type: 'string', SearchShow: true,
        IsForeignKey: true, ForeignKeyGetListUrl: '/Menu/GetMenuItemSelect'
      },
      {
        Name: '_Status', DisplayName: '状态', Type: 'number', SearchShow: true,
        IsForeignKey: true, ForeignKeyGetListUrl: '/EnumManage/UseStatusEnum'
      },
      { Name: '_CreateUserId', DisplayName: '新增人ID', MaxLength: 50, Editable: false },
      { Name: '_CreateUserName', DisplayName: '新增人', MaxLength: 20, Editable: false },
      { Name: '_CreateDate', Type: 'datetime', DisplayName: '新增时间', inputType: 'datetime', SearchShow: true, Editable: false },
      { Name: '_ModifyUserId', DisplayName: '修改人ID', MaxLength: 50 },
      { Name: '_ModifyUserName', DisplayName: '修改人', MaxLength: 20 },
      { Name: '_ModifyDate', DisplayName: '修改时间', Type: 'datetime', inputType: 'datetime' }
    ]
    return {
      Fields: SetArrField,
      controllerName: 'Menu' // CRUD控制器
    }
  }
}
</script>
