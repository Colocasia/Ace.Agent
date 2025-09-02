# AceAgent

[![Build Native Libraries](https://github.com/Colocasia/Ace.Agent/actions/workflows/build-native.yml/badge.svg)](https://github.com/Colocasia/Ace.Agent/actions/workflows/build-native.yml)

> A .NET 8-based intelligent agent system, inspired by [trae-agent](https://github.com/bytedance/trae-agent)

[中文文档](README.zh.md) | English

AceAgent is a powerful LLM-driven agent system designed for general software engineering tasks. It provides a robust CLI interface that can understand natural language instructions and execute complex software engineering workflows using various tools and LLM providers.

## 🧪 Test Status

| Platform | Architecture | Status | Description |
|----------|--------------|--------|-------------|
| Linux | x64 | ✅ Passed | Supports all language parser tests |
| Windows | x64 | ✅ Passed | Supports exe+dll test execution |
| macOS | x64 | ✅ Passed | Intel chip Mac support |
| macOS | ARM64 | ✅ Passed | Apple Silicon Mac support |

All platforms support code parsing tests for the following languages:
- C/C++
- C#
- Java
- JavaScript
- Python
- Rust
- TypeScript

## ✨ Features

🌊 **Lakeview Summaries**: Provides concise summary analysis for agent steps  
🤖 **Multi-LLM Support**: Supports OpenAI, Anthropic, Doubao, and other APIs  
🛠️ **Rich Tool Ecosystem**: File editing, command execution, reasoning analysis, and more  
🎯 **CLI Interface**: Supports both chat mode and task execution mode  
📊 **Trajectory Recording**: Detailed logging of all agent operations for debugging and analysis  
⚙️ **Flexible Configuration**: YAML-based configuration system with environment variable support  
🚀 **Type Safety**: C#/.NET strong type system provides better reliability

## 🚀 Quick Start

### Requirements

- .NET 8 SDK
- API keys for your chosen LLM provider (OpenAI, Anthropic, Doubao, etc.)

### Installation

```bash
git clone https://github.com/Colocasia/Ace.Agent.git
cd Ace.Agent
dotnet restore
dotnet build
```

### ⚙️ Configuration

1. Initialize configuration file:
```bash
dotnet run --project src/AceAgent.CLI -- config init
```

2. Set API keys:
```bash
# OpenAI
dotnet run --project src/AceAgent.CLI -- config set openai_api_key "your-openai-api-key"

# Anthropic
dotnet run --project src/AceAgent.CLI -- config set anthropic_api_key "your-anthropic-api-key"

# Doubao
dotnet run --project src/AceAgent.CLI -- config set doubao_api_key "your-doubao-api-key"
```

3. Validate configuration:
```bash
dotnet run --project src/AceAgent.CLI -- config validate
```

## 📖 Usage

### Basic Commands

```bash
# Task execution
dotnet run --project src/AceAgent.CLI -- execute "Create a Hello World Python script"

# Chat mode
dotnet run --project src/AceAgent.CLI -- chat

# View configuration
dotnet run --project src/AceAgent.CLI -- config list

# Trajectory management
dotnet run --project src/AceAgent.CLI -- trajectory list
```

### Specify Provider and Model

```bash
# Using OpenAI
dotnet run --project src/AceAgent.CLI -- execute "Fix bug in main.py" --provider openai --model gpt-4

# Using Anthropic
dotnet run --project src/AceAgent.CLI -- execute "Add unit tests" --provider anthropic --model claude-3-sonnet-20240229

# Using Doubao
dotnet run --project src/AceAgent.CLI -- execute "Refactor database module" --provider doubao --model doubao-seed-1.6
```

### Advanced Options

```bash
# Save execution trajectory
dotnet run --project src/AceAgent.CLI -- execute "Debug authentication issue" --save-trajectory

# Verbose output
dotnet run --project src/AceAgent.CLI -- chat --verbose

# Custom configuration file
dotnet run --project src/AceAgent.CLI -- execute "Update API endpoints" --config-file custom-config.yaml
```

## 🏗️ Project Architecture

```
AceAgent/
├── src/
│   ├── AceAgent.Core/          # Core interfaces and models
│   ├── AceAgent.LLM/           # LLM provider implementations
│   ├── AceAgent.Tools/         # Tool system
│   ├── AceAgent.Services/      # Services like Lakeview
│   ├── AceAgent.Infrastructure/ # Infrastructure (database, etc.)
│   └── AceAgent.CLI/           # CLI interface
├── tests/
│   └── AceAgent.Tests/         # Unit tests
└── docs/                       # Documentation
```

## 🛠️ Tool System

AceAgent comes with a rich set of built-in tools:

- **FileEditTool**: Safe file editing based on string replacement
- **CommandExecutorTool**: Cross-platform command execution with security policies
- **ReasoningTool**: Structured reasoning and problem analysis
- **TaskCompletionTool**: Task completion status management

## 📊 Trajectory Recording

All agent operations are logged in detail, including:
- Execution steps and timestamps
- Input/output data
- Error information and recovery processes
- Performance metrics

```bash
# View trajectory list
dotnet run --project src/AceAgent.CLI -- trajectory list

# View specific trajectory
dotnet run --project src/AceAgent.CLI -- trajectory show <trajectory-id>

# Delete trajectory
dotnet run --project src/AceAgent.CLI -- trajectory delete <trajectory-id>
```

## 🌊 Lakeview Summaries

The Lakeview service provides intelligent trajectory analysis and summaries:
- Execution step overview
- Performance metrics analysis
- Error pattern identification
- Improvement suggestions

## ⚙️ Configuration System

Configuration file is located at `~/.aceagent/config.yaml`:

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
# ... more configuration options
```

## 🔄 Relationship with Trae-Agent

AceAgent is deeply inspired by [trae-agent](https://github.com/bytedance/trae-agent). While maintaining core functionality alignment, it provides the following advantages:

### Same Core Features
- ✅ Lakeview summary system
- ✅ Multi-LLM provider support
- ✅ Rich tool ecosystem
- ✅ Trajectory recording and analysis
- ✅ Flexible configuration management

### AceAgent's Unique Advantages
- **Type Safety**: C# strong type system provides better reliability
- **Performance**: Compiled language performance advantages
- **Modularity**: Clear layered architecture and dependency injection
- **Cross-platform**: Excellent cross-platform support with .NET 8
- **Enterprise-grade**: Security and maintainability suitable for enterprise environments

For detailed feature comparison, please refer to the [Feature Comparison Report](./功能对比报告.md).

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific tests
dotnet test --filter "TestName"
```

## 📝 Development

### Adding New LLM Providers

1. Implement the `ILLMProvider` interface
2. Register in `LLMProviderFactory`
3. Add corresponding configuration options

### Adding New Tools

1. Implement the `ITool` interface
2. Register in the DI container
3. Update tool registration logic

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Thanks to the [trae-agent](https://github.com/bytedance/trae-agent) project for inspiration and reference
- Thanks to all LLM providers for their contributions to AI development
- Thanks to the .NET community for their support and contributions

## 📞 Contact

For questions or suggestions, please contact us through:
- Submit an Issue
- Start a Discussion
- Send email to [qq358277299@gmail.com]

---

**AceAgent - Making AI agents more reliable and powerful!** 🚀