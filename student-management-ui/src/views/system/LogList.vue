<template>
  <div class="log-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <div class="header-content">
            <h2>系统日志</h2>
            <el-tag type="warning">开发中</el-tag>
          </div>
        </div>
      </template>

      <el-empty description="功能开发中...">
        <template #image>
          <el-icon :size="60" style="margin-bottom: 15px;"><Tools /></el-icon>
        </template>
        <template #description>
          <p>该功能正在开发中，敬请期待</p>
        </template>
      </el-empty>
    </el-card>
  </div>
</template>

<script setup>
import { Tools } from '@element-plus/icons-vue'
</script>

<style scoped>
.log-list {
  padding: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-content {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-content h2 {
  margin: 0;
}
</style> 