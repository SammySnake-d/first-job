<template>
  <div>
    <div class="grade-list">
      <div class="header">
        <h1>成绩管理</h1>
        <div class="header-right">
          <el-button-group class="view-switch">
            <el-button 
              :type="viewMode === 'chart' ? 'primary' : 'default'"
              @click="viewMode = 'chart'"
            >
              <el-icon><DataLine /></el-icon>
              图表模式
            </el-button>
            <el-button 
              :type="viewMode === 'card' ? 'primary' : 'default'"
              @click="viewMode = 'card'"
            >
              <el-icon><Grid /></el-icon>
              卡片模式
            </el-button>
          </el-button-group>
          <el-button type="primary" @click="handleAdd">录入成绩</el-button>
        </div>
      </div>

      <div v-if="viewMode === 'chart'" class="statistics-section">
        <el-row :gutter="20">
          <el-col :span="12">
            <el-card shadow="hover">
              <template #header>
                <div class="card-header">
                  <span>成绩分布</span>
                </div>
              </template>
              <div ref="gradeDistributionChart" class="chart" style="height: 400px;"></div>
            </el-card>
          </el-col>
          <el-col :span="12">
            <el-card shadow="hover">
              <template #header>
                <div class="card-header">
                  <span>成绩趋势</span>
                </div>
              </template>
              <div ref="gradeTrendChart" class="chart" style="height: 400px;"></div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <div v-else class="statistics-cards">
        <el-row :gutter="20">
          <el-col :span="6">
            <el-card shadow="hover" class="score-card">
              <div class="score-content">
                <div class="score-label">平均分</div>
                <div class="score-value">{{ statistics.averageScore.toFixed(1) }}</div>
                <div class="score-icon">
                  <el-icon><Histogram /></el-icon>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :span="6">
            <el-card shadow="hover" class="score-card highest">
              <div class="score-content">
                <div class="score-label">最高分</div>
                <div class="score-value">{{ statistics.highestScore.toFixed(1) }}</div>
                <div class="score-icon">
                  <el-icon><TopRight /></el-icon>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :span="6">
            <el-card shadow="hover" class="score-card lowest">
              <div class="score-content">
                <div class="score-label">最低分</div>
                <div class="score-value">{{ statistics.lowestScore.toFixed(1) }}</div>
                <div class="score-icon">
                  <el-icon><BottomRight /></el-icon>
                </div>
              </div>
            </el-card>
          </el-col>
          <el-col :span="6">
            <el-card shadow="hover" class="score-card pass-rate">
              <div class="score-content">
                <div class="score-label">及格率</div>
                <div class="score-value">{{ (statistics.passRate * 100).toFixed(1) }}%</div>
                <div class="score-icon">
                  <el-icon><DataAnalysis /></el-icon>
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>

      <el-form :inline="true" class="search-form" @submit.prevent="handleSearch">
        <el-form-item label="班级">
          <el-select 
            v-model="searchForm.classId" 
            placeholder="选择班级" 
            @change="handleClassChange" 
            clearable
            style="width: 200px"
          >
            <el-option
              v-for="item in classes"
              :key="item.Id"
              :label="formatClassInfo(item)"
              :value="item.Id"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="学生">
          <el-select 
            v-model="searchForm.studentId" 
            placeholder="输入学号或姓名搜索" 
            @change="handleStudentChange" 
            filterable 
            remote
            :remote-method="searchStudents"
            :loading="studentSearchLoading"
            clearable
            style="width: 240px"
          >
            <el-option
              v-for="item in students"
              :key="item.Id"
              :label="item.Name"
              :value="item.Id"
            >
              <div class="student-info">
                <el-tag size="small" class="student-number">{{ item.StudentNumber }}</el-tag>
                <span class="student-name">{{ item.Name }}</span>
              </div>
            </el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="课程">
          <el-select 
            v-model="searchForm.courseId" 
            placeholder="输入课程名称或代码搜索" 
            filterable 
            remote
            :remote-method="searchCourses"
            :loading="courseSearchLoading"
            clearable
            style="width: 250px"
          >
            <el-option
              v-for="item in courses"
              :key="item.Id"
              :label="item.Name"
              :value="item.Id"
            >
              <div class="course-info">
                <span class="course-name">{{ item.Name }}</span>
                <span class="course-code">{{ item.Code || '-' }}</span>
              </div>
            </el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="等级">
          <el-select 
            v-model="searchForm.gradeLevel" 
            placeholder="选择等级" 
            clearable
            style="width: 120px"
          >
            <el-option label="A" value="A" />
            <el-option label="B" value="B" />
            <el-option label="C" value="C" />
            <el-option label="D" value="D" />
            <el-option label="F" value="F" />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="resetSearch">重置</el-button>
        </el-form-item>
      </el-form>

      <el-table :data="grades" style="width: 100%" v-loading="loading">
        <el-table-column 
          prop="Student.StudentNumber" 
          label="学号" 
          width="120" 
          sortable
          :sort-method="(a, b) => a.Student.StudentNumber.localeCompare(b.Student.StudentNumber)"
        >
          <template #default="scope">
            {{ scope.row.Student?.StudentNumber || '-' }}
          </template>
        </el-table-column>
        <el-table-column 
          prop="Student.Name" 
          label="姓名" 
          width="100"
        >
          <template #default="scope">
            {{ scope.row.Student?.Name || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="班级" width="120">
          <template #default="scope">
            {{ scope.row.Student?.Class?.Grade }}{{ scope.row.Student?.Class?.Name }}
          </template>
        </el-table-column>
        <el-table-column 
          prop="Course.Name" 
          label="课程名称" 
          width="180"
        >
          <template #default="scope">
            {{ scope.row.CourseSelection?.Course?.Name || '-' }}
            <el-tag size="small" style="margin-left: 5px">
              {{ scope.row.CourseSelection?.Course?.Code || '-' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="Score" label="分数" width="100" sortable />
        <el-table-column prop="GradeLevel" label="等级" width="80">
          <template #default="scope">
            <el-tag :type="getGradeLevelType(scope.row.GradeLevel)">
              {{ scope.row.GradeLevel }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="GradeDate" label="录入日期" width="180">
          <template #default="scope">
            {{ new Date(scope.row.GradeDate).toLocaleDateString() }}
          </template>
        </el-table-column>
        <el-table-column prop="Comments" label="备注" show-overflow-tooltip />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="scope">
            <el-button-group>
              <el-button size="small" @click="handleEdit(scope.row)">编辑</el-button>
              <el-button size="small" type="danger" @click="handleDelete(scope.row)">删除</el-button>
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

      <el-dialog
        v-model="dialogVisible"
        :title="form.id ? '编辑成绩' : '录入成绩'"
        width="500px"
      >
        <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
          <el-form-item label="学号" prop="studentId">
            <el-select
              v-model="form.studentId"
              placeholder="请输入学号"
              filterable
              remote
              :remote-method="searchStudentByNumber"
              :loading="studentSearchLoading"
              @change="handleStudentSelect"
              style="width: 100%"
            >
              <el-option
                v-for="item in formStudents"
                :key="item.id"
                :label="item.studentNumber"
                :value="item.id"
              >
                <div class="student-info">
                  <el-tag size="small" class="student-number">{{ item.studentNumber }}</el-tag>
                  <span class="student-name">{{ item.name }}</span>
                </div>
              </el-option>
            </el-select>
          </el-form-item>

          <el-form-item label="姓名">
            <el-input v-model="selectedStudentName" disabled />
          </el-form-item>
          
          <el-form-item label="课程" prop="courseSelectionId">
            <el-select 
              v-model="form.courseSelectionId" 
              placeholder="输入课程名称或代码搜索"
              filterable
              remote
              :remote-method="searchCourseByName"
              :loading="courseSearchLoading"
              :disabled="!form.studentId"
              style="width: 100%"
            >
              <el-option
                v-for="item in courseSelections"
                :key="item.Id"
                :label="item.Course?.Name"
                :value="item.Id"
              >
                <div class="course-info">
                  <span class="course-name">{{ item.Course?.Name }}</span>
                  <el-tag size="small" class="course-code">{{ item.Course?.Code }}</el-tag>
                </div>
              </el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="分数" prop="score">
            <el-input
              v-model.number="form.score"
              type="number"
              min="0"
              max="100"
              placeholder="请输入分数"
            />
          </el-form-item>
          <el-form-item label="等级">
            <el-tag :type="getGradeLevelType(calculatedGradeLevel)">
              {{ calculatedGradeLevel }}
            </el-tag>
          </el-form-item>
          <el-form-item label="备注" prop="comments">
            <el-input 
              v-model="form.comments" 
              type="textarea" 
              :rows="3"
              placeholder="请输入备注信息"
            />
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
    </div>

    <div id="print-section">
      <div class="print-header">
        <h2>学生成绩单</h2>
        <div class="print-info">
          <div class="student-info">
            <p>学号：{{ searchForm.studentId ? students.find(s => s.Id === searchForm.studentId)?.StudentNumber : '全部' }}</p>
            <p>姓名：{{ searchForm.studentId ? students.find(s => s.Id === searchForm.studentId)?.Name : '全部' }}</p>
          </div>
          <div class="class-info">
            <p>班级：{{ searchForm.classId ? formatClassInfo(classes.find(c => c.Id === searchForm.classId)) : '全部' }}</p>
            <p>学期：{{ searchForm.semester ? (searchForm.semester === '1' ? '第一学期' : '第二学期') : '全部' }}</p>
            <p>学年：{{ searchForm.year || '全部' }}</p>
          </div>
          <div class="print-meta">
            <p>打印时间：{{ new Date().toLocaleString() }}</p>
          </div>
        </div>
      </div>
      <table class="print-table">
        <thead>
          <tr>
            <th>学号</th>
            <th>姓名</th>
            <th>班级</th>
            <th>课程代码</th>
            <th>课程名称</th>
            <th>分数</th>
            <th>等级</th>
            <th>备注</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="grade in grades" :key="grade.Id">
            <td>{{ grade.Student?.StudentNumber || '-' }}</td>
            <td>{{ grade.Student?.Name || '-' }}</td>
            <td>{{ grade.Student?.Class ? formatClassInfo(grade.Student.Class) : '-' }}</td>
            <td>{{ grade.CourseSelection?.Course?.Code || '-' }}</td>
            <td>{{ grade.CourseSelection?.Course?.Name || '-' }}</td>
            <td>{{ grade.Score }}</td>
            <td>{{ grade.GradeLevel }}</td>
            <td>{{ grade.Comments || '-' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch, nextTick, onUnmounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import * as echarts from 'echarts'
import request from '@/utils/request'
import { formatClassInfo } from '@/utils/format'
import { 
  DataLine, 
  Grid, 
  Histogram, 
  TopRight, 
  BottomRight, 
  DataAnalysis 
} from '@element-plus/icons-vue'

// 视图模式
const viewMode = ref('card') // 默认卡片模式

// 数据列表
const grades = ref([])
const classes = ref([])
const students = ref([])
const courses = ref([])
const courseSelections = ref([])

// 分页
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

// 对话框和加载状态
const dialogVisible = ref(false)
const loading = ref(false)
const submitting = ref(false)
const formRef = ref(null)

// 搜索相关
const studentSearchLoading = ref(false)
const courseSearchLoading = ref(false)
const selectedClassId = ref(null)
const formStudents = ref([])

// 搜索表
const searchForm = ref({
  classId: null,
  studentId: null,
  courseId: null,
  gradeLevel: null
})

// 成绩表单
const form = ref({
  id: null,
  studentId: null,
  courseSelectionId: null,
  score: 0,
  comments: '',
  gradeDate: new Date().toISOString()
})

// 统计数据
const statistics = ref({
  averageScore: 0,
  highestScore: 0,
  lowestScore: 0,
  passRate: 0
})

// 表单验证规则
const rules = {
  studentId: [
    { required: true, message: '请选择学生', trigger: 'change' }
  ],
  courseSelectionId: [
    { required: true, message: '请选择课程', trigger: 'change' }
  ],
  score: [
    { required: true, message: '请输入分数', trigger: 'blur' },
    { 
      validator: (rule, value, callback) => {
        const score = parseFloat(value)
        if (isNaN(score)) {
          callback(new Error('请输入有效的数字'))
        } else if (score < 0) {
          callback(new Error('分数不能小于0'))
        } else if (score > 100) {
          callback(new Error('分数不能超过100'))
        } else {
          callback()
        }
      },
      trigger: ['blur', 'change']
    }
  ]
}

// 计算属性
const calculatedGradeLevel = computed(() => {
  const score = form.value.score
  if (score >= 90) return 'A'
  if (score >= 80) return 'B'
  if (score >= 70) return 'C'
  if (score >= 60) return 'D'
  return 'F'
})

const selectedStudentName = ref('')

// 图表DOM引用
const gradeDistributionChart = ref(null)
const gradeTrendChart = ref(null)

// 图表实例
let distributionChart = null
let trendChart = null

// 监听视图模式变化
watch(viewMode, (newMode) => {
  if (newMode === 'chart') {
    nextTick(() => {
      initCharts()
    })
  }
})

// 初始化图表
const initCharts = () => {
  // 销毁旧的实例
  if (distributionChart) {
    distributionChart.dispose()
  }
  if (trendChart) {
    trendChart.dispose()
  }

  // 确保DOM元素存在
  if (!gradeDistributionChart.value || !gradeTrendChart.value) {
    return
  }

  // 成绩分布图
  distributionChart = echarts.init(gradeDistributionChart.value)
  distributionChart.setOption({
    title: { text: '成绩分布' },
    tooltip: { trigger: 'axis' },
    xAxis: {
      type: 'category',
      data: ['0-60', '60-70', '70-80', '80-90', '90-100']
    },
    yAxis: { type: 'value' },
    series: [{
      data: calculateGradeDistribution(grades.value),
      type: 'bar',
      barWidth: '60%',
      itemStyle: {
        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
          { offset: 0, color: '#83bff6' },
          { offset: 0.5, color: '#188df0' },
          { offset: 1, color: '#188df0' }
        ])
      }
    }]
  })

  // 成绩趋势图
  trendChart = echarts.init(gradeTrendChart.value)
  trendChart.setOption({
    title: { text: '成绩趋势' },
    tooltip: { trigger: 'axis' },
    xAxis: { type: 'time' },
    yAxis: { type: 'value' },
    series: [{
      name: '成绩',
      type: 'line',
      smooth: true,
      data: calculateGradeTrend(grades.value),
      itemStyle: { color: '#409EFF' }
    }]
  })
}

// 计算成绩分布
const calculateGradeDistribution = (grades) => {
  const distribution = [0, 0, 0, 0, 0] // 分别对应 0-60, 60-70, 70-80, 80-90, 90-100
  grades.forEach(grade => {
    const score = grade.Score
    if (score < 60) distribution[0]++
    else if (score < 70) distribution[1]++
    else if (score < 80) distribution[2]++
    else if (score < 90) distribution[3]++
    else distribution[4]++
  })
  return distribution
}

// 计算成绩趋势
const calculateGradeTrend = (grades) => {
  return grades
    .sort((a, b) => new Date(a.GradeDate) - new Date(b.GradeDate))
    .map(grade => [new Date(grade.GradeDate), grade.Score])
}

// 初始化
onMounted(async () => {
  await Promise.all([
    fetchClasses(),
    fetchCourses()
  ])
  await fetchGrades()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  if (distributionChart) {
    distributionChart.dispose()
  }
  if (trendChart) {
    trendChart.dispose()
  }
})

// 获取数据方法
const fetchClasses = async () => {
  try {
    const response = await request.get('/api/class')
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
  }
}

const fetchCourses = async () => {
  try {
    const response = await request.get('/api/course')
    if (response?.data?.data) {
      courses.value = response.data.data
    } else if (Array.isArray(response?.data)) {
      courses.value = response.data
    } else if (Array.isArray(response)) {
      courses.value = response
    } else {
      courses.value = []
    }
  } catch (error) {
    console.error('获取课程列表失败:', error)
    ElMessage.error('获取课程列表失败')
  }
}

const fetchGrades = async () => {
  loading.value = true
  try {
    const params = {
      page: currentPage.value,
      pageSize: pageSize.value,
      classId: searchForm.value.classId,
      studentId: searchForm.value.studentId,
      courseId: searchForm.value.courseId,
      gradeLevel: searchForm.value.gradeLevel
    }

    // 移除未定义的参数
    Object.keys(params).forEach(key => {
      if (params[key] === undefined || params[key] === null || params[key] === '') {
        delete params[key]
      }
    })

    const response = await request.get('/api/grades', { params })
    if (response?.data?.data) {
      grades.value = response.data.data
      total.value = response.data.total || response.data.data.length
      calculateStatistics(grades.value)
      if (viewMode.value === 'chart') {
        nextTick(() => {
          initCharts()
        })
      }
    } else if (Array.isArray(response?.data)) {
      grades.value = response.data
      total.value = response.data.length
      calculateStatistics(grades.value)
      if (viewMode.value === 'chart') {
        nextTick(() => {
          initCharts()
        })
      }
    } else if (Array.isArray(response)) {
      grades.value = response
      total.value = response.length
      calculateStatistics(grades.value)
      if (viewMode.value === 'chart') {
        nextTick(() => {
          initCharts()
        })
      }
    } else {
      grades.value = []
      total.value = 0
      calculateStatistics([])
    }
  } catch (error) {
    console.error('获取成绩列表失败:', error)
    ElMessage.error('获取成绩列表失败')
    grades.value = []
    total.value = 0
    calculateStatistics([])
  } finally {
    loading.value = false
  }
}

const fetchCourseSelections = async (studentId, courseName = '') => {
  if (!studentId) {
    courseSelections.value = []
    return
  }
  try {
    const response = await request.get(`/api/course-selection/student/${studentId}`)
    courseSelections.value = response.data || []
  } catch (error) {
    console.error('获取选课记录失败:', error)
    ElMessage.error('获取选课记录失败')
  }
}

// 搜索方法
const searchStudents = async (query) => {
  if (!query) {
    students.value = []
    return
  }
  studentSearchLoading.value = true
  try {
    const response = await request.get('/api/student', {
      params: {
        pageIndex: 0,
        pageSize: 10,
        keyword: query,
        classId: searchForm.value.classId || undefined
      }
    })
    if (response?.data?.data) {
      students.value = response.data.data
    } else if (Array.isArray(response?.data)) {
      students.value = response.data
    } else if (Array.isArray(response)) {
      students.value = response
    } else {
      students.value = []
    }
  } catch (error) {
    console.error('搜索学生失败:', error)
    ElMessage.error('搜索学生失败')
    students.value = []
  } finally {
    studentSearchLoading.value = false
  }
}

const searchCourses = async (query) => {
  if (!query) {
    courses.value = []
    return
  }
  courseSearchLoading.value = true
  try {
    const response = await request.get('/api/course', {
      params: {
        keyword: query,
        pageIndex: 0,
        pageSize: 10
      }
    })
    if (response?.data?.data) {
      courses.value = response.data.data.filter(c => c && c.Code && c.Name)
    } else if (Array.isArray(response?.data)) {
      courses.value = response.data.filter(c => c && c.Code && c.Name)
    } else if (Array.isArray(response)) {
      courses.value = response.filter(c => c && c.Code && c.Name)
    } else {
      courses.value = []
    }
  } catch (error) {
    console.error('搜索课程失败:', error)
    ElMessage.error('搜索课程失败')
  } finally {
    courseSearchLoading.value = false
  }
}

const searchStudentByNumber = async (query) => {
  if (!query) {
    formStudents.value = []
    return
  }
  
  try {
    studentSearchLoading.value = true
    const response = await request.get('/api/student', {
      params: {
        keyword: query,
        pageIndex: 0,
        pageSize: 10
      }
    })
    
    let students = []
    if (response?.data?.data) {
      students = response.data.data
    } else if (Array.isArray(response?.data)) {
      students = response.data
    } else if (Array.isArray(response)) {
      students = response
    }
    
    formStudents.value = students.map(student => ({
      id: student.Id,
      studentNumber: student.StudentNumber,
      name: student.Name,
      class: student.Class
    })).filter(student => student.studentNumber && student.name)
    
    if (formStudents.value.length === 0) {
      ElMessage.warning('未找到匹配的学生')
    }
  } catch (error) {
    console.error('搜索学生失败:', error)
    ElMessage.error('搜索学生失败')
    formStudents.value = []
  } finally {
    studentSearchLoading.value = false
  }
}

const searchCourseByName = async (query) => {
  if (!form.value.studentId || !query) {
    courseSelections.value = []
    return
  }

  try {
    courseSearchLoading.value = true
    const response = await request.get(`/api/course-selection/student/${form.value.studentId}`)
    
    let selections = []
    if (response?.data?.data) {
      selections = response.data.data
    } else if (Array.isArray(response?.data)) {
      selections = response.data
    } else if (Array.isArray(response)) {
      selections = response
    }

    courseSelections.value = selections.filter(cs => {
      if (!cs?.Course?.Name || !cs?.Course?.Code) return false
      const courseName = cs.Course.Name.toLowerCase()
      const courseCode = cs.Course.Code.toLowerCase()
      const searchQuery = query.toLowerCase()
      return courseName.includes(searchQuery) || courseCode.includes(searchQuery)
    })

    if (courseSelections.value.length === 0) {
      ElMessage.warning('未找到匹配的课程')
    }
  } catch (error) {
    console.error('搜索课程失败:', error)
    ElMessage.error('搜索课程失败')
    courseSelections.value = []
  } finally {
    courseSearchLoading.value = false
  }
}

const handleStudentSelect = async (studentId) => {
  if (!studentId) {
    form.value.courseSelectionId = null
    courseSelections.value = []
    selectedStudentName.value = ''
    return
  }

  try {
    const student = formStudents.value.find(s => s.id === studentId)
    if (student) {
      selectedStudentName.value = student.name
      const response = await request.get(`/api/course-selection/student/${studentId}`)
      
      let selections = []
      if (response?.data?.data) {
        selections = response.data.data
      } else if (Array.isArray(response?.data)) {
        selections = response.data
      } else if (Array.isArray(response)) {
        selections = response
      }
      
      courseSelections.value = selections.filter(cs => cs?.Course?.Name && cs?.Course?.Code)
      
      if (courseSelections.value.length === 0) {
        ElMessage.warning('该学生暂无可录入成绩的课程')
      }
    }
  } catch (error) {
    console.error('获取学生选课信息失败:', error)
    ElMessage.error('获取学生选课信息失败')
    courseSelections.value = []
  }
}

// 事件处理方法
const handleSearch = async () => {
  currentPage.value = 1
  await fetchGrades()
}

const resetSearch = async () => {
  searchForm.value = {
    classId: null,
    studentId: null,
    courseId: null,
    gradeLevel: null
  }
  students.value = []
  courses.value = []
  currentPage.value = 1
  await fetchGrades()
}

const handleClassChange = async (classId) => {
  searchForm.value.studentId = null
  if (classId) {
    try {
      const response = await request.get('/api/student', {
        params: {
          classId: classId,
          pageSize: 100
        }
      })
      if (response?.data?.data) {
        students.value = response.data.data
      } else if (Array.isArray(response?.data)) {
        students.value = response.data
      } else if (Array.isArray(response)) {
        students.value = response
      } else {
        students.value = []
      }
    } catch (error) {
      console.error('获取班级学生失败:', error)
      ElMessage.error('获取班级学生失败')
    }
  } else {
    students.value = []
  }
  await fetchGrades()
}

const handleStudentChange = async () => {
  currentPage.value = 1
  await fetchGrades()
}

const handleSizeChange = (val) => {
  pageSize.value = val
  fetchGrades()
}

const handleCurrentChange = (val) => {
  currentPage.value = val
  fetchGrades()
}

// 表单处理方法
const handleAdd = () => {
  form.value = {
    id: null,
    studentId: null,
    courseSelectionId: null,
    score: 0,
    comments: '',
    gradeDate: new Date().toISOString()
  }
  courseSelections.value = []
  selectedStudentName.value = ''
  dialogVisible.value = true
}

const handleEdit = async (row) => {
  try {
    console.log('编辑行数据:', row)
    // 先获取学生信息
    const studentId = row.Student?.Id || row.StudentId
    if (!studentId) {
      ElMessage.error('无法获取学生信息')
      return
    }
    
    const student = await request.get(`/api/student/${studentId}`)
    console.log('获取到的学生信息:', student)
    
    if (student) {
      formStudents.value = [{
        id: student.Id,
        studentNumber: student.StudentNumber,
        name: student.Name,
        class: student.Class || row.Student.Class
      }]
      selectedStudentName.value = student.Name

      // 获取选课信息
      const response = await request.get(`/api/course-selection/student/${studentId}`)
      console.log('获取到的选课信息:', response)
      
      if (response?.data?.data) {
        courseSelections.value = response.data.data
      } else if (Array.isArray(response?.data)) {
        courseSelections.value = response.data
      } else if (Array.isArray(response)) {
        courseSelections.value = response
      } else {
        courseSelections.value = []
      }

      // 所有数据加载完成后，设置表单数据
      form.value = {
        id: row.Id,
        studentId: studentId,
        courseSelectionId: row.CourseSelection?.Id || row.CourseSelectionId,
        score: parseFloat(row.Score),
        comments: row.Comments || '',
        gradeDate: row.GradeDate
      }

      console.log('设置的表单数据:', form.value)
      // 最后打开对话框
      dialogVisible.value = true
    } else {
      ElMessage.error('获取学生信息失败')
    }
  } catch (error) {
    console.error('加载成绩信息失败:', error)
    console.error('详细错误信息:', error.response?.data)
    ElMessage.error('加载成绩信息失败')
  }
}

const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm('确定要删除这条成绩记录吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await request.delete(`/api/grades/${row.Id}`)
    ElMessage.success('成绩删除成功')
    await fetchGrades()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除成绩失败:', error)
      ElMessage.error('删除成绩失败')
    }
  }
}

const handleSubmit = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid) => {
    if (valid) {
      try {
        submitting.value = true
        const data = {
          StudentId: parseInt(form.value.studentId),
          CourseSelectionId: parseInt(form.value.courseSelectionId),
          Score: parseFloat(form.value.score),
          GradeLevel: calculatedGradeLevel.value,
          GradeDate: new Date().toISOString(),
          Comments: form.value.comments || ''
        }

        if (form.value.id) {
          await request.put(`/api/grades/${form.value.id}`, data)
          ElMessage.success('修改成功')
        } else {
          await request.post('/api/grades', data)
          ElMessage.success('录入成功')
        }

        dialogVisible.value = false
        await fetchGrades()
      } catch (error) {
        console.error('保存失败:', error)
        if (error.response?.data?.message) {
          ElMessage.error(error.response.data.message)
        } else {
          ElMessage.error(form.value.id ? '修改失败' : '录入失败')
        }
      } finally {
        submitting.value = false
      }
    }
  })
}

