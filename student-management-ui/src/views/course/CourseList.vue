<template>
  <div class="course-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <h2>课程管理</h2>
          <div class="buttons">
            <el-button type="primary" @click="handleAdd">添加课程</el-button>
          </div>
        </div>
      </template>

      <!-- 搜索表单 -->
      <el-form :inline="true" :model="searchForm" class="search-form">
        <el-form-item>
          <el-input
            v-model="searchForm.keyword"
            placeholder="课程代码/名称"
            clearable
            @keyup.enter="handleSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :loading="loading" @click="handleSearch">搜索</el-button>
          <el-button :loading="loading" @click="resetSearch">重置</el-button>
        </el-form-item>
      </el-form>

      <!-- 课程列表 -->
      <el-table
        v-loading="loading"
        :data="courses"
        style="width: 100%"
        border
        stripe
      >
        <el-table-column prop="Code" label="课程代码" width="120" sortable fixed />
        <el-table-column prop="Name" label="课程名称" width="180" />
        <el-table-column prop="Credits" label="学分" width="80" sortable />
        <el-table-column prop="TeacherName" label="任课教师" width="120" />
        <el-table-column prop="Description" label="课程描述" min-width="200" show-overflow-tooltip />
        <el-table-column prop="PrerequisiteCourse" label="先修课程" width="180">
          <template #default="scope">
            <el-tag 
              size="small" 
              type="info" 
              v-if="getPrerequisiteCourseName(scope.row.PrerequisiteCourseId)"
            >
              {{ getPrerequisiteCourseName(scope.row.PrerequisiteCourseId) }}
            </el-tag>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="scope">
            <el-button-group>
              <el-button
                type="primary"
                :icon="Edit"
                size="small"
                @click="handleEdit(scope.row)"
              >
                编辑
              </el-button>
              <el-button
                type="danger"
                :icon="Delete"
                size="small"
                @click="handleDelete(scope.row)"
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

      <!-- 添加/编辑课程对话框 -->
      <el-dialog
        v-model="dialogVisible"
        :title="form.Id ? '编辑课程' : '添加课程'"
        width="500px"
      >
        <el-form
          ref="formRef"
          :model="form"
          :rules="rules"
          label-width="100px"
        >
          <el-form-item label="课程代码" prop="Code">
            <el-input v-model="form.Code" placeholder="请输入课程代码" />
          </el-form-item>
          <el-form-item label="课程名称" prop="Name">
            <el-input v-model="form.Name" placeholder="请输入课程名称" />
          </el-form-item>
          <el-form-item label="学分" prop="Credits">
            <el-input-number
              v-model="form.Credits"
              :min="1"
              :max="10"
              :step="1"
              :precision="0"
              style="width: 120px"
            />
          </el-form-item>
          <el-form-item label="任课教师" prop="TeacherName">
            <el-input v-model="form.TeacherName" placeholder="请输入任课教师姓名" />
          </el-form-item>
          <el-form-item label="课程描述" prop="Description">
            <el-input
              v-model="form.Description"
              type="textarea"
              :rows="3"
              placeholder="请输入课程描述"
            />
          </el-form-item>
          <el-form-item label="先修课程" prop="PrerequisiteCourseId">
            <el-select
              v-model="form.PrerequisiteCourseId"
              placeholder="选择先修课程"
              clearable
              filterable
              style="width: 100%"
              :disabled="!form.Id && prerequisiteCourses.length === 0"
            >
              <el-option
                v-for="item in availablePrerequisiteCourses"
                :key="item.Id"
                :label="item.Name"
                :value="item.Id"
              >
                <span>{{ item.Name }}</span>
                <span style="float: right; color: #8492a6; font-size: 13px">
                  {{ item.Code }}
                </span>
              </el-option>
            </el-select>
          </el-form-item>
        </el-form>
        <template #footer>
          <span class="dialog-footer">
            <el-button @click="dialogVisible = false">取消</el-button>
            <el-button type="primary" @click="handleSubmit">确定</el-button>
          </span>
        </template>
      </el-dialog>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Edit, Delete, Search } from '@element-plus/icons-vue'
import { useRouter } from 'vue-router'
import request from '@/utils/request'

