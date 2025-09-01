# AceAgent 配置管理

AceAgent 使用 YAML 格式的配置文件来管理各种设置和参数。

## 配置文件位置

默认配置文件位置：`~/.aceagent/config.yaml`

## 快速开始

1. 复制示例配置文件：
   ```bash
   mkdir -p ~/.aceagent
   cp config.example.yaml ~/.aceagent/config.yaml
   ```

2. 编辑配置文件，设置你的 API 密钥：
   ```bash
   nano ~/.aceagent/config.yaml
   ```

3. 至少需要配置一个 LLM 提供商的 API 密钥：
   - OpenAI: `openai_api_key`
   - Anthropic: `anthropic_api_key`
   - 豆包: `doubao_api_key`

## 配置项说明

### LLM 提供商配置

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `default_provider` | 默认使用的 LLM 提供商 | `openai` |
| `openai_api_key` | OpenAI API 密钥 | - |
| `openai_default_model` | OpenAI 默认模型 | `gpt-4` |
| `openai_base_url` | OpenAI API 基础 URL | `https://api.openai.com/v1` |
| `anthropic_api_key` | Anthropic API 密钥 | - |
| `anthropic_default_model` | Anthropic 默认模型 | `claude-3-sonnet-20240229` |
| `doubao_api_key` | 豆包 API 密钥 | - |
| `doubao_default_model` | 豆包默认模型 | `doubao-pro-4k` |
| `doubao_base_url` | 豆包 API 基础 URL | `https://ark.cn-beijing.volces.com/api/v3` |

### 生成参数

| 配置项 | 说明 | 默认值 | 范围 |
|--------|------|--------|------|
| `temperature` | 生成温度，控制随机性 | `0.7` | 0.0-2.0 |
| `max_tokens` | 最大生成 token 数 | `2000` | 1-4096 |
| `top_p` | 核采样参数 | `1.0` | 0.0-1.0 |

### 工具配置

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `file_edit_backup` | 编辑文件前是否创建备份 | `true` |
| `command_timeout` | 命令执行超时时间（秒） | `30` |

### 轨迹记录配置

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `trajectory_enabled` | 是否启用轨迹记录 | `true` |
| `trajectory_db_path` | 轨迹数据库文件路径 | `~/.aceagent/trajectories.db` |
| `auto_save_trajectory` | 是否自动保存轨迹 | `true` |

### 日志配置

| 配置项 | 说明 | 默认值 | 可选值 |
|--------|------|--------|--------|
| `log_level` | 日志级别 | `Information` | `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical` |
| `log_file` | 日志文件路径 | `~/.aceagent/logs/aceagent.log` | - |

## 环境变量支持

所有配置项都可以通过环境变量覆盖，环境变量名格式为 `ACEAGENT_<配置项名大写>`。

例如：
```bash
export ACEAGENT_OPENAI_API_KEY="your-api-key"
export ACEAGENT_DEFAULT_PROVIDER="anthropic"
export ACEAGENT_TEMPERATURE="0.5"
```

环境变量的优先级高于配置文件。

## 命令行配置管理

AceAgent 提供了命令行工具来管理配置：

```bash
# 查看当前配置
aceagent config list

# 设置配置项
aceagent config set openai_api_key "your-api-key"

# 获取配置项
aceagent config get default_provider

# 删除配置项
aceagent config remove openai_api_key

# 重置配置到默认值
aceagent config reset

# 验证配置
aceagent config validate

# 导出配置
aceagent config export backup.yaml

# 导入配置
aceagent config import backup.yaml
```

## 配置验证

启动时，AceAgent 会验证必需的配置项：

- 至少需要配置一个 LLM 提供商的 API 密钥
- 配置文件格式必须正确
- 数值类型的配置项必须在有效范围内

如果配置验证失败，AceAgent 会显示错误信息并退出。

## 安全注意事项

1. **保护 API 密钥**：确保配置文件的权限设置正确，避免泄露 API 密钥
   ```bash
   chmod 600 ~/.aceagent/config.yaml
   ```

2. **使用环境变量**：在生产环境中，建议使用环境变量而不是配置文件来存储敏感信息

3. **定期轮换密钥**：定期更新 API 密钥以提高安全性

## 故障排除

### 配置文件不存在
如果配置文件不存在，AceAgent 会使用默认配置，但可能缺少必需的 API 密钥。

### 配置格式错误
如果 YAML 格式有误，AceAgent 会显示解析错误。请检查：
- 缩进是否正确（使用空格，不要使用制表符）
- 引号是否匹配
- 特殊字符是否正确转义

### API 密钥无效
如果 API 密钥无效，LLM 调用会失败。请检查：
- 密钥是否正确
- 密钥是否有足够的权限
- 账户是否有足够的余额

### 网络连接问题
如果无法连接到 LLM 服务，请检查：
- 网络连接是否正常
- 代理设置是否正确
- 防火墙是否阻止了连接