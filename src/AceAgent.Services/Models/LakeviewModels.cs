using System;
using System.Collections.Generic;
using AceAgent.Core.Models;

namespace AceAgent.Services.Models
{
    /// <summary>
    /// Lakeview配置选项
    /// </summary>
    public class LakeviewOptions
    {
        /// <summary>
        /// 是否生成AI总结
        /// </summary>
        public bool GenerateAISummary { get; set; } = true;
        
        /// <summary>
        /// 是否生成建议
        /// </summary>
        public bool GenerateRecommendations { get; set; } = true;
        
        /// <summary>
        /// 是否生成可视化数据
        /// </summary>
        public bool GenerateVisualizationData { get; set; } = true;
        
        /// <summary>
        /// 用于AI总结的LLM模型
        /// </summary>
        public string? LLMModel { get; set; }
        
        /// <summary>
        /// 总结详细程度（1-5，5最详细）
        /// </summary>
        public int DetailLevel { get; set; } = 3;
        
        /// <summary>
        /// 是否包含敏感信息
        /// </summary>
        public bool IncludeSensitiveInfo { get; set; } = false;
        
        /// <summary>
        /// 自定义分析维度
        /// </summary>
        public List<string> CustomAnalysisDimensions { get; set; } = new();
    }

    /// <summary>
    /// Lakeview轨迹报告
    /// </summary>
    public class LakeviewReport
    {
        /// <summary>
        /// 报告ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 轨迹ID
        /// </summary>
        public string TrajectoryId { get; set; } = string.Empty;
        
        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime GeneratedAt { get; set; }
        
        /// <summary>
        /// 配置选项
        /// </summary>
        public LakeviewOptions Options { get; set; } = new();
        
        /// <summary>
        /// 基础信息
        /// </summary>
        public BasicInfo BasicInfo { get; set; } = new();
        
        /// <summary>
        /// 执行统计
        /// </summary>
        public ExecutionStatistics ExecutionStatistics { get; set; } = new();
        
        /// <summary>
        /// 步骤分析
        /// </summary>
        public List<StepAnalysisItem> StepAnalysis { get; set; } = new();
        
        /// <summary>
        /// 性能分析
        /// </summary>
        public PerformanceAnalysis PerformanceAnalysis { get; set; } = new();
        
        /// <summary>
        /// 错误分析
        /// </summary>
        public ErrorAnalysis ErrorAnalysis { get; set; } = new();
        
        /// <summary>
        /// AI生成的总结
        /// </summary>
        public string? AISummary { get; set; }
        
        /// <summary>
        /// 建议列表
        /// </summary>
        public List<string> Recommendations { get; set; } = new();
        
        /// <summary>
        /// 可视化数据
        /// </summary>
        public VisualizationData? VisualizationData { get; set; }
    }

    /// <summary>
    /// Lakeview会话报告
    /// </summary>
    public class LakeviewSessionReport
    {
        /// <summary>
        /// 报告ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 会话ID
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime GeneratedAt { get; set; }
        
        /// <summary>
        /// 配置选项
        /// </summary>
        public LakeviewOptions Options { get; set; } = new();
        
        /// <summary>
        /// 轨迹数量
        /// </summary>
        public int TrajectoryCount { get; set; }
        
        /// <summary>
        /// 会话统计
        /// </summary>
        public SessionStatistics SessionStatistics { get; set; } = new();
        
        /// <summary>
        /// 轨迹摘要列表
        /// </summary>
        public List<TrajectorySummary> TrajectorySummaries { get; set; } = new();
        
        /// <summary>
        /// 趋势分析
        /// </summary>
        public TrendAnalysis TrendAnalysis { get; set; } = new();
        
        /// <summary>
        /// AI生成的会话总结
        /// </summary>
        public string? AISummary { get; set; }
    }

    /// <summary>
    /// Lakeview比较报告
    /// </summary>
    public class LakeviewComparisonReport
    {
        /// <summary>
        /// 报告ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 比较的轨迹ID列表
        /// </summary>
        public List<string> TrajectoryIds { get; set; } = new();
        
        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime GeneratedAt { get; set; }
        
        /// <summary>
        /// 配置选项
        /// </summary>
        public LakeviewOptions Options { get; set; } = new();
        
        /// <summary>
        /// 基础指标比较
        /// </summary>
        public BasicComparison BasicComparison { get; set; } = new();
        
        /// <summary>
        /// 性能比较
        /// </summary>
        public PerformanceComparison PerformanceComparison { get; set; } = new();
        
        /// <summary>
        /// 步骤比较
        /// </summary>
        public StepComparison StepComparison { get; set; } = new();
        
        /// <summary>
        /// 成功率比较
        /// </summary>
        public SuccessRateComparison SuccessRateComparison { get; set; } = new();
    }

    /// <summary>
    /// 基础信息
    /// </summary>
    public class BasicInfo
    {
        public string TrajectoryId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TrajectoryStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public int StepCount { get; set; }
        public bool Success { get; set; }
        public int ErrorCount { get; set; }
    }

