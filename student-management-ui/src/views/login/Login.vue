<template>
  <div class="login-wrapper" :class="{ 'dark': isDark }">
    <!-- 背景动画 -->
    <div class="bg-animation">
      <svg height="1337" width="1337" class="bg-blob-1">
        <defs>
          <path id="path-1" opacity="1" fill-rule="evenodd" d="M1337,668.5 C1337,1037.455193874239 1037.455193874239,1337 668.5,1337 C523.6725684305388,1337 337,1236 370.50000000000006,1094 C434.03835568300906,824.6732385973953 6.906089672974592e-14,892.6277623047779 0,668.5000000000001 C0,299.5448061257611 299.5448061257609,1.1368683772161603e-13 668.4999999999999,0 C1037.455193874239,0 1337,299.544806125761 1337,668.5Z"></path>
          <linearGradient id="linearGradient-2" x1="0.79" y1="0.62" x2="0.21" y2="0.86">
            <stop offset="0" stop-opacity="1" stop-color="#28aff0"></stop>
            <stop offset="1" stop-opacity="1" stop-color="#120fc4"></stop>
          </linearGradient>
        </defs>
        <g opacity="1">
          <use xlink:href="#path-1" fill="url(#linearGradient-2)" fill-opacity="1"></use>
        </g>
      </svg>
      <svg class="bg-blob-2" height="896" width="967">
        <defs>
          <path id="path-2" opacity="1" fill-rule="evenodd" d="M896,448 C1142.6325445712241,465.5747656464056 695.2579309733121,896 448,896 C200.74206902668806,896 5.684341886080802e-14,695.2579309733121 0,448.0000000000001 C0,200.74206902668806 200.74206902668791,5.684341886080802e-14 447.99999999999994,0 C695.2579309733121,0 475,418 896,448Z"></path>
          <linearGradient id="linearGradient-3" x1="0.5" y1="0" x2="0.5" y2="1">
            <stop offset="0" stop-opacity="1" stop-color="#28aff0"></stop>
            <stop offset="1" stop-opacity="1" stop-color="#120fc4"></stop>
          </linearGradient>
        </defs>
        <g opacity="1">
          <use xlink:href="#path-2" fill="url(#linearGradient-3)" fill-opacity="1"></use>
        </g>
      </svg>
    </div>

    <!-- 登录框 -->
    <div class="login-container">
      <div class="login-box">
        <!-- Logo -->
        <div class="login-header">
          <div class="logo">
            <svg class="logo-icon" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 3L1 9L12 15L21 10.09V17H23V9M5 13.18V17.18L12 21L19 17.18V13.18L12 17L5 13.18Z" 
                    fill="currentColor"/>
            </svg>
            <span class="logo-text">学生管理系统</span>
          </div>
        </div>

        <!-- 登录表单 -->
        <el-form 
          ref="loginFormRef"
          :model="formData"
          :rules="rules" 
          class="login-form"
        >
          <el-form-item prop="username">
            <el-input
              v-model="formData.username"
              placeholder="请输入用户名"
              :prefix-icon="User"
              class="custom-input"
              clearable
            />
          </el-form-item>
          
          <el-form-item prop="password">
            <el-input
              v-model="formData.password"
              type="password"
              placeholder="请输入密码"
              show-password
              :prefix-icon="Lock"
              class="custom-input"
              @keyup.enter="handleLogin"
            />
          </el-form-item>

          <div class="form-options">
            <el-checkbox v-model="rememberMe">记住账号</el-checkbox>
            <el-button link type="primary" @click="handleForgotPassword">
              忘记密码?
            </el-button>
          </div>

          <div class="form-actions">
            <el-button class="clear-btn" @click="clearForm">清除</el-button>
            <el-button type="primary" class="login-btn" :loading="loading" @click="handleLogin">
              登录
            </el-button>
          </div>
        </el-form>

        <!-- 底部工具栏 -->
        <div class="login-footer">
          <el-button @click="toggleTheme">
            <el-icon>
              <svg v-if="!isDark" class="theme-icon" viewBox="0 0 24 24">
                <path d="M12 3c.132 0 .263 0 .393 0a7.5 7.5 0 0 0 7.92 12.446a9 9 0 1 1 -8.313 -12.454z" 
                      fill="currentColor"/>
              </svg>
              <svg v-else class="theme-icon" viewBox="0 0 24 24">
                <path d="M12 18a6 6 0 1 1 0-12 6 6 0 0 1 0 12zm0-2a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM11 1h2v3h-2V1zm0 19h2v3h-2v-3zM3.515 4.929l1.414-1.414L7.05 5.636 5.636 7.05 3.515 4.93zM16.95 18.364l1.414-1.414 2.121 2.121-1.414 1.414-2.121-2.121zm2.121-14.85l1.414 1.415-2.121 2.121-1.414-1.414 2.121-2.121zM5.636 16.95l1.414 1.414-2.121 2.121-1.414-1.414 2.121-2.121zM23 11v2h-3v-2h3zM4 11v2H1v-2h3z"
                      fill="currentColor"/>
              </svg>
            </el-icon>
          </el-button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'
import authService from '@/services/authService'
import { useUserStore } from '@/stores/user'
import { getLoginTheme, setLoginTheme } from '@/utils/theme'

const router = useRouter()
const userStore = useUserStore()
const loading = ref(false)
const rememberMe = ref(false)
const isDark = ref(getLoginTheme() === 'dark')
const loginFormRef = ref(null)

const formData = reactive({
  username: '',
  password: ''
})

const rules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' }
  ]
}

