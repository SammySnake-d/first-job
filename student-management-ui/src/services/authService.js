import request from '@/utils/request'

// 导出默认对象
export default {
    // 登录
    login( data ) {
        return request( {
            url: '/api/auth/login',
            method: 'post',
            data
        } )
    },

    // 重置密码
    resetPassword( data ) {
        return request( {
            url: '/api/auth/reset-password',
            method: 'post',
            data
        } )
    },

    // 登出
    logout() {
        return request( {
            url: '/api/auth/logout',
            method: 'post'
        } )
    },

    // 获取用户信息
    getUserInfo() {
        return request( {
            url: '/api/auth/user-info',
            method: 'get'
        } )
    },

    // 检查用户名是否可用
    checkUsername( username ) {
        return request( {
            url: '/api/auth/check-username',
            method: 'get',
            params: { username }
        } )
    },

    // 验证用户名
    verifyUsername( username ) {
        return request( {
            url: '/api/auth/forgot-password/verify',
            method: 'post',
            data: { username }
        } )
    },

    // 重置密码(忘记密码流程)
    resetForgotPassword( data ) {
        return request( {
            url: '/api/auth/forgot-password/reset',
            method: 'post',
            data
        } )
    },

    // 修改密码
    changePassword(data) {
        return request({
            url: '/api/auth/change-password',
            method: 'post',
            data
        })
    },

    // 获取个人信息
    getProfile() {
        return request({
            url: '/api/auth/profile',
            method: 'get'
        })
    }
}