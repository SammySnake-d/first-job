@echo off
echo 检查前端项目...

+ REM 检查 package.json 是否存在
+ if not exist "package.json" (
+     echo "错误: package.json 文件不存在"
+     exit /b 1
+ )

if not exist "node_modules" (
    echo 正在安装前端依赖...
+   call npm install --force
)

+ REM 检查必要的依赖是否安装
+ echo 检查核心依赖...
+ call npm ls vue element-plus pinia vue-router axios --json > nul 2>&1
+ if %ERRORLEVEL% NEQ 0 (
+     echo 重新安装依赖...
+     call npm install --force
+ )

echo 启动前端服务...
call npm run dev 