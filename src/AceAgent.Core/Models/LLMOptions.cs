using System.Collections.Generic;

namespace AceAgent.Core.Models
{
    /// <summary>
    /// LLM选项
    /// </summary>
    public class LLMOptions
    {
        /// <summary>
        /// 模型名称
        /// </summary>
        public string? Model { get; set; }
        
        /// <summary>
        /// 温度参数（0.0-2.0）
        /// </summary>
        public double? Temperature { get; set; }
        
        /// <summary>
        /// 最大Token数量
        /// </summary>
        public int? MaxTokens { get; set; }
        
        /// <summary>
        /// Top-p参数
        /// </summary>
        public double? TopP { get; set; }
        
        /// <summary>
        /// 频率惩罚
        /// </summary>
        public double? FrequencyPenalty { get; set; }
        
        /// <summary>
        /// 存在惩罚
        /// </summary>
        public double? PresencePenalty { get; set; }
        
        /// <summary>
        /// 停止序列
        /// </summary>
        public List<string>? Stop { get; set; }
        
        /// <summary>
        /// 是否启用流式响应
        /// </summary>
        public bool Stream { get; set; } = false;
        
        /// <summary>
        /// 工具定义
        /// </summary>
        public List<ToolDefinition>? Tools { get; set; }
        
        /// <summary>
        /// 工具选择策略
        /// </summary>
        public string? ToolChoice { get; set; }
        
        /// <summary>
        /// 附加参数
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; } = new();
    }
    
    /// <summary>
    /// 工具定义
    /// </summary>
    public class ToolDefinition
    {
        /// <summary>
        /// 工具类型
        /// </summary>
        public string Type { get; set; } = "function";
        
        /// <summary>
        /// 函数定义
        /// </summary>
        public FunctionDefinition? Function { get; set; }
    }
    
    /// <summary>
    /// 函数定义
    /// </summary>
    public class FunctionDefinition
    {
        /// <summary>
        /// 函数名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 函数描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 参数模式（JSON Schema）
        /// </summary>
        public object? Parameters { get; set; }
    }
}