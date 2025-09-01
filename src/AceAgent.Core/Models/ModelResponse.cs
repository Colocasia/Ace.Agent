using System;
using System.Collections.Generic;

namespace AceAgent.Core.Models
{
    /// <summary>
    /// 模型响应
    /// </summary>
    public class ModelResponse
    {
        /// <summary>
        /// 响应内容
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// 工具调用列表
        /// </summary>
        public List<ToolCall> ToolCalls { get; set; } = new();
        
        /// <summary>
        /// 使用的模型名称
        /// </summary>
        public string Model { get; set; } = string.Empty;
        
        /// <summary>
        /// 完成原因
        /// </summary>
        public string FinishReason { get; set; } = string.Empty;
        
        /// <summary>
        /// Token使用情况
        /// </summary>
        public TokenUsage? Usage { get; set; }
        
        /// <summary>
        /// 响应时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 响应ID
        /// </summary>
        public string ResponseId { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 附加元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
    
    /// <summary>
    /// 工具调用
    /// </summary>
    public class ToolCall
    {
        /// <summary>
        /// 调用ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 工具名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 工具参数（JSON格式）
        /// </summary>
        public string Arguments { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Token使用情况
    /// </summary>
    public class TokenUsage
    {
        /// <summary>
        /// 提示Token数量
        /// </summary>
        public int PromptTokens { get; set; }
        
        /// <summary>
        /// 完成Token数量
        /// </summary>
        public int CompletionTokens { get; set; }
        
        /// <summary>
        /// 总Token数量
        /// </summary>
        public int TotalTokens { get; set; }
    }
}