const handleLogin = async () => {
   // 1. 表单引用检查
  if (!loginFormRef.value) return
  
  try {
    // 2. 表单验证
    await loginFormRef.value.validate()

    // 3. 设置加载状态
    loading.value = true
    
    const response = await authService.login({
      username: formData.username.trim(),
      password: formData.password
    })
    
    // 4. 处理响应
    if (response.user) {
      if (rememberMe.value) {
        localStorage.setItem('remembered-username', formData.username.trim())
      }
      userStore.setUser(response.user)
      ElMessage.success( response.message || '登录成功' )
      // 5. 导航到学生管理页面
      router.push('/student')
    }
  } catch (error) {
    // 5. 处理错误
    console.error('登录失败:', error)
    ElMessage.error(error.response?.data?.message || '登录失败')
  } finally {
    // 6. 重置加载状态
    loading.value = false
  }
}

const handleForgotPassword = () => {
  router.push('/forgot-password')
}

const clearForm = () => {
  formData.username = ''
  formData.password = ''
  rememberMe.value = false
  localStorage.removeItem('remembered-username')
  if (loginFormRef.value) {
    loginFormRef.value.resetFields()
  }
}

const toggleTheme = () => {
  isDark.value = !isDark.value
  setLoginTheme(isDark.value ? 'dark' : 'light')
}

onMounted(() => {
  const savedUsername = localStorage.getItem('remembered-username')
  if (savedUsername) {
    formData.username = savedUsername
    rememberMe.value = true
  }
})
</script>

<style scoped>
.login-wrapper {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  overflow: hidden;
  background: #f0f2f5;
  transition: all 0.3s ease-in-out;
}

.login-wrapper.dark {
  background: #062b74;
}

.bg-animation {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 0;
}

.bg-blob-1 {
  position: absolute;
  top: -20%;
  right: -20%;
  opacity: 0.8;
}

.bg-blob-2 {
  position: absolute;
  bottom: -20%;
  left: -20%;
  opacity: 0.8;
}

.login-container {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 420px;
  padding: 0 20px;
}

.login-box {
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  padding: 40px;
  transition: all 0.3s ease-in-out;
}

.dark .login-box {
  background: #1a1a1a;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
}

.login-header {
  text-align: center;
  margin-bottom: 40px;
}

.logo {
  display: inline-flex;
  align-items: center;
  gap: 12px;
}

.logo-icon {
  width: 32px;
  height: 32px;
  color: #2c3e50;
  transition: color 0.3s ease-in-out;
}

.dark .logo-icon {
  color: #ffffff;
}

.logo-text {
  font-size: 24px;
  color: #0091ff;
  font-weight: 500;
}

.dark .logo-text {
  color: #1890ff;
}

.custom-input :deep(.el-input__wrapper) {
  background-color: #f5f5f5;
  box-shadow: none !important;
  border: none;
  height: 40px;
  transition: all 0.3s ease-in-out;
  border-radius: 8px;
  padding: 0 12px;
}

.custom-input :deep(.el-input__inner) {
  height: 40px;
  line-height: 40px;
  background-color: transparent;
  color: #606266;
  -webkit-text-fill-color: #606266;
  opacity: 1;
}

.custom-input :deep(.el-input__prefix-icon) {
  color: #909399;
  font-size: 16px;
  line-height: 40px;
}

.dark .custom-input :deep(.el-input__wrapper) {
  background-color: #2a2a2a;
}

.dark .custom-input :deep(.el-input__inner) {
  color: #fff;
}

.custom-input :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px #0091ff !important;
}

.dark .custom-input :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px #1890ff !important;
}

.custom-input :deep(.el-input__wrapper:hover) {
  background-color: #f0f0f0;
}

.dark .custom-input :deep(.el-input__wrapper:hover) {
  background-color: #363636;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 20px 0;
  color: #606266;
}

.dark .form-options {
  color: #909399;
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

.clear-btn,
.login-btn {
  flex: 1;
  height: 40px;
}

.clear-btn {
  background-color: #f5f5f5;
  border: none;
  color: #606266;
}

.dark .clear-btn {
  background-color: #2a2a2a;
  color: #909399;
}

.login-btn {
  background-color: #0091ff;
  border: none;
}

.dark .login-btn {
  background-color: #1890ff;
}

.login-footer {
  margin-top: 40px;
  display: flex;
  justify-content: center;
}

.login-footer :deep(.el-button) {
  border: none;
  background: transparent;
  padding: 8px;
  color: #606266;
  border-radius: 50%;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease-in-out;
}

.dark .login-footer :deep(.el-button) {
  color: #ffffff;
}

.login-footer :deep(.el-button:hover) {
  background-color: rgba(0, 0, 0, 0.05);
}

.dark .login-footer :deep(.el-button:hover) {
  background-color: rgba(255, 255, 255, 0.1);
}

.theme-icon {
  width: 20px;
  height: 20px;
  color: inherit;
  transition: transform 0.3s;
}

/* 响应式调整 */
@media screen and (max-width: 480px) {
  .login-container {
    padding: 0 16px;
  }
  
  .login-box {
    padding: 30px 20px;
  }
}

:deep(.dark) {
  --el-bg-color: #1a1a1a;
  --el-text-color-primary: #ffffff;
  --el-border-color: #434343;
}

/* 添加全局过渡效果 */
* {
  transition: background-color 0.3s ease-in-out, 
              color 0.3s ease-in-out, 
              border-color 0.3s ease-in-out,
              box-shadow 0.3s ease-in-out;
}
</style>