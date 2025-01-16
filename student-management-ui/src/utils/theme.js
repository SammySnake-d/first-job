// 主题类型
export const ThemeType = {
  LIGHT: 'light',
  DARK: 'dark'
}

// 获取登录页面主题
export const getLoginTheme = () => {
  return localStorage.getItem('login-theme') || ThemeType.LIGHT
}

// 设置登录页面主题
export const setLoginTheme = (theme) => {
  localStorage.setItem('login-theme', theme)
}

// 切换登录页面主题
export const toggleLoginTheme = () => {
  const currentTheme = getLoginTheme()
  const newTheme = currentTheme === ThemeType.LIGHT ? ThemeType.DARK : ThemeType.LIGHT
  setLoginTheme(newTheme)
  return newTheme
}

// 原有的方法改名为系统主题
export const getSystemTheme = () => {
  return localStorage.getItem('systemTheme') || 'light'
}

export const setSystemTheme = (theme) => {
  localStorage.setItem('systemTheme', theme)
  document.documentElement.setAttribute('data-theme', theme)
} 