    /// <summary>
    /// 执行统计
    /// </summary>
    public class ExecutionStatistics
    {
        public int TotalSteps { get; set; }
        public int SuccessfulSteps { get; set; }
        public int FailedSteps { get; set; }
        public int SkippedSteps { get; set; }
        public TimeSpan AverageStepDuration { get; set; }
        public TimeSpan LongestStepDuration { get; set; }
        public TimeSpan ShortestStepDuration { get; set; }
        public Dictionary<string, int> StepTypeDistribution { get; set; } = new();
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// 步骤分析项
    /// </summary>
    public class StepAnalysisItem
    {
        public int StepNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public StepType Type { get; set; }
        public StepStatus Status { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public int InputSize { get; set; }
        public int OutputSize { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public double Efficiency { get; set; }
        public int Complexity { get; set; }
    }

    /// <summary>
    /// 性能分析
    /// </summary>
    public class PerformanceAnalysis
    {
        public double OverallEfficiency { get; set; }
        public double ThroughputStepsPerSecond { get; set; }
        public double ResourceUtilization { get; set; }
        public List<PerformanceBottleneck> Bottlenecks { get; set; } = new();
        public List<string> OptimizationOpportunities { get; set; } = new();
    }

    /// <summary>
    /// 性能瓶颈
    /// </summary>
    public class PerformanceBottleneck
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public double PercentageOfTotal { get; set; }
    }

    /// <summary>
    /// 错误分析
    /// </summary>
    public class ErrorAnalysis
    {
        public int TotalErrors { get; set; }
        public double ErrorRate { get; set; }
        public List<ErrorPattern> ErrorPatterns { get; set; } = new();
        public int CriticalErrors { get; set; }
        public int RecoverableErrors { get; set; }
        public Dictionary<string, int> ErrorDistribution { get; set; } = new();
    }

    /// <summary>
    /// 错误模式
    /// </summary>
    public class ErrorPattern
    {
        public string ErrorType { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public List<int> AffectedSteps { get; set; } = new();
        public DateTime FirstOccurrence { get; set; }
        public DateTime LastOccurrence { get; set; }
    }

    /// <summary>
    /// 可视化数据
    /// </summary>
    public class VisualizationData
    {
        public List<TimelinePoint> TimelineData { get; set; } = new();
        public List<PerformancePoint> PerformanceChart { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> TypeDistribution { get; set; } = new();
    }

    /// <summary>
    /// 时间线数据点
    /// </summary>
    public class TimelinePoint
    {
        public int StepNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 性能数据点
    /// </summary>
    public class PerformancePoint
    {
        public int StepNumber { get; set; }
        public double ExecutionTime { get; set; }
        public int InputSize { get; set; }
        public int OutputSize { get; set; }
    }

    /// <summary>
    /// 会话统计
    /// </summary>
    public class SessionStatistics
    {
        public int TotalTrajectories { get; set; }
        public int SuccessfulTrajectories { get; set; }
        public int FailedTrajectories { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public double AverageStepsPerTrajectory { get; set; }
        public int TotalSteps { get; set; }
        public double OverallSuccessRate { get; set; }
    }

    /// <summary>
    /// 轨迹摘要
    /// </summary>
    public class TrajectorySummary
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TrajectoryStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int StepCount { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool Success { get; set; }
    }

    /// <summary>
    /// 趋势分析
    /// </summary>
    public class TrendAnalysis
    {
        public List<TrendPoint> ExecutionTimeTrend { get; set; } = new();
        public List<TrendPoint> SuccessRateTrend { get; set; } = new();
        public List<TrendPoint> ComplexityTrend { get; set; } = new();
        public List<TrendPoint> ErrorRateTrend { get; set; } = new();
    }

    /// <summary>
    /// 趋势数据点
    /// </summary>
    public class TrendPoint
    {
        public int Index { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 基础指标比较
    /// </summary>
    public class BasicComparison
    {
        public List<double> ExecutionTimes { get; set; } = new();
        public List<int> StepCounts { get; set; } = new();
        public List<double> SuccessRates { get; set; } = new();
        public List<double> ErrorRates { get; set; } = new();
    }

    /// <summary>
    /// 性能比较
    /// </summary>
    public class PerformanceComparison
    {
        public List<double> AverageExecutionTimes { get; set; } = new();
        public List<double> ThroughputRates { get; set; } = new();
        public List<double> ResourceUtilization { get; set; } = new();
    }

    /// <summary>
    /// 步骤比较
    /// </summary>
    public class StepComparison
    {
        public List<Dictionary<string, int>> StepTypeDistributions { get; set; } = new();
        public List<double> AverageStepDurations { get; set; } = new();
    }

    /// <summary>
    /// 成功率比较
    /// </summary>
    public class SuccessRateComparison
    {
        public List<double> OverallSuccessRates { get; set; } = new();
        public List<double> StepSuccessRates { get; set; } = new();
        public List<double> ErrorRecoveryRates { get; set; } = new();
    }
}