using System.Collections.Generic;

namespace AceAgent.Core.Models
{
    /// <summary>
    /// 模型信息
    /// </summary>
    public class ModelInfo
    {
        /// <summary>
        /// 模型名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 模型显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// 模型描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 提供商名称
        /// </summary>
        public string Provider { get; set; } = string.Empty;
        
        /// <summary>
        /// 最大上下文长度
        /// </summary>
        public int MaxContextLength { get; set; }
        
        /// <summary>
        /// 最大输出Token数量
        /// </summary>
        public int MaxOutputTokens { get; set; }
        
        /// <summary>
        /// 是否支持工具调用
        /// </summary>
        public bool SupportsTools { get; set; }
        
        /// <summary>
        /// 是否支持流式响应
        /// </summary>
        public bool SupportsStreaming { get; set; }
        
        /// <summary>
        /// 是否支持视觉输入
        /// </summary>
        public bool SupportsVision { get; set; }
        
        /// <summary>
        /// 输入价格（每1K Token）
        /// </summary>
        public decimal InputPricePer1K { get; set; }
        
        /// <summary>
        /// 输出价格（每1K Token）
        /// </summary>
        public decimal OutputPricePer1K { get; set; }
        
        /// <summary>
        /// 支持的语言列表
        /// </summary>
        public List<string> SupportedLanguages { get; set; } = new();
        
        /// <summary>
        /// 模型标签
        /// </summary>
        public List<string> Tags { get; set; } = new();
        
        /// <summary>
        /// 附加元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}