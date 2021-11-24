import request from '@/utils/request'
const profix = process.env.NODE_ENV === 'development' ? '/vue-element-admin' : ''

export function fetchList(query) {
  return request({
    url: `${profix}/article/list`,
    method: 'get',
    params: query
  })
}

export function fetchArticle(id) {
  return request({
    url: `${profix}/article/detail`,
    method: 'get',
    params: { id }
  })
}

export function fetchPv(pv) {
  return request({
    url: `${profix}/article/pv`,
    method: 'get',
    params: { pv }
  })
}

export function createArticle(data) {
  return request({
    url: `${profix}/article/create`,
    method: 'post',
    data
  })
}

export function updateArticle(data) {
  return request({
    url: `${profix}/article/update`,
    method: 'post',
    data
  })
}
