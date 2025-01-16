<template>
  <div class="user-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <h2>用户管理</h2>
          <el-button type="primary" @click="handleAdd">添加用户</el-button>
        </div>
      </template>

      <!-- 搜索表单 -->
      <el-form :inline="true" :model="searchForm" class="search-form">
        <el-form-item label="关键字">
          <el-input
            v-model="searchForm.keyword"
            placeholder="用户名/姓名/学号/邮箱"
            clearable
            @keyup.enter="handleSearch"
          >
            <template #append>
              <el-button :icon="Search" @click="handleSearch" />
            </template>
          </el-input>
        </el-form-item>

        <el-form-item label="角色">
          <el-select 
            v-model="searchForm.role" 
            placeholder="选择角色" 
            clearable 
            @change="handleSearch"
            class="role-select"
          >
            <el-option label="超级管理员" value="1" />
            <el-option label="管理员" value="2" />
            <el-option label="教师" value="3" />
            <el-option label="学生" value="4" />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="resetSearch">重置</el-button>
        </el-form-item>
      </el-form>

      <!-- 用户列表 -->
      <el-table :data="users" border style="width: 100%" v-loading="loading">
        <el-table-column prop="Username" label="用户名" width="150" />
        <el-table-column prop="Name" label="姓名" width="120">
          <template #default="{ row }">
            {{ row.Name || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="StudentNumber" label="学号" width="120">
          <template #default="{ row }">
            {{ row.StudentNumber || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="Role" label="角色" width="120">
          <template #default="{ row }">
            <el-tag :type="getRoleTagType(row.Role)">
              {{ getRoleText(row.Role) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="Status" label="启用忘记密码" width="150">
          <template #default="{ row }">
            <el-switch
              v-model="row.Status"
              :active-value="1"
              :inactive-value="2"
              @change="handleStatusChange(row)"
            />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="250">
          <template #default="{ row }">
            <el-button-group>
              <el-button 
                type="primary" 
                @click="handleEdit(row)"
                :disabled="row.Role === 4"
                v-if="row.Role !== 4"
              >
                编辑
              </el-button>
              <el-button type="warning" @click="handleResetPassword(row)">重置密码</el-button>
              <el-button 
                type="danger" 
                @click="handleDelete(row)"
                :disabled="row.Role === 1"
              >
                删除
              </el-button>
            </el-button-group>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>

      <!-- 添加重置密码对话框 -->
      <reset-password-form
        v-model:visible="resetPasswordVisible"
        :userId="selectedUserId"
        @success="handleResetPasswordSuccess"
      />
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import userService from '@/services/userService'
import ResetPasswordForm from './ResetPasswordForm.vue'
import { Search } from '@element-plus/icons-vue'

const router = useRouter()
const loading = ref(false)
const users = ref([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

const searchForm = ref({
  keyword: '',
  role: ''
})

const resetPasswordVisible = ref(false)
const selectedUserId = ref(null)

// 获取用户列表
const fetchUsers = async () => {
  loading.value = true
  try {
    const params = {
      page: currentPage.value,
      pageSize: pageSize.value,
      keyword: searchForm.value.keyword?.trim(),
      role: searchForm.value.role
    }
    const response = await userService.getUsers(params)
    if (response && response.data) {
      users.value = response.data
      total.value = response.total
    } else {
      users.value = []
      total.value = 0
      ElMessage.warning('未获取到用户数据')
    }
  } catch (error) {
    console.error('获取用户列表失败:', error)
    ElMessage.error(error.response?.data?.message || '获取用户列表失败')
    users.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

// 搜索
const handleSearch = () => {
  currentPage.value = 1
  fetchUsers()
}

// 重置搜索
const resetSearch = () => {
  searchForm.value = {
    keyword: '',
    role: ''
  }
  currentPage.value = 1
  fetchUsers()
}

// 添加用户
const handleAdd = () => {
  router.push('/user/add')
}

// 编辑用户
const handleEdit = (row) => {
  if (row.Role === 4) {
    ElMessage.warning('学生信息需要在学生管理模块中修改')
    return
  }
  router.push(`/user/edit/${row.Id}`)
}

// 删除用户
const handleDelete = async (row) => {
  if (row.Role === 1) {
    ElMessage.warning('超级管理员账户不能删除')
    return
  }

  try {
    await ElMessageBox.confirm('确定要删除该用户吗？', '警告', {
      type: 'warning'
    })
    await userService.deleteUser(row.Id)
    ElMessage.success('删除成功')
    fetchUsers()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除用户失败:', error)
    }
  }
}

// 更新用户状态
const handleStatusChange = async (row) => {
  try {
    await userService.updateUserStatus(row.Id, row.Status)
    ElMessage.success('状态更新成功')
  } catch (error) {
    console.error('更新用户状态失败:', error)
    row.Status = row.Status === 1 ? 2 : 1
    ElMessage.error(error.response?.data?.message || '更新用户状态失败')
  }
}

// 重置密码
const handleResetPassword = (row) => {
  console.log('Reset password for user:', row)
  selectedUserId.value = row.Id
  resetPasswordVisible.value = true
}

const handleResetPasswordSuccess = () => {
  // 可以在这里添加刷新列表等操作
  ElMessage.success('密码已重置')
}

// 获取角色标签类型
const getRoleTagType = (role) => {
  const types = {
    1: 'danger',
    2: 'warning',
    3: 'success',
    4: 'info'
  }
  return types[role] || 'info'
}

// 获取角色显示文本
const getRoleText = (role) => {
  const texts = {
    1: '超级管理员',
    2: '管理员',
    3: '教师',
    4: '学生'
  }
  return texts[role] || '未知角色'
}

// 分页处理
const handleSizeChange = (val) => {
  pageSize.value = val
  fetchUsers()
}

const handleCurrentChange = (val) => {
  currentPage.value = val
  fetchUsers()
}

onMounted(() => {
  fetchUsers()
})
</script>

<style scoped>
.user-list {
  padding: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.search-form {
  margin-bottom: 20px;
  padding: 20px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.el-input {
  width: 300px;
}

/* 添加角色选择框的样式 */
.role-select {
  width: 200px !important;  /* 使用 !important 确保样式生效 */
}

/* 确保表单项之间有合适的间距 */
.el-form-item {
  margin-right: 20px;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}
</style> 