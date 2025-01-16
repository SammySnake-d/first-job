#!/bin/bash

# 检查是否安装了 Entity Framework Core 工具
dotnet tool install --global dotnet-ef 2>/dev/null || true

# 清理项目
echo "清理项目..."
dotnet clean

# 检查并还原 NuGet 包
echo "还原项目依赖..."
dotnet restore --force

# 应用数据库迁移
echo "应用数据库迁移..."
dotnet ef database update

# 启动项目
echo "启动后端服务..."
dotnet run 