<template>
  <div class="course-selection">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <h2>选课管理</h2>
          <div class="buttons">
            <el-button type="primary" @click="handleAdd">选课</el-button>
          </div>
        </div>
      </template>

      <!-- 搜索表单 -->
      <el-form :inline="true" :model="searchForm" class="search-form">
        <el-form-item label="学生" v-if="userStore.user.role !== 4">
          <el-input
            v-model="searchForm.studentKeyword"
            placeholder="输入学号或姓名搜索"
            clearable
          />
        </el-form-item>
        <el-form-item label="课程">
          <el-input
            v-model="searchForm.courseKeyword"
            placeholder="输入课程名称搜索"
            clearable
          />
        </el-form-item>
        <el-form-item label="学期">
          <el-select v-model="searchForm.semester" placeholder="选择学期" clearable>
            <el-option
              v-for="item in semesterOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="resetSearch">重置</el-button>
        </el-form-item>
      </el-form>

      <!-- 选课表格 -->
      <el-table v-loading="loading" :data="courseSelections" border style="width: 100%">
        <el-table-column 
          prop="Student.StudentNumber" 
          label="学号" 
          width="120"
          sortable="custom"
        />
        <el-table-column 
          prop="Student.Name" 
          label="姓名" 
          width="100"
          sortable="custom"
        />
        <el-table-column 
          prop="Course.Code" 
          label="课程代码" 
          width="120"
          sortable="custom"
        />
        <el-table-column 
          prop="Course.Name" 
          label="课程名称" 
          width="180"
          sortable="custom"
        />
        <el-table-column 
          prop="Semester" 
          label="学期" 
          width="100"
          sortable="custom"
        >
          <template #default="scope">
            {{ scope.row.Semester === '1' ? '第一学期' : '第二学期' }}
          </template>
        </el-table-column>
        <el-table-column 
          prop="Year" 
          label="学年" 
          width="100"
          sortable="custom"
        />
        <el-table-column 
          prop="SelectionDate" 
          label="选课时间" 
          width="180"
          sortable="custom"
        >
          <template #default="scope">
            {{ new Date(scope.row.SelectionDate).toLocaleString() }}
          </template>
        </el-table-column>
        <el-table-column prop="Status" label="状态" width="100">
          <template #default="scope">
            <el-tag :type="getStatusType(scope.row.Status)">
              {{ getStatusText(scope.row.Status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="scope">
            <el-button-group>
              <el-button
                v-if="scope.row.Status === 'Selected'"
                type="danger"
                size="small"
                @click="handleDrop(scope.row)"
              >
                退课
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

      <!-- 选课对话框 -->
      <el-dialog
        v-model="dialogVisible"
        :title="dialogType === 'add' ? '新增选课' : '编辑选课'"
        width="500px"
      >
        <el-form :model="form" :rules="rules" ref="formRef" label-width="100px">
          <el-form-item label="学生" prop="studentId" v-if="userStore.user.role !== 4">
            <el-select
              v-model="form.studentId"
              filterable
              remote
              :remote-method="searchStudents"
              :loading="studentSearchLoading"
              placeholder="请输入学号搜索"
              style="width: 100%"
            >
              <template #value-template="{ item }">
                <div class="student-info" v-if="item">
                  <span class="student-name">{{ item.name }}</span>
                  <el-tag size="small" class="student-number">{{ item.studentNumber }}</el-tag>
                </div>
              </template>
              
              <el-option
                v-for="item in studentOptions"
                :key="item.id"
                :label="item.name"
                :value="item.id"
              >
                <div class="student-info">
                  <span class="student-name">{{ item.name }}</span>
                  <el-tag size="small" class="student-number">{{ item.studentNumber }}</el-tag>
                </div>
              </el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="课程" prop="courseId">
            <el-select
              v-model="form.courseId"
              placeholder="输入课程名称或代码搜索"
              filterable
              remote
              :remote-method="searchCourse"
              :loading="courseSearchLoading"
              style="width: 100%"
            >
              <el-option
                v-for="item in courses"
                :key="item.Id"
                :label="`${item.Name} (${item.Code})`"
                :value="item.Id"
              >
                <div class="course-info">
                  <span class="course-name">{{ item.Name }}</span>
                  <el-tag size="small" class="course-code">{{ item.Code }}</el-tag>
                </div>
              </el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="学期" prop="semester">
            <el-select v-model="form.semester" placeholder="选择学期">
              <el-option label="第一学期" value="1" />
              <el-option label="第二学期" value="2" />
            </el-select>
          </el-form-item>
          <el-form-item label="学年" prop="year">
            <el-select
              v-model="form.year"
              placeholder="选择学年"
              style="width: 120px"
            >
              <el-option
                v-for="year in availableYears"
                :key="year"
                :label="`${year}年`"
                :value="year"
              />
            </el-select>
          </el-form-item>
        </el-form>
        <template #footer>
          <span class="dialog-footer">
            <el-button @click="dialogVisible = false">取消</el-button>
            <el-button type="primary" @click="handleSubmit" :loading="submitting">
              确定
            </el-button>
          </span>
        </template>
      </el-dialog>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Refresh } from '@element-plus/icons-vue'
import request from '@/utils/request'
import { debounce } from 'lodash-es'
import { formatClassInfo } from '@/utils/format'
import { useUserStore } from '@/stores/user'

// 添加类型定义
interface UserStore {
  user: {
    role: number;
    studentNumber?: string;
  }
}

interface SearchFormType {
  studentKeyword: string | null;
  courseKeyword: string | null;
  semester: string | null;
  year: number | null;
  status: string | null;
  sortBy: string;
  sortOrder: string;
  studentId?: number | null;
  courseId?: number | null;
}

// 定义 semester options
const semesterOptions = [
  { value: '1', label: '第一学期' },
  { value: '2', label: '第二学期' }
]

const userStore = useUserStore() as unknown as UserStore
const dialogType = ref('add')

// 其他变量定义保持不变
const loading = ref(false)
const studentSearchLoading = ref(false)
const courseSearchLoading = ref(false)
const submitting = ref(false)
const dialogVisible = ref(false)
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)
const formRef = ref(null)

const students = ref<any[]>([])
const courses = ref([])
const courseSelections = ref([])
const searchCache = ref(new Map())

// 修改 searchForm 的类型
const searchForm = ref<SearchFormType>({
  studentKeyword: null,
  courseKeyword: null,
  semester: null,
  year: null,
  status: null,
  sortBy: 'SelectionDate',
  sortOrder: 'desc',
  studentId: null,
  courseId: null
})

const form = ref({
  studentId: null,
  courseId: null,
  semester: '1',
  year: new Date().getFullYear()
})

const rules = {
  studentId: [
    { required: true, message: '请选择学生', trigger: 'change' }
  ],
  courseId: [
    { required: true, message: '请选择课程', trigger: 'change' }
  ],
  semester: [
    { required: true, message: '请选择学期', trigger: 'change' }
  ],
  year: [
    { required: true, message: '请输入学年', trigger: 'blur' }
  ]
}

const getStatusType = (status) => {
  switch (status) {
    case 'Selected':
      return 'primary'
    case 'Completed':
      return 'success'
    case 'Dropped':
      return 'danger'
    default:
      return 'info'
  }
}

const getStatusText = (status) => {
  switch (status) {
    case 'Selected':
      return '已选'
    case 'Completed':
      return '已完成'
    case 'Dropped':
      return '已退课'
    default:
      return '未知'
  }
}

// 添加一个变量存储当前学生信息
const currentStudent = ref(null)

// 修改初始化函数
const initCurrentStudent = async () => {
  try {
    console.log('开始获取学生信息 GET /api/auth/profile')
    const response = await request.get('/api/auth/profile')
    console.log('API响应:', response)
    
    if (response?.id) {
      currentStudentId.value = response.id
      // 保存完整的学生信息
      currentStudent.value = response
      console.log('已保存学生信息:', currentStudent.value)
    } else {
      console.error('响应中没有找到学生信息:', response)
    }
  } catch (error) {
    console.error('获取学生信息失败:', error)
    ElMessage.error('获取学生信息失败')
  }
}

const loadData = async () => {
  try {
    loading.value = true
    let response;

    // 根据用户角色使用不同的API
    if (userStore.user.role === 4 && currentStudentId.value) {
      // 学生用户使用 student/{studentId} 接口
      console.log('学生用户获取选课记录:', currentStudentId.value)
      response = await request.get(`/api/course-selection/student/${currentStudentId.value}`, {
        params: {
          courseName: searchForm.value.courseKeyword || undefined,
          semester: searchForm.value.semester || undefined,
          status: searchForm.value.status || undefined
        }
      })
    } else {
      // 管理员用户使用原有接口
      const params = {
        Page: currentPage.value,
        PageSize: pageSize.value,
        StudentKeyword: searchForm.value.studentKeyword || undefined,
        CourseKeyword: searchForm.value.courseKeyword || undefined,
        Semester: searchForm.value.semester || undefined,
        Year: searchForm.value.year || undefined,
        Status: searchForm.value.status || undefined
      }

      // 移除未定义的参数
      Object.keys(params).forEach(key => {
        if (params[key] === undefined || params[key] === null) {
          delete params[key]
        }
      })
      
      response = await request.get('/api/course-selection', { params })
    }

    console.log('API响应原始数据:', response)

    // 处理响应数据
    if (response?.data?.data) {
      let sortedData = [...response.data.data]
      // 如果是学生用户，补充学生信息
      if (userStore.user.role === 4 && currentStudent.value) {
        sortedData = sortedData.map(record => ({
          ...record,
          Student: {
            Id: currentStudent.value.id,
            StudentNumber: currentStudent.value.studentNumber,
            Name: currentStudent.value.name,
            // 其他学生信息...
          }
        }))
      }
      if (searchForm.value.sortBy) {
        sortedData = sortData(sortedData, searchForm.value.sortBy, searchForm.value.sortOrder)
      }
      courseSelections.value = sortedData
      total.value = response.data.total || response.data.data.length
    } else if (Array.isArray(response?.data)) {
      let sortedData = [...response.data]
      // 如果是学生用户，补充学生信息
      if (userStore.user.role === 4 && currentStudent.value) {
        sortedData = sortedData.map(record => ({
          ...record,
          Student: {
            Id: currentStudent.value.id,
            StudentNumber: currentStudent.value.studentNumber,
            Name: currentStudent.value.name,
            // 其他学生信息...
          }
        }))
      }
      if (searchForm.value.sortBy) {
        sortedData = sortData(sortedData, searchForm.value.sortBy, searchForm.value.sortOrder)
      }
      courseSelections.value = sortedData
      total.value = response.data.length
    } else if (Array.isArray(response)) {
      let sortedData = [...response]
      // 如果是学生用户，补充学生信息
      if (userStore.user.role === 4 && currentStudent.value) {
        sortedData = sortedData.map(record => ({
          ...record,
          Student: {
            Id: currentStudent.value.id,
            StudentNumber: currentStudent.value.studentNumber,
            Name: currentStudent.value.name,
            // 其他学生信息...
          }
        }))
      }
      if (searchForm.value.sortBy) {
        sortedData = sortData(sortedData, searchForm.value.sortBy, searchForm.value.sortOrder)
      }
      courseSelections.value = sortedData
      total.value = response.length
    } else {
      courseSelections.value = []
      total.value = 0
    }
  } catch (error) {
    console.error('加载选课列表失败:', error)
    ElMessage.error('获取选课列表失败')
    courseSelections.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

const getCacheKey = (type, query) => `${type}-${query}`

// 生成可选学年列表（前5年到后2年）
const availableYears = computed(() => {
  const currentYear = new Date().getFullYear()
  const years = []
  for (let i = -5; i <= 2; i++) {
    years.push(currentYear + i)
  }
  return years.sort((a, b) => b - a)
})

// 添加 studentOptions 计算属性
const studentOptions = computed(() => {
  return students.value.map(student => ({
    id: student.Id,
    studentNumber: student.StudentNumber,
    name: student.Name
  }))
})

// 修改搜索学生的函数，添加调试信息
const searchStudents = async (query: string) => {
  if (!query) {
    students.value = []
    console.log('搜索条件为空，清空学生列表')
    return
  }

  try {
    studentSearchLoading.value = true
    console.log('开始搜索学生，关键词:', query)
    
    const response = await request.get('/api/student', {
      params: {
        keyword: query,
        pageSize: 10
      }
    })
    
    console.log('后端返回的原始数据:', response)

    if (response?.data?.data) {
      students.value = response.data.data
    } else if (Array.isArray(response?.data)) {
      students.value = response.data
    } else if (Array.isArray(response)) {
      students.value = response
    } else {
      students.value = []
    }

    console.log('处理后的学生数据:', students.value)
    console.log('转换后的选项数据:', studentOptions.value)

  } catch (error) {
    console.error('搜索学生失败:', error)
    console.error('错误详情:', {
      message: error.message,
      response: error.response?.data,
      status: error.response?.status
    })
    ElMessage.error('搜索学生失败')
    students.value = []
  } finally {
    studentSearchLoading.value = false
  }
}

// 添加存储当前学生ID的变量
const currentStudentId = ref(null)

// 修改搜索课程的函数
const searchCourse = async (query) => {
  if (!query) {
    courses.value = []
    return
  }
  
  try {
    courseSearchLoading.value = true
    let response;
    
    if (userStore.user.role === 4) {
      // 学生用户使用 available API
      if (!currentStudentId.value) {
        console.error('未找到学生ID')
        return
      }
      
      const url = `/api/course/available/${currentStudentId.value}`
      console.log(`请求可选课程 GET ${url}, 关键词:`, query)
      
      response = await request.get(url, {
        params: { keyword: query }
      })
      console.log('可选课程响应:', response)
      
      // 提取课程数据
      let availableCourses = []
      if (Array.isArray(response)) {
        availableCourses = response
      } else if (response?.data) {
        availableCourses = response.data
      }
      
      // 转换为下拉菜单选项格式
      courses.value = availableCourses.map(course => ({
        Id: course.Id,
        Name: course.Name,
        Code: course.Code,
        Credits: course.Credits,
        Description: course.Description
      }))
      
      console.log('处理后的课程选项:', courses.value)
    } else {
      // 非学生账户使用原有的搜索逻辑
      console.log('管理员搜索课程 GET /api/course, 关键词:', query)
      response = await request.get('/api/course', {
        params: { keyword: query }
      })
      
      if (Array.isArray(response)) {
        courses.value = response.map(course => ({
          Id: course.Id || course.id,
          Name: course.Name || course.name,
          Code: course.Code || course.code,
          Credits: course.Credits || course.credits,
          Description: course.Description || course.description,
          PrerequisiteCourseId: course.PrerequisiteCourseId || course.prerequisiteCourseId
        }))
      } else if (response?.data) {
        courses.value = response.data.map(course => ({
          Id: course.Id || course.id,
          Name: course.Name || course.name,
          Code: course.Code || course.code,
          Credits: course.Credits || course.credits,
          Description: course.Description || course.description,
          PrerequisiteCourseId: course.PrerequisiteCourseId || course.prerequisiteCourseId
        }))
      } else {
        courses.value = []
      }
    }
  } catch (error) {
    console.error('搜索课程失败:', error)
    ElMessage.error('搜索课程失败')
    courses.value = []
  } finally {
    courseSearchLoading.value = false
  }
}

const handleSearch = () => {
  currentPage.value = 1
  loadData()
}

const resetSearch = () => {
  searchForm.value = {
    studentKeyword: null,
    courseKeyword: null,
    semester: null,
    year: null,
    status: null,
    sortBy: 'SelectionDate',
    sortOrder: 'desc',
    studentId: null,
    courseId: null
  }
  handleSearch()
}

const handleAdd = () => {
  form.value = {
    studentId: null,
    courseId: null,
    semester: '1',
    year: new Date().getFullYear()
  }
  dialogVisible.value = true
}

const handleSubmit = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid) => {
    if (valid) {
      try {
        submitting.value = true
        console.log('开始提交选课，表单数据:', form.value)
        
        let selectedStudent = null
        if (userStore.user.role === 4) {
          // 使用已存储的学生ID
          console.log('当前用户是学生，使用存储的ID:', currentStudentId.value)
          selectedStudent = { Id: currentStudentId.value }
        } else {
          // 非学生用户需要选择学生
          console.log('非学生用户，从选择的学生中获取ID:', form.value.studentId)
          selectedStudent = students.value.find(s => s.Id === form.value.studentId)
        }
        console.log('选中的学生信息:', selectedStudent)
        
        const selectedCourse = courses.value.find(c => c.Id === form.value.courseId)
        console.log('选中的课程信息:', selectedCourse)
        
        if (!selectedCourse || !selectedStudent) {
          console.error('验证失败:', {
            hasCourse: !!selectedCourse,
            hasStudent: !!selectedStudent,
            courseId: form.value.courseId,
            studentId: selectedStudent?.Id,
            currentStudentId: currentStudentId.value,
            formStudentId: form.value.studentId,
            availableCourses: courses.value
          })
          ElMessage.error(userStore.user.role === 4 ? '请选择有效的课程' : '请选择有效的学生和课程')
          return
        }
        
        const data = {
          StudentId: selectedStudent.Id,
          CourseId: selectedCourse.Id,
          Semester: form.value.semester,
          Year: form.value.year,
          Status: 'Selected'
        }
        
        console.log('准备发送的选课数据:', data)
        
        const response = await request.post('/api/course-selection', data)
        console.log('选课响应数据:', response)
        
        ElMessage.success('选课成功')
        dialogVisible.value = false
        loadData()
      } catch (error) {
        
        console.error('错误详情:', {
          status: error.response?.status,
          statusText: error.response?.statusText,
          data: error.response?.data,
          headers: error.response?.headers,
          config: error.config,
          formData: form.value,
          selectedStudentId: currentStudentId.value
        })
        
        if (error.response?.data?.message) {
         
        } else if (error.response?.data?.errors) {
          const errors = Object.values(error.response.data.errors).flat()
          ElMessage.error(`选课失败: ${errors.join(', ')}`)
        } else {
          ElMessage.error('选课失败，请检查输入数据是否正确')
        }
      } finally {
        submitting.value = false
      }
    }
  })
}

