using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Models;

namespace AceAgent.Core.Interfaces
{
    /// <summary>
    /// LLM提供商接口
    /// </summary>
    public interface ILLMProvider
    {
        /// <summary>
        /// 提供商名称
        /// </summary>
        string ProviderName { get; }
        
        /// <summary>
        /// 生成响应
        /// </summary>
        /// <param name="messages">消息列表</param>
        /// <param name="options">LLM选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>模型响应</returns>
        Task<ModelResponse> GenerateResponseAsync(
            IEnumerable<Message> messages, 
            LLMOptions? options = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 获取支持的模型列表
        /// </summary>
        /// <returns>模型名称列表</returns>
        IEnumerable<string> GetSupportedModels();
        
        /// <summary>
        /// 验证配置
        /// </summary>
        /// <returns>配置是否有效</returns>
        Task<bool> ValidateConfigurationAsync();
        
        /// <summary>
        /// 获取模型信息
        /// </summary>
        /// <param name="modelName">模型名称</param>
        /// <returns>模型信息</returns>
        ModelInfo? GetModelInfo(string modelName);
    }
}