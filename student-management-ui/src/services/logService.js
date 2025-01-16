import request from '@/utils/request'

export default {
  getLogs(params) {
    return request({
      url: '/api/logs/list',
      method: 'get',
      params
    })
  }
} 