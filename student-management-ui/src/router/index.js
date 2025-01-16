import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { ElMessage } from 'element-plus'

// 定义角色权限映射
const rolePermissions = {
  // 学生权限
  4: {
    allowedRoutes: [
      '/my-grades', // 成绩查看
      '/course/selection', // 选课
    ],
    menuItems: [
      {
        title: '我的成绩',
        path: '/my-grades',
        icon: 'School'
      },
      {
        title: '选课管理',
        path: '/course/selection',
        icon: 'Reading'
      }
    ]
  },
  // 教师权限
  3: {
    allowedRoutes: [
      '/student',
      '/student/add',
      '/student/edit/:id',
      '/student/:id/grades',
      '/class',
      '/course',
      '/course/selection',
      '/grade'
    ],
    menuItems: [
      {
        title: '学生管理',
        path: '/student',
        icon: 'User'
      },
      {
        title: '班级管理',
        path: '/class',
        icon: 'School'
      },
      {
        title: '课程管理',
        path: '/course',
        icon: 'Reading'
      },
      {
        title: '选课管理',
        path: '/course/selection',
        icon: 'List'
      },
      {
        title: '成绩管理',
        path: '/grade',
        icon: 'Document'
      }
    ]
  },
  // 管理员权限
  2: {
    allowedRoutes: [
      '/student',
      '/student/add',
      '/student/edit/:id',
      '/student/:id/grades',
      '/class',
      '/course',
      '/course/selection',
      '/grade',
      '/user',
      '/user/add',
      '/user/edit',
      '/user/edit/:id'
    ],
    menuItems: [
      {
        title: '学生管理',
        path: '/student',
        icon: 'User'
      },
      {
        title: '班级管理',
        path: '/class',
        icon: 'School'
      },
      {
        title: '课程管理',
        path: '/course',
        icon: 'Reading'
      },
      {
        title: '选课管理',
        path: '/course/selection',
        icon: 'List'
      },
      {
        title: '成绩管理',
        path: '/grade',
        icon: 'Document'
      },
      {
        title: '用户管理',
        path: '/user',
        icon: 'UserFilled'
      }
    ]
  },
  // 超级管理员权限
  1: {
    allowedRoutes: [
      '/student',
      '/student/add',
      '/student/edit/:id',
      '/student/:id/grades',
      '/class',
      '/course',
      '/course/selection',
      '/grade',
      '/user',
      '/user/edit/:id',
      '/user/add',
      '/user/edit',
      '/system/log'
    ],
    menuItems: [
      {
        title: '学生管理',
        path: '/student',
        icon: 'User'
      },
      {
        title: '班级管理',
        path: '/class',
        icon: 'School'
      },
      {
        title: '课程管理',
        path: '/course',
        icon: 'Reading'
      },
      {
        title: '选课管理',
        path: '/course/selection',
        icon: 'List'
      },
      {
        title: '成绩管理',
        path: '/grade',
        icon: 'Document'
      },
      {
        title: '用户管理',
        path: '/user',
        icon: 'UserFilled'
      },
      {
        title: '系统日志',
        path: '/system/log',
        icon: 'Document'
      }
    ]
  }
}

