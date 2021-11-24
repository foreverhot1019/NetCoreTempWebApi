<template>
  <div ref="div_AutoCRUDLocal">
    <el-row style="padding: 3px 0px 3px 0px"><!--Table按钮组--><!--上右下左-->
      <el-col>
        <el-button-group>
          <el-button type="primary" icon="el-icon-plus" :disabled="!$route.meta.Create" @click="handleAddRow">新增</el-button>
          <el-button icon="el-icon-download" :disabled="!$route.meta.Export" @click="ExportXls(tb_OrdCustomer_data,'导出OrdCustomer')">导出</el-button>
          <el-button icon="el-icon-upload" :disabled="!$route.meta.Import" @click="ImportXls">导入</el-button>
          <el-button type="danger" :disabled="($route.meta.Delete ? selctRows.length===0 : true)" @click="handledelSeltRow">批量删除</el-button>
        </el-button-group>
      </el-col>
    </el-row>
    <!--Table列表-->
    <el-row>
      <el-col>
        <el-table
          ref="tb_OrderCuntomer"
          style="width: 100%"
          max-height="300"
          row-key="Id"
          border
          stripe
          :data="tb_OrdCustomer_data"
          :v-loading="tbLoading"
          :default-sort="{prop: keyFieldName,order:'descending'}"
          @selection-change="handleSelectionChange"
          @sort-change="tbSortChange"
          @row-dblclick="handledblclick"
        >
          <el-table-column type="selection" width="41" />
          <template>
            <el-table-column
              v-for="field in ArrListField"
              :key="field.Name"
              show-overflow-tooltip
              :width="field.Width_List||120"
              :sortable="field.Sortable?'custom':false"
              :prop="field.Name"
              :label="field.DisplayName"
              :formatter="formatter(field)"
            />
          </template>
        </el-table>
      </el-col>
    </el-row>
    <!--form编辑弹出框-->
    <el-dialog
      v-if="OrderCuntomer !== null && JSON.stringify(OrderCuntomer) !== '{}'"
      ref="OrderCuntomerDialog"
      v-loading="dlgLoading"
      width="60%"
      center
      append-to-body
      :close-on-click-modal="false"
      :show-close="false"
      :visible.sync="DialogVisible"
      :fullscreen="dlgfullscreen"
      :before-close="(done)=>{dlgClose(done)}"
    >
      <div slot="title" style="" class="el-dialog__title" @dblclick="dlgfullscreen = !dlgfullscreen">
        <el-row>
          <el-col :span="8" style="cursor:move" v-html="'&nbsp;'" />
          <el-col :span="8" style="cursor:move">{{ dgTitle }}</el-col>
          <el-col :span="8" style="text-align:right">
            <el-button type="primary" icon="el-icon-check" :disabled="!$route.meta.Edit" title="确 定" circle @click="dlgok_func" />
            <el-button type="danger" icon="el-icon-close" title="取 消" circle @click="dlgClose" />
          </el-col>
        </el-row>
      </div>
      <el-form ref="OrderCuntomerForm" :model="OrderCuntomer" :label-position="label_position" inline>
        <!--autocomplete-->
        <!-- <el-form-item :label-width="formLabelWidth" label="中文名" prop="NameChs"
           :rules="el_FormFieldRules({Name:'NameChs',DisplayName:'中文名',Required:true,Editable:true,MinLength:0,MaxLength:50})">
          <el-autocomplete v-model="OrderCuntomer['NameChs']" style="width:178px"
           popper-class="my-autocomplete"
           value-key="NameChs"
           :select="OrdCuntomerSelt"
           :fetch-suggestions="OrdCustomerQSearch" >
            <template slot-scope="{ item }">
             <div class="name" style="text-overflow: ellipsis overflow: hidden">{{ item.NameChs }}</div>
             <span class="addr" style="font-size: 12px color: #b4b4b4">{{ item.Sex|Vue_Sexformatter }}-{{ item.Birthday|Vue_dateformatter }}</span>
            </template>
          </el-autocomplete>
        </el-form-item> -->
        <el-form-item
          v-for="field in ArrFormField"
          :key="field.Name"
          :label-width="form_LabelWidth"
          :label="field.DisplayName"
          :prop="field.Name"
          :rules="el_FormFieldRules(field)"
        >
          <component
            :is="el_inputType(field)"
            v-if="!field.IsForeignKey && field.FormShow && field.inputType !== 'tagedit'"
            v-model="OrderCuntomer[field.Name]"
            :type="el_inputProtoType(field)"
            :disabled="field.IsKey||field.Name===refFieldName||(!field.Editable&&(isNaN(OrderCuntomer[keyFieldName])?OrderCuntomer[keyFieldName].indexOf('_') < 0:OrderCuntomer[keyFieldName]>0))"
            :prop="field.Name"
            :precision="field.Precision"
            :clearable="true"
            :show-word-limit="(field.MaxLength||50)>0"
            :maxlength="field.MaxLength||50"
            :minlength="field.MinLength||50"
            :style="{'width':(field.Width_input||178)+'px'}"
          >
            <i
              v-if="field.Name.toLowerCase().indexOf('password')>=0"
              v-show="field.Name.toLowerCase().indexOf('password')>=0"
              slot="suffix"
              class="el-input__icon fa"
              :class="el_inputClass(field)"
              @click="pswView(field)"
            />
          </component>
          <component
            :is="el_inputType(field)"
            v-else-if="field.FormShow && field.inputType === 'tagedit'"
            v-model="OrderCuntomer[field.Name]"
            :style="{'width':(field.Width_input||178)+'px'}"
            :editable="field.Editable"
          />
          <el-select
            v-else
            v-model="OrderCuntomer[field.Name]"
            reserve-keyword
            clearable
            :remote-method="q=>el_remoteMethod(q,field,'form')"
            :loading="el_selt.el_selt_loading"
            :style="{'width':(field.Width_input||178)+'px'}"
            :multiple="field.multiple"
          >
            <template v-if="el_selt[field.Name+'_form']">
              <el-option
                v-for="item in el_selt[field.Name+'_form'].ArrOption"
                :key="item.key"
                :label="item.displayName"
                :value="item.value"
              />
            </template>
          </el-select>
        </el-form-item>
        <el-tabs v-if="ArrTabEditField&&ArrTabEditField.length>0" ref="el_Tab" v-model="TabActiveName" type="border-card" @tab-click="TabClick">
          <template v-for="tab in ArrTabEditField">
            <el-tab-pane :key="tab.Name" :label="tab.DisplayName" :name="tab.Name">
              <AutoCRUDLocal
                v-if="OrderCuntomer[tab.Name+'_config'].dlgVisible"
                :ref="tab.Name"
                v-model="OrderCuntomer[tab.Name]"
                :ref-field-val="OrderCuntomer[keyFieldName]"
                :ref-field-name="fields[tab.Name+'Fields'].refFieldName"
                :del-data="OrderCuntomer[tab.Name+'_config'].delData"
                :fields="fields[tab.Name+'Fields']"
              />
              <!-- _self 获取vue实例 filters
              :Fields="tab.Name+'Fields'|getVueDataByStr"
              :Fields="$data[tab.Name+'Fields']"
              :Fields="_self[tab.Name+'Fields']" -->
              <!--keep-alive 保持上次渲染时的状态
              include: 字符串或正则表达式。只有匹配的组件会被缓存。
              exclude: 字符串或正则表达式。任何匹配的组件都不会被缓存。-->
            </el-tab-pane>
          </template>
        </el-tabs><!--TabEdit-->
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button type="primary" icon="el-icon-check" :disabled="!$route.meta.Edit" @click="dlgok_func">确 定</el-button>
        <el-button type="default" icon="el-icon-close" @click="dlgClose">取 消</el-button>
      </span>
    </el-dialog>
  </div>
