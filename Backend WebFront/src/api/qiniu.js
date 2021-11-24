import request from '@/utils/request'
const profix = process.env.NODE_ENV === 'development' ? '/qiniu' : ''

export function getToken() {
  return request({
    url: `${profix}/upload/token`, // 假地址 自行替换
    method: 'get'
  })
}
