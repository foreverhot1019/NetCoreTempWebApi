<template>
  <div id="div_Role">
    <AutoCRUD ref="AutoCRUD" :controller-name="controllerName" :fields="Fields" @ParentgetSubmitData="execDataBeforeSubmit" />
    <el-drawer
      ref="drawer"
      title="菜单选择"
      direction="rtl"
      :show-close="false"
      :visible.sync="showDrawer"
      :before-close="closeDrawer"
    >
      <el-container>
        <el-main>
          <el-tree
            ref="tree"
            lazy
            show-checkbox
            node-key="_MenuId"
            class="permission-tree"
            :check-strictly="checkStrictly"
            :props="treeProps"
            :load="loadMenu"
          />
        </el-main>
        <el-footer>
          <el-button type="primary" @click="closeDrawer">确定</el-button>
        </el-footer>
      </el-container>
    </el-drawer>
  </div>
</template>
<script>
import AutoCRUD from '@/components/AutoCRUD' // AutoCRUD组件
import * as utils from '@/utils'
import { getMenuTree } from '@/api/menu'
import { getRoleMenu } from '@/api/role'

// 自定义列数据(覆盖BaseArrField-ArrField行值)
// CustomerFields.Currency = {
//   Name: 'Currency', //名称
//   DisplayName:  '名称',//显示名称
//   IsKey: true, //主键-默认：false
//   Editable: true, //可编辑-默认：true
//   Required: true, //必填-默认：false
//   Type: 'string', //'datetime/number/string/boolean';//类型-默认: string
//   inputType: 'text', //'password/datetime/text/tagedit/tabedit';//form中的input类型-默认: text
//   multiple: true, //select多选
//   IsForeignKey: true, //外键渲染为Select
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
      { Name: '_ID', DisplayName: 'Id', IsKey: true, Required: true, Sortable: true },
      { Name: '_Name', DisplayName: '权限名', Required: true, Sortable: true, Editable: true, SearchShow: true, MaxLength: 20, MinLength: 3 },
      { Name: '_Sort', DisplayName: '排序', Required: true, MaxLength: 20, Type: 'number' },
      { Name: '_Remark', DisplayName: '备注', Width_List: '290', Width_input: '178' },
      {
        Name: '_Status', DisplayName: '状态', Type: 'number', SearchShow: true,
        IsForeignKey: true, ForeignKeyGetListUrl: '/EnumManage/UseStatusEnum'
      },
      { Name: '_CreateUserId', DisplayName: '新增人ID', MaxLength: 50, Editable: false },
      { Name: '_CreateUserName', DisplayName: '新增人', MaxLength: 20, Editable: false },
      { Name: '_CreateDate', Type: 'datetime', DisplayName: '新增时间', inputType: 'datetime', SearchShow: true, Editable: false },
      { Name: '_ModifyUserId', DisplayName: '修改人ID', MaxLength: 50 },
      { Name: '_ModifyUserName', DisplayName: '修改人', MaxLength: 20 },
      { Name: '_ModifyDate', DisplayName: '修改时间', Type: 'datetime', inputType: 'datetime' },
      { Name: '_ArrMenu', DisplayName: '允许发放类型', formShow: false },
      // tab页面
      { Name: '_ArrObj', DisplayName: '允许发放类型', Width_input: '378', inputType: 'tabedit', Editable: true }
    ]
    SetArrField['_ArrObjFields'] = [
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
        Name: '_ParentMenuId', DisplayName: '上级菜单', Type: 'number', SearchShow: true,
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
      { Name: '_ModifyDate', DisplayName: '修改时间', Type: 'datetime', inputType: 'datetime' },
      { Name: '_RoleId', DisplayName: 'RoleId', Type: 'string', Required: true, Editable: false },
      { Name: '_ArrTest', DisplayName: '测试Tab', inputType: 'tabedit', Editable: true }
    ]
    SetArrField['_ArrObjFields'].refFieldName = '_RoleId' // 与主表关联字段名称

    SetArrField['_ArrObjFields']['_ArrTestFields'] = [
      { Name: '_ID', DisplayName: 'Id', Type: 'string', IsKey: true },
      { Name: '_TestName', DisplayName: 'Test名称', Required: true, MaxLength: 20, SearchShow: true },
      { Name: '_TestVal', DisplayName: 'Test值', Required: true, SearchShow: true },
      { Name: '_TestIntVal', DisplayName: 'TestInt', Required: false, Type: 'number', Precision: 3 },
      { Name: '_TestBool', DisplayName: 'TestInt', Required: false, Type: 'boolean' },
      {
        Name: '_Status', DisplayName: '状态', Type: 'number', SearchShow: true,
        IsForeignKey: true, ForeignKeyGetListUrl: '/EnumManage/UseStatusEnum'
      },
      { Name: '_CreateUserId', DisplayName: '新增人ID', MaxLength: 50, Editable: false },
      { Name: '_CreateUserName', DisplayName: '新增人', MaxLength: 20, Editable: false },
      { Name: '_CreateDate', Type: 'datetime', DisplayName: '新增时间', inputType: 'datetime', SearchShow: true, Editable: false },
      { Name: '_ModifyUserId', DisplayName: '修改人ID', MaxLength: 50 },
      { Name: '_ModifyUserName', DisplayName: '修改人', MaxLength: 20 },
      { Name: '_ModifyDate', DisplayName: '修改时间', Type: 'datetime', inputType: 'datetime' },
      { Name: '_MenuId', DisplayName: '_MenuId', Type: 'string', Required: true, Editable: false }
    ]
    SetArrField['_ArrObjFields']['_ArrTestFields'].refFieldName = '_MenuId'// 与主表关联字段名称
    return {
      Fields: SetArrField,
      controllerName: 'Role', // CRUD控器
      showDrawer: false, // 打开抽屉
      crrrentId: '', // 当前编辑数据
      checkStrictly: false,
      treeProps: {
        label: '_Name',
        children: '_Children',
        isLeaf: 'leaf'
      }
    }
  },
  methods: {
    getRoleMenus() {
      const thisVue = this
      console.log(thisVue.crrrentId)
      if (utils.objIsEmpty(thisVue.crrrentId)) {
        thisVue.$message({
          duration: 10, // 不自动关闭
          showClose: true,
          message: '获取用户权限错误:用户Id不能为空',
          type: 'error'
        })
      } else {
        // 新增无需搜索
        if (thisVue.crrrentId.indexOf('_') >= 0) {
          return
        }
        getRoleMenu(thisVue.crrrentId)
          .then(res => {
            const arrRoleMenu = res
            if (thisVue.$refs.tree) {
              const SeltMenuId = arrRoleMenu.map(x => x._MenuId)
              thisVue.checkStrictly = true
              thisVue.$nextTick(() => {
                thisVue.$refs.tree.setCheckedKeys(SeltMenuId)
                // set checked state of a node not affects its father and child nodes
                thisVue.checkStrictly = false
                console.log('getRoleMenu', SeltMenuId)
              })
            }
          })
          .catch(error => {
            const ErrMsg = error.errMessage || '意外错误'
            thisVue.$message({
              duration: 10, // 不自动关闭
              showClose: true,
              message: `获取用户权限错误: ${ErrMsg}`,
              type: 'error'
            })
          })
      }
    }, // 获取用户权限end
    // 子组件 发送请求前 处理数据
    execDataBeforeSubmit(callback) {
      // 子组件执行 dlgSubmit=> emit('ParentgetSubmitData')(就是本组件设置的ParentgetSubmitData，即execDataBeforeSubmit)
      // 本组件 callback 返回信息给 子组件 判断是否继续执行
      // 获取提交数据
      const thisVue = this
      if (!thisVue.showDrawer) {
        const childVue = thisVue.$refs['AutoCRUD'] // 转换为子组件
        // 获取crud form
        const MyForm = childVue.$refs['MyForm']
        MyForm.clearValidate() // 清除验证
        MyForm.validate(function(valid) {
          if (!valid) {
            if (callback) {
              callback({ Success: true }) // 不执行callback 让子组件停止往下执行
            } else {
              return
            }
          } else {
            thisVue.crrrentId = childVue.curr_rowdata[childVue.keyFieldName]
            thisVue.getRoleMenus() // 获取用户权限数据
            thisVue.showDrawer = true // 打开抽屉
            if (callback) {
              // 返回true，子组件不继续执行
              // 返回false，子组件继续执行
              callback({ Success: true }) // 不执行callback 让子组件停止往下执行
            } else {
              return
            }
          }
        })
      } else {
        return
      }
    }, // 提交数据前触发抽屉选择
    // 搜索数据
    loadMenu(node, resolve) {
      const thisVue = this
      // console.log('loadMenu', thisVue.$refs.tree.getCheckedKeys())
      let parentMenuId = '-'
      if (node.data && node.data._ID !== '-') {
        parentMenuId = node.data._ID
      }
      getMenuTree(parentMenuId).then(res => {
        const tree = res
        if (tree.length > 0) {
          tree.forEach(item => {
            item._MenuId = item._ID
            item.leaf = false
          })
        } else {
          node.leaf = true
        }
        resolve(tree)
      }).catch(err => {
        const errMsg = err.errMessge || err
        thisVue.$message({
          duration: 10, // 不自动关闭
          showClose: true,
          message: `获取菜单错误:${errMsg}`,
          type: 'error'
        })
      })
    },
    closeDrawer(closeFunc) {
      const thisVue = this // 转换为子组件
      const childVue = this.$refs['AutoCRUD'] // 转换为子组件
      // 赋值权限数据到当前编辑数据集
      childVue.curr_rowdata._ArrMenu = thisVue.$refs.tree.getCheckedKeys().map((item) => {
        return { _ID: item }
      })
      if (typeof closeFunc === 'function') {
        closeFunc()
      }
      childVue.dlgSubmit() // 触发子组件保存事件
      thisVue.showDrawer = false // 打开抽屉
    } // 关闭抽屉时触发子组件保存
  }
}
</script>
