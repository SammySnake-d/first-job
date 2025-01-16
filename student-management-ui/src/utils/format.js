// 格式化班级显示
export const formatClassInfo = (classData) => {
  if (!classData) return ''
  return `${classData.Grade}-${classData.Name}班`
} 