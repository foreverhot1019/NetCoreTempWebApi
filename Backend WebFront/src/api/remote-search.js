import request from '@/utils/request'
const profix = process.env.NODE_ENV === 'development' ? '/vue-element-admin' : ''

export function searchUser(name) {
  return request({
    url: `${profix}/search/user`,
    method: 'get',
    params: { name }
  })
}

export function transactionList(query) {
  return request({
    url: `${profix}/transaction/list`,
    method: 'get',
    params: query
  })
}
