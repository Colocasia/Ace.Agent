using System;
using System.Collections.Generic;
using AceAgent.Core.Interfaces;

namespace AceAgent.LLM
{
    /// <summary>
    /// LLM提供商工厂
    /// </summary>
    public class LLMProviderFactory
    {
        private readonly Dictionary<string, Func<LLMProviderConfig, ILLMProvider>> _providers;

        public LLMProviderFactory()
        {
            _providers = new Dictionary<string, Func<LLMProviderConfig, ILLMProvider>>(StringComparer.OrdinalIgnoreCase)
            {
                ["openai"] = config => new OpenAIProvider(config.ApiKey, config.BaseUrl),
                ["anthropic"] = config => new AnthropicProvider(config.ApiKey, config.BaseUrl),
                ["doubao"] = config => new DoubaoProvider(config.ApiKey, config.BaseUrl)
            };
        }

        /// <summary>
        /// 创建LLM提供商实例
        /// </summary>
        /// <param name="providerName">提供商名称</param>
        /// <param name="config">配置信息</param>
        /// <returns>LLM提供商实例</returns>
        public ILLMProvider CreateProvider(string providerName, LLMProviderConfig config)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("提供商名称不能为空", nameof(providerName));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!_providers.TryGetValue(providerName, out var factory))
                throw new NotSupportedException($"不支持的LLM提供商: {providerName}");

            return factory(config);
        }

        /// <summary>
        /// 获取支持的提供商列表
        /// </summary>
        /// <returns>提供商名称列表</returns>
        public IEnumerable<string> GetSupportedProviders()
        {
            return _providers.Keys;
        }

        /// <summary>
        /// 注册自定义提供商
        /// </summary>
        /// <param name="providerName">提供商名称</param>
        /// <param name="factory">提供商工厂方法</param>
        public void RegisterProvider(string providerName, Func<LLMProviderConfig, ILLMProvider> factory)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("提供商名称不能为空", nameof(providerName));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _providers[providerName] = factory;
        }

        /// <summary>
        /// 检查是否支持指定提供商
        /// </summary>
        /// <param name="providerName">提供商名称</param>
        /// <returns>是否支持</returns>
        public bool IsProviderSupported(string providerName)
        {
            return !string.IsNullOrWhiteSpace(providerName) && _providers.ContainsKey(providerName);
        }
    }

    /// <summary>
    /// LLM提供商配置
    /// </summary>
    public class LLMProviderConfig
    {
        /// <summary>
        /// API密钥
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// 基础URL
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// 默认模型
        /// </summary>
        public string? DefaultModel { get; set; }

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// 附加配置
        /// </summary>
        public Dictionary<string, object> AdditionalConfig { get; set; } = new();
    }
}