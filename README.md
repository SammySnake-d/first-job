# 学籍管理系统

## 环境要求
- Node.js 16+
- .NET 7.0 SDK
- SQL Server
- MySQL 8.0+

## 快速开始

1. 克隆项目到本地
2. 配置数据库连接字符串
   - 打开 `StudentManagement.API/appsettings.json`
   - 修改 MySQL连接字符串中的用户名和密码：
     ```json
     "DefaultConnection": "Server=localhost;Database=StudentManagement;User=你的用户名;Password=你的密码;Port=3306;CharSet=utf8mb4;AllowLoadLocalInfile=true"
     ```
3. 启动后端服务
   - Windows: 双击运行 `StudentManagement.API/start.bat`
   - Linux/Mac: 在终端中运行 `./start.sh`
4. 启动前端服务
   - Windows: 双击运行 `student-management-ui/start.bat`
   - Linux/Mac: 在终端中运行 `./start.sh`
5. 在浏览器中访问 `http://localhost:7681`
6. Swagger UI 地址 `http://localhost:5284/swagger/index.html`

## 默认账号
- 超级管理员：superadmin / 123456