</template>
<script>
import moment from 'moment'
import { objIsEmpty } from '@/utils'
import TagEdit from '@/components/TagEdit' // 标签编辑展示
import elementExt from '@/utils/elementExtention'
import _ from 'lodash'

// 自定义列数据(覆盖BaseArrField-ArrField行值)
// customerFields.Currency = {
//   Name: 'Currency', //名称
//   DisplayName: '授权币制',//显示名称
//   IsKey: true, //主键
//   Editable: true, //可编辑
//   Required: true, //必填
//   Type: 'string', //'datetime/number/string/boolean';//类型-默认string
//   inputType: 'text', //'password/datetime/text/tagedit/tabedit';//form中的input类型-默认text
//   IsForeignKey: true, //外键渲染为Select
//   multiple: true, //select多选
//   ForeignKeyGetListUrl: '/api/GetPagerPARA_CURR_FromCache', //获取外键数据Url
//   isEmail: true, //邮件格式
//   FormOrder: 1, //Form排序
//   FormShow: true, //Form中展示
//   ListOrder: 1, //列表排序
//   ListShow: true, //列表展示
//   MaxLength: 50, //最大长度
//   MinLength: 10, //最小长度
//   Precision: 2 //小数位位数 //Type为number时，可设置小数位
//   SearchOrder: 1, //搜索排序
//   SearchShow: true, //搜索中展示
//   Sortable: true, //是否可排序
//   Width_List: '120', //列表-列宽度 <=0 默认*，>0 此宽度为准
//   Width_input: '178', //Form-input宽度 <=0 默认*，>0 此宽度为准
// }