// 工具方法
const calculateStatistics = (gradesList) => {
  if (!gradesList || gradesList.length === 0) {
    statistics.value = {
      averageScore: 0,
      highestScore: 0,
      lowestScore: 0,
      passRate: 0
    }
    return
  }

  const scores = gradesList.map(g => g.Score)
  const passCount = gradesList.filter(g => g.Score >= 60).length

  statistics.value = {
    averageScore: scores.reduce((a, b) => a + b, 0) / scores.length,
    highestScore: Math.max(...scores),
    lowestScore: Math.min(...scores),
    passRate: passCount / scores.length
  }

  // 更新图表
  nextTick(() => {
    if (viewMode.value === 'chart') {
      initCharts()
    }
  })
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

// 修改 watch，只在新增时使用
watch(() => form.value.studentId, async (newVal, oldVal) => {
  // 只在新增时触发
  if (!form.value.id) {
    form.value.courseSelectionId = null
    if (newVal) {
      await fetchCourseSelections(newVal)
    } else {
      courseSelections.value = []
    }
  }
})

// 处理窗口大小变化
const handleResize = () => {
  if (distributionChart) {
    distributionChart.resize()
  }
  if (trendChart) {
    trendChart.resize()
  }
}

// 修改打印方法
const handlePrint = () => {
  // 可以在打印前进行一些数据处理
  nextTick(() => {
    window.print()
  })
}
</script>

<style scoped>
.grade-list {
  padding: 20px;
  height: calc(100vh - 84px);  /* 减去顶部导航栏的高度 */
  overflow-y: auto;  /* 添加纵向滚动 */
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.search-form {
  margin-bottom: 20px;
  padding: 20px;
  background-color: #f5f7fa;
  border-radius: 4px;
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.search-form .el-form-item {
  margin-bottom: 10px;
  margin-right: 20px;
}

.statistics-section {
  margin-bottom: 20px;
}

.chart {
  min-height: 400px;
  width: 100%;
}

.statistics-cards {
  margin-bottom: 20px;
}

.statistic-item {
  text-align: center;
}

.statistic-title {
  font-size: 14px;
  color: #909399;
  margin-bottom: 10px;
}

.statistic-value {
  font-size: 24px;
  font-weight: bold;
  color: #303133;
  margin-bottom: 10px;
}

.mini-chart {
  height: 50px;
  width: 100%;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

.el-select {
  width: 100%;
}

.el-option {
  .student-info {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;
  }

  .student-name {
    font-weight: 500;
  }

  .student-number {
    margin-left: 8px;
    font-size: 12px;
  }
}

.el-input.is-disabled .el-input__inner {
  color: #606266;
}

.course-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.course-name {
  font-weight: 500;
}

.course-code {
  margin-left: 8px;
  font-size: 12px;
}

.el-select {
  width: 220px;
}

.student-info,
.course-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.student-number,
.course-code {
  color: #909399;
  font-size: 13px;
}

.student-name,
.course-name {
  font-weight: 500;
}

.el-select {
  width: 100%;
}

.student-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.student-name {
  font-weight: 500;
}

.student-number {
  margin-left: 8px;
  font-size: 12px;
}

.header-right {
  display: flex;
  gap: 16px;
  align-items: center;
}

.view-switch {
  margin-right: 8px;
}

.score-card {
  transition: all 0.3s;
}

.score-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 2px 12px 0 rgba(0,0,0,.1);
}

.score-content {
  position: relative;
  padding: 20px;
  overflow: hidden;
}

.score-label {
  font-size: 16px;
  color: #909399;
  margin-bottom: 8px;
}

.score-value {
  font-size: 28px;
  font-weight: bold;
  color: #303133;
}

.score-icon {
  position: absolute;
  right: 20px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 48px;
  opacity: 0.2;
  color: #409EFF;
}

.highest .score-value,
.highest .score-icon {
  color: #67C23A;
}

.lowest .score-value,
.lowest .score-icon {
  color: #F56C6C;
}

.pass-rate .score-value,
.pass-rate .score-icon {
  color: #E6A23C;
}

.statistics-section,
.statistics-cards {
  animation: fade-in 0.3s ease-in-out;
}

@keyframes fade-in {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.student-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.student-name {
  font-weight: 500;
}

.student-number {
  margin-left: 8px;
  font-size: 12px;
}

/* 打印样式 */
@media print {
  /* 隐藏所有非打印内容 */
  html > body > div#app > div.layout-container > *:not(#print-section),
  .grade-list,
  .el-aside,
  .el-header,
  .navbar {
    display: none !important;
  }

  #print-section {
    display: block !important;
    position: fixed !important;
    left: 0 !important;
    top: 0 !important;
    width: 100% !important;
    height: 100% !important;
    background: white !important;
    padding: 20px;
    margin: 0;
    overflow: visible !important;
  }

  /* 打印页面设置 */
  @page {
    size: A4 portrait;
    margin: 1cm;
    orphans: 4;
    widows: 4;
  }

  /* 打印头部样式 */
  .print-header {
    margin-bottom: 30px;
    page-break-after: avoid;
  }

  .print-header h2 {
    text-align: center;
    margin-bottom: 20px;
    font-size: 24px;
    font-weight: bold;
  }

  .print-info {
    margin-bottom: 20px;
    font-size: 14px;
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 20px;
    page-break-inside: avoid;
  }

  .print-info p {
    margin: 5px 0;
  }

  .print-meta {
    grid-column: 1 / -1;
    border-top: 1px solid #eee;
    padding-top: 10px;
    margin-top: 10px;
  }

  /* 打印表格样式 */
  .print-table {
    width: 100%;
    border-collapse: collapse;
    table-layout: fixed;
    page-break-inside: auto;
  }

  .print-table thead {
    display: table-header-group;
    page-break-before: avoid;
    page-break-after: avoid;
  }

  .print-table tbody {
    page-break-inside: auto;
  }

  .print-table tr {
    page-break-inside: avoid;
    page-break-after: auto;
  }

  .print-table th,
  .print-table td {
    border: 1px solid #000;
    padding: 8px;
    text-align: center;
    font-size: 12px;
    word-break: break-all;
    white-space: normal;
  }

  .print-table th {
    background-color: #f5f5f5 !important;
    font-weight: bold;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
  }

  /* 设置列宽 */
  .print-table th:nth-child(1) { width: 12%; }  /* 学号 */
  .print-table th:nth-child(2) { width: 8%; }   /* 姓名 */
  .print-table th:nth-child(3) { width: 12%; }  /* 班级 */
  .print-table th:nth-child(4) { width: 10%; }  /* 课程代码 */
  .print-table th:nth-child(5) { width: 18%; }  /* 课程名称 */
  .print-table th:nth-child(6) { width: 8%; }   /* 分数 */
  .print-table th:nth-child(7) { width: 8%; }   /* 等级 */
  .print-table th:nth-child(8) { width: 24%; }  /* 备注 */

  /* 确保打印时背景色和边框能够显示 */
  * {
    -webkit-print-color-adjust: exact !important;
    print-color-adjust: exact !important;
  }

  /* 避免内容被截断 */
  .print-table td,
  .print-table th {
    overflow: visible !important;
    white-space: normal !important;
    text-overflow: initial !important;
  }
}

/* 正常显示时隐藏打印内容 */
#print-section {
  display: none;
}

/* 修改禁用状态下输入框的样式 */
:deep(.el-input.is-disabled .el-input__wrapper) {
  background-color: var(--el-fill-color-light) !important;
  box-shadow: 0 0 0 1px var(--el-fill-color-light) inset !important;
}

/* 确保内部输入框也保持一致背景色 */
:deep(.el-input.is-disabled .el-input__wrapper .el-input__inner) {
  background-color: var(--el-fill-color-light) !important;
}
</style> 