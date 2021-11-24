<template>
  <div id="div_UserManage">
    <AutoCRUD
      ref="AutoCRUD"
      :controller-name="controllerName"
      :fields="Fields"
      @ParentgetSubmitData="execDataBeforeSubmit"
    />
    <el-drawer
      ref="drawer"
      title="权限选择"
      direction="btt"
      :show-close="false"
      size="50%"
      :visible.sync="showDrawer"
      :before-close="closeDrawer"
    >
      <el-container>
        <el-main>
          <el-select
            v-model="seltRole"
            filterable
            placeholder="请选择"
            clearable
          >
            <el-option
              v-for="item in roleOptions"
              :key="item._ID"
              :label="item._Name"
              :value="item._Name"
            />
          </el-select>
          &nbsp;
          <el-button type="primary" @click="saveRole">新增</el-button>
          &nbsp;
          <el-table :data="crrrentUserRole" border>
            <el-table-column property="UserName" label="用户名" width="150" />
            <el-table-column property="Role" label="权限" width="200" />
            <el-table-column label="操作" width="100">
              <template slot-scope="scope">
                <el-button icon="remove" @click="removeRole(scope.row)">
                  删除
                </el-button>
              </template>
            </el-table-column>
          </el-table>
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
import _ from 'lodash'
import { getAllRoles } from '@/api/role'
import { getUser } from '@/api/user'
import * as utils from '@/utils'

// 自定义列数据(覆盖BaseArrField-ArrField行值)
// CustomerFields.Currency = {
//   Name: 'Currency', //名称
//   DisplayName:  '授权币制',//显示名称
//   IsKey: true, //主键v-on:-默认：false
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
      { Name: '_ID', DisplayName: 'Id', Type: 'string', IsKey: true },
      { Name: '_Name', DisplayName: '账户名称', Required: true, MaxLength: 20, SearchShow: true, Sortable: true },
      { Name: '_SecurityStamp', DisplayName: '安全票根', ListShow: false, FormShow: false, MaxLength: 64, Width_List: '290', Width_input: '178' },
      { Name: '_Email', DisplayName: '邮箱', Required: true, isEmail: true },
      { Name: '_PhoneNumber6', DisplayName: '电话' },
      { Name: '_Password', DisplayName: '密码', inputType: 'password', Required: true },
      { Name: 'Confirm_Password', DisplayName: '确认密码', inputType: 'password' },
      { Name: '_Resource', DisplayName: '资源', Sortable: true },
      { Name: 'LockoutEnabled', DisplayName: '锁定', Type: 'boolean', Sortable: true },
      { Name: 'AccessFailedCount', DisplayName: '登录失败', Type: 'number', IsKey: false, Required: true, Editable: false, FormShow: false },
      { Name: 'LockoutEnd', DisplayName: '锁定结束', Width_List: '*', Type: 'datetime', inputType: 'datetime', Editable: false, FormShow: false },
      { Name: 'Tags', DisplayName: '标签', Width_List: '*', Width_input: '178', inputType: 'tagedit', Sortable: true },
      { Name: '_Roles', DisplayName: '权限', Editable: false, FormShow: false, ListShow: false }
    ]
    return {
      seltRole: '', // 选择的权限
      roleOptions: [], // 权限数据集合
      crrrentUserId: '', // 当前编辑数据
      crrrentUserName: '', // 当前编辑数据
      crrrentUserRole: [], // 当前权限数据
      showDrawer: false, // 打开抽屉
      Fields: SetArrField,
      controllerName: 'User' // CRUD控制器
    }
  },
  created() {
    const thisVue = this
    getAllRoles()
      .then((res) => {
        console.log('getAllRoles()', res)
        thisVue.roleOptions = res.arrData || res
      })
      .catch((error) => {
        thisVue.$message({
          duration: 10, // 不自动关闭
          showClose: true,
          message: `获取用户权限错误:${error.message}`,
          type: 'error'
        })
      })
  }, // 数据初始化，还未渲染dom,在此处设置的数据 不受影响
  methods: {
    // 子组件 发送请求前 处理数据
    execDataBeforeSubmit(callback) {
      // 子组件执行 dlgSubmit=> emit('ParentgetSubmitData')(就是本组件设置的ParentgetSubmitData，即execDataBeforeSubmit)
      // 本组件 callback 返回信息给 子组件 判断是否继续执行
      // 获取提交数据
      const thisVue = this
      if (!thisVue.showDrawer) {
        const childVue = this.$refs['AutoCRUD'] // 转换为子组件
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
            thisVue.crrrentUserId = childVue.curr_rowdata[childVue.keyFiledName]
            thisVue.crrrentUserName = childVue.curr_rowdata._Name
            thisVue.getUserRole() // 获取用户权限数据
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
    getUserRole() {
      var that = this
      console.log(that.crrrentUserId)
      const childVue = this.$refs['AutoCRUD']
      if (utils.objIsEmpty(that.crrrentUserId)) {
        that.$message({
          duration: 10, // 不自动关闭
          showClose: true,
          message: '获取用户权限错误:用户Id不能为空',
          type: 'error'
        })
      } else {
        // 新增无需搜索
        if (that.crrrentUserId.indexOf('_') >= 0) {
          return
        }
        getUser(that.crrrentUserId, 0)
          .then((res) => {
            var { _Roles: arrRole } = res
            that.crrrentUserRole = arrRole.map((item) => {
              return { UserName: that.crrrentUserName, Role: item }
            })
            // 赋值权限
            childVue.curr_rowdata._Roles = _.defaultsDeep([], res) // 深度拷贝数据Object.assign只能深度拷贝一层
          })
          .catch((error) => {
            const ErrMsg = error.errMessage || '意外错误'
            that.$message({
              duration: 10, // 不自动关闭
              showClose: true,
              message: `获取用户权限错误: ${ErrMsg}`,
              type: 'error'
            })
          })
      }
    }, // 获取用户权限end
    closeDrawer(closeFunc) {
      const thisVue = this // 转换为子组件
      const childVue = this.$refs['AutoCRUD'] // 转换为子组件
      // 赋值权限数据到当前编辑数据集
      childVue.curr_rowdata._Roles = this.crrrentUserRole.map((item) => {
        return item.Role
      })
      if (typeof closeFunc === 'function') {
        closeFunc()
      }
      childVue.dlgSubmit() // 触发子组件保存事件
      thisVue.showDrawer = false // 打开抽屉
    }, // 关闭抽屉时触发子组件保存
    saveRole(e) {
      const thisVue = this // 转换为子组件
      if (!utils.objIsEmpty(thisVue.seltRole)) {
        this.crrrentUserRole.push({
          UserName: thisVue.crrrentUserName,
          Role: thisVue.seltRole
        })
        thisVue.seltRole = '' // 清空选择的权限
      }
    }, // 保存权限数据
    removeRole(row) {
      if (!utils.objIsEmpty(row.Role)) {
        const index = this.crrrentUserRole.map((x, index) => {
          if (x.Role === row.Role) {
            return index
          } else {
            return -1
          }
        }).filter(x => x >= 0)
        if (index.length > 0) {
          this.crrrentUserRole.splice(index[0], 1)
        }
      }
    } // 保存权限数据
  }
}
</script>