const router = useRouter()
const courses = ref([])
const prerequisiteCourses = ref([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)
const loading = ref(false)
const dialogVisible = ref(false)
const formRef = ref(null)
const searchForm = ref({
  keyword: ''
})

const form = ref({
  Id: null,
  Code: '',
  Name: '',
  Credits: 1,
  TeacherName: '',
  Description: '',
  PrerequisiteCourseId: null
})

const rules = {
  Code: [
    { required: true, message: '请输入课程代码', trigger: 'blur' },
    { min: 2, max: 20, message: '长度在 2 到 20 个字符', trigger: 'blur' }
  ],
  Name: [
    { required: true, message: '请输入课程名称', trigger: 'blur' },
    { min: 2, max: 50, message: '长度在 2 到 50 个字符', trigger: 'blur' }
  ],
  Credits: [
    { required: true, message: '请输入学分', trigger: 'blur' },
    { type: 'number', min: 1, max: 10, message: '学分必须在 1 到 10 之间', trigger: 'blur' }
  ],
  TeacherName: [
    { required: true, message: '请输入任课教师姓名', trigger: 'blur' },
    { min: 2, max: 50, message: '长度在 2 到 50 个字符', trigger: 'blur' }
  ],
  Description: [
    { required: true, message: '请输入课程描述', trigger: 'blur' },
    { min: 10, max: 500, message: '长度在 10 到 500 个字符', trigger: 'blur' }
  ]
}

// 计算可选的先修课程列表
const availablePrerequisiteCourses = computed(() => {
  // 如果是新增课程，返回所有课程
  if (!form.value.Id) {
    return prerequisiteCourses.value
  }
  
  // 如果是编辑课程，需要过滤掉：
  // 1. 当前课程��身
  // 2. 以当前课程为先修课程的所有课程（防止循环依赖）
  return prerequisiteCourses.value.filter(course => {
    // 排除当前课程自身
    if (course.Id === form.value.Id) {
      return false
    }
    
    // 排除以当前课程为先修课程的课程
    const isPrerequisiteForCurrent = courses.value.some(c => 
      c.PrerequisiteCourseId === form.value.Id && c.Id === course.Id
    )
    
    return !isPrerequisiteForCurrent
  })
})

// 添加获取先修课程名称的方法
const getPrerequisiteCourseName = (prerequisiteCourseId) => {
  if (!prerequisiteCourseId) return null
  const course = prerequisiteCourses.value.find(c => c.Id === prerequisiteCourseId)
  return course ? course.Name : null
}

// 获取课程列表
const fetchCourses = async () => {
  try {
    loading.value = true
    const params = {
      pageIndex: currentPage.value - 1,
      pageSize: pageSize.value,
      keyword: searchForm.value.keyword || ''
    }

    // 移除空值参数
    Object.keys(params).forEach(key => {
      if (params[key] === '' || params[key] === null || params[key] === undefined) {
        delete params[key]
      }
    })

    console.log('搜索参数:', params)
    const response = await request.get('/api/course', { params })
    console.log('获取到的课程列表:', response)
    
    if (response?.data?.data) {
      courses.value = response.data.data
      total.value = response.data.total || response.data.data.length
      // 确保先修课程列表已加载
      if (prerequisiteCourses.value.length === 0) {
        await fetchPrerequisiteCourses()
      }
    } else if (Array.isArray(response?.data)) {
      courses.value = response.data
      total.value = response.data.length
    } else if (Array.isArray(response)) {
      courses.value = response
      total.value = response.length
    } else {
      courses.value = []
      total.value = 0
    }
  } catch (error) {
    console.error('获取课程列表失败:', error)
    ElMessage.error('获取课程列表失败')
    courses.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

// 获取所有课程（用于选择先修课程）
const fetchPrerequisiteCourses = async () => {
  try {
    const response = await request.get('/api/course')
    if (response?.data?.data) {
      prerequisiteCourses.value = response.data.data
    } else if (Array.isArray(response?.data)) {
      prerequisiteCourses.value = response.data
    } else if (Array.isArray(response)) {
      prerequisiteCourses.value = response
    } else {
      prerequisiteCourses.value = []
    }
  } catch (error) {
    console.error('获取课程列表失败:', error)
    ElMessage.error('获取课程列表失败')
    prerequisiteCourses.value = []
  }
}

// 搜索
const handleSearch = async () => {
  currentPage.value = 1
  await fetchCourses()
}

// 重置搜索
const resetSearch = async () => {
  searchForm.value.keyword = ''
  currentPage.value = 1
  await fetchCourses()
}

// 页码变化
const handleCurrentChange = (val) => {
  currentPage.value = val
  fetchCourses()
}

// 每页数量变化
const handleSizeChange = (val) => {
  pageSize.value = val
  currentPage.value = 1
  fetchCourses()
}

// 添加课程
const handleAdd = () => {
  form.value = {
    Id: null,
    Code: '',
    Name: '',
    Credits: 1,
    TeacherName: '',
    Description: '',
    PrerequisiteCourseId: null
  }
  dialogVisible.value = true
}

// 获取课程的所有先修课程链
const getPrerequisiteChain = (courseId, chain = new Set()) => {
  const course = courses.value.find(c => c.Id === courseId)
  if (!course || !course.PrerequisiteCourseId || chain.has(courseId)) {
    return chain
  }
  
  chain.add(courseId)
  return getPrerequisiteChain(course.PrerequisiteCourseId, chain)
}

// 检查是否存在循环依赖
const checkCircularDependency = (courseId, newPrerequisiteId) => {
  // 获取新先修课程的所有先修课程链
  const prerequisiteChain = getPrerequisiteChain(newPrerequisiteId)
  
  // 如果当前课程在其先修课程链中，说明存在循环
  if (prerequisiteChain.has(courseId)) {
    return true
  }
  
  // 检查是否有其他课程依赖当前课程，并且这些课程在新先修课程的依赖链中
  const dependentCourses = courses.value.filter(c => c.PrerequisiteCourseId === courseId)
  for (const dependentCourse of dependentCourses) {
    if (prerequisiteChain.has(dependentCourse.Id)) {
      return true
    }
  }
  
  return false
}

// 编辑课程
const handleEdit = (row) => {
  form.value = {
    Id: row.Id,
    Code: row.Code,
    Name: row.Name,
    Credits: row.Credits,
    TeacherName: row.TeacherName,
    Description: row.Description,
    PrerequisiteCourseId: row.PrerequisiteCourseId
  }
  
  // 检查是否存在循环依赖
  if (form.value.PrerequisiteCourseId) {
    const hasCircularDependency = checkCircularDependency(
      form.value.Id, 
      form.value.PrerequisiteCourseId
    )
    if (hasCircularDependency) {
      form.value.PrerequisiteCourseId = null
      ElMessage.warning('检测到循环依赖关系，已清除先修课程设置。请确保课程依赖关系不形成闭环。')
    }
  }
  
  dialogVisible.value = true
}

// 删除课程
const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm('确定要删除该课程吗？此操作不可恢复！', '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    
    await request.delete(`/api/course/${row.Id}`)
    ElMessage.success('删除成功')
    if (courses.value.length === 1 && currentPage.value > 1) {
      currentPage.value--
    }
    fetchCourses()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除失败:', error)
      ElMessage.error('删除失败')
    }
  }
}

// 提交表单
const handleSubmit = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid) => {
    if (valid) {
      // 检查是否存在循环依赖
      if (form.value.PrerequisiteCourseId) {
        const hasCircularDependency = checkCircularDependency(
          form.value.Id, 
          form.value.PrerequisiteCourseId
        )
        if (hasCircularDependency) {
          ElMessage.error('检测到循环依赖关系，请重新选择先修课程')
          return
        }
      }
      
      try {
        const data = {
          code: form.value.Code,
          name: form.value.Name,
          credits: Math.round(Number(form.value.Credits)),
          teacherName: form.value.TeacherName,
          description: form.value.Description,
          prerequisiteCourseId: form.value.PrerequisiteCourseId
        }
        
        console.log('提交的数据:', data)
        
        if (form.value.Id) {
          data.id = form.value.Id
          await request.put(`/api/course/${form.value.Id}`, data)
          ElMessage.success('更新成功')
        } else {
          const response = await request.post('/api/course', data)
          console.log('服务器响应:', response)
          ElMessage.success('添加成功')
        }
        
        dialogVisible.value = false
        fetchCourses()
      } catch (error) {
        console.error('保存失败:', error)
        console.error('错误响应:', error.response?.data)
        const errorMessage = error.response?.data?.message || 
                           error.response?.data?.title || 
                           error.response?.data?.errors?.join(', ') ||
                           '保存失败'
        ElMessage.error(errorMessage)
      }
    }
  })
}

onMounted(() => {
  fetchCourses()
  fetchPrerequisiteCourses()
})
</script>

<style scoped>
.course-list {
  padding: 20px;
  height: calc(100vh - 84px);
  overflow-y: auto;
}

.box-card {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.buttons {
  display: flex;
  gap: 10px;
}

.search-form {
  margin-bottom: 20px;
  padding: 20px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

:deep(.el-form--inline .el-form-item) {
  margin-right: 16px;
  margin-bottom: 16px;
}

:deep(.el-select) {
  width: 160px;
}

:deep(.el-input) {
  width: 200px;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

:deep(.el-table .cell) {
  white-space: nowrap;
}
</style> 