using System;
using System.Collections.Generic;

namespace AceAgent.Core.Models
{
    /// <summary>
    /// 工具执行结果
    /// </summary>
    public class ToolResult
    {
        /// <summary>
        /// 执行是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// 结果数据
        /// </summary>
        public object? Data { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; set; }
        
        /// <summary>
        /// 执行时间（毫秒）
        /// </summary>
        public long ExecutionTimeMs { get; set; }
        
        /// <summary>
        /// 附加元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <param name="data">结果数据</param>
        /// <returns>成功结果</returns>
        public static ToolResult CreateSuccess(string message, object? data = null)
        {
            return new ToolResult
            {
                Success = true,
                Message = message,
                Data = data
            };
        }
        
        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="error">详细错误信息</param>
        /// <returns>失败结果</returns>
        public static ToolResult Failure(string message, string? error = null)
        {
            return new ToolResult
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
        
        /// <summary>
        /// 创建异常结果
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <returns>异常结果</returns>
        public static ToolResult FromException(Exception exception)
        {
            return new ToolResult
            {
                Success = false,
                Message = "工具执行时发生异常",
                Error = exception.Message,
                Metadata = new Dictionary<string, object>
                {
                    ["ExceptionType"] = exception.GetType().Name,
                    ["StackTrace"] = exception.StackTrace ?? string.Empty
                }
            };
        }
    }
}