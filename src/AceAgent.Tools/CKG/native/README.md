# Native Libraries Build System

本目录包含用于构建跨平台原生库的配置和源代码。

## 目录结构

```
native/
├── CMakeLists.txt          # 主要的CMake构建配置
├── ckg_wrapper.cpp         # C++包装器源代码
├── ckg_wrapper.h           # C++包装器头文件
├── test_all_languages.cpp  # 测试程序
├── languages/              # Tree-sitter语言解析器
│   ├── tree-sitter-c/
│   ├── tree-sitter-cpp/
│   ├── tree-sitter-csharp/
│   ├── tree-sitter-go/
│   ├── tree-sitter-java/
│   ├── tree-sitter-javascript/
│   ├── tree-sitter-python/
│   ├── tree-sitter-rust/
│   ├── tree-sitter-typescript/
│   └── tree-sitter-tsx/
├── build/                  # 本地构建目录（被gitignore忽略）
└── runtimes/              # 跨平台编译产物
    ├── win-x64/native/
    ├── linux-x64/native/
    ├── osx-x64/native/
    └── osx-arm64/native/
```

## 本地构建

### 前置要求
- CMake 3.13+
- C/C++编译器（GCC、Clang或MSVC）
- Make或Ninja

### 构建步骤

```bash
cd src/AceAgent.Tools/CKG/native
mkdir build && cd build
cmake ..
make
```

### 测试

```bash
./test_all_languages
```

### 清理

```bash
make clean-all
```

## GitHub Actions自动构建

项目配置了GitHub Actions工作流（`.github/workflows/build-native.yml`），可以自动在多个平台上构建原生库：

- **Windows x64** - 生成 `ckg_wrapper.dll`
- **Linux x64** - 生成 `libckg_wrapper.so`
- **macOS x64** - 生成 `ckg_wrapper.dylib`
- **macOS ARM64** - 生成 `ckg_wrapper.dylib`

### 触发条件

- 推送到 `main` 或 `develop` 分支
- 创建针对 `main` 或 `develop` 分支的Pull Request
- 手动触发（workflow_dispatch）

### 编译产物管理

编译完成后，GitHub Actions会自动：
1. 将编译产物上传为构建工件
2. 在推送到 `main` 分支时，自动将编译产物提交到仓库的 `runtimes/` 目录

## 支持的语言和ABI版本

当前支持的Tree-sitter语言解析器：

| 语言 | ABI版本 | 状态 |
|------|---------|------|
| C | 14 | ✅ |
| C++ | 14 | ✅ |
| C# | 14 | ✅ |
| Go | 14 | ✅ |
| Java | 14 | ✅ |
| JavaScript | 15 | ✅ |
| Python | 14 | ✅ |
| Rust | 15 | ✅ |
| TypeScript | 14 | ✅ |
| TSX | 14 | ✅ |

## 故障排除

### 常见问题

1. **CMake配置失败**
   - 确保CMake版本 >= 3.13
   - 检查编译器是否正确安装

2. **链接错误**
   - 确保所有子模块都已正确初始化
   - 检查Tree-sitter库是否正确构建

3. **测试失败**
   - 检查动态库是否正确生成
   - 确保所有语言解析器都已编译

### 调试构建

```bash
# 启用详细输出
cmake .. -DCMAKE_VERBOSE_MAKEFILE=ON
make VERBOSE=1

# 调试模式构建
cmake .. -DCMAKE_BUILD_TYPE=Debug
make
```