import { objIsEmpty, generateTimeSpan } from '@/utils'
import BaseApi from '@/api/BaseApi'
import _ from 'lodash'

export default {
  fieldsUpdate: function() {
    const thisVue = this
    // thisVue.$set(thisVue, 'ArrEnumField', [])
    // thisVue.$set(thisVue, 'ArrFormField', [])
    // thisVue.$set(thisVue, 'ArrListField', [])
    // thisVue.$set(thisVue, 'ArrSearchField', [])
    // thisVue.$set(thisVue, 'ArrTagEditField', [])
    // thisVue.$set(thisVue, 'ArrTabEditField', [])
    const ArrEnumField = thisVue.ArrEnumField // 所有外键select字段
    let ArrFormField = thisVue.ArrFormField // 添加/编辑字段 通过此配置渲染,FormShow：true(默认)
    let ArrListField = thisVue.ArrListField // table展示列 通过此配置渲染,ListShow：true(默认)
    let ArrSearchField = thisVue.ArrSearchField // 搜索字段数据通过此配置渲染,SearchShow：false(默认)
    const ArrTagEditField = thisVue.ArrTagEditField // 所有数组编辑字段
    const ArrTabEditField = thisVue.ArrTabEditField // Tab编辑字段
    // 设置自定义列 覆盖Fields
    if (!objIsEmpty(thisVue.customerFields)) {
      Object.entries(thisVue.CustomerFields).forEach(([key, value]) => {
        const OField = thisVue.fields.filter(val => {
          return val.Name === key
        })
        if (OField.length > 0) {
          Object.assign(OField[0], value)
        }
      })
    }
    let IsFormOrder = false // form排序
    let IsListOrder = false // 列表排序
    let IsSearchOrder = false // 搜索排序
    if (!objIsEmpty(thisVue.fields)) {
      // 列表/编辑/搜索 字段集合
      thisVue.fields.forEach((item, idx) => {
        item.Editable = (item.Editable || item.Editable === undefined) // 编辑:默认true
        item.FormShow = (item.FormShow || item.FormShow === undefined) // Form中展示:默认true
        item.Type = item.Type || 'string' // 默认数据类型 datetime/number/string/boolean
        if (item.FormShow && !item.IsKey && item.inputType !== 'tabedit') {
          ArrFormField.push(item)
        }
        item.FormOrder = item.FormOrder || 0
        if (item.FormOrder > 0) {
          IsFormOrder = true
        }
        if ((item.ListShow || item.ListShow === undefined) && !item.IsKey) {
          ArrListField.push(item)
        }
        item.IsListOrder = item.IsListOrder || 0
        if (item.IsListOrder > 0) {
          IsListOrder = true
        }
        if (item.SearchShow && !item.IsKey) {
          ArrSearchField.push(item)
        }
        item.SearchOrder = item.SearchOrder || 0
        if (item.SearchOrder > 0) {
          IsSearchOrder = true
        }
        if (item.inputType === 'tagedit') {
          ArrTagEditField.push(item)
        }
        if (item.inputType === 'tabedit') {
          ArrTabEditField.push(item)
        }
        if (item.IsForeignKey) {
          ArrEnumField.push(item)
        }
        if (item.IsKey) {
          thisVue.keyFields.push(item)
        }
      })
      if (thisVue.keyFields && thisVue.keyFields.length > 0) {
        thisVue.keyFieldName = thisVue.keyFields[0].Name || 'Id'
      }
      if (IsFormOrder) {
        ArrFormField = ArrFormField.sort((a, b) => {
          const aOrder = a.FormOrder || 0
          const bOrder = b.FormOrder || 0
          return aOrder > bOrder
        })
      }
      if (IsListOrder) {
        ArrListField = ArrListField.sort((a, b) => {
          const aOrder = a.ListOrder || 0
          const bOrder = b.ListOrder || 0
          return aOrder > bOrder
        })
      }
      if (IsSearchOrder) {
        ArrSearchField = ArrSearchField.sort((a, b) => {
          const aOrder = a.SearchOrder || 0
          const bOrder = b.SearchOrder || 0
          return aOrder > bOrder
        })
      }
    }
    thisVue.ArrEnumField.forEach(function(item) { // 所有select枚举
      thisVue.el_remoteMethod('', item, 'search,form', true)
      // thisVue.el_remoteMethod('', item, '', true)
    })
  }, // 赋值渲染然字段
  el_FormFieldRules: function(rowConfig, isSearchForm) {
    // 是否搜索form
    var tIsSearchForm = typeof (isSearchForm)
    const inputType = rowConfig.inputType || 'text'
    const Type = rowConfig.Type || 'string'
    if (tIsSearchForm === 'undefined' || isSearchForm === null || tIsSearchForm !== 'boolean') {
      isSearchForm = false
    }
    var ArrRules = []
    if (!rowConfig.Editable && !isSearchForm) {
      return ArrRules
    }
    if (rowConfig.Required && !isSearchForm && Type !== 'boolean') {
      ArrRules.push({ required: true, message: '请输入' + rowConfig.DisplayName || rowConfig.Name, trigger: ['blur', 'change'] })
    }
    var name = rowConfig.Name.toLowerCase()
    if (name === 'email' || rowConfig.isEmail) {
      ArrRules.push({ type: 'email', message: '请输入正确的邮箱地址', trigger: ['blur', 'change'] })
    }
    if (inputType === 'password') {
      ArrRules.push({ validator: this.$Validtors.PasswordValidator, trigger: ['blur', 'change'] })
      if (name.toLowerCase().indexOf('confirm') >= 0) {
        ArrRules.push({ validator: (rule, value, callback) => this.$Validtors.PasswordcConfirmValidator(rule, value, callback, this), trigger: ['blur', 'change'] })
      }
    }
    if (name.indexOf('idcard') === 0 && inputType === 'text') {
      ArrRules.push({ validator: this.$Validtors.IdCardValidator, trigger: 'blur' })
    }
    if (Type === 'string' && (rowConfig.MinLength || rowConfig.MaxLength)) {
      if (inputType !== 'tagedit' && inputType !== 'tabedit') {
        var rule = { trigger: ['blur', 'change'] }
        if (rowConfig.MinLength) {
          rule.min = rowConfig.MinLength
          if (rowConfig.MaxLength) {
            rule.message = '字符长度必须介于 ' + rowConfig.MinLength + ' 到 ' + rowConfig.MaxLength + ' 之间'
          } else {
            rule.message = '字符长度 必须大于 ' + rowConfig.MinLength
          }
        }
        if (rowConfig.MaxLength) {
          rule.max = rowConfig.MaxLength
          if (rowConfig.MinLength) {
            rule.message = '字符长度 必须介于 ' + rowConfig.MinLength + ' 到 ' + rowConfig.MaxLength + ' 之间'
          } else {
            rule.message = '字符长度 必须小于 ' + rowConfig.MaxLength
          }
        }
        ArrRules.push(rule)
      }
    }
    return ArrRules
  }, // 输出input验证规则
  el_inputType: function(rowConfig) {
    var elInputType = 'input'
    if (rowConfig.Type === 'number') {
      elInputType = 'input-number'
    }
    if (rowConfig.Type === 'boolean') {
      elInputType = 'checkbox'
    }
    if (rowConfig.Type === 'datetime') {
      elInputType = 'date-picker'
    }
    if (rowConfig.inputType === 'tagedit') {
      return 'TagEdit'
    }
    // ES5 模板替换
    return `el-${elInputType}`// 'el-'+elInputType
  }, // 判断input输出格式
  el_inputProtoType: function(field, isSearchForm) { // el_input-Type属性
    const inputType = field.inputType || 'text'
    if (!isSearchForm && !field.Editable) {
      return inputType
    }
    // 是否搜索form
    var tisSearchForm = typeof (isSearchForm)
    if (tisSearchForm === undefined || isSearchForm === null || tisSearchForm !== 'boolean') {
      isSearchForm = false
    }
    let filterData = isSearchForm ? this.filters.filterRules : this.curr_rowdata
    filterData = filterData || {}
    const p = '$' + field.Name + 'inputType'
    // 设置零时变量，记录$inputType
    if (objIsEmpty(filterData[p])) {
      if (inputType === 'datetime' && isSearchForm) {
        return 'daterange'
      } if (inputType === 'text' && field.MaxLength > 100) {
        return 'textarea'
      } else {
        return inputType
      }
    } else {
      return filterData[p]
    }
  }, // el_input-Type属性
  el_inputClass: function(field) {
    if (field.Name.toLowerCase().indexOf('password') >= 0) {
      const currRowData = this.curr_rowdata
      const name = '$' + field.Name + 'pswView'
      const inputClass = { 'fa-eye-slash': false, 'fa-eye': currRowData[name] }
      if (objIsEmpty(currRowData[name])) {
        inputClass['fa-eye-slash'] = true
      } else {
        inputClass['fa-eye-slash'] = !currRowData[name]
        inputClass['fa-eye'] = currRowData[name]
      }
      return inputClass
    } else {
      return { 'el-icon-edit': true }
    }
  }, // password 显示/隐藏 class
  pswView: function(field) { // 密码框 显示隐藏
    var pswView = '$' + field.Name + 'pswView'
    var inputType = '$' + field.Name + 'inputType'
    if (objIsEmpty(this.curr_rowdata[pswView])) {
      this.$set(this.curr_rowdata, pswView, true)
      this.$set(this.curr_rowdata, inputType, 'text')
    } else if (!this.curr_rowdata[pswView]) {
      this.curr_rowdata[pswView] = true
      this.curr_rowdata[inputType] = 'text'
    } else {
      this.curr_rowdata[pswView] = false
      this.curr_rowdata[inputType] = 'password'
    }
  }, // 密码框 显示隐藏
  keydown: function(e) { // dom原生控件keydown事件 v-on:keydown.native='keydown'
    return true
    // var event = e || window.event // 事件
    // var keycode = event.keycode || event.which // 键码
    // // 取消事件冒泡(W3C)
    // if (event && event.stopPropagation)
    //   event.stopPropagation()
    // else
    //   // IE中取消事件冒泡
    //   window.event.cancelBubble = true
    // // 阻止默认浏览器动作(W3C)
    // if (event && event.preventDefault)
    //   event.preventDefault()
    // else // IE中阻止函数器默认动作的方式
    //   window.event.returnValue = false
    // console.log('keydown', event, this)
    // event.returnValue = false
    // // window.event.returnValue = false
    // return false
  }, // dom原生控件keydown事件 v-on:keydown.native='keydown'
  keydownInt: function(e) {
    // var input =$(e.target);
    // var startPos = input.selectionStart;
    // var endPos = input.selectionEnd;
    // console.log('keydownInt',e,input,startPos,endPos);
    // e.stopPropagation();//停止冒泡
    // var value = e.target.value
    var key = e.key
    if (key === '.') {
      e.preventDefault()// 阻止原生事件
      return false
    }
  }, // 数字类型keydown
  keydownFloat: function(e, precision) {
    // console.log('keydownFloat',e);
    // e.stopPropagation();//停止冒泡
    var value = e.target.value
    var key = e.key
    if (objIsEmpty(precision)) {
      if (key === '.') {
        e.preventDefault() // 阻止原生事件
        return false
      }
    } else {
      precision = parseInt(precision)
      if (isNaN(precision)) {
        if (key === '.') {
          e.preventDefault() // 阻止原生事件
          return false
        }
      } else {
        var idx = value.indexOf('.')
        if (idx > 0) {
          var pointStr = value.slice(idx)
          if (pointStr.length >= precision + 1) {
            e.preventDefault() // 阻止原生事件
            return false
          }
        }
      }
    }
  }, // 浮点数字类型keydown
  // 导出 导入 Excel
  ExportXls: function(JsonData, fileName) {
    // console.log('ExportXls')
    const title = this.title
    require(['xlsx', 'file-saver'], function(XLSX, FileSaver) {
      const sheetName = title
      const wb = XLSX.utils.book_new() // 工作簿对象包含一SheetNames数组，以及一个表对象映射表名称到表对象。XLSX.utils.book_new实用函数创建一个新的工作簿对象。
      const ws = XLSX.utils.json_to_sheet(JsonData)
      wb.SheetNames.push(sheetName)
      wb.Sheets[sheetName] = ws
      const defaultCellStyle = { font: { name: 'Verdana', sz: 13, color: 'FF00FF88' }, fill: { fgColor: { rgb: 'FFFFAA00' }}}// 设置表格的样式
      const wopts = { bookType: 'xlsx', bookSST: false, type: 'binary', cellStyles: true, defaultCellStyle: defaultCellStyle, showGridLines: false } // 配置参数和样式
      // let wb = XLSX.utils.table_to_book(thisVue.$refs['Mytb'])
      /* get binary string as output */
      const wbout = XLSX.write(wb, wopts)
      try {
        const s2ab = function(s) { // 字符串转字符流
          if (typeof ArrayBuffer !== 'undefined') {
            var buf = new ArrayBuffer(s.length)
            var view = new Uint8Array(buf)
            for (var i = 0; i !== s.length; ++i) view[i] = s.charCodeAt(i) & 0xff
            return buf
          } else {
            var buff = new Array(s.length)
            for (var x = 0; x !== s.length; ++x) buff[x] = s.charCodeAt(x) & 0xFF
            return buff
          }
        }
        FileSaver.saveAs(new Blob([s2ab(wbout)], { type: 'application/octet-stream' }), fileName + '.xlsx')
      } catch (e) {
        if (typeof console !== 'undefined') {
          console.log(e, wbout)
        }
      }
      return wbout
    })
  }, // 导出数据
  ImportXls: function() {
    // console.log('ImportXls')
  }, // 导入数据
  formatter: function(field) { // el-table-column 数据显示转换
    var formatter = null
    switch (field.Type) {
      case 'boolean':
        formatter = this.$formatter.boolformatter
        break
      case 'date':
        formatter = this.$formatter.dateformatter
        break
      case 'datetime':
        formatter = this.$formatter.datetimeformatter
        break
      default:
        formatter = null
        break
    }
    var lowerName = field.Name.toLowerCase()
    if (lowerName.indexOf('sex') === 0) {
      formatter = this.$formatter.Sexformatter
    }
    if (!objIsEmpty(field.ForeignKeyGetListUrl)) {
      formatter = this.$formatter.joinformatter
    }
    // if (lower_Name.indexOf('photo') >= 0){
    //    formatter = this.$formatter.photoformatter
    // }
    return formatter
  }, // el-table-column 数据显示转换
  el_remoteMethod: function(query, field, profx, forceload) {
    const thisVue = this
    const ArrOptionName = []
    // 分解profx 多个相同数据集 搜索一次
    profx.split(',').forEach(item => {
      ArrOptionName.push(field.Name + '_' + item)
    })
    if (!objIsEmpty(query) || !objIsEmpty(forceload)) {
      thisVue.el_selt.el_selt_loading = true
      var paramData = { searhFilters: JSON.stringify([{ field: 'q', op: 'equals', value: query }]) }
      const url = field.ForeignKeyGetListUrl// '/MenuItems/GetData'
      BaseApi.Get(paramData, url).then(res => {
        const { data: rows } = res
        try {
          ArrOptionName.forEach(OptionName => {
            if (objIsEmpty(thisVue.el_selt[OptionName])) {
              thisVue.$set(thisVue.el_selt, OptionName, {})
            }
            if (objIsEmpty(rows)) {
              thisVue.$set(thisVue.el_selt[OptionName], 'ArrOption', res)
            } else {
              thisVue.$set(thisVue.el_selt[OptionName], 'ArrOption', rows)
            }
          })
        } catch (e) {
          thisVue.$message({
            duration: 0, // 不自动关闭
            showClose: true,
            message: '数据处理，出现错误',
            type: 'error'
          })
        }
        thisVue.el_selt.el_selt_loading = false// 加载完毕
      }).catch(err => {
        thisVue.el_selt.el_selt_loading = false// 加载完毕
        thisVue.$message({
          duration: 0, // 不自动关闭
          showClose: true,
          message: `获取数据出现错误:${err}`,
          type: 'error'
        })
      })
    } else {
      ArrOptionName.forEach(OptionName => {
        if (objIsEmpty(thisVue.el_selt[OptionName])) {
          thisVue.el_selt[OptionName] = {}
          thisVue.el_selt[OptionName]['ArrOption'] = []
        }
      })
    }
  }, // 外键触发搜索
  getSubmitData(data) { // 获取提交数据
    if (data) { // 如果父组件重写了getSubmitData方法，获取到数据后，立即返回数据
      return data
    }
    // 处理特殊数据格式如dictionary,带select得Array
    const thisVue = this
    const MyForm = this.$refs['MyForm']
    var batchSaveData = { // 批量操作数据
      inserted: [],
      deleted: [],
      updated: []
    }
    // MyForm.resetFields()// 清除验证
    MyForm.clearValidate()// 清除验证
    MyForm.validate(function(valid) {
      if (valid) {
        const keyField = thisVue.getkeyFiled()
        const postData = _.defaultsDeep({}, thisVue.curr_rowdata)
        console.log('dlgSubmit', postData)
        const Id = postData[keyField.Name] || postData.Id
        if (isNaN(Id) && Id.substr(0, 1) === '_') {
          batchSaveData.inserted.push(postData)
        } else if (Id <= 0) {
          batchSaveData.inserted.push(postData)
        } else {
          batchSaveData.updated.push(postData)
        }
      } else {
        console.log('error submit!!')
        batchSaveData = false
      }
    })
    return batchSaveData
  }, // 获取提交数据
  generateNextKeyId() {
    let nextId // 下一个主键值
    const keyField = this.getkeyFiled()
    if (keyField && keyField.Type !== undefined) {
      const type = keyField.Type.toLowerCase()
      if (type === 'string') {
        // nextId = '_' + generateUUID()
        nextId = '_' + generateTimeSpan()
      } else if (type === 'number') {
        nextId = --this.addNum
      }
    }
    if (!nextId) {
      nextId = --this.addNum
    }
    return { nextId: nextId, keyField: keyField.Name }
  }, // 获取下一个主键Id
  /** 获取第一个主键 */
  getkeyFiled() {
    const thisVue = this
    let keyField
    if (thisVue.keyFields && thisVue.keyFields.length > 0) {
      keyField = thisVue.keyFields[0]
    }
    return keyField
  } // 获取第一个主键
}
