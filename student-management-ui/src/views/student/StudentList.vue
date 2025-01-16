<template>
  <div class="student-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <h2>学生管理</h2>
          <div class="buttons">
            <el-button type="primary" @click="handleAdd">添加学生</el-button>
            <el-upload
              class="upload-demo"
              action=""
              :auto-upload="false"
              :show-file-list="false"
              :on-change="handleImport"
              accept=".xlsx,.xls"
            >
              <el-button type="success">导入</el-button>
            </el-upload>
            <el-button type="warning" @click="handleExport">导出</el-button>
            <el-button 
              type="danger" 
              @click="handleBatchDelete"
              :disabled="selectedStudents.length === 0"
            >
              批量删除
            </el-button>
            <el-button type="info" @click="handleDownloadTemplate">下载模板</el-button>
          </div>
        </div>
      </template>

      <!-- 搜索表单 -->
      <el-form :inline="true" :model="searchForm" class="search-form">
        <el-form-item label="关键字">
          <el-input
            v-model="searchForm.keyword"
            placeholder="学号/姓名"
            clearable
            @keyup.enter="handleSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </el-form-item>

        <el-form-item label="班级">
          <el-select v-model="searchForm.classId" placeholder="选择班级" clearable>
            <el-option
              v-for="item in classes"
              :key="item.Id"
              :label="formatClassInfo(item)"
              :value="item.Id"
            >
              <span>{{ formatClassInfo(item) }}</span>
            </el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="性别">
          <el-select v-model="searchForm.gender" placeholder="选择性别" clearable>
            <el-option label="男" value="男" />
            <el-option label="女" value="女" />
          </el-select>
        </el-form-item>

        <el-form-item label="状态">
          <el-select v-model="searchForm.status" placeholder="选择状态" clearable>
            <el-option label="在读" value="Normal" />
            <el-option label="休学" value="Suspended" />
            <el-option label="毕业" value="Graduated" />
            <el-option label="退学" value="Dropped" />
          </el-select>
        </el-form-item>

        <!-- <el-form-item label="入学日期">
          <el-date-picker
            v-model="searchForm.dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
          />
        </el-form-item> -->

        <el-form-item>
          <el-button type="primary" :loading="loading" @click="handleSearch">搜索</el-button>
          <el-button :loading="loading" @click="resetSearch">重置</el-button>
        </el-form-item>
      </el-form>

      <el-table
        v-loading="loading"
        :data="students"
        style="width: 100%"
        border
        stripe
        @selection-change="handleSelectionChange"
        @empty="handleEmpty"
      >
        <template #empty>
          <el-empty description="暂无数据" />
        </template>
        
        <el-table-column type="selection" width="55" />
        <el-table-column prop="StudentNumber" label="学号" width="120" sortable fixed />
        <el-table-column prop="Name" label="姓名" width="100" />
        <el-table-column label="班级" width="150">
          <template #default="scope">
            <el-tag size="small" type="info" v-if="scope.row.Class">
              {{ formatClassInfo(scope.row.Class) }}
            </el-tag>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column prop="Gender" label="性别" width="60">
          <template #default="scope">
            <el-tag :type="getGenderTagType(scope.row.Gender)" size="small">
              {{ scope.row.Gender }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="DateOfBirth" label="出生日期" width="120">
          <template #default="scope">
            {{ formatDate(scope.row.DateOfBirth) }}
          </template>
        </el-table-column>
        <el-table-column prop="ContactNumber" label="联系电话" width="120" />
        <el-table-column prop="Email" label="邮箱" min-width="180" show-overflow-tooltip />
        <el-table-column prop="Status" label="状态" width="100">
          <template #default="scope">
            <el-tag :type="getStatusType(scope.row.Status)" size="small">
              {{ getStatusText(scope.row.Status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="250" fixed="right">
          <template #default="scope">
            <el-button-group>
              <el-button size="small" type="primary" :icon="Edit" @click="handleEdit(scope.row)">
                编辑
              </el-button>
              <el-button size="small" type="success" :icon="List" @click="handleViewGrades(scope.row)">
                成绩
              </el-button>
              <el-button size="small" type="danger" :icon="Delete" @click="handleDelete(scope.row)">
                删除
              </el-button>
            </el-button-group>
          </template>
        </el-table-column>
      </el-table>

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
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, onUpdated } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Upload, Download, Search, Edit, Delete, List } from '@element-plus/icons-vue'
import { useRouter } from 'vue-router'
import request from '@/utils/request'
import { formatClassInfo } from '@/utils/format'

const router = useRouter()
const students = ref([])
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)
const selectedStudents = ref([])
const loading = ref(false)
const searchForm = ref({
  keyword: '',  // 关键字搜索（学号、姓名）
  classId: undefined,  // 班级筛选
  gender: '',  // 性别筛选
  status: '',  // 状态筛选
  dateRange: []  // 入学日期范围
})
const classes = ref([])  // 班级列表

// 获取班级列表
const getClasses = async () => {
  try {
    const response = await request.get('/api/class')
    console.log('获取到的班级列表:', response)
    if (response?.data?.data) {
      classes.value = response.data.data
    } else if (Array.isArray(response?.data)) {
      classes.value = response.data
    } else if (Array.isArray(response)) {
      classes.value = response
    } else {
      classes.value = []
    }
  } catch (error) {
    console.error('获取班级列表失败:', error)
    ElMessage.error('获取班级列表失败')
    classes.value = []
  }
}

// 获取学生列表
const fetchStudents = async () => {
  try {
    loading.value = true
    const params = {
      pageIndex: currentPage.value - 1,
      pageSize: pageSize.value,
      keyword: searchForm.value.keyword || '',
      classId: searchForm.value.classId,
      gender: searchForm.value.gender || '',
      status: searchForm.value.status || '',
      startDate: searchForm.value.dateRange?.[0] || '',
      endDate: searchForm.value.dateRange?.[1] || ''
    }

    // 移空值参数
    Object.keys(params).forEach(key => {
      if (params[key] === '' || params[key] === null || params[key] === undefined) {
        delete params[key]
      }
    })

    console.log('搜索参数:', params)
    const response = await request.get('/api/student', { params })
    console.log('获取到的学生列表:', response)
    
    // 直接使用返回的数据
    if (response) {
      students.value = Array.isArray(response) ? response : []
      total.value = students.value.length
      
      // 添加调试日志
      console.log('处理后的数据:', {
        studentsLength: students.value.length,
        firstStudent: students.value[0],
        isArray: Array.isArray(response),
        responseType: typeof response
      })
    } else {
      students.value = []
      total.value = 0
    }
  } catch (error) {
    console.error('获取学生列表失败:', error)
    ElMessage.error('获取学生列表失败')
    students.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

// 添加监听器以便调试
watch(() => students.value, (newVal) => {
  console.log('students数据变化:', {
    length: newVal.length,
    data: newVal,
    isEmpty: newVal.length === 0
  })
}, { deep: true })

// 添加学生
const handleAdd = () => {
  router.push('/student/add')
}

// 导入学生
const handleImport = async (file) => {
  try {
    const formData = new FormData()
    if (!file || !file.raw) {
      ElMessage.error('请选择文件')
      return
    }
    formData.append('file', file.raw, file.raw.name)
    await request.post('/api/student/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
        'Accept': 'application/json'
      }
    })
    ElMessage.success('导入成功')
    fetchStudents()
  } catch (error) {
    const errorMessage = error.response?.data || '导入失败'
    console.error('导入失败:', errorMessage)
    ElMessage.error(errorMessage)
  }
}

// 导出学生
const handleExport = async () => {
  try {
    const response = await request.get('/api/student/export', {
      responseType: 'blob'
    })
    const url = window.URL.createObjectURL(new Blob([response]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', '学生信息表.xlsx')
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('导出失败:', error)
    ElMessage.error('导出失败')
  }
}

// 搜索
const handleSearch = async () => {
  try {
    currentPage.value = 1
    await fetchStudents()
  } catch (error) {
    console.error('搜索失败:', error)
    ElMessage.error('搜索失败')
  }
}

// 重置搜索
const resetSearch = async () => {
  try {
    searchForm.value = {
      keyword: '',
      classId: undefined,
      gender: '',
      status: '',
      dateRange: []
    }
    currentPage.value = 1
    await fetchStudents()
  } catch (error) {
    console.error('重置搜索失败:', error)
    ElMessage.error('重置搜索失败')
  }
}

// 编辑学生
const handleEdit = (row) => {
  router.push(`/student/edit/${row.Id}`)
}

// 查看成绩
const handleViewGrades = (row) => {
  router.push(`/student/${row.Id}/grades`)
}

// 删除学生
const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm('确定要删除该学生吗？此操作不可恢复！', '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    
    await request.delete(`/api/student/${row.Id}`)
    ElMessage.success('删除成功')
    if (students.value.length === 1 && currentPage.value > 1) {
      currentPage.value--
    }
    fetchStudents()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除失败:', error)
      ElMessage.error('删除失败')
    }
  }
}

// 批量删除
const handleBatchDelete = async () => {
  if (selectedStudents.value.length === 0) {
    ElMessage.warning('请选择要删除的学生')
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定要删除选中的 ${selectedStudents.value.length} 名学生吗？此操作不可恢复！`,
      '警告',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )

    // 使用 Promise.all 并行处理所有删除请求
    await Promise.all(
      selectedStudents.value.map(student => 
        request.delete(`/api/student/${student.Id}`)
      )
    )

    ElMessage.success('批量删除成功')
    
    // 如果当前页的所有数据都被删除了，且不是第一页，则跳转到上一页
    if (students.value.length === selectedStudents.value.length && currentPage.value > 1) {
      currentPage.value--
    }
    
    // 重新加载数据
    await fetchStudents()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('批量删除失败:', error)
      ElMessage.error('批量删除失败')
    }
  }
}

// 选择变化
const handleSelectionChange = (selection) => {
  selectedStudents.value = selection
}

// 页码变化
const handleCurrentChange = (val) => {
  currentPage.value = val
  fetchStudents()
}

// 每页数量变化
const handleSizeChange = (val) => {
  pageSize.value = val
  currentPage.value = 1
  fetchStudents()
}

// 格式化日期
const formatDate = (date) => {
  if (!date) return ''
  const d = new Date(date)
  if (isNaN(d.getTime())) return ''
  return d.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

// 获取状态类型
const getStatusType = (status) => {
  const types = {
    'Normal': 'success',
    'Suspended': 'warning',
    'Graduated': 'info',
    'Repeated': 'danger'
  }
  return types[status] || 'info'
}

// 获取状态文本
const getStatusText = (status) => {
  const texts = {
    'Normal': '在读',
    'Suspended': '休学',
    'Graduated': '毕业',
    'Dropped': '退学',
    'Transferred': '转学'
  }
  return texts[status] || status
}

// 修改性别标签的显示
const getGenderTagType = (gender) => {
  return gender === '男' ? 'primary' : 'success'
}

// 添加下载模板方法
const handleDownloadTemplate = async () => {
  try {
    const response = await request.get('/api/student/template', {
      responseType: 'blob'
    })
    const url = window.URL.createObjectURL(new Blob([response]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', '学生信息导入模板.xlsx')
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('下载模板失败:', error)
    ElMessage.error('下载模板失败')
  }
}

onMounted(() => {
  console.log('组件挂载')
  getClasses()
  fetchStudents()
})

// 监听students变化
watch(students, (newVal) => {
  console.log('students数据变化:', newVal)
})

// 监听整个组件的生命周期
onUpdated(() => {
  console.log('组件更新:', {
    studentsLength: students.value.length,
    loading: loading.value,
    currentPage: currentPage.value,
    pageSize: pageSize.value
  })
})
</script>

<style scoped>
.student-list {
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

:deep(.el-date-editor) {
  width: 240px;
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