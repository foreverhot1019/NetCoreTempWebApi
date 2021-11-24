import Layout from '@/layout'

const sysmanagerRouter = {
  path: '/sysmanager',
  component: Layout,
  redirect: 'noRedirect', // 默认跳转到地址
  name: 'sysmanager',
  meta: {
    title: '系统管理',
    icon: 'el-icon-setting'
  },
  children: [
    {
      path: 'Role',
      component: () => import('@/views/Role/index'),
      name: 'Role',
      meta: {
        title: '权限管理',
        Create: true,
        Edit: true,
        Delete: true,
        Import: true,
        Export: true
      }
    },
    {
      path: 'User',
      component: () => import('@/views/User/User'),
      name: 'User',
      meta: {
        title: '账户管理',
        Create: true,
        Edit: true,
        Delete: true,
        Import: true,
        Export: true
      }
    },
    {
      path: 'Menu',
      component: () => import('@/views/Menu/index'),
      name: 'Menu',
      meta: {
        title: '菜单管理',
        Create: true,
        Edit: true,
        Delete: true,
        Import: true,
        Export: true
      }
    }
  ]
}

export default sysmanagerRouter