export default {
  name: 'AutoCRUDLocal',
  components: {
    TagEdit // 标签编辑展示
  },
  model: {
    prop: 'tbData',
    event: 'change'
  },
  props: {
    refFieldName: { // 关联字段名称
      type: String,
      required: true
    },
    refFieldVal: { // 关联字段值
      type: [String, Number],
      // default: 'Number',
      required: true
    },
    tbData: { // 数据集合
      type: Array,
      required: true
    },
    delData: { // 记录删除数据
      type: Array,
      required: true
    },
    formlabelWidth: {
      type: String,
      default: '120px'
    },
    formlabelPosition: {
      type: String,
      default: 'right'
    },
    customerFields: { // 自定义列数据(覆盖BaseArrField-ArrField行值)
      type: Object,
      required: false,
      default: () => {}
    },
    fields: { // 所有要渲染的字段
      type: [Array, String],
      required: true
    }
  },
  data: function() {
    var data = {
      addNum: 0, // 新增序号
      TabActiveName: '', // 选中tab名称
      OrderId: this.refFieldVal, // 订单Id
      tb_OrdCustomer_data: this.tbData, // 当前列表数据集合
      OrderCuntomer: {}, // 当前编辑数据
      OrderCuntomer_Original: {}, // 当前编辑数据原始
      form_LabelWidth: this.formlabelWidth, // Label宽度
      label_position: this.formlabelPosition, // Label排列位置
      DelData: this.delData, // 记录删除的数据
      UserRoles: {}, // 权限
      DialogVisible: false, // 弹出框显示
      dlgfullscreen: false, // 弹出框全屏
      dlgLoading: false, // 编辑弹出框加载中
      tbLoading: false, // 数据列表加载中
      selctRows: [], // 选择的数据
      el_selt: {
        el_selt_loading: false // 选择框 搜索状态
      }, // select数据
      ArrEnumField: [], // 所有外键select字段
      ArrFormField: [], // 添加/编辑字段 通过此配置渲染
      ArrListField: [], // table展示列 通过此配置渲染
      ArrSearchField: [], // 搜索字段数据通过此配置渲染
      ArrTagEditField: [], // [string]字段数据通过此配置渲染
      ArrTabEditField: [], // ArrTabEditField // Tab展示字段数据通过此配置渲染
      keyFields: [], // 主键集合
      keyFieldName: 'Id' // 主键名
    }
    var pagiNation = { // 翻页控件数据
      pageSizes: [1, 10, 20, 50, 100, 200, 300, 500],
      pageSize: 10,
      currentPage: 1,
      layout: 'total, sizes, prev, pager, next, jumper',
      total: 0
    }
    var filters = { // 搜索数据
      page: pagiNation.currentPage,
      rows: pagiNation.pageSize,
      sort: 'Id',
      order: 'desc',
      filterRules: {}// 查询条件
    }
    data.pagiNation = pagiNation
    data.filters = filters
    console.log('data', data, this)
    return data
  },
  computed: { // 计算属性
    dgTitle: function() {
      const dgTitle = this.title || ''
      // debugger
      // this.UserRoles.Edit = false// 修改不了 writable 为 false 属性const化
      // Object.defineProperty(this.UserRoles,'Edit',{configurable:true,writable:true})
      // Object.defineProperty(this.UserRoles,'Edit',{value:false})
      // var tCurrRowData = typeof (this.OrderCuntomer)
      // if (tCurrRowData === 'undefined' || this.OrderCuntomer === null || JSON.stringify(this.OrderCuntomer) === '{}') {
      if (objIsEmpty(this.OrderCuntomer)) {
        return '未知'
      }
      var keyId = this.OrderCuntomer[this.keyFieldName]
      if (isNaN(keyId) ? keyId.indexOf('_') >= 0 : keyId <= 0) {
        return `${dgTitle}新增`
      } else {
        if (this.$route.meta.Edit) {
          return `${dgTitle}编辑`
        } else {
          return `${dgTitle}查看`
        }
      }
    }
  }, // 计算属性
  watch: {
    // tbData: {
    //   handler: (newval, oldval) => {
    //     // console.log('watch-tbData', newval, oldval)
    //     this.tb_OrdCustomer_data = objIsEmpty(newval) ? [] : newval
    //   }
    //   // true:在 wacth 里声明了之后，就会立即先去执行里面的handler方法
    //   // false:不会在绑定的时候就执行。
    //   // immediate: true
    //   // deep: true
    // },
    // delData: {
    //   handler: (newval, oldval) => {
    //     console.log('watch-delData', newval, oldval)
    //     this.DelData = objIsEmpty(newval) ? [] : newval
    //     // if (!objIsEmpty(newval)) {
    //     //   this.DelData = newval
    //     // } else {
    //     //   this.DelData = []
    //     // }
    //   }
    //   // true:在 wacth 里声明了之后，就会立即先去执行里面的handler方法
    //   // false:不会在绑定的时候就执行。
    //   // immediate: true
    //   // deep: true
    // },
    formlabelWidth: {
      handler: (newval, oldval) => {
        this.form_LabelWidth = objIsEmpty(newval) ? '120' : newval
      }
      // true:在 wacth 里声明了之后，就会立即先去执行里面的handler方法
      // false:不会在绑定的时候就执行。
      // immediate: true
      // deep: true
    },
    fields: {
      handler: (newval, oldval) => {
        this.fieldsUpdate()
      }
      // true:在 wacth 里声明了之后，就会立即先去执行里面的handler方法
      // false:不会在绑定的时候就执行。
      // immediate: true
      // deep: true
    }
  }, // 监听属性变化
  created: function() {
    this.fieldsUpdate()
  }, // 数据初始化，还未渲染dom,在此处设置的数据 不受响应
  mounted: function() {
    if (!objIsEmpty(this.tb_OrdCustomer_data)) {
      var addNum = parseInt(this.tb_OrdCustomer_data.addNum)
      this.addNum = isNaN(addNum) ? 0 : addNum
    }// 记录上次渲染时，新增数据Num
    var thisVue = this
    this.ArrEnumField.forEach(function(item) {
      thisVue.el_remoteMethod('', item, 'form', true)
    }) // 外键触发搜索初始化
    // const {
    //   Edit, // 修改
    //   Create, // 创建
    //   Delete, // 删除
    //   Audit, // 审核
    //   Import, // 导入
    //   Export // 导出
    // } = this.$route.meta
    // Object.defineProperty(this, 'UserRoles', {
    //   value: {
    //     Edit, // 修改
    //     Create, // 创建
    //     Delete, // 删除
    //     Audit, // 审核
    //     Import, // 导入
    //     Export // 导出
    //   }
    // })
    // this.$forceUpdate() // 强制刷新组件
    /* 设置属性不能修改 相当于const  {value:{}}等价于 {value : {},writable : false,configurable : false,enumerable : false} */
    // Object.defineProperty(this, 'UserRoles', { value: {} })
    // var setterFunc = function (newVal) {
    //   var err = '不允许修改值'
    //   if (typeof (console) === 'undefined') {
    //     alert(err)
    //   } else {
    //     console.log(err)
    //   }
    // }
    // const Edit = this.$route.meta.Edit || false // 修改
    // const Create = this.$route.meta.Create || false // 创建
    // const Delete = this.$route.meta.Delete || false // 删除
    // const Audit = this.$route.meta.Audit || false // 审核
    // const Import = this.$route.meta.Import || false // 导入
    // const Export = this.$route.meta.Export || false // 导出
    // Object.defineProperty(this.UserRoles, 'Edit', { configurable: false, get: function () { return Edit } })
    // Object.defineProperty(this.UserRoles, 'Create', { configurable: false, get: function () { return Create } })
    // Object.defineProperty(this.UserRoles, 'Delete', { configurable: false, get: function () { return Delete } })
    // Object.defineProperty(this.UserRoles, 'Audit', { configurable: false, get: function () { return Audit } })
    // Object.defineProperty(this.UserRoles, 'Import', { configurable: false, get: function () { return Import } })
    // Object.defineProperty(this.UserRoles, 'Export', { configurable: false, get: function () { return Export } })
    console.log('mounted', this.UserRoles)
  }, // 相当于构造函数，渲染完dom后触发
  methods: {
    fieldsUpdate: elementExt.fieldsUpdate, // 赋值渲染然字段
    el_FormFieldRules: elementExt.el_FormFieldRules, //  输出input验证规则// 赋值渲染然字段
    el_inputType: elementExt.el_inputType, // 判断input输出格式
    el_inputProtoType: elementExt.el_inputProtoType, // el_input-Type属性
    el_inputClass: elementExt.el_inputClass, // password 显示/隐藏 class
    pswView: elementExt.pswView, // 密码框 显示隐藏
    getkeyFiled: elementExt.getkeyFiled, // 赋值渲染然字段
    generateNextKeyId: elementExt.generateNextKeyId, // 获取下一个主键Id
    handleAddRow: function(e) {
      console.log('handleAddRow', e)
      const { nextId, keyField } = this.generateNextKeyId()// 下一个主键值
      var newRow = { _New: true } // 主键字段赋值
      newRow[keyField || 'Id'] = nextId || --this.addNum
      newRow[this.refFieldName] = this.refFieldVal // 关联字段赋值
      // Tag值为null赋值空数组
      this.ArrTagEditField.forEach(field => {
        if (field.inputType === 'tagedit') {
          newRow[field.Name] = []
        }
      })
      // 赋值删除&添加序号字段
      this.ArrTabEditField.forEach(tab => {
        newRow[tab.Name] = []
        const conf = `${tab.Name}_config`
        newRow[conf] = {}
        newRow[conf].delData = [] // 记录删除数据
        newRow[conf].dlgVisible = false // 弹出状态
        newRow[conf].addNum = -1 // 记录新增序号
      })
      this.DialogVisible = true
      this.OrderCuntomer = newRow
      this.dlgLoading = false// 编辑弹出框加载中
      // this.tb_OrdCustomer_data.push(newRow)
      // this.tb_OrdCustomer_data.addNum = this.addNum// 记录上次添加数-<keep-alive>
      this.SetTabActiveByName() // 根据上次活动Tab名称-设置Tab活动
    }, // 增加行数据 弹出框添加
    handledblclick: function(row, column, event) {
      if (event) {
        if (event.stopPropagation) {
          // 停止冒泡
          event.stopPropagation()
        } else {
          // 停止冒泡
          event.cancelBubble = true
        }
      }
      // 取消选中
      window.getSelection ? window.getSelection().removeAllRanges() : document.selection.empty()
      this.DialogVisible = true
      this.OrderCuntomer_Original = row
      this.OrderCuntomer = _.defaultsDeep({}, row) // 深拷贝
      if (row[this.TabActiveName]) {
        row[this.TabActiveName].dlgVisible = true
      }
      const currRowdata = this.OrderCuntomer // _.deepClone(this.OrderCuntomer)
      const thisVue = this
      Object.keys(currRowdata).forEach(function(item, index) {
        const val = currRowdata[item] + ''
        if (!objIsEmpty(val)) {
          // json-date时间戳 转 date
          if (val.indexOf('/Date(') >= 0) {
            // eslint-disable-next-line new-cap
            var MDate = new moment(val)
            if (MDate.isValid()) {
              currRowdata[item] = MDate.toDate()
            }
          }
          // select 字段获取最新显示数据
          var ArrFilter = thisVue.ArrEnumField.filter(function(field) { return field.Name === item })
          if (ArrFilter.length > 0) {
            const OFilter = ArrFilter[0]
            const url = OFilter.ForeignKeyGetListUrl// '/MenuItems/GetData'
            if (!objIsEmpty(url) & url.indexOf('GetPagerEnum') < 0) {
              thisVue.el_remoteMethod('', OFilter, 'form', true)
            }
          }
        }
        // } else {
        //   var qArrTagEdit = thisVue.ArrTagEditField.filter(function (field) { return field.Name === item })
        //   if (qArrTagEdit.length > 0) {
        //     currRowdata[item] = []
        //   }
        // }
      })
      // Tag值为null赋值空数组
      this.ArrTagEditField.forEach(field => {
        if (field.inputType === 'tagedit') {
          currRowdata[field.Name] = []
        }
      })
      // 编辑数据-子数据集为空时，赋值[]
      thisVue.ArrTabEditField.forEach(item => {
        const tagVal = currRowdata[item.Name]
        // Tab值为null赋值空数组
        if (objIsEmpty(tagVal)) {
          currRowdata[item.Name] = []
        }
        const conf = `${item.Name}_config`
        if (objIsEmpty(currRowdata[conf])) {
          currRowdata[conf] = {}
          currRowdata[conf].delData = [] // 记录删除数据
          currRowdata[conf].dlgVisible = false // 弹出状态
          currRowdata[conf].addNum = -1 // 记录新增序号
        }
      })
      this.SetTabActiveByName() // 根据上次活动Tab名称-设置Tab活动
      console.log('row-dblclick', row)
    }, // 双击行
    handleSelectionChange: function(selections) {
      this.selctRows = selections
      console.log('handleSelectionChange', selections)
    }, // 选择数据变更
    handledelSeltRow: function(e) {
      // console.log('handledelSeltRow', e, this.selctRows)
      if (this.selctRows.length <= 0) {
        this.$message({
          duration: 0, // 不自动关闭
          showClose: true,
          message: '错误:未选择需要删除的数据',
          type: 'error'
        })
      } else {
        var thisVue = this
        thisVue.tbLoading = true// 加载中
        var deltRowIndex = this.tb_OrdCustomer_data.map(function(item, i) {
          var hasEl = thisVue.selctRows.some((el, x) => {
            return el[thisVue.keyFieldName] === item[thisVue.keyFieldName]
          })
          if (hasEl) {
            return i
          } else {
            return null
          }
        })
        deltRowIndex = deltRowIndex.filter(item => item !== null)
        deltRowIndex = deltRowIndex.reverse()
        if (deltRowIndex.length >= 0) {
          deltRowIndex.forEach(function(ArrIdx) {
            const KeyId = thisVue.tb_OrdCustomer_data[ArrIdx][thisVue.keyFieldName]
            thisVue.tb_OrdCustomer_data.splice(ArrIdx, 1)
            thisVue.delData.push(KeyId)// 记录删除数据
            // thisVue.tb_OrdCustomer_data.delData.push(KeyId)// 记录删除数据
          })
          thisVue.$emit('chang', thisVue.tb_OrdCustomer_data)// 触发 v-model 修改
          // console.log(thisVue, thisVue.tb_OrdCustomer_data)
          thisVue.$emit('updateData', thisVue.tb_OrdCustomer_data)// 回调父组件自定义方法
          // thisVue.$emit('curr_rowdataChange', thisVue.tb_OrdCustomer_data)
        }
        thisVue.tbLoading = false// 加载中
      }
      // rows.splice(index, 1)
    }, // 批量删除选中行数据
    dlgok_func: function() { // 弹出框关闭前
      const thisVue = this
      const MyForm = thisVue.$refs['OrderCuntomerForm']
      // MyForm.resetFields()// 重置验证
      MyForm.clearValidate()// 清除验证
      MyForm.validate(function(valid) {
        if (valid) {
          thisVue.$emit('chang', thisVue.tb_OrdCustomer_data)// 触发 v-model 修改
          if (thisVue.OrderCuntomer._New) {
            thisVue.tb_OrdCustomer_data.push(thisVue.OrderCuntomer)
            thisVue.OrderCuntomer._New = false
          } else {
            // 更新原始数据，触发数据更新
            Object.assign(thisVue.OrderCuntomer_Original, thisVue.OrderCuntomer)
          }
          thisVue.tb_OrdCustomer_data.addNum = thisVue.addNum// 记录上次添加数-<keep-alive>
          // 关闭弹出框
          thisVue.DialogVisible = false
          thisVue.$emit('dlgok_func')// 回调父组件自定义方法
        } else {
          return false
        }
      })
    },
    // 导出 导入 Excel
    ExportXls: elementExt.ExportXls, // 导出数据
    ImportXls: function() {
      // console.log('ImportXls')
    }, // 导入数据
    formatter: elementExt.formatter, // el-table-column 数据显示转换
    el_remoteMethod: elementExt.el_remoteMethod, // 外键触发搜索
    dlgClose: function(doneFunc) {
      const thisVue = this
      const MyForm = this.$refs['OrderCuntomerForm']
      // MyForm.resetFields()// 重置验证
      MyForm.clearValidate()// 清除验证
      MyForm.validate(function(valid) {
        if (valid) {
          thisVue.OrderCuntomer._New = false
          // 更新原始数据，触发数据更新
          // Object.assign(thisVue.OrderCuntomer_Original, thisVue.OrderCuntomer)
          thisVue.DialogVisible = false
          thisVue.$emit('dlgok_func')// 触发父组件事件
        } else {
          thisVue.$confirm(`${thisVue.dgTitle}验证错误, 强制${(thisVue.OrderCuntomer._New ? '新增' : '编辑')}?`, '提示', {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
          }).then(function() {
            if (thisVue.OrderCuntomer._New) {
              thisVue.tb_OrdCustomer_data.push(thisVue.OrderCuntomer)
              thisVue.OrderCuntomer._New = false
            } else {
              // 更新原始数据，触发数据更新
              Object.assign(thisVue.OrderCuntomer_Original, thisVue.OrderCuntomer)
            }
            thisVue.tb_OrdCustomer_data.addNum = thisVue.addNum// 记录上次添加数-<keep-alive>
            if (typeof (doneFunc) === 'function') {
              doneFunc()
            } else {
              thisVue.DialogVisible = false
              thisVue.$emit('dlgok_func') // 触发父组件事件
            }
          }).catch(function() {
            if (typeof (doneFunc) === 'function') {
              doneFunc()
            } else {
              thisVue.DialogVisible = false
            }
            // var OrderCuntomer = thisVue.OrderCuntomer
            // var delIndex = null
            // thisVue.tb_OrdCustomer_data.forEach(function (item, idx) {
            //   if (item.Id === OrderCuntomer.Id) {
            //     delIndex = idx
            //     return false
            //   }
            // })
            // if (delIndex != null) {
            //   thisVue.tb_OrdCustomer_data.splice(delIndex, 1)
            // }
          })
        }
      })
    }, // 弹出框关闭
    SetTabActiveByName() { // 根据上次活动Tab名称-设置Tab活动
      const row = this.OrderCuntomer
      if (this.TabActiveName) {
        let TabActive = row[`${this.TabActiveName}_config`]
        if (!TabActive && !isNaN(this.TabActiveName)) {
          const tabName = this.$refs.el_Tab.panes[this.TabActiveName].name
          TabActive = row[`${tabName}_config`]
          this.TabActiveName = tabName
        }
        if (TabActive) {
          TabActive.dlgVisible = true
        }
      }
    },
    TabClick: function(tab, event) {
      // console.log('TabClick', tab)
      this.TabActiveName = tab.name
      const conf = `${this.TabActiveName}_config`
      var tabObjComponent = this.OrderCuntomer[conf]
      tabObjComponent.dlgVisible = true // 设置异步组件显示
    }, // tab点击事件
    tbSortChange: function(sortObj) { // {column:列,prop:字段,sort:排序}
      // console.log('tbSortChange', sortObj)
      var IsReload = false
      let sort, order
      if (!(typeof (sortObj) === 'undefined' || sortObj === null || JSON.stringify(sortObj) === '{}')) {
        sort = sortObj.prop
        if (!(typeof (sortObj.prop) === 'undefined' || sortObj.prop === null || sortObj.prop === '')) {
          sort = sortObj.prop
        } else {
          sort = 'Id'
        }
        order = sortObj.order
        if (!(typeof (order) === 'undefined' || order === null || order === '')) {
          order = sortObj.order.replace('ending', '')
        } else {
          order = 'desc'
        }
        if (this.filters.sort !== sort || this.filters.order !== order) {
          IsReload = true
          this.filters.sort = sort
          this.filters.order = order
        }
      } else {
        if (this.filters.sort !== 'Id' || this.filters.order !== 'descending') {
          IsReload = true
        }
        this.filters.sort = 'Id'
        this.filters.order = 'desc'
      }
      if (IsReload) {
        this.tb_OrdCustomer_data = this.tb_OrdCustomer_data.sort((a, b) => {
          if (order === 'desc') {
            return a[sort] > b[sort]
          } else {
            return a[sort] < b[sort]
          }
        })
        // if (this.pagiNation.currentPage === 1) {
        //   this.tb_GetData()
        // } else {
        //   this.pageCurrentChange(1)// 重新获取数据
        // }
      }
    } // table排序变更
  }
}
</script>
