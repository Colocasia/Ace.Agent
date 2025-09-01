using System;
using System.Collections.Generic;

namespace AceAgent.Core.Models
{
    /// <summary>
    /// 轨迹信息
    /// </summary>
    public class Trajectory
    {
        /// <summary>
        /// 轨迹ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 会话ID
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// 轨迹状态
        /// </summary>
        public TrajectoryStatus Status { get; set; }
        
        /// <summary>
        /// 轨迹描述
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 执行步骤列表
        /// </summary>
        public List<TrajectoryStep> Steps { get; set; } = new();
        
        /// <summary>
        /// 最终结果
        /// </summary>
        public TrajectoryResult? Result { get; set; }
        
        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        /// <summary>
        /// 总执行时间（毫秒）
        /// </summary>
        public long TotalExecutionTimeMs => EndTime.HasValue ? 
            (long)(EndTime.Value - StartTime).TotalMilliseconds : 0;
    }
    
    /// <summary>
    /// 轨迹状态
    /// </summary>
    public enum TrajectoryStatus
    {
        /// <summary>
        /// 进行中
        /// </summary>
        InProgress,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        
        /// <summary>
        /// 已失败
        /// </summary>
        Failed,
        
        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled
    }
    
    /// <summary>
    /// 轨迹步骤
    /// </summary>
    public class TrajectoryStep
    {
        /// <summary>
        /// 步骤ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 步骤序号
        /// </summary>
        public int StepNumber { get; set; }
        
        /// <summary>
        /// 步骤类型
        /// </summary>
        public StepType Type { get; set; }
        
        /// <summary>
        /// 步骤名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 步骤描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 输入数据
        /// </summary>
        public object? Input { get; set; }
        
        /// <summary>
        /// 输出数据
        /// </summary>
        public object? Output { get; set; }
        
        /// <summary>
        /// 输入数据（字符串形式）
        /// </summary>
        public string? InputData { get; set; }
        
        /// <summary>
        /// 输出数据（字符串形式）
        /// </summary>
        public string? OutputData { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// 执行状态
        /// </summary>
        public StepStatus Status { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; set; }
        
        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        /// <summary>
        /// 执行时间（毫秒）
        /// </summary>
        public long ExecutionTimeMs => EndTime.HasValue ? 
            (long)(EndTime.Value - StartTime).TotalMilliseconds : 0;
    }
    
    /// <summary>
    /// 步骤类型
    /// </summary>
    public enum StepType
    {
        /// <summary>
        /// LLM调用
        /// </summary>
        LLMCall,
        
        /// <summary>
        /// 工具执行
        /// </summary>
        ToolExecution,
        
        /// <summary>
        /// 用户输入
        /// </summary>
        UserInput,
        
        /// <summary>
        /// 系统操作
        /// </summary>
        SystemOperation
    }
    
    /// <summary>
    /// 步骤状态
    /// </summary>
    public enum StepStatus
    {
        /// <summary>
        /// 进行中
        /// </summary>
        InProgress,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        
        /// <summary>
        /// 已失败
        /// </summary>
        Failed,
        
        /// <summary>
        /// 已跳过
        /// </summary>
        Skipped
    }
    
    /// <summary>
    /// 轨迹结果
    /// </summary>
    public class TrajectoryResult
    {
        /// <summary>
        /// 是否成功
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
        /// 统计信息
        /// </summary>
        public TrajectoryStatistics Statistics { get; set; } = new();
    }
    
    /// <summary>
    /// 轨迹统计信息
    /// </summary>
    public class TrajectoryStatistics
    {
        /// <summary>
        /// 总步骤数
        /// </summary>
        public int TotalSteps { get; set; }
        
        /// <summary>
        /// 成功步骤数
        /// </summary>
        public int SuccessfulSteps { get; set; }
        
        /// <summary>
        /// 失败步骤数
        /// </summary>
        public int FailedSteps { get; set; }
        
        /// <summary>
        /// LLM调用次数
        /// </summary>
        public int LLMCalls { get; set; }
        
        /// <summary>
        /// 工具执行次数
        /// </summary>
        public int ToolExecutions { get; set; }
        
        /// <summary>
        /// 总Token使用量
        /// </summary>
        public TokenUsage? TotalTokenUsage { get; set; }
    }
    
    /// <summary>
    /// 轨迹搜索查询
    /// </summary>
    public class TrajectorySearchQuery
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public string? SessionId { get; set; }
        
        /// <summary>
        /// 状态过滤
        /// </summary>
        public TrajectoryStatus? Status { get; set; }
        
        /// <summary>
        /// 开始时间范围 - 从
        /// </summary>
        public DateTime? StartTimeAfter { get; set; }
        
        /// <summary>
        /// 开始时间范围 - 到
        /// </summary>
        public DateTime? StartTimeBefore { get; set; }
        
        /// <summary>
        /// 结束时间范围 - 从
        /// </summary>
        public DateTime? EndTimeAfter { get; set; }
        
        /// <summary>
        /// 结束时间范围 - 到
        /// </summary>
        public DateTime? EndTimeBefore { get; set; }
        
        /// <summary>
        /// 关键词搜索
        /// </summary>
        public string? Keywords { get; set; }
        
        /// <summary>
        /// 元数据过滤
        /// </summary>
        public Dictionary<string, object>? MetadataFilter { get; set; }
        
        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize { get; set; } = 50;
        
        /// <summary>
        /// 页码
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 限制数量
        /// </summary>
        public int Limit { get; set; } = 50;
        
        /// <summary>
        /// 偏移量
        /// </summary>
        public int Offset { get; set; } = 0;
    }
}