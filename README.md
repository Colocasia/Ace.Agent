# AceAgent

[![Build Native Libraries](https://github.com/Colocasia/Ace.Agent/actions/workflows/build-native.yml/badge.svg)](https://github.com/Colocasia/Ace.Agent/actions/workflows/build-native.yml)

> 基于.NET 8的智能代理系统，灵感来源于 [trae-agent](https://github.com/bytedance/trae-agent)

AceAgent是一个功能强大的LLM驱动的代理系统，专为通用软件工程任务而设计。它提供了强大的CLI界面，能够理解自然语言指令并使用各种工具和LLM提供商执行复杂的软件工程工作流程。

## 🧪 测试状态

| 平台 | 架构 | 状态 | 说明 |
|------|------|------|------|
| Linux | x64 | ✅ 通过 | 支持所有语言解析器测试 |
| Windows | x64 | ✅ 通过 | 支持exe+dll方式运行测试 |
| macOS | x64 | ✅ 通过 | Intel芯片Mac支持 |
| macOS | ARM64 | ✅ 通过 | Apple Silicon Mac支持 |

所有平台都支持以下语言的代码解析测试：
- C/C++
- C#
- Java
- JavaScript
- Python
- Rust
- TypeScript

## ✨ 特性

🌊 **Lakeview总结**: 为代理步骤提供简洁的总结分析  
🤖 **多LLM支持**: 支持OpenAI、Anthropic、Doubao等API  
🛠️ **丰富的工具生态**: 文件编辑、命令执行、推理分析等  
🎯 **CLI界面**: 支持聊天模式和任务执行模式  
📊 **轨迹记录**: 详细记录所有代理操作，便于调试和分析  
⚙️ **灵活配置**: 基于YAML的配置系统，支持环境变量  
🚀 **类型安全**: 基于C#/.NET的强类型系统，提供更好的可靠性

## 🚀 快速开始

### 环境要求

- .NET 8 SDK
- 选择的LLM提供商的API密钥 (OpenAI, Anthropic, Doubao等)

### 安装

```bash
git clone https://github.com/your-username/AceAgent.git
cd AceAgent
dotnet restore
dotnet build
```

### ⚙️ 配置

1. 初始化配置文件：
```bash
dotnet run --project src/AceAgent.CLI -- config init
```

2. 设置API密钥：
```bash
# OpenAI
dotnet run --project src/AceAgent.CLI -- config set openai_api_key "your-openai-api-key"

# Anthropic
dotnet run --project src/AceAgent.CLI -- config set anthropic_api_key "your-anthropic-api-key"

# Doubao
dotnet run --project src/AceAgent.CLI -- config set doubao_api_key "your-doubao-api-key"
```

3. 验证配置：
```bash
dotnet run --project src/AceAgent.CLI -- config validate
```

## 📖 使用方法

### 基本命令

```bash
# 任务执行
dotnet run --project src/AceAgent.CLI -- execute "创建一个Hello World Python脚本"

# 聊天模式
dotnet run --project src/AceAgent.CLI -- chat

# 查看配置
dotnet run --project src/AceAgent.CLI -- config list

# 轨迹管理
dotnet run --project src/AceAgent.CLI -- trajectory list
```

### 指定提供商和模型

```bash
# 使用OpenAI
dotnet run --project src/AceAgent.CLI -- execute "修复main.py中的bug" --provider openai --model gpt-4

# 使用Anthropic
dotnet run --project src/AceAgent.CLI -- execute "添加单元测试" --provider anthropic --model claude-3-sonnet-20240229

# 使用Doubao
dotnet run --project src/AceAgent.CLI -- execute "重构数据库模块" --provider doubao --model doubao-seed-1.6
```

### 高级选项

```bash
# 保存执行轨迹
dotnet run --project src/AceAgent.CLI -- execute "调试认证问题" --save-trajectory

# 详细输出
dotnet run --project src/AceAgent.CLI -- chat --verbose

# 自定义配置文件
dotnet run --project src/AceAgent.CLI -- execute "更新API端点" --config-file custom-config.yaml
```

## 🏗️ 项目架构

```
AceAgent/
├── src/
│   ├── AceAgent.Core/          # 核心接口和模型
│   ├── AceAgent.LLM/           # LLM提供商实现
│   ├── AceAgent.Tools/         # 工具系统
│   ├── AceAgent.Services/      # Lakeview等服务
│   ├── AceAgent.Infrastructure/ # 基础设施(数据库等)
│   └── AceAgent.CLI/           # CLI界面
├── tests/
│   └── AceAgent.Tests/         # 单元测试
└── docs/                       # 文档
```

## 🛠️ 工具系统

AceAgent内置了丰富的工具集：

- **FileEditTool**: 基于字符串替换的安全文件编辑
- **CommandExecutorTool**: 跨平台命令执行，支持安全策略
- **ReasoningTool**: 结构化推理和问题分析
- **TaskCompletionTool**: 任务完成状态管理

## 📊 轨迹记录

所有代理操作都会被详细记录，包括：
- 执行步骤和时间
- 输入输出数据
- 错误信息和恢复过程
- 性能指标

```bash
# 查看轨迹列表
dotnet run --project src/AceAgent.CLI -- trajectory list

# 查看特定轨迹
dotnet run --project src/AceAgent.CLI -- trajectory show <trajectory-id>

# 删除轨迹
dotnet run --project src/AceAgent.CLI -- trajectory delete <trajectory-id>
```

## 🌊 Lakeview总结

Lakeview服务提供智能的轨迹分析和总结：
- 执行步骤概览
- 性能指标分析
- 错误模式识别
- 改进建议

## ⚙️ 配置系统

配置文件位于 `~/.aceagent/config.yaml`：

```yaml
default_provider: openai
openai_api_key: your-key
openai_default_model: gpt-4
anthropic_api_key: your-key
anthropic_default_model: claude-3-sonnet-20240229
doubao_api_key: your-key
doubao_default_model: doubao-seed-1.6
max_tokens: 4096
temperature: 0.7
# ... 更多配置选项
```

## 🔄 与Trae-Agent的关系

AceAgent深受 [trae-agent](https://github.com/bytedance/trae-agent) 启发，在保持核心功能对齐的同时，提供了以下优势：

### 相同的核心功能
- ✅ Lakeview总结系统
- ✅ 多LLM提供商支持
- ✅ 丰富的工具生态系统
- ✅ 轨迹记录和分析
- ✅ 灵活的配置管理

### AceAgent的独特优势
- **类型安全**: C#强类型系统提供更好的可靠性
- **性能**: 编译型语言的性能优势
- **模块化**: 清晰的分层架构和依赖注入
- **跨平台**: .NET 8的优秀跨平台支持
- **企业级**: 适合企业环境的安全性和可维护性

详细的功能对比请参考 [功能对比报告](./功能对比报告.md)。

## 🧪 测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试
dotnet test --filter "TestName"
```

## 📝 开发

### 添加新的LLM提供商

1. 实现 `ILLMProvider` 接口
2. 在 `LLMProviderFactory` 中注册
3. 添加相应的配置选项

### 添加新工具

1. 实现 `ITool` 接口
2. 在DI容器中注册
3. 更新工具注册逻辑

## 🤝 贡献

欢迎贡献代码！请遵循以下步骤：

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🙏 致谢

- 感谢 [trae-agent](https://github.com/bytedance/trae-agent) 项目提供的灵感和参考
- 感谢所有LLM提供商为AI发展做出的贡献
- 感谢.NET社区的支持和贡献

## 📞 联系

如有问题或建议，请通过以下方式联系：
- 提交 Issue
- 发起 Discussion
- 发送邮件至 [your-email@example.com]

---

**AceAgent - 让AI代理更可靠、更强大！** 🚀