<template>
  <div class="course-form">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <span class="title">{{ isEdit ? '编辑课程' : '添加课程' }}</span>
          <el-button @click="router.back()">返回</el-button>
        </div>
      </template>

      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="100px"
        class="form"
      >
        <el-form-item label="课程代码" prop="code">
          <el-input v-model="form.code" placeholder="请输入课程代码" />
        </el-form-item>

        <el-form-item label="课程名称" prop="name">
          <el-input v-model="form.name" placeholder="请输入课程名称" />
        </el-form-item>

        <el-form-item label="学分" prop="credits">
          <el-input-number
            v-model="form.credits"
            :min="1"
            :max="10"
            :precision="0"
            style="width: 180px"
          />
        </el-form-item>

        <el-form-item label="先修课程" prop="prerequisiteCourseId">
          <el-select
            v-model="form.prerequisiteCourseId"
            placeholder="选择先修课程"
            clearable
            style="width: 100%"
          >
            <el-option
              v-for="course in courses"
              :key="course.id"
              :label="course.name"
              :value="course.id"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="课程描述" prop="description">
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="4"
            placeholder="请输入课程描述"
          />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :loading="loading" @click="handleSubmit">
            {{ isEdit ? '保存' : '创建' }}
          </el-button>
          <el-button @click="router.back()">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import request from '@/utils/request'

const router = useRouter()
const route = useRoute()
const formRef = ref(null)
const loading = ref(false)
const courses = ref([])

const isEdit = computed(() => route.params.id !== undefined)

const form = ref({
  code: '',
  name: '',
  credits: 1,
  description: '',
  prerequisiteCourseId: null
})

const rules = {
  code: [
    { required: true, message: '请输入课程代码', trigger: 'blur' },
    { min: 2, max: 20, message: '长度在 2 到 20 个字符', trigger: 'blur' }
  ],
  name: [
    { required: true, message: '请输入课程名称', trigger: 'blur' },
    { min: 2, max: 50, message: '长度在 2 到 50 个字符', trigger: 'blur' }
  ],
  credits: [
    { required: true, message: '请输入学分', trigger: 'blur' },
    { type: 'number', min: 1, max: 10, message: '学分必须在1-10之间', trigger: 'blur' }
  ],
  description: [
    { required: true, message: '请输入课程描述', trigger: 'blur' },
    { min: 10, max: 500, message: '长度在 10 到 500 个字符', trigger: 'blur' }
  ]
}

// 获取课程列表(用于选择先修课程)
const getCourses = async () => {
  try {
    const response = await request.get('/api/course')
    courses.value = Array.isArray(response) ? response : response.data || []
    
    // 如果是编辑模式，从列表中移除当前课程(避免自己选择自己作为先修课程)
    if (isEdit.value) {
      courses.value = courses.value.filter(c => c.id !== parseInt(route.params.id))
    }
  } catch (error) {
    console.error('获取课程列表失败:', error)
    ElMessage.error('获取课程列表失败')
  }
}

// 获取课程详情
const getCourse = async (id) => {
  try {
    loading.value = true
    const response = await request.get(`/api/course/${id}`)
    form.value = {
      ...response,
      prerequisiteCourseId: response.prerequisiteCourseId || null
    }
  } catch (error) {
    console.error('获取课程详情失败:', error)
    ElMessage.error('获取课程详情失败')
  } finally {
    loading.value = false
  }
}

// 提交表单
const handleSubmit = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid) => {
    if (valid) {
      try {
        loading.value = true
        
        // 构造提交的数据
        const submitData = {
          code: form.value.code,
          name: form.value.name,
          credits: form.value.credits,
          description: form.value.description,
          prerequisiteCourseId: form.value.prerequisiteCourseId || null
        }

        // 如果是编辑模式，添加 id
        if (isEdit.value) {
          submitData.id = parseInt(route.params.id)
          await request.put(`/api/course/${route.params.id}`, submitData)
          ElMessage.success('更新成功')
        } else {
          await request.post('/api/course', submitData)
          ElMessage.success('创建成功')
        }
        
        router.back()
      } catch (error) {
        console.error(isEdit.value ? '更新失败:' : '创建失败:', error)
        ElMessage.error(isEdit.value ? '更新失败' : '创建失败')
      } finally {
        loading.value = false
      }
    }
  })
}

onMounted(async () => {
  await getCourses()
  if (isEdit.value) {
    await getCourse(route.params.id)
  }
})
</script>

<style scoped>
.course-form {
  padding: 20px;
  height: 100%;
  overflow-y: auto;
  box-sizing: border-box;
}

.box-card {
  max-width: 800px;
  margin: 0 auto;
  height: auto;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.title {
  font-size: 18px;
  font-weight: bold;
}

.form {
  margin-top: 20px;
  padding-bottom: 20px;
}

:deep(.el-form-item) {
  margin-bottom: 22px;
}

:deep(.el-input-number) {
  width: 180px;
}

:deep(.el-textarea__inner) {
  min-height: 120px;
}
</style> 