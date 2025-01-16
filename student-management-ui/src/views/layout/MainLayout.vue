<template>
    <div class="app-container" >
      <el-container class="layout-container">
        <el-aside width="220px" class="aside">
          <div class="logo">
            <h2>学籍管理系统</h2>
          </div>
          <el-menu
            router
            :default-active="$route.path"
            class="menu"
            background-color="#304156"
            text-color="#49d0d2"
            active-text-color="#ffffff"
          >
            <el-menu-item 
              v-for="item in menuItems" 
              :key="item.path" 
              :index="item.path"
            >
              <el-icon>
                <component :is="getIcon(item.icon)" />
              </el-icon>
              <span>{{ item.title }}</span>
            </el-menu-item>
          </el-menu>
        </el-aside>
        
        <el-container>
          <el-header class="header">
            <div class="header-left">
              <el-breadcrumb separator="/">
                <el-breadcrumb-item :to="{ path: '/' }">首页</el-breadcrumb-item>
                <el-breadcrumb-item>{{ getCurrentMenu }}</el-breadcrumb-item>
              </el-breadcrumb>
            </div>
            <div class="header-right">
              
              <el-dropdown @command="handleCommand">
                <span class="user-info">
                  {{ userStore.user?.name }}
                  <el-icon class="el-icon--right"><arrow-down /></el-icon>
                </span>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="profile">个人信息</el-dropdown-item>
                    <el-dropdown-item command="password">修改密码</el-dropdown-item>
                    <el-dropdown-item divided command="logout">退出登录</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>
          </el-header>
          
          <el-main class="main-container">
            <router-view />
          </el-main>
        </el-container>
      </el-container>
      
      <!-- 添加修改密码对话框 -->
      <el-dialog
        v-model="changePasswordVisible"
        title="修改密码"
        width="400px"
        @close="handleCloseDialog"
      >
        <el-form
          ref="passwordFormRef"
          :model="passwordForm"
          :rules="passwordRules"
          label-width="100px"
        >
          <el-form-item label="新密码" prop="newPassword">
            <el-input
              v-model="passwordForm.newPassword"
              type="password"
              show-password
              placeholder="请输入新密码"
            />
          </el-form-item>
          <el-form-item label="确认密码" prop="confirmPassword">
            <el-input
              v-model="passwordForm.confirmPassword"
              type="password"
              show-password
              placeholder="请再次输入新密码"
            />
          </el-form-item>
        </el-form>
        <template #footer>
          <span class="dialog-footer">
            <el-button @click="changePasswordVisible = false">取消</el-button>
            <el-button type="primary" :loading="submitting" @click="handleChangePassword">
              确认
            </el-button>
          </span>
        </template>
      </el-dialog>
      
      <!-- 个人信息对话框 -->
      <el-dialog
        v-model="profileVisible"
        title="个人信息"
        width="500px"
      >
        <div class="profile-info" v-if="profileData">
          <!-- 学生信息展示 -->
          <template v-if="isStudent">
            <el-descriptions :column="1" border>
              <el-descriptions-item label="学号">{{ profileData.studentNumber }}</el-descriptions-item>
              <el-descriptions-item label="姓名">{{ profileData.name }}</el-descriptions-item>
              <el-descriptions-item label="性别">{{ profileData.gender }}</el-descriptions-item>
              <el-descriptions-item label="班级">{{ profileData.className }}</el-descriptions-item>
              <el-descriptions-item label="年级">{{ profileData.grade }}</el-descriptions-item>
              <el-descriptions-item label="邮箱">{{ profileData.email }}</el-descriptions-item>
              <el-descriptions-item label="联系电话">{{ profileData.contactNumber }}</el-descriptions-item>
              <el-descriptions-item label="入学日期">{{ formatDate(profileData.enrollmentDate) }}</el-descriptions-item>
              <el-descriptions-item label="状态">
                <el-tag :type="getStatusType(profileData.status)">
                  {{ getStatusText(profileData.status) }}
                </el-tag>
              </el-descriptions-item>
            </el-descriptions>
          </template>
          
          <!-- 非学生用户信息展示 -->
          <template v-else>
            <el-descriptions :column="1" border>
              <el-descriptions-item label="用户名">{{ profileData.username }}</el-descriptions-item>
              <el-descriptions-item label="姓名">{{ profileData.name }}</el-descriptions-item>
              <el-descriptions-item label="角色">{{ getRoleText(profileData.role) }}</el-descriptions-item>
              <el-descriptions-item label="邮箱">{{ profileData.email }}</el-descriptions-item>
              <el-descriptions-item label="联系电话">{{ profileData.phone }}</el-descriptions-item>
              <el-descriptions-item label="创建时间">{{ formatDate(profileData.createdAt) }}</el-descriptions-item>
              <el-descriptions-item label="状态">
                <el-tag :type="getStatusType(profileData.status)">
                  {{ getStatusText(profileData.status) }}
                </el-tag>
              </el-descriptions-item>
            </el-descriptions>
          </template>
        </div>
      </el-dialog>
    </div>
  </template>
  
  <script setup>
  import { computed, ref, reactive, onMounted, watch } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { 
    User, 
    School, 
    Reading, 
    List, 
    ArrowDown, 
    Document, 
    
    UserFilled 
  } from '@element-plus/icons-vue'
  import { ElMessage, ElMessageBox } from 'element-plus'
  import { useUserStore } from '@/stores/user'
  import authService from '@/services/authService'

  import { rolePermissions } from '@/router'
  
  const route = useRoute()
  const router = useRouter()
  const userStore = useUserStore()
  
  
  const getCurrentMenu = computed(() => {
    switch (route.path) {
      case '/student':
      case '/student/add':
      case '/student/edit':
        return '学生管理'
      case '/class':
        return '班级管理'
      case '/course':
        return '课程管理'
      case '/course/selection':
        return '选课管理'
      case '/grade':
        return '成绩管理'
      case '/user':
        return '用户管理'
      case '/system/log':
        return '系统日志'
      default:
        return '首页'
    }
  })
  
  const handleCommand = async (command) => {
    switch (command) {
      case 'profile':
        handleViewProfile()
        break
      case 'password':
        changePasswordVisible.value = true
        break
      case 'logout':
        await handleLogout()
        break
    }
  }
  
  const handleLogout = async () => {
    try {
      await ElMessageBox.confirm(
        '确定要退出登录吗？',
        '提示',
        {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        }
      )
      
      try {
        await authService.logout()
      } catch (error) {
        console.error('退出登录请求失败:', error)
      }
      
      // 先清理用户状态
      userStore.clearUser()
      
      // 显示退出成功消息
      ElMessage.success('已退出登录')
      
      // 使用 window.location.href 强制跳转
      window.location.href = '/login'
    } catch (error) {
      if (error !== 'cancel') {
        console.error('退出登录失败:', error)
        ElMessage.error('退出登录失败')
      }
    }
  }
  
  // 添加修改密码相关的响应式变量
  const changePasswordVisible = ref(false)
  const passwordFormRef = ref(null)
  const submitting = ref(false)
  
  const passwordForm = reactive({
    newPassword: '',
    confirmPassword: ''
  })
  
  const passwordRules = {
    newPassword: [
      { required: true, message: '请输入新密码', trigger: 'blur' },
      { min: 6, message: '密码长度不能小于6位', trigger: 'blur' }
    ],
    confirmPassword: [
      { required: true, message: '请再次输入新密码', trigger: 'blur' },
      {
        validator: (rule, value, callback) => {
          if (value !== passwordForm.newPassword) {
            callback(new Error('两次输入的密码不一致'))
          } else {
            callback()
          }
        },
        trigger: 'blur'
      }
    ]
  }
  
  // 处理修改密码
  const handleChangePassword = async () => {
    if (!passwordFormRef.value) return
    
    try {
      await passwordFormRef.value.validate()
      submitting.value = true
      
      await authService.changePassword({
        username: userStore.user.username,
        newPassword: passwordForm.newPassword,
        confirmPassword: passwordForm.confirmPassword
      })
      
      ElMessage.success('密码修改成功')
      changePasswordVisible.value = false
      handleCloseDialog()
    } catch (error) {
      console.error('修改密码失败:', error)
      ElMessage.error(error.response?.data?.message || '修改密码失败')
    } finally {
      submitting.value = false
    }
  }
  
  // 关闭对话框时重置表单
  const handleCloseDialog = () => {
    passwordForm.newPassword = ''
    passwordForm.confirmPassword = ''
    if (passwordFormRef.value) {
      passwordFormRef.value.resetFields()
    }
  }
  
  // 个人信息相关
  const profileVisible = ref(false)
  const profileData = ref(null)
  const isStudent = computed(() => userStore.user?.role === 4) // 4 代表学生角色
  
  // 获取个人信息
  const handleViewProfile = async () => {
    try {
      const response = await authService.getProfile()
      profileData.value = response
      profileVisible.value = true
    } catch (error) {
      console.error('获取个人信息失败:', error)
      ElMessage.error('获取个人信息失败')
    }
  }
  
  // 辅助函数
  const formatDate = (date) => {
    if (!date) return '-'
    return new Date(date).toLocaleDateString('zh-CN')
  }
  
  const getStatusType = (status) => {
    const types = {
      'Active': 'success',
      'Suspended': 'warning',
      'Graduated': 'info',
      'Dropped': 'danger'
    }
    return types[status] || 'info'
  }
  
  const getStatusText = (status) => {
    const texts = {
      'Active': '在读',
      'Suspended': '休学',
      'Graduated': '毕业',
      'Dropped': '退学'
    }
    return texts[status] || status
  }
  
  const getRoleText = (role) => {
    const roles = {
      1: '超级管理员',
      2: '管理员',
      3: '教师',
      4: '学生'
    }
    return roles[role] || '未知'
  }
  
  // 根据用户角色获取菜单项
  const menuItems = computed(() => {
    const role = userStore.user?.role
    return rolePermissions[role]?.menuItems || []
  })
  
  // 添加 getIcon 方法
  const getIcon = (iconName) => {
    const iconMap = {
      'User': User,
      'School': School,
      'Reading': Reading,
      'List': List,
      'Document': Document,
      'UserFilled': UserFilled
    }
    return iconMap[iconName] || Document // 默认返回 Document 图标
  }
  </script>
  
  <style>
  * {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
  }
  
  html, body, #app {
    height: 100%;
    width: 100%;
    overflow: hidden;
  }
  
  /* MainLayout 独立的样式变量 */
  :root {
    --main-bg-color: #f0f2f5;
    --menu-bg-color: #304156;
    --menu-text-color: #49d0d2;
    --menu-active-color: #ffffff;
    --menu-hover-bg: #263445;
    --header-bg-color: #ffffff;
    --header-border-color: #e6e6e6;
    --header-shadow: rgba(0,21,41,.08);
    --text-color: #606266;
  }
  
  .app-container {
    height: 100%;
    width: 100%;
    overflow: hidden;
    position: fixed;
    top: 0;
    left: 0;
  }
  
  .layout-container {
    height: 100%;
    width: 100%;
    display: flex;
  }
  
  .aside {
    background-color: #304156;
    height: 100%;
    overflow-y: auto;
    overflow-x: hidden;
    flex-shrink: 0;
    transition: all 0.3s;
    box-shadow: 2px 0 6px rgba(0,21,41,.35);
  }
  
  .logo {
    height: 60px;
    line-height: 60px;
    text-align: center;
    background-color: #2b2f3a;
    transition: background-color 0.3s;
  }
  
  .logo h2 {
    margin: 0;
    color: #66b1ff;
    font-size: 18px;
    font-weight: 600;
  }
  
  .menu {
    border-right: none;
    user-select: none;
  }
  
  :deep(.el-menu) {
    border-right: none !important;
  }
  
  :deep(.el-menu-item) {
    height: 56px;
    line-height: 56px;
    color: #66b1ff !important;
    font-size: 14px;
    padding-left: 20px !important;
    margin: 4px 0;
    transition: all 0.3s;
  }
  
  :deep(.el-menu-item:hover) {
    background-color: #263445 !important;
    color: #a6d1ff !important;
    padding-left: 25px !important;
  }
  
  :deep(.el-menu-item.is-active) {
    background-color: #263445 !important;
    color: #a6d1ff !important;
    font-weight: bold;
  }
  
  :deep(.el-menu-item .el-icon) {
    color: #66b1ff !important;
    font-size: 18px;
    margin-right: 12px;
    vertical-align: middle;
  }
  
  :deep(.el-menu-item:hover .el-icon) {
    color: #a6d1ff !important;
  }
  
  :deep(.el-menu-item.is-active .el-icon) {
    color: #a6d1ff !important;
  }
  
  :deep(.el-menu-item span) {
    margin-left: 4px;
    vertical-align: middle;
  }
  
  /* 滚动条样式 */
  .aside::-webkit-scrollbar {
    width: 6px;
  }
  
  .aside::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.2);
    border-radius: 3px;
  }
  
  .aside::-webkit-scrollbar-track {
    background: transparent;
  }
  
  .header {
    background-color: #fff;
    border-bottom: 1px solid #e6e6e6;
    box-shadow: 0 1px 4px rgba(0,21,41,.08);
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0 20px;
    height: 60px;
  }
  
  .main-container {
    height: calc(100vh - 60px);
    padding: 0;
    overflow: hidden;
    background-color: #f0f2f5;
    flex: 1;
  }
  
  .header-right {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    height: 100%;
  }
  
  .user-info {
    cursor: pointer;
    display: flex;
    align-items: center;
    color: #606266;
  }
  
  
  /* 修改密码对话框样式 */
  .dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
  }
  
  /* 个人信息样式 */
  .profile-info {
    padding: 20px;
  }
  
  :deep(.el-descriptions__label) {
    width: 120px;
    justify-content: flex-end;
  }
  </style>
  