<template>
  <div class="student-form-container">
    <el-card class="box-card" v-loading="loading">
      <template #header>
        <div class="card-header">
          <span>{{ route.params.id ? '编辑学生' : '添加学生' }}</span>
         
        </div>
      </template>

      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="120px"
        class="student-form"
      >
        <el-form-item label="学号" prop="studentNumber">
          <el-input v-model="form.studentNumber" placeholder="请输入学号" />
        </el-form-item>

        <el-form-item label="姓名" prop="name">
          <el-input v-model="form.name" placeholder="请输入姓名" />
        </el-form-item>

        <el-form-item label="性别" prop="gender">
          <el-select v-model="form.gender" placeholder="请选择性别">
            <el-option label="男" :value="1" />
            <el-option label="女" :value="2" />
          </el-select>
        </el-form-item>

        <el-form-item label="出生日期" prop="dateOfBirth">
          <el-date-picker
            v-model="form.dateOfBirth"
            type="date"
            placeholder="选择出生日期"
            format="YYYY-MM-DD"
            value-format="YYYY-MM-DD"
          />
        </el-form-item>

        <el-form-item label="邮箱" prop="email">
          <el-input v-model="form.email" placeholder="请输入邮箱" />
        </el-form-item>

        <el-form-item label="电话" prop="phone">
          <el-input v-model="form.phone" placeholder="请输入电话" />
        </el-form-item>

        <el-form-item label="地址" prop="address">
          <el-input v-model="form.address" placeholder="请输入地址" />
        </el-form-item>

        <el-form-item label="班级" prop="classId">
          <el-select v-model="form.classId" placeholder="请选择班级">
            <el-option
              v-for="item in classList"
              :key="item.Id"
              :label="formatClassInfo(item)"
              :value="item.Id"
            >
              <span>{{ formatClassInfo(item) }}</span>
            </el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="状态" prop="status">
          <el-select v-model="form.status" placeholder="请选择状态">
            <el-option label="在读" value="Normal" />
            <el-option label="休学" value="Suspended" />
            <el-option label="毕业" value="Graduated" />
            <el-option label="退学" value="Dropped" />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="submitForm">保存</el-button>
          <el-button @click="goBack">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import request from '@/utils/request'
import { formatClassInfo } from '@/utils/format'

const route = useRoute()
const router = useRouter()
const formRef = ref(null)
const classList = ref([])
const loading = ref(false)

const form = ref({
  studentNumber: '',
  name: '',
  gender: undefined,
  email: '',
  phone: '',
  classId: undefined,
  dateOfBirth: '',
  address: '',
  status: 'Normal'
})

const rules = {
  studentNumber: [
    { required: true, message: '请输入学号', trigger: 'blur' },
    { min: 3, max: 20, message: '长度在 3 到 20 个字符', trigger: 'blur' }
  ],
  name: [
    { required: true, message: '请输入姓名', trigger: 'blur' },
    { min: 2, max: 20, message: '长度在 2 到 20 个字符', trigger: 'blur' }
  ],
  gender: [
    { required: true, message: '请选择性别', trigger: 'change' }
  ],
  email: [
    { required: true, message: '请输入邮箱', trigger: 'blur' },
    { type: 'email', message: '请输入正确的邮箱地址', trigger: 'blur' }
  ],
  phone: [
    { required: true, message: '请输入电话', trigger: 'blur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号码', trigger: 'blur' }
  ],
  classId: [
    { required: true, message: '请选择班级', trigger: 'change' }
  ],
  dateOfBirth: [
    { required: true, message: '请选择出生日期', trigger: 'change' }
  ],
  status: [
    { required: true, message: '请选择状态', trigger: 'change' }
  ]
}

// 获取班级列表
const getClassList = async () => {
  try {
    const response = await request.get('/api/class')
    if (response?.data?.data) {
      classList.value = response.data.data
    } else if (Array.isArray(response?.data)) {
      classList.value = response.data
    } else if (Array.isArray(response)) {
      classList.value = response
    } else {
      classList.value = []
    }
  } catch (error) {
    console.error('获取班级列表失败:', error)
    ElMessage.error('获取班级列表失败')
    classList.value = []
  }
}

