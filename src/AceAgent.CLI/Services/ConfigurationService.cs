using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.Extensions.Logging;

namespace AceAgent.CLI.Services
{
    /// <summary>
    /// 配置管理服务
    /// </summary>
    public class ConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;
        private readonly string _defaultConfigPath;
        private Dictionary<string, object> _configuration;
        private readonly ISerializer _yamlSerializer;
        private readonly IDeserializer _yamlDeserializer;

        /// <summary>
        /// 初始化ConfigurationService实例
        /// </summary>
        /// <param name="logger">日志记录器</param>
        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
            _defaultConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aceagent", "config.yaml");
            _configuration = new Dictionary<string, object>();
            
            _yamlSerializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            
            _yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            // 尝试加载默认配置
            _ = Task.Run(async () =>
            {
                try
                {
                    await LoadConfigAsync(_defaultConfigPath);
                }
                catch
                {
                    // 忽略加载默认配置的错误
                }
            });
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public async Task LoadConfigAsync(string configPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    _logger.LogWarning($"配置文件不存在: {configPath}");
                    return;
                }

                var yamlContent = await File.ReadAllTextAsync(configPath);
                var config = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yamlContent);
                
                if (config != null)
                {
                    _configuration = config;
                    _logger.LogInformation($"已加载配置文件: {configPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载配置文件失败: {configPath}");
                throw;
            }
        }

        /// <summary>
        /// 确保配置已加载
        /// </summary>
        private async Task EnsureConfigLoadedAsync()
        {
            if (_configuration.Count == 0 && File.Exists(_defaultConfigPath))
            {
                await LoadConfigAsync(_defaultConfigPath);
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public async Task SaveConfigAsync(string? configPath = null)
        {
            try
            {
                configPath ??= _defaultConfigPath;
                
                // 确保目录存在
                var directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var yamlContent = _yamlSerializer.Serialize(_configuration);
                await File.WriteAllTextAsync(configPath, yamlContent);
                
                _logger.LogInformation($"配置已保存到: {configPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存配置文件失败: {configPath}");
                throw;
            }
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        public async Task<string?> GetConfigAsync(string key)
        {
            // 确保配置已加载
            await EnsureConfigLoadedAsync();
            
            if (_configuration.TryGetValue(key, out var value))
            {
                return value?.ToString();
            }

            // 尝试从环境变量获取
            var envKey = $"ACEAGENT_{key.ToUpperInvariant()}";
            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }

            return null;
        }

        /// <summary>
        /// 设置配置值
        /// </summary>
        public async Task SetConfigAsync(string key, string value)
        {
            // 确保配置已加载
            await EnsureConfigLoadedAsync();
            _configuration[key] = value;
            await SaveConfigAsync();
            _logger.LogDebug($"设置配置: {key} = {value}");
        }

        /// <summary>
        /// 获取所有配置
        /// </summary>
        public async Task<Dictionary<string, object>> GetAllConfigsAsync()
        {
            // 确保配置已加载
            await EnsureConfigLoadedAsync();
            return new Dictionary<string, object>(_configuration);
        }

        /// <summary>
        /// 删除配置项
        /// </summary>
        public async Task RemoveConfigAsync(string key)
        {
            if (_configuration.Remove(key))
            {
                await SaveConfigAsync();
                _logger.LogDebug($"删除配置: {key}");
            }
        }

        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        public async Task<bool> HasConfigAsync(string key)
        {
            await Task.CompletedTask;
            return _configuration.ContainsKey(key) || 
                   !string.IsNullOrEmpty(Environment.GetEnvironmentVariable($"ACEAGENT_{key.ToUpperInvariant()}"));
        }

        /// <summary>
        /// 初始化默认配置
        /// </summary>
        public async Task InitializeDefaultConfigAsync()
        {
            var defaultConfig = new Dictionary<string, object>
            {
                // LLM提供商配置
                ["default_provider"] = "openai",
                ["openai_default_model"] = "gpt-4",
                ["anthropic_default_model"] = "claude-3-sonnet-20240229",
                ["doubao_default_model"] = "doubao-pro-4k",
                
                // 生成参数
                ["temperature"] = 0.7,
                ["max_tokens"] = 2000,
                ["top_p"] = 1.0,
                
                // 工具配置
                ["file_edit_backup"] = true,
                ["command_timeout"] = 30,
                
                // 轨迹记录配置
                ["trajectory_enabled"] = true,
                ["trajectory_db_path"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aceagent", "trajectories.db"),
                
                // 日志配置
                ["log_level"] = "Information",
                ["log_file"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aceagent", "logs", "aceagent.log")
            };

            foreach (var kvp in defaultConfig)
            {
                if (!_configuration.ContainsKey(kvp.Key))
                {
                    _configuration[kvp.Key] = kvp.Value;
                }
            }

            await SaveConfigAsync();
            _logger.LogInformation("默认配置已初始化");
        }

        /// <summary>
        /// 验证必需的配置
        /// </summary>
        public async Task<List<string>> ValidateRequiredConfigsAsync()
        {
            var missingConfigs = new List<string>();
            var requiredConfigs = new[]
            {
                "openai_api_key",
                "anthropic_api_key", 
                "doubao_api_key"
            };

            foreach (var config in requiredConfigs)
            {
                if (!await HasConfigAsync(config))
                {
                    missingConfigs.Add(config);
                }
            }

            return missingConfigs;
        }

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        public string GetConfigPath()
        {
            return _defaultConfigPath;
        }

        /// <summary>
        /// 重置配置到默认值
        /// </summary>
        public async Task ResetConfigAsync()
        {
            _configuration.Clear();
            await InitializeDefaultConfigAsync();
            _logger.LogInformation("配置已重置为默认值");
        }

        /// <summary>
        /// 导出配置到指定路径
        /// </summary>
        public async Task ExportConfigAsync(string exportPath)
        {
            try
            {
                var yamlContent = _yamlSerializer.Serialize(_configuration);
                await File.WriteAllTextAsync(exportPath, yamlContent);
                _logger.LogInformation($"配置已导出到: {exportPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"导出配置失败: {exportPath}");
                throw;
            }
        }

        /// <summary>
        /// 从指定路径导入配置
        /// </summary>
        public async Task ImportConfigAsync(string importPath)
        {
            try
            {
                if (!File.Exists(importPath))
                {
                    throw new FileNotFoundException($"配置文件不存在: {importPath}");
                }

                await LoadConfigAsync(importPath);
                await SaveConfigAsync(); // 保存到默认位置
                _logger.LogInformation($"配置已从 {importPath} 导入");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"导入配置失败: {importPath}");
                throw;
            }
        }
    }
}