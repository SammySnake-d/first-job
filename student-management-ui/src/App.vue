<template>
  <router-view v-if="isReady" />
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useUserStore } from '@/stores/user'

const isReady = ref(false)
const userStore = useUserStore()

onMounted(() => {
   // 初始化认证状态
   userStore.initializeAuth()
   isReady.value = true
})
</script>

<style>
#app {
  width: 100%;
  height: 100%;
}
</style>
