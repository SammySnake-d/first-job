@echo off
echo 检查后端项目...

REM 检查是否安装了 Entity Framework Core 工具
dotnet tool install --global dotnet-ef 2>nul
if %ERRORLEVEL% NEQ 0 echo Entity Framework Core 工具已安装

REM 清理项目
echo 清理项目...
dotnet clean

REM 检查并还原 NuGet 包
echo 还原项目依赖...
dotnet restore --force

REM 应用数据库迁移
echo 应用数据库迁移...
dotnet ef database update

REM 启动项目
echo 启动后端服务...
dotnet run 