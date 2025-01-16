<template>
  <div class="grades-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <h2>学生成绩详情</h2>
          <div class="student-info">
            <span>年级：{{ studentInfo.grade || '-' }}</span>
            <span>班级：{{ studentInfo.className || '-' }}</span>
            <span>学号：{{ studentInfo.studentNumber || '-' }}</span>
            <span>姓名：{{ studentInfo.studentName || '-' }}</span>
          </div>
        </div>
      </template>

      <el-table
        v-loading="loading"
        :data="grades"
        style="width: 100%"
        border
        stripe
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
            <el-tag :type="getGradeLevelType(scope.row.gradeLevel)">
              {{ scope.row.gradeLevel }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="semester" label="学期" width="120" />
        <el-table-column prop="remark" label="备注" show-overflow-tooltip />
      </el-table>

      <div class="action-buttons">
        <el-button @click="$router.back()">返回</el-button>
        <el-button type="primary" @click="handlePrint">打印成绩单</el-button>
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useRoute, useRouter } from 'vue-router'
import request from '@/utils/request'

const route = useRoute()
const router = useRouter()
const grades = ref([])
const loading = ref(false)
const studentInfo = ref({
  studentNumber: '',
  studentName: '',
  grade: '',
  className: ''
})

const fetchStudentInfo = async () => {
  try {
    const studentResponse = await request.get(`/api/student/${route.params.id}`)
    console.log('获取到的学生信息:', studentResponse)
    
    const student = studentResponse
    if (student) {
      if (student.ClassId) {
        const classResponse = await request.get(`/api/class/${student.ClassId}`)
        console.log('获取到的班级信息:', classResponse)
        if (classResponse) {
          studentInfo.value = {
            studentNumber: student.StudentNumber,
            studentName: student.Name,
            grade: classResponse.Grade,
            className: classResponse.Name
          }
        }
      } else {
        studentInfo.value = {
          studentNumber: student.StudentNumber,
          studentName: student.Name,
          grade: '-',
          className: '-'
        }
      }
    }
  } catch (error) {
    console.error('获取学生信息失败:', error)
    console.error('详细错误:', error.response?.data)
    ElMessage.error('获取学生信息失败')
  }
}

const getScoreTagType = (score) => {
  if (score >= 90) return 'success'
  if (score >= 60) return 'warning'
  return 'danger'
}

const getGradeLevelType = (level) => {
  const types = {
    'A': 'success',
    'B': 'info',
    'C': 'warning',
    'D': 'warning',
    'F': 'danger'
  }
  return types[level] || 'info'
}

const fetchGrades = async () => {
  loading.value = true
  try {
    const response = await request.get(`/api/student/${route.params.id}/grades`)
    console.log('获取到的成绩数据:', response)
    if (response && response.length > 0) {
      console.log('第一条成绩数据结构:', JSON.stringify(response[0], null, 2))
    }
    
    let gradesData = []
    if (Array.isArray(response)) {
      gradesData = response
    } else if (Array.isArray(response?.data)) {
      gradesData = response.data
    } else if (response?.data?.data) {
      gradesData = response.data.data
    }

    grades.value = gradesData.map(grade => {
      console.log('处理单条成绩数据:', grade)

      return {
        courseName: grade.courseName || '-',
        teacherName: grade.teacherName || '-',
        score: grade.score || 0,
        gradeLevel: grade.gradeLevel || 'F',
        semester: grade.semester || '-',
        remark: grade.remark || ''
      }
    })
    console.log('最终处理后的成绩数据:', grades.value)
  } catch (error) {
    console.error('获取成绩失败:', error)
    console.error('详细错误:', error.response?.data)
    ElMessage.error('获取成绩失败')
    grades.value = []
  } finally {
    loading.value = false
  }
}

const handlePrint = () => {
  window.print()
}

onMounted(() => {
  const currentTheme = document.documentElement.getAttribute('data-theme')
  
  fetchStudentInfo()
  fetchGrades()
  
  if (currentTheme) {
    document.documentElement.setAttribute('data-theme', currentTheme)
  }
})
</script>

<style scoped>
.grades-list {
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

.student-info {
  display: flex;
  gap: 20px;
  color: #606266;
  font-size: 14px;
  flex-wrap: wrap;
}

.student-info span {
  font-weight: 500;
  white-space: nowrap;
}

.action-buttons {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

:deep(.el-table .cell) {
  white-space: nowrap;
}

@media print {
  .action-buttons {
    display: none;
  }
}
</style> 