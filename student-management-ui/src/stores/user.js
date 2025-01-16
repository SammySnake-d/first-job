import { defineStore } from 'pinia'

export const useUserStore = defineStore({
  id: 'user',
  
  state: () => ({
    user: null,
    token: null
  }),

  actions: {
    setUser(userData) {
      this.user = userData
      this.token = userData.token
      localStorage.setItem('token', userData.token)
    },

    clearUser() {
      this.user = null
      this.token = null
      localStorage.removeItem('token')
    },

    initializeAuth() {
      const token = localStorage.getItem('token')
      if (token) {
        this.token = token
      }
    }
  },

  persist: {
    enabled: true,
    strategies: [
      {
        key: 'user',
        storage: localStorage,
      },
    ],
  }
}) 