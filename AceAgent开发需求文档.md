# AceAgent 实用需求文档

**项目**: AceAgent - C# AI编程助手  
**版本**: v2.0 (务实版)  
**日期**: 2025年1月  

---

## 1. 项目目标

### 做什么
创建一个C#版本的AI编程助手，能够：
- 自动编辑代码文件
- 执行命令行操作
- 生成项目文档
- 记录操作过程

### 为什么做
- Python的Trae Agent很好用，但在.NET项目中使用不方便
- 需要一个原生的C#工具，更好地集成到.NET开发流程
- 企业环境更容易接受.NET工具

### 成功标准
- 能完成Trae Agent 80%的常用功能
- 安装和使用比Trae Agent更简单
- 在.NET项目中的表现比Trae Agent更好

---

## 2. 核心功能

### 2.1 文件编辑功能

**具体需求**：
```csharp
// 用户输入："在Program.cs中添加日志配置"
// 工具应该能够：
1. 找到Program.cs文件
2. 识别合适的插入位置
3. 添加日志相关代码
4. 保存文件
```

**必须支持的文件类型**：
- .cs (C#代码)
- .csproj (项目文件)
- .json (配置文件)
- .md (文档)
- .yml/.yaml (配置)

**安全要求**：
- 编辑前自动备份
- 只能编辑工作目录内的文件
- 不能编辑系统文件

**验收标准**：
- 能正确处理UTF-8编码
- 保持原文件的换行符格式
- 编辑后代码能正常编译

### 2.2 命令执行功能

**具体需求**：
```bash
# 用户说："构建项目并运行测试"
# 工具应该执行：
dotnet build
dotnet test
```

**必须支持的命令**：
- `dotnet` 相关命令
- `git` 基础操作
- `npm`/`yarn` (前端项目)
- 文件操作命令 (cp, mv, mkdir等)

**安全限制**：
- 禁止执行系统管理命令
- 禁止网络相关命令
- 禁止删除重要文件

**验收标准**：
- 命令执行超时控制 (30秒)
- 实时显示命令输出
- 正确处理命令失败情况

### 2.3 AI对话功能

**具体需求**：
- 支持OpenAI GPT-4
- 支持Azure OpenAI
- 支持本地模型 (可选)

**配置示例**：
```yaml
llm:
  provider: "openai"
  model: "gpt-4"
  api_key: "${OPENAI_API_KEY}"
  max_tokens: 4000
```

**验收标准**：
- API调用失败时有重试机制
- 支持流式响应
- Token使用量统计

### 2.4 操作记录功能

**具体需求**：
- 记录每次对话的完整过程
- 记录所有文件修改
- 记录所有命令执行

**存储格式**：
```json
{
  "session_id": "uuid",
  "timestamp": "2025-01-20T10:30:00Z",
  "user_input": "添加日志配置",
  "actions": [
    {
      "type": "file_edit",
      "file": "Program.cs",
      "changes": "..."
    }
  ],
  "result": "success"
}
```

**验收标准**：
- 能查看历史记录
- 能重放操作过程
- 记录文件大小控制在合理范围

---

## 3. 用户界面

### 3.1 命令行界面

**基础命令**：
```bash
# 安装
dotnet tool install -g AceAgent

# 基本使用
ace "创建一个Web API项目"
ace "添加Entity Framework"
ace "生成用户管理的CRUD接口"

# 交互模式
ace --interactive

# 查看历史
ace --history
ace --replay <session-id>
```

**配置命令**：
```bash
# 设置API密钥
ace config set openai.api_key "your-key"

# 查看配置
ace config show

# 测试配置
ace config test
```

### 3.2 输出格式

**进度显示**：
```
🤖 AceAgent v1.0
📝 分析需求: 创建Web API项目
🔍 检查当前目录...
✅ 目录检查完成
🏗️  创建项目结构...
  ├── 创建 Controllers/
  ├── 创建 Models/
  └── 创建 Program.cs
✅ 项目创建完成
📊 总用时: 15秒, Token使用: 1,234
```

**错误显示**：
```
❌ 错误: 无法创建文件 Program.cs
💡 建议: 检查目录权限或文件是否已存在
🔧 解决方案: 运行 'chmod +w .' 或删除现有文件
```

---

## 4. 技术实现

### 4.1 项目结构
```
AceAgent/
├── src/
│   ├── AceAgent.Core/          # 核心逻辑
│   ├── AceAgent.Tools/         # 工具实现
│   ├── AceAgent.LLM/          # AI模型集成
│   └── AceAgent.CLI/          # 命令行界面
├── tests/
└── docs/
```

### 4.2 关键依赖
```xml
<PackageReference Include="System.CommandLine" Version="2.0.0" />
<PackageReference Include="YamlDotNet" Version="13.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Spectre.Console" Version="0.47.0" />
```

### 4.3 核心接口
```csharp
public interface ITool
{
    string Name { get; }
    Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken);
}

public interface ILLMProvider
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken);
}

public interface ITrajectoryRecorder
{
    Task RecordAsync(TrajectoryStep step);
    Task<List<TrajectoryStep>> GetHistoryAsync();
}
```

---

## 5. 开发计划

### 第一阶段 (4周) - MVP
- [ ] 基础CLI框架
- [ ] OpenAI集成
- [ ] 文件编辑工具
- [ ] 简单命令执行

### 第二阶段 (4周) - 完善功能
- [ ] 操作记录
- [ ] 配置管理
- [ ] 错误处理
- [ ] 单元测试

### 第三阶段 (4周) - 优化发布
- [ ] 性能优化
- [ ] 文档完善
- [ ] NuGet发布
- [ ] 用户反馈收集

---

## 6. 实际使用场景

### 6.1 日常开发场景

**场景1: 快速创建项目**
```bash
# 用户输入
ace "创建一个Web API项目，包含用户注册登录功能"

# 期望执行步骤
1. dotnet new webapi -n UserManagementAPI
2. cd UserManagementAPI
3. dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
4. 创建 Models/User.cs
5. 创建 Controllers/AuthController.cs
6. 修改 Program.cs 添加Identity配置
7. dotnet build (验证编译成功)

# 验收标准
- 项目能正常编译
- 包含完整的注册/登录API
- 代码符合C#规范
```

**场景2: 代码重构**
```bash
# 用户输入
ace "将UserController中的重复代码提取到BaseController"

# 期望执行步骤
1. 分析UserController.cs中的重复代码
2. 创建 Controllers/BaseController.cs
3. 将公共方法移动到BaseController
4. 修改UserController继承BaseController
5. 更新其他Controller使用BaseController

# 验收标准
- 代码重复度降低
- 所有Controller正常工作
- 保持原有API接口不变
```

**场景3: 添加新功能**
```bash
# 用户输入
ace "给用户管理添加头像上传功能"

# 期望执行步骤
1. 在User模型中添加AvatarUrl属性
2. 创建文件上传的Controller方法
3. 添加文件存储配置
4. 更新数据库迁移
5. 添加相关的DTO类

# 验收标准
- 能成功上传图片文件
- 文件大小和格式验证
- 数据库正确存储文件路径
```

**场景4: 问题诊断**
```bash
# 用户输入
ace "项目启动时出现数据库连接错误，帮我检查和修复"

# 期望执行步骤
1. 检查appsettings.json中的连接字符串
2. 验证数据库服务是否运行
3. 检查Entity Framework配置
4. 运行数据库迁移
5. 测试数据库连接

# 验收标准
- 识别出具体的错误原因
- 提供可行的解决方案
- 项目能正常启动
```

### 6.2 团队协作场景

**场景5: 代码规范检查**
```bash
# 用户输入
ace "检查项目代码规范，修复不符合团队标准的地方"

# 期望执行步骤
1. 运行代码分析工具
2. 检查命名规范
3. 修复代码格式问题
4. 添加缺失的XML注释
5. 统一using语句顺序

# 验收标准
- 代码通过静态分析
- 符合团队编码规范
- 公共API有完整注释
```

**场景6: 文档生成**
```bash
# 用户输入
ace "为当前API项目生成完整的技术文档"

# 期望执行步骤
1. 分析项目结构和API接口
2. 生成API文档(Swagger)
3. 创建README.md
4. 生成部署说明
5. 创建开发环境搭建指南

# 验收标准
- 文档内容准确完整
- 包含所有API接口说明
- 新开发者能根据文档快速上手
```

### 6.3 性能测试场景

**场景7: 性能优化**
```bash
# 用户输入
ace "分析项目性能瓶颈，优化数据库查询"

# 期望执行步骤
1. 分析现有数据库查询
2. 识别N+1查询问题
3. 添加适当的Include语句
4. 优化复杂查询
5. 添加数据库索引建议

# 验收标准
- 查询性能提升明显
- 减少数据库往返次数
- 提供性能对比报告
```

### 6.4 验收标准

**功能验收**:
- [ ] 能处理上述7个典型场景
- [ ] 每个场景的成功率 > 90%
- [ ] 生成的代码能正常编译运行
- [ ] 操作过程有清晰的进度提示

**性能验收**:
- [ ] 工具启动时间 < 3秒
- [ ] 简单任务(文件编辑) < 10秒
- [ ] 复杂任务(项目创建) < 60秒
- [ ] 内存使用 < 300MB

**用户体验验收**:
- [ ] 错误信息清晰易懂
- [ ] 提供具体的解决建议
- [ ] 支持操作撤销和重做
- [ ] 有完整的操作日志

**兼容性验收**:
- [ ] Windows 10+ (.NET 8.0)
- [ ] macOS 12+ (.NET 8.0)
- [ ] Ubuntu 20.04+ (.NET 8.0)
- [ ] 支持主流IDE集成

---

## 7. 风险和限制

### 7.1 技术风险
- **AI模型限制**: GPT-4的上下文长度限制可能影响大文件处理
- **解决方案**: 实现文件分块处理

- **命令执行安全**: 恶意命令可能损坏系统
- **解决方案**: 严格的命令白名单和沙箱机制

### 7.2 使用限制
- 需要网络连接 (AI API调用)
- 需要API密钥 (OpenAI等)
- 主要适用于.NET项目

### 7.3 成本考虑
- OpenAI API调用费用
- 建议用户自己申请API密钥
- 提供本地模型选项降低成本

---

## 8. 后续规划

### 8.1 短期目标 (3个月)
- 完成MVP版本
- 收集用户反馈
- 修复主要问题

### 8.2 中期目标 (6个月)
- 支持更多AI模型
- 添加Visual Studio扩展
- 支持团队协作功能

### 8.3 长期目标 (1年)
- 构建插件生态
- 支持自定义工具
- 企业版功能

---

**文档说明**:
- 这是一个务实的需求文档，专注于具体的功能和实现
- 每个需求都有明确的验收标准
- 避免过度设计，优先实现核心功能
- 基于实际使用场景制定需求