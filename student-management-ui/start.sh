#!/bin/bash

# 检查 package.json 是否存在
if [ ! -f "package.json" ]; then
    echo "错误: package.json 文件不存在"
    exit 1
fi

# 检查并安装前端依赖
if [ ! -d "node_modules" ]; then
    echo "正在安装前端依赖..."
    npm install --force
fi

# 检查必要的依赖是否安装
echo "检查核心依赖..."
if ! npm ls vue element-plus pinia vue-router axios --json > /dev/null 2>&1; then
    echo "重新安装依赖..."
    npm install --force
fi

# 启动前端服务
npm run dev 