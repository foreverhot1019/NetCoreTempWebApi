import request from '@/utils/request'
import { objIsEmpty } from '@/utils'

var MyFunc = {
  controllName: '', // 控制器名称
  url: ``,
  SetController: (ctrName) => {
    if (!objIsEmpty(ctrName)) {
      MyFunc.controllName = ctrName
      MyFunc.url = `/${ctrName}`
    }
  },
  /**
   * 获取数据列表
   * @param {object} paramData 发送数据
   * @param {string} purl 请求url，默认为MyFunc-url
   * @returns {object}
   */
  Get: (data, purl) => {
    let url = MyFunc.url
    if (!objIsEmpty(purl)) {
      url = purl
    }
    // const { searhFilters, ...paramData } = data
    return new Promise((resolve, reject) => {
      if (MyFunc.valid(url)) {
        var ret = { IsSuccess: false, ErrMessage: 'Url未设置' }
        reject(ret)
      }
      // const queryparam = Object.keys(paramData).map(key => `${key}=${paramData[key]}`)
      // url += '?' + queryparam.join('&')
      request({
        url: url,
        method: 'get',
        // data: { searhFilters: searhFilters },
        params: data
      }).then(res => {
        resolve(res)
      }).catch(err => {
        reject(err)
      })
    })
  },
  /**
  * 获取数据对象
  * @param {object} paramData 发送QueryParam数据
  * @param {object} data 发送body数据
  * @param {string} purl 请求url，默认为MyFunc-url
  * @returns {object}
  */
  GetObject: (paramData, data, purl) => {
    let url = MyFunc.url
    if (!objIsEmpty(purl)) {
      url = purl
    }
    return new Promise((resolve, reject) => {
      if (MyFunc.valid(url)) {
        var ret = { IsSuccess: false, ErrMessage: 'Url未设置' }
        reject(ret)
      }
      const queryparam = Object.keys(paramData).map((key, index) => index === 0 ? '' : `${key}=${paramData[key]}`)
      request({
        url: `${url}/${escape(paramData[0])}/?${queryparam.join('&')}`,
        method: 'get',
        data: data
      }).then(res => {
        resolve(res)
      }).catch(err => {
        reject(err)
      })
    })
  },
  /**
   * 新增数据
   * @param {object} postData 发送数据
   * @returns {object}
   */
  Add: (postData, purl) => {
    let url = MyFunc.url
    if (!objIsEmpty(purl)) {
      url = purl
    }
    return new Promise((resolve, reject) => {
      if (MyFunc.valid(url)) {
        var ret = { IsSuccess: false, ErrMessage: 'Url未设置' }
        reject(ret)
      }
      request({
        url: url,
        method: 'post',
        headers: {
          'Content-Type': 'application/json'
        },
        data: postData
      }).then(res => {
        resolve(res)
      }).catch(err => {
        reject(err)
      })
    })
  },
  /**
   * 编辑
   * @param Id {int} 数据主键
   * @param {object} postData 发送数据
   * @returns {object}
   */
  Edit: (Id, postData, purl) => {
    let url = MyFunc.url
    if (!objIsEmpty(purl)) {
      url = purl
    }
    return new Promise((resolve, reject) => {
      if (MyFunc.valid(url)) {
        var ret = { IsSuccess: false, ErrMessage: 'Url未设置' }
        reject(ret)
      }
      request({
        url: `${url}/${escape(Id)}`,
        method: 'put',
        headers: {
          'Content-Type': 'application/json'
        },
        data: postData
      }).then(res => {
        resolve(res)
      }).catch(err => {
        reject(err)
      })
    })
  },
  /**
   * 删除
   * @param Id {int} 数据主键
   * @returns {object}
   */
  Delete: (Id, purl) => {
    let url = MyFunc.url
    if (!objIsEmpty(purl)) {
      url = purl
    }
    return new Promise((resolve, reject) => {
      if (MyFunc.valid(url)) {
        var ret = { IsSuccess: false, ErrMessage: 'Url未设置' }
        reject(ret)
      }
      request({
        url: `${url}/${escape(Id)}`,
        method: 'delete'
      }).then(res => {
        resolve(res)
      }).catch(err => {
        reject(err)
      })
    })
  },
  /**
   * 验证数据格式
   */
  valid: (url) => {
    var purlIsEmpty = objIsEmpty(url)
    return purlIsEmpty
    // return objIsEmpty(MyFunc.controllName)
  }
}
export default MyFunc

// export default class BaseApi {
//   #controllName
//   #url
//   constructor (controllName) {
//     MyFunc.#controllName = controllName
//     MyFunc.#url = `/api/${controllName}`
//   }
//   /**
//    * 获取数据列表
//    * @param {object} paramData 发送数据
//    * @returns {object}
//    */
//   Get (paramData) {
//     if (MyFunc.valid()) {
//       return { Success: false, ErrMsg: '控制器未设置' }
//     }
//     return request({
//       url: MyFunc.#url,
//       method: 'get',
//       params: paramData
//     })
//   }
//   /**
//    * 新增数据
//    * @param {object} postData 发送数据
//    * @returns {object}
//    */
//   Add (postData) {
//     if (MyFunc.valid()) {
//       return { Success: false, ErrMsg: '控制器未设置' }
//     }
//     return request({
//       url: MyFunc.#url,
//       method: 'post',
//       data: postData
//     })
//   }
//   /**
//    * 编辑
//    * @param Id {int|string} 数据主键
//    * @param {object} postData 发送数据
//    * @returns {object}
//    */
//   Edit (Id, postData) {
//     if (MyFunc.valid()) {
//       return { Success: false, ErrMsg: '控制器未设置' }
//     }
//     return request({
//       url: `${MyFunc.#url}/${Id}`,
//       method: 'put',
//       data: postData
//     })
//   }
//   /**
//    * 删除
//    * @param Id {int|string} 数据主键
//    * @returns {object}
//    */
//   Delete (Id) {
//     if (MyFunc.valid()) {
//       return { Success: false, ErrMsg: '控制器未设置' }
//     }
//     return request({
//       url: `${MyFunc.#url}/${Id}`,
//       method: 'delete'
//     })
//   }
//   /**
//    * 验证控制器
//    */
//   valid () {
//     return objIsEmpty(MyFunc.#controllName)
//   }
// }
