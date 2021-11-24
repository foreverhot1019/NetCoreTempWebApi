import request from '@/utils/request'

const profix = process.env.NODE_ENV === 'development' ? '/vue-element-admin' : ''

export function getMenuTree(ParentMenuId) {
  return request({
    url: `${profix}/menu/getMenuTree`,
    method: 'get',
    params: { ParentMenuId: ParentMenuId }
  })
}
