using System;
using System.Collections.Generic;

namespace AceAgent.Core.Models
{
    /// <summary>
    /// 消息角色
    /// </summary>
    public enum MessageRole
    {
        System,
        User,
        Assistant,
        Tool
    }
    
    /// <summary>
    /// 消息类
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 消息角色
        /// </summary>
        public MessageRole Role { get; set; }
        
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// 工具调用ID（仅当Role为Tool时使用）
        /// </summary>
        public string? ToolCallId { get; set; }
        
        /// <summary>
        /// 工具名称（仅当Role为Tool时使用）
        /// </summary>
        public string? ToolName { get; set; }
        
        /// <summary>
        /// 消息时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 附加元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        /// <summary>
        /// 创建系统消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <returns>系统消息</returns>
        public static Message System(string content)
        {
            return new Message
            {
                Role = MessageRole.System,
                Content = content
            };
        }
        
        /// <summary>
        /// 创建用户消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <returns>用户消息</returns>
        public static Message User(string content)
        {
            return new Message
            {
                Role = MessageRole.User,
                Content = content
            };
        }
        
        /// <summary>
        /// 创建助手消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <returns>助手消息</returns>
        public static Message Assistant(string content)
        {
            return new Message
            {
                Role = MessageRole.Assistant,
                Content = content
            };
        }
        
        /// <summary>
        /// 创建工具消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="toolCallId">工具调用ID</param>
        /// <param name="toolName">工具名称</param>
        /// <returns>工具消息</returns>
        public static Message Tool(string content, string toolCallId, string toolName)
        {
            return new Message
            {
                Role = MessageRole.Tool,
                Content = content,
                ToolCallId = toolCallId,
                ToolName = toolName
            };
        }
    }
}