const handleDrop = async (row) => {
  try {
    await ElMessageBox.confirm('确定要退选该课程吗？', '提示', {
      type: 'warning'
    })
    
    console.log('退课请求:', row.Id)
    
    const response = await request.delete(`/api/course-selection/${row.Id}`)
    console.log('退课响应数据:', response.data)
    
    ElMessage.success('退课成功')
    loadData()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('退课失败:', error)
      console.error('错误详情:', {
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data
      })
      ElMessage.error(error.response?.data?.message || '退失败')
    }
  }
}

const handleSizeChange = (val) => {
  pageSize.value = val
  loadData()
}

const handleCurrentChange = (val) => {
  currentPage.value = val
  loadData()
}

const handleSortChange = ({ prop, order }) => {
  if (!prop) {
    searchForm.value.sortBy = 'SelectionDate'
    searchForm.value.sortOrder = 'desc'
  } else {
    const sortFieldMap = {
      'Student.StudentNumber': 'StudentNumber',
      'Student.Name': 'StudentName',
      'Course.Code': 'CourseCode',
      'Course.Name': 'CourseName',
      'Semester': 'Semester',
      'Year': 'Year',
      'SelectionDate': 'SelectionDate'
    }

    searchForm.value.sortBy = sortFieldMap[prop] || prop
    searchForm.value.sortOrder = order === 'ascending' ? 'asc' : 'desc'
  }
  
  // 直接对当前数据进行排序，不需要重新请求
  courseSelections.value = sortData(
    courseSelections.value,
    searchForm.value.sortBy,
    searchForm.value.sortOrder
  )
}

