import request from '@/utils/request'

const profix = process.env.NODE_ENV === 'development' ? '/vue-element-admin' : ''

export function getRoutes() {
  return request({
    url: `${profix}/routes`,
    method: 'get'
  })
}

export function getRoles() {
  return request({
    url: `${profix}/role`,
    method: 'get'
  })
}

/**
 * 获取所有权限
 */
export function getAllRoles() {
  console.log('getAllRoles')
  return request({
    url: `${profix}/role`,
    method: 'get',
    params: { filterRules: [], pageNationToken: '', limit: 10000 }
  })
}

/**
 * 根据RoleId获取MenuId
 * @param {string} RoleId
 * @returns
 */
export function getRoleMenu(RoleId) {
  return request({
    url: `${profix}/role/getRoleMenu`,
    method: 'get',
    params: { RoleId: RoleId }
  })
}

export function addRole(data) {
  return request({
    url: `${profix}/role`,
    method: 'post',
    data
  })
}

export function updateRole(id, data) {
  return request({
    url: `/role/${id}`,
    method: 'put',
    data
  })
}

export function deleteRole(id) {
  return request({
    url: `/role/${id}`,
    method: 'delete'
  })
}
