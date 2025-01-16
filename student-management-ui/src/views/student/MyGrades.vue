<template>
  <div class="my-grades">
    <div class="grades-content">
      <el-card class="box-card">
        <template #header>
          <div class="card-header">
            <h2>我的成绩单</h2>
            <div class="student-info">
              <el-descriptions :column="2" border>
                <el-descriptions-item label="学号">{{ studentInfo.studentNumber }}</el-descriptions-item>
                <el-descriptions-item label="姓名">{{ studentInfo.name }}</el-descriptions-item>
                <el-descriptions-item label="班级">{{ studentInfo.className }}</el-descriptions-item>
                <el-descriptions-item label="年级">{{ studentInfo.grade }}</el-descriptions-item>
              </el-descriptions>
            </div>
          </div>
        </template>

        <!-- 成绩筛选 -->
        <div class="filter-container">
          <el-form :inline="true" :model="filterForm">
            <el-form-item label="学期">
              <el-select 
                v-model="filterForm.semester" 
                placeholder="选择学期" 
                clearable
                style="width: 200px;"
              >
                <el-option
                  v-for="item in semesterOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                />
              </el-select>
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="handleFilter">查询</el-button>
              <el-button @click="resetFilter">重置</el-button>
            </el-form-item>
          </el-form>
        </div>

        <!-- 成绩统计 -->
        <div class="statistics-container">
          <el-row :gutter="20">
            <el-col :span="6">
              <el-card shadow="hover">
                <template #header>平均分</template>
                <div class="statistic-value">{{ statistics.averageScore.toFixed(1) }}</div>
              </el-card>
            </el-col>
            <el-col :span="6">
              <el-card shadow="hover">
                <template #header>最高分</template>
                <div class="statistic-value">{{ statistics.highestScore }}</div>
              </el-card>
            </el-col>
            <el-col :span="6">
              <el-card shadow="hover">
                <template #header>最低分</template>
                <div class="statistic-value">{{ statistics.lowestScore }}</div>
              </el-card>
            </el-col>
            <el-col :span="6">
              <el-card shadow="hover">
                <template #header>及格率</template>
                <div class="statistic-value">{{ (statistics.passRate * 100).toFixed(1) }}%</div>
              </el-card>
            </el-col>
          </el-row>
        </div>

        <!-- 成绩表格 -->
        <el-table
          v-loading="loading"
          :data="grades"
          border
          style="width: 100%"
        >
          <el-table-column prop="courseName" label="课程名称" />
          <el-table-column prop="teacherName" label="任课教师" width="120" />
          <el-table-column prop="score" label="分数" width="100">
            <template #default="scope">
              <el-tag :type="getScoreTagType(scope.row.score)">
                {{ scope.row.score }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="gradeLevel" label="等级" width="100">
            <template #default="scope">
              <el-tag :type="getGradeLevelTagType(scope.row.score)">
                {{ getGradeLevel(scope.row.score) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="semester" label="学期" width="120" />
          <el-table-column prop="remark" label="备注" show-overflow-tooltip />
        </el-table>

        <!-- 导出/打印按钮 -->
        <div class="action-buttons">
          <el-button type="primary" @click="handleExport">导出成绩单</el-button>
          <el-button type="success" @click="handlePrint">打印成绩单</el-button>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useUserStore } from '@/stores/user'
import request from '@/utils/request'
import { ElMessage } from 'element-plus'

const userStore = useUserStore()
const loading = ref(false)
const grades = ref([])
const studentInfo = ref({})
const filterForm = ref({
  semester: ''
})

const statistics = ref({
  averageScore: 0,
  highestScore: 0,
  lowestScore: 0,
  passRate: 0
})

const semesterOptions = [
  { label: '第一学期', value: '1' },
  { label: '第二学期', value: '2' }
]

// 获取学生信息
const fetchStudentInfo = async () => {
  try {
    // 1. 获取当前用户信息
    const userProfile = await request.get('/api/auth/profile')
    console.log('1. 获取到的用户信息:', userProfile)

    if (!userProfile?.id) {
      throw new Error('未找到用户信息')
    }

    // 直接使用返回的信息设置学生信息
    studentInfo.value = {
      id: userProfile.id,
      studentNumber: userProfile.studentNumber,
      name: userProfile.name,
      className: userProfile.className || '-',
      grade: userProfile.grade || '-'
    }

    console.log('2. 设置的学生信息:', studentInfo.value)
  } catch (error) {
    console.error('获取学生信息失败:', error)
    console.error('错误详情:', {
      message: error.message,
      response: error.response?.data,
      status: error.response?.status
    })
    ElMessage.error(`获取学生信息失败: ${error.message}`)
  }
}

// 获取成绩列表
const fetchGrades = async () => {
  loading.value = true
  try {
    if (!studentInfo.value?.id) {
      throw new Error('未找到学生ID')
    }

    console.log('1. 开始获取成绩，学生ID:', studentInfo.value.id)
    
    const response = await request.get(`/api/student/${studentInfo.value.id}/grades`)
    console.log('2. 获取到的原始成绩数据:', response)

    let gradesData = []
    if (Array.isArray(response)) {
      gradesData = response
    } else if (Array.isArray(response?.data)) {
      gradesData = response.data
    } else if (response?.data?.data) {
      gradesData = response.data.data
    }
    console.log('3. 处理后的成绩数据:', gradesData)

    // 处理并过滤成绩数据
    const processedGrades = gradesData.map(grade => ({
      courseName: grade.courseName || grade.Course?.Name || '-',
      teacherName: grade.teacherName || grade.Course?.TeacherName || '-',
      score: grade.score || grade.Score || 0,
      semester: grade.semester || '-',
      remark: grade.remark || grade.Comments || ''
    }))

    // 根据学期过滤
    grades.value = filterForm.value.semester
      ? processedGrades.filter(grade => grade.semester === filterForm.value.semester)
      : processedGrades

    console.log('4. 过滤后的成绩数据:', grades.value)
    calculateStatistics()
  } catch (error) {
    console.error('获取成绩失败:', error)
    console.error('错误详情:', {
      message: error.message,
      response: error.response?.data,
      status: error.response?.status,
      studentInfo: studentInfo.value
    })
    ElMessage.error(`获取成绩失败: ${error.message}`)
    grades.value = []
  } finally {
    loading.value = false
  }
}

// 计算统计数据
const calculateStatistics = () => {
  if (grades.value.length === 0) {
    statistics.value = {
      averageScore: 0,
      highestScore: 0,
      lowestScore: 0,
      passRate: 0
    }
    return
  }

  const scores = grades.value.map(g => g.score)
  statistics.value = {
    averageScore: scores.reduce((a, b) => a + b, 0) / scores.length,
    highestScore: Math.max(...scores),
    lowestScore: Math.min(...scores),
    passRate: scores.filter(s => s >= 60).length / scores.length
  }
}

// 处理筛选
const handleFilter = async () => {
  loading.value = true
  try {
    // 重新获取所有成绩并过滤
    await fetchGrades()
    ElMessage.success('查询成功')
  } catch (error) {
    console.error('查询失败:', error)
    ElMessage.error('查询失败')
  } finally {
    loading.value = false
  }
}

const resetFilter = async () => {
  loading.value = true
  try {
    filterForm.value.semester = ''
    await fetchGrades()
    ElMessage.success('重置成功')
  } catch (error) {
    console.error('重置失败:', error)
    ElMessage.error('重置失败')
  } finally {
    loading.value = false
  }
}

// 标签类型处理函数
const getScoreTagType = (score) => {
  if (score >= 90) return 'success'
  if (score >= 60) return 'warning'
  return 'danger'
}

const getGradeLevel = (score) => {
  if (score >= 90) return 'A'
  if (score >= 80) return 'B'
  if (score >= 70) return 'C'
  if (score >= 60) return 'D'
  return 'F'
}

const getGradeLevelTagType = (score) => {
  if (score >= 90) return 'success'
  if (score >= 80) return 'info'
  if (score >= 70) return ''
  if (score >= 60) return 'warning'
  return 'danger'
}

// 导出成绩单
const handleExport = () => {
  // TODO: 实现导出功能
  ElMessage.info('导出功能开发中')
}

// 打印成绩单
const handlePrint = () => {
  window.print()
}

onMounted(async () => {
  await fetchStudentInfo() // 等待学生信息获取完成
  await fetchGrades()     // 再获取成绩信息
})
</script>

<style scoped>
.my-grades {
  padding: 20px;
  height: 100%;
  overflow: hidden;
}

.grades-content {
  height: calc(100vh - 84px);  /* 减去顶部导航栏的高度 */
  overflow-y: auto;
  padding-bottom: 20px;  /* 底部留出一些空间 */
}

/* 美化滚动条样式 */
.grades-content::-webkit-scrollbar {
  width: 6px;
}

.grades-content::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.grades-content::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 3px;
}

.grades-content::-webkit-scrollbar-thumb:hover {
  background: #555;
}

.card-header {
  margin-bottom: 20px;
}

.student-info {
  margin-top: 20px;
}

.filter-container {
  margin: 20px 0;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  gap: 10px;
}

.filter-container .el-form {
  margin-bottom: 0;
}

.filter-container .el-form-item {
  margin-bottom: 0;
  margin-right: 16px;
}

.filter-container .el-select {
  width: 200px;
}

.statistics-container {
  margin: 20px 0;
}

.statistic-value {
  font-size: 24px;
  font-weight: bold;
  text-align: center;
  color: #409EFF;
}

.action-buttons {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

@media print {
  .filter-container,
  .action-buttons {
    display: none;
  }

  .grades-content {
    height: auto;
    overflow: visible;
  }
}
</style> 