// 获取学生详情
const getStudent = async (id) => {
  try {
    loading.value = true
    const response = await request.get(`/api/student/${id}`)
    if (response?.data) {
      const studentData = response.data
      form.value = {
        studentNumber: studentData.StudentNumber || '',
        name: studentData.Name || '',
        gender: studentData.Gender === '男' ? 1 : 2,
        email: studentData.Email || '',
        phone: studentData.ContactNumber || '',
        classId: studentData.ClassId,
        dateOfBirth: studentData.DateOfBirth ? new Date(studentData.DateOfBirth).toISOString().split('T')[0] : '',
        address: studentData.Address || '',
        status: studentData.Status || 'Normal'
      }
    } else if (response) {
      const studentData = response
      form.value = {
        studentNumber: studentData.StudentNumber || '',
        name: studentData.Name || '',
        gender: studentData.Gender === '男' ? 1 : 2,
        email: studentData.Email || '',
        phone: studentData.ContactNumber || '',
        classId: studentData.ClassId,
        dateOfBirth: studentData.DateOfBirth ? new Date(studentData.DateOfBirth).toISOString().split('T')[0] : '',
        address: studentData.Address || '',
        status: studentData.Status || 'Normal'
      }
    }
  } catch (error) {
    console.error('获取学生详情失败:', error)
    ElMessage.error('获取学生详情失败')
  } finally {
    loading.value = false
  }
}

// 提交表单
const submitForm = async () => {
  if (!formRef.value) return

  try {
    await formRef.value.validate()
    
    const id = route.params.id
    const selectedClass = classList.value.find(c => c.Id === form.value.classId)
    if (!selectedClass) {
      ElMessage.error('请选择有效的班级')
      return
    }
    
    const submitData = {
      Id: id ? parseInt(id) : 0,
      StudentNumber: form.value.studentNumber,
      Name: form.value.name,
      Gender: form.value.gender === 1 ? '男' : '女',
      Email: form.value.email,
      ContactNumber: form.value.phone,
      ClassId: selectedClass.Id,
      DateOfBirth: form.value.dateOfBirth,
      Address: form.value.address || '',
      Status: form.value.status,
      EnrollmentDate: new Date().toISOString()
    }
    
    console.log('提交的数据:', submitData)
    
    try {
      if (id) {
        const response = await request.put(`/api/student/${id}`, submitData)
        console.log('Update response:', response)
        if (response?.data || response) {
          ElMessage.success('更新成功')
          goBack()
        } else {
          throw new Error(response?.message || '更新失败')
        }
      } else {
        const response = await request.post('/api/student', submitData)
        console.log('Create response:', response)
        if (response?.data || response) {
          ElMessage.success('添加成功')
          goBack()
        } else {
          throw new Error('添加失败，请稍后重试')
        }
      }
    } catch (error) {
      console.error('保存失败:', error)
      let errorMessage = '保存失败，请检查输入数据'
      if (error.response?.data) {
        errorMessage = error.response.data.message || 
                      error.response.data.error || 
                      error.response.data.details ||
                      errorMessage
      } else if (error.message) {
        errorMessage = error.message
      }
      ElMessage.error(errorMessage)
    }
  } catch (error) {
    console.error('表单验证失败:', error)
    return
  }
}

const goBack = () => {
  router.push('/student')
}

// 在组件挂载时获取数据
onMounted(async () => {
  try {
    // 先获取班级列表
    await getClassList()
    
    // 如果是编辑模式，获取学生信息
    const id = route.params.id
    if (id) {
      await getStudent(id)
    }
  } catch (error) {
    console.error('初始化失败:', error)
    ElMessage.error('初始化失败，请刷新页面重试')
  }
})
</script>

<style scoped>
.student-form-container {
  padding: 20px;
  height: 100%;
  overflow-y: auto;
}

/* 美化滚动条样式 */
.student-form-container::-webkit-scrollbar {
  width: 6px;
}

.student-form-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.student-form-container::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 3px;
}

.student-form-container::-webkit-scrollbar-thumb:hover {
  background: #555;
}

.box-card {
  margin: 0 auto;
  max-width: 800px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.student-form {
  margin-top: 20px;
}

:deep(.el-select) {
  width: 100%;
}
</style> 