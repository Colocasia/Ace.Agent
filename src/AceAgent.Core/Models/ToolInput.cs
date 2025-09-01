using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AceAgent.Core.Models
{
    /// <summary>
    /// 工具输入参数
    /// </summary>
    public class ToolInput
    {
        /// <summary>
        /// 参数字典
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        /// <summary>
        /// 原始输入文本
        /// </summary>
        public string RawInput { get; set; } = string.Empty;
        
        /// <summary>
        /// 工作目录
        /// </summary>
        public string WorkingDirectory { get; set; } = Environment.CurrentDirectory;
        
        /// <summary>
        /// 获取指定类型的参数值
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="key">参数键</param>
        /// <returns>参数值</returns>
        public T? GetParameter<T>(string key)
        {
            if (!Parameters.TryGetValue(key, out var value))
                return default;
                
            if (value is T directValue)
                return directValue;
                
            if (value is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }
            
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }
        
        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="key">参数键</param>
        /// <param name="value">参数值</param>
        public void SetParameter(string key, object value)
        {
            Parameters[key] = value;
        }
        
        /// <summary>
        /// 转换为指定类型的输入对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>转换后的对象</returns>
        public T As<T>() where T : class, new()
        {
            var json = JsonSerializer.Serialize(Parameters);
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
    }
}