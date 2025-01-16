import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import ElementPlus from 'element-plus'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import 'element-plus/dist/index.css'
import zhCn from 'element-plus/dist/locale/zh-cn.mjs'
import App from './App.vue'
import router from './router'
import axios from 'axios'
import { getLoginTheme } from '@/utils/theme'

// 配置 axios 默认值
axios.defaults.baseURL = 'http://localhost:5284'
axios.defaults.headers.common['Accept'] = 'application/json'
axios.defaults.headers.post['Content-Type'] = 'application/json'
axios.defaults.withCredentials = true

const app = createApp( App )

// 注册 Element Plus 图标
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

// 创建 pinia 实例
const pinia = createPinia()
// 使用持久化插件
pinia.use( piniaPluginPersistedstate )

app.use( pinia )
app.use( ElementPlus, {
    locale: zhCn,
} )
app.use( router )

app.mount( '#app' )
