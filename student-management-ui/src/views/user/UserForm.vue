<template>
  <div class="user-form">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <h2>{{ isEdit ? '编辑用户' : '添加用户' }}</h2>
        </div>
      </template>

      <el-form 
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="120px"
        class="form-content"
      >
        <el-form-item label="用户名" prop="Username">
          <el-input 
            v-model="form.Username" 
            :disabled="isEdit && (form.Role === 1 || form.Role === 4)"
          />
        </el-form-item>

        <el-form-item 
          label="密码" 
          prop="Password"
          v-if="!isEdit"
        >
          <el-input v-model="form.Password" type="password" show-password />
        </el-form-item>

        <el-form-item label="姓名" prop="Name">
          <el-input v-model="form.Name" />
        </el-form-item>

        <el-form-item label="角色" prop="Role">
          <el-select 
            v-model="form.Role" 
            placeholder="请选择角色"
            :disabled="form.Role === 1"
          >
            <el-option 
              v-if="form.Role === 1" 
              label="超级管理员" 
              :value="1" 
            />
            <el-option 
              v-if="form.Role !== 1" 
              label="管理员" 
              :value="2" 
            />
            <el-option 
              v-if="form.Role !== 1" 
              label="教师" 
              :value="3" 
            />
            <el-option 
              v-if="!isEdit && form.Role !== 1" 
              label="学生" 
              :value="4" 
            />
          </el-select>
        </el-form-item>

        <el-form-item 
          label="学号" 
          prop="StudentNumber"
          v-if="form.Role === 4 && !isEdit"
        >
          <el-input v-model="form.StudentNumber" placeholder="请输入学号" />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="submitForm">{{ isEdit ? '保存' : '创建' }}</el-button>
          <el-button @click="goBack">返回</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import userService from '@/services/userService'

const route = useRoute()
const router = useRouter()
const formRef = ref(null)
const isEdit = ref(false)

const form = ref({
  Username: '',
  Password: '',
  Name: '',
  StudentNumber: '',
  Role: ''
})

const rules = {
  Username: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 20, message: '长度在 3 到 20 个字符', trigger: 'blur' }
  ],
  Password: [
    { required: !isEdit.value, message: '请输入密码', trigger: 'blur' },
    { min: 6, max: 20, message: '长度在 6 到 20 个字符', trigger: 'blur' }
  ],
  Name: [
    { required: true, message: '请输入姓名', trigger: 'blur' }
  ],
  StudentNumber: [
    { 
      required: form.value.Role === 4, 
      message: '学生用户必须输入学号', 
      trigger: ['blur', 'change'] 
    }
  ],
  Role: [
    { required: true, message: '请选择角色', trigger: 'change' }
  ]
}

const submitForm = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid) => {
    if (valid) {
      try {
        const data = {
          Username: form.value.Username,
          Password: form.value.Password,
          Name: form.value.Name,
          StudentNumber: form.value.Role === 4 ? form.value.StudentNumber : '',
          Role: form.value.Role
        }

        if (isEdit.value) {
          delete data.Password;
          delete data.StudentNumber;
          await userService.updateUser(route.params.id, data)
          ElMessage.success('用户更新成功')
        } else {
          await userService.createUser(data)
          ElMessage.success('用户创建成功')
        }
        router.push('/user')
      } catch (error) {
        console.error(isEdit.value ? '更新用户失败:' : '创建用户失败:', error)
        let errorMessage = error.response?.data?.message || (isEdit.value ? '更新用户失败' : '创建用户失败')
        ElMessage.error(errorMessage)
      }
    }
  })
}

const loadUser = async (id) => {
  try {
    const user = await userService.getUserById(id)
    if (user.Role === 4) {
      ElMessage.warning('学生信息需要在学生管理模块中修改')
      router.push('/user')
      return
    }
    form.value = {
      Username: user.Username,
      Name: user.Name,
      Role: user.Role
    }
  } catch (error) {
    console.error('加载用户数据失败:', error)
    ElMessage.error('加载用户数据失败')
    router.push('/user')
  }
}

const goBack = () => {
  router.push('/user')
}

onMounted(async () => {
  const id = route.params.id
  if (id) {
    isEdit.value = true
    await loadUser(id)
  }
})

watch(() => form.value.Role, (newRole) => {
  if (newRole !== 4) {
    form.value.StudentNumber = '';
  }
  if (formRef.value) {
    formRef.value.validateField('StudentNumber');
  }
})
</script>

<style scoped>
.user-form {
  padding: 20px;
}

.form-content {
  max-width: 600px;
  margin: 0 auto;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style> 