import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '@/router'
import { useUserStore } from '@/stores/user'

const request = axios.create( {
    baseURL: 'http://localhost:5284',
    timeout: 5000,
    headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
    },
    withCredentials: true
} )

request.interceptors.request.use(
    config => {
        const userStore = useUserStore()
        if ( userStore.token ) {
            config.headers['Authorization'] = `Bearer ${userStore.token}`
        }
        return config
    },
    error => {
        return Promise.reject( error )
    }
)

request.interceptors.response.use(
    response => {
        return response.data
    },
    error => {
        if ( error.response ) {
            switch ( error.response.status ) {
                case 401:
                    const userStore = useUserStore()
                    userStore.clearUser()
                    localStorage.removeItem( 'token' )
                    window.location.href = '/login'
                    ElMessage.error( error.response.data?.message || '未授权，请重新登录' )
                    break
                case 403:
                    ElMessage.error( '没有权限访问' )
                    break
                case 500:
                    ElMessage.error( error.response.data?.message || '服务器错误' )
                    break
                case 404:
                    break
                default:
                    ElMessage.error( error.response.data?.message || '请求失败' )
            }
        } else {
            ElMessage.error( '网络错误，请检查网络连接' )
        }
        return Promise.reject( error )
    }
)

export default request 