// 添加前端排序函数
const sortData = (data: any[], sortBy: string, sortOrder: string): any[] => {
  const sortFunctions = {
    'StudentNumber': (a: any, b: any) => a.Student?.StudentNumber?.localeCompare(b.Student?.StudentNumber),
    'StudentName': (a: any, b: any) => a.Student?.Name?.localeCompare(b.Student?.Name),
    'CourseCode': (a: any, b: any) => a.Course?.Code?.localeCompare(b.Course?.Code),
    'CourseName': (a: any, b: any) => a.Course?.Name?.localeCompare(b.Course?.Name),
    'Semester': (a: any, b: any) => a.Semester?.localeCompare(b.Semester),
    'Year': (a: any, b: any) => Number(a.Year) - Number(b.Year),
    'SelectionDate': (a: any, b: any) => new Date(a.SelectionDate).getTime() - new Date(b.SelectionDate).getTime()
  }

  const sortFunction = sortFunctions[sortBy]
  if (!sortFunction) return data

  const sortedData = [...data].sort((a, b) => {
    const result = sortFunction(a, b)
    return sortOrder === 'desc' ? -result : result
  })

  return sortedData
}

onMounted(async () => {
  if (userStore.user.role === 4) {
    console.log('学生用户，开始初始化')
    await initCurrentStudent()
    searchForm.value.studentKeyword = userStore.user.studentNumber
  }
  await handleSearch()
})
</script>

<style scoped>
.course-selection {
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

.student-info,
.course-info {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.student-name,
.course-name {
  font-size: 14px;
  color: #606266;
}

.student-number,
.course-code {
  margin-left: 8px;
  font-size: 12px;
}

.el-tag {
  margin-left: 12px;
}
</style> 