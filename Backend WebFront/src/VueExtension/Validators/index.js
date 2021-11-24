import { objIsEmpty } from '@/utils'
// 全局 验证规则
var $Validtors = {
  PasswordValidator: function(rule, value, callback) {
    if (objIsEmpty(value)) {
      callback() // 验证通过也必须调用callback
    } else {
      if (value.length < 6 || value.length > 20) {
        return callback(new Error('密码必须介于6-20位之间'))
      }
      if (!/[A-Z]/.test(value) || !/[a-z]/.test(value)) {
        return callback(new Error('密码至少含有一个大写&小写字母'))
      }
      // var AllCHN_reg = /^[\u4E00-\u9FA5]+$/ // 全为中文
      var hasCHNReg = /[\u4e00-\u9fa5]+/ // 含有中文
      if (hasCHNReg.test(value)) {
        return callback(new Error('密码不能含有中文'))
      }
      callback() // 验证通过也必须调用callback
    }
  },
  PasswordcConfirmValidator: function(rule, value, callback, thisVue) {
    // thisVue 带过来当前，vue实例
    console.log('PasswordcConfirmValidator', thisVue.curr_rowdata)
    if (objIsEmpty(value)) {
      callback() // 验证通过也必须调用callback
    } else {
      const field = rule.field
      const suffix = 'confirm'
      const idx = field.toLowerCase().indexOf(suffix)
      if (idx >= 0) {
        const passwordfield = field.substring(idx + suffix.length)
        if (passwordfield && thisVue.curr_rowdata[passwordfield] !== value) {
          return callback(new Error('2次密码必须一致'))
        }
      }
      callback() // 验证通过也必须调用callback
    }
  },
  IdCardValidator: function(rule, value, callback) {
    if (objIsEmpty(value)) {
      callback() // 验证通过也必须调用callback
    } else {
      // 身份证正则
      const reg = /(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)/
      if (value === '') {
        callback(new Error('身份证号不能为空'))
      } else if (!reg.test(value)) {
        callback(new Error('请输入正确的身份证号'))
      } else {
        callback() // 验证通过也必须调用callback
      }
    }
  },
  IdCardHKValidator: function(rule, value, callback) {
    if (objIsEmpty(value)) {
      callback() // 验证通过也必须调用callback
    } else {
      let str = value
      // var strValidChars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'
      // basic check length
      if (str.length < 8) {
        callback(new Error('请输入正确的香港身份证号'))
      }
      // handling bracket
      if (str.charAt(str.length - 3) === '(' && str.charAt(str.length - 1) === ')') {
        str = str.substring(0, str.length - 3) + str.charAt(str.length - 2)
      }
      // convert to upper case
      str = str.toUpperCase()
      // regular expression to check pattern and split
      var hkidPat = /^([A-Z]{1,2})([0-9]{6})([A0-9])$/
      var matchArray = str.match(hkidPat)
      // not match, return false
      if (matchArray == null) {
        callback(new Error('请输入正确的香港身份证号'))
      }
      callback() // 验证通过也必须调用callback
    }
  }
}
export default $Validtors
