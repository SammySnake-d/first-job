import { ref, watch } from 'vue'
import { useStorage } from '@vueuse/core'

export const useTheme = () => {
  const theme = useStorage('theme', 'light')
  const isDark = ref(theme.value === 'dark')

  const toggleTheme = () => {
    isDark.value = !isDark.value
    theme.value = isDark.value ? 'dark' : 'light'
  }

  watch(isDark, (val) => {
    document.documentElement.classList.toggle('dark', val)
    const htmlEl = document.querySelector('html')
    if (htmlEl) {
      htmlEl.setAttribute('data-theme', val ? 'dark' : 'light')
    }
  }, { immediate: true })

  return {
    isDark,
    toggleTheme
  }
} 