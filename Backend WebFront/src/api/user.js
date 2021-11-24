import request from '@/utils/request'

const profix = process.env.NODE_ENV === 'development' ? '/vue-element-admin' : ''

export function login(data) {
  return request({
    url: `${profix}/user/login`,
    method: 'post',
    data
  })
}

export function getInfo(token) {
  return request({
    url: `${profix}/user/info`,
    method: 'get',
    params: { token }
  })
}

export function logout() {
  return request({
    url: `${profix}/user/logout`,
    method: 'post'
  })
}

/**
 * 获取账户
 * @param {*} id 账户Id
 * @param {*} rangekey 账户rangeKey
 * @returns 账户
 */
export function getUser(id, rangekey) {
  console.log('getUser', id)
  return request({
    url: `${profix}/user/${escape(id)}`,
    method: 'get',
    params: { rangekey: rangekey }
  })
}