// 导出权限配置供其他组件使用
export { rolePermissions }

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import( '@/views/login/Login.vue' )
  },
  {
    path: '/forgot-password',
    name: 'ForgotPassword',
    component: () => import( '@/views/login/ForgotPassword.vue' ),
    meta: {
      title: '忘记密码',
      requiresAuth: false
    }
  },
  {
    path: '/',
    component: () => import( '@/views/layout/MainLayout.vue' ),
    children: [
      {
        path: '',
        redirect: '/student'
      },
      {
        path: 'student',
        name: 'StudentList',
        component: () => import( '@/views/student/StudentList.vue' ),
        meta: { title: '学生管理' }
      },
      {
        path: 'student/add',
        name: 'StudentAdd',
        component: () => import( '@/views/student/StudentForm.vue' ),
        meta: { title: '添加学生' }
      },
      {
        path: 'student/edit/:id',
        name: 'StudentEdit',
        component: () => import( '@/views/student/StudentForm.vue' ),
        meta: { title: '编辑学生' }
      },
      {
        path: 'class',
        name: 'ClassList',
        component: () => import( '@/views/class/ClassList.vue' ),
        meta: { title: '班级管理' }
      },
      {
        path: 'course',
        name: 'CourseList',
        component: () => import( '@/views/course/CourseList.vue' ),
        meta: { title: '课程管理' }
      },
      {
        path: 'course/selection',
        name: 'CourseSelection',
        component: () => import( '@/views/course-selection/CourseSelection.vue' ),
        meta: { title: '选课管理' }
      },
      {
        path: 'grade',
        name: 'GradeList',
        component: () => import( '@/views/grade/GradeList.vue' ),
        meta: { title: '成绩管理' }
      },
      {
        path: 'user',
        name: 'UserList',
        component: () => import( '@/views/user/UserList.vue' ),
        meta: {
          title: '用户管理',
          requiresAuth: true,
          roles: ['超级管理员', '管理员']  // 只允许管理员访问
        }
      },
      {
        path: 'user/add',
        name: 'UserAdd',
        component: () => import( '@/views/user/UserForm.vue' ),
        meta: {
          title: '添加用户',
          requiresAuth: true,
          roles: ['超级管理员', '管理员']
        }
      },
      {
        path: 'user/edit/:id',
        name: 'UserEdit',
        component: () => import( '@/views/user/UserForm.vue' ),
        meta: {
          title: '编辑用户',
          requiresAuth: true,
          roles: ['超级管理员', '管理员']
        }
      },
      {
        path: 'student/:id/grades',
        name: 'StudentGrades',
        component: () => import( '@/views/student/StudentGrades.vue' ),
        meta: {
          title: '学生成绩',
          requiresAuth: true
        }
      },
      {
        path: 'system/log',
        name: 'SystemLog',
        component: () => import( '@/views/system/LogList.vue' ),
        meta: {
          title: '系统日志',
          requiresAuth: true,
          roles: ['超级管理员', '管理员']
        }
      },
      {
        path: 'my-grades',
        name: 'MyGrades',
        component: () => import( '@/views/student/MyGrades.vue' ),
        meta: {
          title: '我的成绩',
          requiresAuth: true
        }
      }
    ]
  }
]

const router = createRouter( {
  history: createWebHistory( import.meta.env.BASE_URL ),
  routes
} )

// 修改路由守卫，保持原有功能的同时添加权限控制
router.beforeEach( ( to, from, next ) => {
  const userStore = useUserStore()
  const user = userStore.user

  // 白名单路由
  const whiteList = ['/login', '/forgot-password']

  if ( user ) {
    // 检查 token 是否过期
    const token = localStorage.getItem( 'token' )
    if ( !token ) {
      userStore.clearUser()
      window.location.href = 'about:blank'
      setTimeout( () => {
        window.location.href = '/login'
      }, 100 )
      return
    }

    // 获取用户角色的权限配置
    const roleConfig = rolePermissions[user.role]

    if ( to.path === '/login' ) {
      next( '/' )
      return
    }

    // 检查路由权限
    if ( to.meta.roles ) {
      const hasRole = to.meta.roles.includes( getRoleText( user.role ) )
      if ( !hasRole ) {
        next( roleConfig.menuItems[0].path )
        return
      }
    }

    // 如果访问的不是允许的路由，重定向到第一个允许的路由
    if ( !roleConfig.allowedRoutes.some( path => {
      // 将路由配置中的参数占位符转换为正则表达式
      const pattern = path.replace( /:\w+/g, '[^/]+' )
      const regex = new RegExp( `^${pattern}$` )
      return regex.test( to.path )
    } ) ) {
      next( roleConfig.menuItems[0].path )
      return
    }

    next()
  } else {
    if ( whiteList.includes( to.path ) ) {
      next()
    } else {
      next( '/login' )
    }
  }
} )

// 辅助函数：获取角色文本
function getRoleText( role ) {
  const roles = {
    1: '超级管理员',
    2: '管理员',
    3: '教师',
    4: '学生'
  }
  return roles[role] || '未知'
}

export default router
