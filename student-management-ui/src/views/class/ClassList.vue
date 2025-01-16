<template>
  <div class="class-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <h2>班级管理</h2>
          <el-button type="primary" @click="handleAdd">
            <el-icon><Plus /></el-icon>添加班级
          </el-button>
        </div>
      </template>

      <el-table 
        :data="classes" 
        border 
        style="width: 100%"
        v-loading="loading"
        stripe
      >
        <el-table-column 
          prop="Name" 
          label="班级名称" 
          min-width="150"
        >
          <template #default="{ row }">
            {{ `${row.Name}班` }}
          </template>
        </el-table-column>
        
        <el-table-column 
          prop="Grade" 
          label="年级"
          width="120"
        />
        
        <el-table-column 
          prop="Year" 
          label="学年"
          width="120"
        />
        
        <el-table-column 
          label="操作"
          width="200"
          fixed="right"
        >
          <template #default="scope">
            <el-button-group>
              <el-button
                type="primary"
                size="small"
                @click="handleEdit(scope.row)"
              >
                编辑
              </el-button>
              <el-button
                type="danger"
                size="small"
                @click="handleDelete(scope.row)"
              >
                删除
              </el-button>
            </el-button-group>
          </template>
        </el-table-column>
      </el-table>

      <el-dialog
        v-model="dialogVisible"
        :title="form.Id ? '编辑班级' : '添加班级'"
        width="500px"
      >
        <el-form
          ref="formRef"
          :model="form"
          :rules="rules"
          label-width="80px"
        >
          <el-form-item label="年级" prop="Grade">
            <el-select 
              v-model="form.Grade" 
              placeholder="请选择年级"
              style="width: 100%"
            >
              <el-option label="一年级" value="一年级" />
              <el-option label="二年级" value="二年级" />
              <el-option label="三年级" value="三年级" />
              <el-option label="四年级" value="四年级" />
            </el-select>
          </el-form-item>
          
          <el-form-item label="班级" prop="Name">
            <el-input 
              v-model="form.Name" 
              placeholder="请输入班级名称"
              maxlength="20"
              show-word-limit
            />
          </el-form-item>
          
          <el-form-item label="学年" prop="Year">
            <el-input-number 
              v-model="form.Year" 
              :min="2000" 
              :max="2100"
              style="width: 100%"
            />
          </el-form-item>
        </el-form>
        <template #footer>
          <span class="dialog-footer">
            <el-button @click="dialogVisible = false">取消</el-button>
            <el-button 
              type="primary" 
              @click="handleSubmit"
              :loading="submitting"
            >
              确定
            </el-button>
          </span>
        </template>
      </el-dialog>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import request from '@/utils/request'
import { formatClassInfo } from '@/utils/format'

const classes = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const formRef = ref(null)

const initForm = () => {
  return {
    Id: null,
    Name: '',
    Grade: '',
    Year: new Date().getFullYear(),
    Students: []
  }
}

const form = ref(initForm())

const rules = {
  Name: [
    { required: true, message: '请输入班级名称', trigger: 'blur' },
    { min: 2, max: 20, message: '长度在 2 到 20 个字符', trigger: 'blur' }
  ],
  Grade: [
    { required: true, message: '请选择年级', trigger: 'change' }
  ],
  Year: [
    { required: true, message: '请选择学年', trigger: 'change' }
  ]
}

const yearPickerOptions = {
  disabledDate(time) {
    return time.getFullYear() < 2000 || time.getFullYear() > 2100
  }
}

const fetchClasses = async () => {
  try {
    const response = await request.get('/api/class')
    console.log('获取到的班级列表:', response)
    classes.value = Array.isArray(response) ? response : response.data || []
  } catch (error) {
    console.error('获取班级列表失败:', error)
    ElMessage.error('获取班级列表失败')
  }
}

const handleAdd = () => {
  form.value = initForm()
  dialogVisible.value = true
}

const handleEdit = (row) => {
  form.value = {
    ...row,
    Year: row.Year,
    Students: row.Students || []
  }
  dialogVisible.value = true
}

const handleSubmit = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid) => {
    if (valid) {
      try {
        const data = {
          Name: form.value.Name.trim(),
          Grade: form.value.Grade,
          Year: parseInt(form.value.Year),
          Students: []
        }
        
        console.log('提交的数据:', data)
        
        if (form.value.Id) {
          data.Id = form.value.Id
          await request.put(`/api/class/${form.value.Id}`, data)
          ElMessage.success('更新成功')
        } else {
          const response = await request.post('/api/class', data)
          console.log('服务器响应:', response)
          ElMessage.success('添加成功')
        }
        dialogVisible.value = false
        await fetchClasses()
      } catch (error) {
        console.error('详细错误信息:', error.response?.data)
        const message = error.response?.data?.message || error.response?.data?.title || (form.value.Id ? '更新失败' : '添加失败')
        ElMessage.error(message)
      }
    }
  })
}

const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm('确定要删除该班级吗？', '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    
    await request.delete(`/api/class/${row.Id}`)
    ElMessage.success('删除成功')
    await fetchClasses()
  } catch (error) {
    if (error !== 'cancel') {
      const message = error.response?.data?.message || '删除失败'
      ElMessage.error(message)
      console.error(error)
    }
  }
}

// 格式化年份显示
const formatYear = (year) => {
  return `${year}年`
}

onMounted(() => {
  fetchClasses()
})
</script>

<style scoped>
.class-list {
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

.card-header h2 {
  margin: 0;
  font-size: 18px;
  color: #303133;
}

:deep(.el-form-item__content) {
  width: calc(100% - 80px);
}

:deep(.el-input-number) {
  width: 100%;
}

.el-button-group {
  .el-button {
    margin-right: 0;
  }
}

:deep(.el-table) {
  margin-top: 20px;
}

:deep(.el-dialog__body) {
  padding-top: 20px;
}
</style> 