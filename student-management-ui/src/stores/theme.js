import { defineStore } from 'pinia'

export const useThemeStore = defineStore( {
  id: 'theme',
  state: () => ( {
    loginTheme: localStorage.getItem( 'loginTheme' ) || 'light',
    mainTheme: localStorage.getItem( 'mainTheme' ) || 'light'
  } ),
  actions: {
    setLoginTheme( theme ) {
      this.loginTheme = theme
      localStorage.setItem( 'loginTheme', theme )
    },
    setMainTheme( theme ) {
      this.mainTheme = theme
      localStorage.setItem( 'mainTheme', theme )
    }
  },
  persist: {
    enabled: true,
    strategies: [
      {
        key: 'theme',
        storage: localStorage,
      },
    ],
  }
} )