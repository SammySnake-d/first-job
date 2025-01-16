import request from '@/utils/request'

export default {
  // 获取用户列表
  getUsers( params ) {
    return request( {
      url: '/api/users/list',
      method: 'get',
      params
    } )
  },

  // 获取用户详情
  getUserById( id ) {
    return request( {
      url: `/api/users/detail/${id}`,
      method: 'get'
    } )
  },

  // 创建用户
  createUser( data ) {
    return request( {
      url: '/api/users/create',
      method: 'post',
      data
    } )
  },

  // 更新用户
  updateUser( id, data ) {
    return request( {
      url: `/api/users/update/${id}`,
      method: 'put',
      data
    } )
  },

  // 删除用户
  deleteUser( id ) {
    return request( {
      url: `/api/users/delete/${id}`,
      method: 'delete'
    } )
  },

  // 重置用户密码
  resetPassword( userId, data ) {
    // console.log('Resetting password for user:', userId, data);
    return request( {
      url: `/api/users/reset-password/${userId}`,
      method: 'put',
      data: {
        NewPassword: data.newPassword,
        ConfirmPassword: data.confirmPassword
      }
    } )
  },

  // 更新用户状态
  updateUserStatus( id, status ) {
    return request( {
      url: `/api/users/status/${id}`,
      method: 'put',
      data: { status }
    } )
  },

  // 批量更新用户状态
  batchUpdateStatus( userIds, status ) {
    return request( {
      url: '/api/users/batch/status',
      method: 'put',
      data: { userIds, status }
    } )
  },

  // 批量重置密码
  batchResetPassword( userIds, newPassword ) {
    return request( {
      url: '/api/users/batch/reset-password',
      method: 'put',
      data: { userIds, newPassword }
    } )
  },

  // 检查用户名是否可用
  checkUsername( username ) {
    return request( {
      url: '/api/users/check-username',
      method: 'get',
      params: { username }
    } )
  }
} 