using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;
using AceAgent.Services.Models;

namespace AceAgent.Services
{
    /// <summary>
    /// Lakeview总结服务
    /// 用于生成任务执行的总结报告和分析
    /// </summary>
    public class LakeviewService
    {
        private readonly ITrajectoryRecorder _trajectoryRecorder;
        private readonly ILLMProvider _llmProvider;

        public LakeviewService(ITrajectoryRecorder trajectoryRecorder, ILLMProvider llmProvider)
        {
            _trajectoryRecorder = trajectoryRecorder ?? throw new ArgumentNullException(nameof(trajectoryRecorder));
            _llmProvider = llmProvider ?? throw new ArgumentNullException(nameof(llmProvider));
        }

        /// <summary>
        /// 生成轨迹总结报告
        /// </summary>
        public async Task<LakeviewReport> GenerateTrajectoryReportAsync(string trajectoryId, LakeviewOptions? options = null)
        {
            options ??= new LakeviewOptions();
            
            var trajectory = await _trajectoryRecorder.GetTrajectoryAsync(trajectoryId);
            if (trajectory == null)
            {
                throw new ArgumentException($"轨迹 {trajectoryId} 不存在");
            }

            var report = new LakeviewReport
            {
                Id = Guid.NewGuid().ToString(),
                TrajectoryId = trajectoryId,
                GeneratedAt = DateTime.UtcNow,
                Options = options
            };

            // 基础信息分析
            report.BasicInfo = AnalyzeBasicInfo(trajectory);
            
            // 执行统计
            report.ExecutionStatistics = CalculateExecutionStatistics(trajectory);
            
            // 步骤分析
            report.StepAnalysis = AnalyzeSteps(trajectory);
            
            // 性能分析
            report.PerformanceAnalysis = AnalyzePerformance(trajectory);
            
            // 错误分析
            report.ErrorAnalysis = AnalyzeErrors(trajectory);
            
            // 生成AI总结（如果启用）
            if (options.GenerateAISummary)
            {
                report.AISummary = await GenerateAISummaryAsync(trajectory, options);
            }
            
            // 生成建议
            if (options.GenerateRecommendations)
            {
                report.Recommendations = GenerateRecommendations(trajectory, report);
            }
            
            // 生成可视化数据
            if (options.GenerateVisualizationData)
            {
                report.VisualizationData = GenerateVisualizationData(trajectory);
            }

            return report;
        }

        /// <summary>
        /// 生成会话总结报告
        /// </summary>
        public async Task<LakeviewSessionReport> GenerateSessionReportAsync(string sessionId, LakeviewOptions? options = null)
        {
            options ??= new LakeviewOptions();
            
            var query = new TrajectorySearchQuery
            {
                SessionId = sessionId,
                Limit = 1000 // 获取所有轨迹
            };
            
            var trajectories = (await _trajectoryRecorder.SearchTrajectoriesAsync(query)).ToList();
            
            if (!trajectories.Any())
            {
                throw new ArgumentException($"会话 {sessionId} 没有找到任何轨迹");
            }

            var sessionReport = new LakeviewSessionReport
            {
                Id = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                GeneratedAt = DateTime.UtcNow,
                Options = options,
                TrajectoryCount = trajectories.Count
            };

            // 会话统计
            sessionReport.SessionStatistics = CalculateSessionStatistics(trajectories);
            
            // 轨迹摘要
            sessionReport.TrajectorySummaries = trajectories.Select(t => new TrajectorySummary
            {
                Id = t.Id,
                Description = t.Description ?? "无描述",
                Status = t.Status,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                StepCount = t.Steps?.Count ?? 0,
                ExecutionTime = TimeSpan.FromMilliseconds(t.TotalExecutionTimeMs),
                Success = t.Result?.Success ?? false
            }).ToList();
            
            // 趋势分析
            sessionReport.TrendAnalysis = AnalyzeTrends(trajectories);
            
            // 生成AI总结（如果启用）
            if (options.GenerateAISummary)
            {
                sessionReport.AISummary = await GenerateSessionAISummaryAsync(trajectories, options);
            }

            return sessionReport;
        }

        /// <summary>
        /// 生成比较报告
        /// </summary>
        public async Task<LakeviewComparisonReport> GenerateComparisonReportAsync(
            List<string> trajectoryIds, 
            LakeviewOptions? options = null)
        {
            options ??= new LakeviewOptions();
            
            var trajectories = new List<Trajectory>();
            foreach (var id in trajectoryIds)
            {
                var trajectory = await _trajectoryRecorder.GetTrajectoryAsync(id);
                if (trajectory != null)
                {
                    trajectories.Add(trajectory);
                }
            }

            if (trajectories.Count < 2)
            {
                throw new ArgumentException("比较报告至少需要2个有效的轨迹");
            }

            var comparisonReport = new LakeviewComparisonReport
            {
                Id = Guid.NewGuid().ToString(),
                TrajectoryIds = trajectoryIds,
                GeneratedAt = DateTime.UtcNow,
                Options = options
            };

            // 基础比较
            comparisonReport.BasicComparison = CompareBasicMetrics(trajectories);
            
            // 性能比较
            comparisonReport.PerformanceComparison = ComparePerformance(trajectories);
            
            // 步骤比较
            comparisonReport.StepComparison = CompareSteps(trajectories);
            
            // 成功率比较
            comparisonReport.SuccessRateComparison = CompareSuccessRates(trajectories);

            return comparisonReport;
        }

        private BasicInfo AnalyzeBasicInfo(Trajectory trajectory)
        {
            return new BasicInfo
            {
                TrajectoryId = trajectory.Id,
                SessionId = trajectory.SessionId,
                Description = trajectory.Description ?? "无描述",
                Status = trajectory.Status,
                StartTime = trajectory.StartTime,
                EndTime = trajectory.EndTime,
                TotalExecutionTime = TimeSpan.FromMilliseconds(trajectory.TotalExecutionTimeMs),
                StepCount = trajectory.Steps?.Count ?? 0,
                Success = trajectory.Result?.Success ?? false,
                ErrorCount = trajectory.Steps?.Count(s => !string.IsNullOrEmpty(s.Error)) ?? 0
            };
        }

        private ExecutionStatistics CalculateExecutionStatistics(Trajectory trajectory)
        {
            var steps = trajectory.Steps ?? new List<TrajectoryStep>();
            
            return new ExecutionStatistics
            {
                TotalSteps = steps.Count,
                SuccessfulSteps = steps.Count(s => s.Status == StepStatus.Completed),
                FailedSteps = steps.Count(s => s.Status == StepStatus.Failed),
                SkippedSteps = steps.Count(s => s.Status == StepStatus.Skipped),
                AverageStepDuration = steps.Any() ? TimeSpan.FromMilliseconds(steps.Average(s => s.ExecutionTimeMs)) : TimeSpan.Zero,
                LongestStepDuration = steps.Any() ? TimeSpan.FromMilliseconds(steps.Max(s => s.ExecutionTimeMs)) : TimeSpan.Zero,
                ShortestStepDuration = steps.Any() ? TimeSpan.FromMilliseconds(steps.Min(s => s.ExecutionTimeMs)) : TimeSpan.Zero,
                StepTypeDistribution = steps.GroupBy(s => s.Type).ToDictionary(g => g.Key.ToString(), g => g.Count()),
                SuccessRate = steps.Any() ? (double)steps.Count(s => s.Status == StepStatus.Completed) / steps.Count : 0
            };
        }

        private List<StepAnalysisItem> AnalyzeSteps(Trajectory trajectory)
        {
            var steps = trajectory.Steps ?? new List<TrajectoryStep>();
            
            return steps.Select(step => new StepAnalysisItem
            {
                StepNumber = step.StepNumber,
                Name = step.Name ?? "未命名步骤",
                Type = step.Type,
                Status = step.Status,
                ExecutionTime = TimeSpan.FromMilliseconds(step.ExecutionTimeMs),
                InputSize = step.InputData?.Length ?? 0,
                OutputSize = step.OutputData?.Length ?? 0,
                HasError = !string.IsNullOrEmpty(step.Error),
                ErrorMessage = step.Error,
                Efficiency = CalculateStepEfficiency(step),
                Complexity = CalculateStepComplexity(step)
            }).ToList();
        }

        private PerformanceAnalysis AnalyzePerformance(Trajectory trajectory)
        {
            var steps = trajectory.Steps ?? new List<TrajectoryStep>();
            var totalTime = TimeSpan.FromMilliseconds(trajectory.TotalExecutionTimeMs);
            
            var bottlenecks = steps
                .Where(s => s.ExecutionTimeMs > 1000) // 超过1秒的步骤
                .OrderByDescending(s => s.ExecutionTimeMs)
                .Take(5)
                .Select(s => new PerformanceBottleneck
                {
                    StepNumber = s.StepNumber,
                    StepName = s.Name ?? "未命名",
                    ExecutionTime = TimeSpan.FromMilliseconds(s.ExecutionTimeMs),
                    PercentageOfTotal = totalTime.TotalMilliseconds > 0 ? s.ExecutionTimeMs / totalTime.TotalMilliseconds * 100 : 0
                })
                .ToList();

            return new PerformanceAnalysis
            {
                OverallEfficiency = CalculateOverallEfficiency(trajectory),
                ThroughputStepsPerSecond = totalTime.TotalSeconds > 0 ? steps.Count / totalTime.TotalSeconds : 0,
                ResourceUtilization = CalculateResourceUtilization(steps),
                Bottlenecks = bottlenecks,
                OptimizationOpportunities = IdentifyOptimizationOpportunities(steps)
            };
        }

        private ErrorAnalysis AnalyzeErrors(Trajectory trajectory)
        {
            var steps = trajectory.Steps ?? new List<TrajectoryStep>();
            var errorSteps = steps.Where(s => !string.IsNullOrEmpty(s.Error)).ToList();
            
            var errorPatterns = errorSteps
                .GroupBy(s => ExtractErrorType(s.Error))
                .Select(g => new ErrorPattern
                {
                    ErrorType = g.Key,
                    Frequency = g.Count(),
                    AffectedSteps = g.Select(s => s.StepNumber).ToList(),
                    FirstOccurrence = g.Min(s => s.StartTime),
                    LastOccurrence = g.Max(s => s.EndTime ?? s.StartTime)
                })
                .OrderByDescending(ep => ep.Frequency)
                .ToList();

            return new ErrorAnalysis
            {
                TotalErrors = errorSteps.Count,
                ErrorRate = steps.Any() ? (double)errorSteps.Count / steps.Count : 0,
                ErrorPatterns = errorPatterns,
                CriticalErrors = errorSteps.Where(s => IsCriticalError(s.Error)).Count(),
                RecoverableErrors = errorSteps.Where(s => !IsCriticalError(s.Error)).Count(),
                ErrorDistribution = errorSteps.GroupBy(s => s.Type).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }

        private async Task<string> GenerateAISummaryAsync(Trajectory trajectory, LakeviewOptions options)
        {
            try
            {
                var prompt = BuildSummaryPrompt(trajectory, options);
                
                var messages = new List<Message>
                {
                    Message.System("你是一个专业的任务执行分析师，请根据提供的轨迹数据生成简洁、准确的总结报告。"),
                    Message.User(prompt)
                };

                var llmOptions = new LLMOptions
                {
                    Model = options.LLMModel ?? "gpt-3.5-turbo",
                    Temperature = 0.3,
                    MaxTokens = 1000
                };

                var response = await _llmProvider.GenerateResponseAsync(messages, llmOptions);
                return response.Content ?? "无法生成AI总结";
            }
            catch (Exception ex)
            {
                return $"生成AI总结时出错：{ex.Message}";
            }
        }

        private async Task<string> GenerateSessionAISummaryAsync(List<Trajectory> trajectories, LakeviewOptions options)
        {
            try
            {
                var prompt = BuildSessionSummaryPrompt(trajectories, options);
                
                var messages = new List<Message>
                {
                    Message.System("你是一个专业的会话分析师，请根据提供的多个轨迹数据生成会话级别的总结报告。"),
                    Message.User(prompt)
                };

                var llmOptions = new LLMOptions
                {
                    Model = options.LLMModel ?? "gpt-3.5-turbo",
                    Temperature = 0.3,
                    MaxTokens = 1500
                };

                var response = await _llmProvider.GenerateResponseAsync(messages, llmOptions);
                return response.Content ?? "无法生成会话AI总结";
            }
            catch (Exception ex)
            {
                return $"生成会话AI总结时出错：{ex.Message}";
            }
        }

        private string BuildSummaryPrompt(Trajectory trajectory, LakeviewOptions options)
        {
            var sb = new StringBuilder();
            sb.AppendLine("请分析以下任务执行轨迹并生成总结：");
            sb.AppendLine();
            sb.AppendLine($"任务描述：{trajectory.Description}");
            sb.AppendLine($"执行状态：{trajectory.Status}");
            sb.AppendLine($"开始时间：{trajectory.StartTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"结束时间：{trajectory.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未结束"}");
            sb.AppendLine($"总执行时间：{TimeSpan.FromMilliseconds(trajectory.TotalExecutionTimeMs)}");
            sb.AppendLine($"步骤数量：{trajectory.Steps?.Count ?? 0}");
            sb.AppendLine();
            
            if (trajectory.Steps?.Any() == true)
            {
                sb.AppendLine("主要步骤：");
                foreach (var step in trajectory.Steps.Take(5))
                {
                    sb.AppendLine($"- {step.Name} ({step.Type}, {step.Status}, {step.ExecutionTimeMs / 1000.0:F1}秒)");
                }
                sb.AppendLine();
            }
            
            if (trajectory.Result != null)
            {
                sb.AppendLine($"执行结果：{(trajectory.Result.Success ? "成功" : "失败")}");
                if (!string.IsNullOrEmpty(trajectory.Result.Message))
                {
                    sb.AppendLine($"结果消息：{trajectory.Result.Message}");
                }
            }
            
            sb.AppendLine();
            sb.AppendLine("请提供：");
            sb.AppendLine("1. 执行概况总结");
            sb.AppendLine("2. 关键成就和问题");
            sb.AppendLine("3. 性能评估");
            sb.AppendLine("4. 改进建议");
            
            return sb.ToString();
        }

        private string BuildSessionSummaryPrompt(List<Trajectory> trajectories, LakeviewOptions options)
        {
            var sb = new StringBuilder();
            sb.AppendLine("请分析以下会话中的多个任务执行轨迹并生成会话总结：");
            sb.AppendLine();
            sb.AppendLine($"轨迹数量：{trajectories.Count}");
            sb.AppendLine($"会话时间跨度：{trajectories.Min(t => t.StartTime):yyyy-MM-dd HH:mm:ss} 至 {trajectories.Max(t => t.EndTime ?? t.StartTime):yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            sb.AppendLine("轨迹概览：");
            foreach (var trajectory in trajectories.Take(10))
            {
                sb.AppendLine($"- {trajectory.Description} ({trajectory.Status}, {trajectory.Steps?.Count ?? 0}步骤)");
            }
            
            if (trajectories.Count > 10)
            {
                sb.AppendLine($"... 还有 {trajectories.Count - 10} 个轨迹");
            }
            
            sb.AppendLine();
            sb.AppendLine("请提供：");
            sb.AppendLine("1. 会话整体概况");
            sb.AppendLine("2. 主要任务类型和模式");
            sb.AppendLine("3. 执行效率分析");
            sb.AppendLine("4. 用户行为洞察");
            sb.AppendLine("5. 系统优化建议");
            
            return sb.ToString();
        }

        private List<string> GenerateRecommendations(Trajectory trajectory, LakeviewReport report)
        {
            var recommendations = new List<string>();
            
            // 基于错误率的建议
            if (report.ErrorAnalysis.ErrorRate > 0.2)
            {
                recommendations.Add("错误率较高，建议加强输入验证和错误处理机制");
            }
            
            // 基于性能的建议
            if (report.PerformanceAnalysis.Bottlenecks.Any())
            {
                recommendations.Add($"发现性能瓶颈，建议优化步骤：{string.Join(", ", report.PerformanceAnalysis.Bottlenecks.Take(3).Select(b => b.StepName))}");
            }
            
            // 基于执行时间的建议
            if (trajectory.TotalExecutionTimeMs > 600000) // 10分钟
            {
                recommendations.Add("执行时间较长，建议考虑并行处理或缓存机制");
            }
            
            // 基于步骤数量的建议
            if (trajectory.Steps?.Count > 20)
            {
                recommendations.Add("步骤数量较多，建议考虑任务分解或批处理");
            }
            
            return recommendations;
        }

        private VisualizationData GenerateVisualizationData(Trajectory trajectory)
        {
            var steps = trajectory.Steps ?? new List<TrajectoryStep>();
            
            return new VisualizationData
            {
                TimelineData = steps.Select(s => new TimelinePoint
                {
                    StepNumber = s.StepNumber,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime ?? s.StartTime,
                    Duration = s.ExecutionTimeMs,
                    Status = s.Status.ToString(),
                    Name = s.Name ?? "未命名"
                }).ToList(),
                
                PerformanceChart = steps.Select(s => new PerformancePoint
                {
                    StepNumber = s.StepNumber,
                    ExecutionTime = s.ExecutionTimeMs,
                    InputSize = s.InputData?.Length ?? 0,
                    OutputSize = s.OutputData?.Length ?? 0
                }).ToList(),
                
                StatusDistribution = steps.GroupBy(s => s.Status).ToDictionary(g => g.Key.ToString(), g => g.Count()),
                TypeDistribution = steps.GroupBy(s => s.Type).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }

        // 辅助方法
        private double CalculateStepEfficiency(TrajectoryStep step)
        {
            // 简化的效率计算：基于输出/输入比和执行时间
            var inputSize = step.InputData?.Length ?? 1;
            var outputSize = step.OutputData?.Length ?? 0;
            var timeSeconds = step.ExecutionTimeMs / 1000.0;
            
            return timeSeconds > 0 ? (outputSize / (double)inputSize) / timeSeconds : 0;
        }

        private int CalculateStepComplexity(TrajectoryStep step)
        {
            // 简化的复杂度计算：基于输入大小、执行时间和类型
            var baseComplexity = step.Type switch
            {
                StepType.LLMCall => 3,
                StepType.ToolExecution => 2,
                StepType.SystemOperation => 2,
                _ => 1
            };
            
            var sizeComplexity = (step.InputData?.Length ?? 0) > 1000 ? 1 : 0;
            var timeComplexity = step.ExecutionTimeMs > 5000 ? 1 : 0;
            
            return baseComplexity + sizeComplexity + timeComplexity;
        }

        private double CalculateOverallEfficiency(Trajectory trajectory)
        {
            var steps = trajectory.Steps ?? new List<TrajectoryStep>();
            if (!steps.Any()) return 0;
            
            var successRate = (double)steps.Count(s => s.Status == StepStatus.Completed) / steps.Count;
            var avgExecutionTime = steps.Average(s => s.ExecutionTimeMs / 1000.0);
            var timeEfficiency = avgExecutionTime > 0 ? Math.Min(1.0 / avgExecutionTime, 1.0) : 0;
            
            return (successRate + timeEfficiency) / 2;
        }

        private double CalculateResourceUtilization(List<TrajectoryStep> steps)
        {
            // 简化的资源利用率计算
            if (!steps.Any()) return 0;
            
            var totalTime = steps.Sum(s => s.ExecutionTimeMs / 1000.0);
            var activeTime = steps.Where(s => s.Status == StepStatus.Completed).Sum(s => s.ExecutionTimeMs / 1000.0);
            
            return totalTime > 0 ? activeTime / totalTime : 0;
        }

        private List<string> IdentifyOptimizationOpportunities(List<TrajectoryStep> steps)
        {
            var opportunities = new List<string>();
            
            // 检查重复的步骤类型
            var duplicateTypes = steps.GroupBy(s => s.Type).Where(g => g.Count() > 3).Select(g => g.Key);
            foreach (var type in duplicateTypes)
            {
                opportunities.Add($"考虑批处理或缓存 {type} 类型的操作");
            }
            
            // 检查长时间运行的步骤
            var longRunningSteps = steps.Where(s => s.ExecutionTimeMs > 10000).ToList();
            if (longRunningSteps.Any())
            {
                opportunities.Add($"优化长时间运行的步骤：{string.Join(", ", longRunningSteps.Take(3).Select(s => s.Name))}");
            }
            
            return opportunities;
        }

        private string ExtractErrorType(string? error)
        {
            if (string.IsNullOrEmpty(error)) return "Unknown";
            
            // 简化的错误类型提取
            if (error.Contains("timeout", StringComparison.OrdinalIgnoreCase)) return "Timeout";
            if (error.Contains("network", StringComparison.OrdinalIgnoreCase)) return "Network";
            if (error.Contains("permission", StringComparison.OrdinalIgnoreCase)) return "Permission";
            if (error.Contains("validation", StringComparison.OrdinalIgnoreCase)) return "Validation";
            if (error.Contains("null", StringComparison.OrdinalIgnoreCase)) return "NullReference";
            
            return "General";
        }

        private bool IsCriticalError(string? error)
        {
            if (string.IsNullOrEmpty(error)) return false;
            
            var criticalKeywords = new[] { "fatal", "critical", "system", "security", "corruption" };
            return criticalKeywords.Any(keyword => error.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private SessionStatistics CalculateSessionStatistics(List<Trajectory> trajectories)
        {
            return new SessionStatistics
            {
                TotalTrajectories = trajectories.Count,
                SuccessfulTrajectories = trajectories.Count(t => t.Result?.Success == true),
                FailedTrajectories = trajectories.Count(t => t.Result?.Success == false),
                AverageExecutionTime = trajectories.Any() ? TimeSpan.FromMilliseconds(trajectories.Average(t => t.TotalExecutionTimeMs)) : TimeSpan.Zero,
                TotalExecutionTime = TimeSpan.FromMilliseconds(trajectories.Sum(t => t.TotalExecutionTimeMs)),
                AverageStepsPerTrajectory = trajectories.Any() ? trajectories.Average(t => t.Steps?.Count ?? 0) : 0,
                TotalSteps = trajectories.Sum(t => t.Steps?.Count ?? 0),
                OverallSuccessRate = trajectories.Any() ? (double)trajectories.Count(t => t.Result?.Success == true) / trajectories.Count : 0
            };
        }

        private TrendAnalysis AnalyzeTrends(List<Trajectory> trajectories)
        {
            var orderedTrajectories = trajectories.OrderBy(t => t.StartTime).ToList();
            
            return new TrendAnalysis
            {
                ExecutionTimeTrend = CalculateTimeTrend(orderedTrajectories),
                SuccessRateTrend = CalculateSuccessRateTrend(orderedTrajectories),
                ComplexityTrend = CalculateComplexityTrend(orderedTrajectories),
                ErrorRateTrend = CalculateErrorRateTrend(orderedTrajectories)
            };
        }

        private List<TrendPoint> CalculateTimeTrend(List<Trajectory> trajectories)
        {
            return trajectories.Select((t, index) => new TrendPoint
            {
                Index = index,
                Value = t.TotalExecutionTimeMs / 1000.0,
                Timestamp = t.StartTime
            }).ToList();
        }

        private List<TrendPoint> CalculateSuccessRateTrend(List<Trajectory> trajectories)
        {
            var windowSize = Math.Max(1, trajectories.Count / 10); // 10个数据点
            var trendPoints = new List<TrendPoint>();
            
            for (int i = 0; i < trajectories.Count; i += windowSize)
            {
                var window = trajectories.Skip(i).Take(windowSize).ToList();
                var successRate = window.Any() ? (double)window.Count(t => t.Result?.Success == true) / window.Count : 0;
                
                trendPoints.Add(new TrendPoint
                {
                    Index = i / windowSize,
                    Value = successRate,
                    Timestamp = window.First().StartTime
                });
            }
            
            return trendPoints;
        }

        private List<TrendPoint> CalculateComplexityTrend(List<Trajectory> trajectories)
        {
            return trajectories.Select((t, index) => new TrendPoint
            {
                Index = index,
                Value = t.Steps?.Count ?? 0,
                Timestamp = t.StartTime
            }).ToList();
        }

        private List<TrendPoint> CalculateErrorRateTrend(List<Trajectory> trajectories)
        {
            return trajectories.Select((t, index) => new TrendPoint
            {
                Index = index,
                Value = t.Steps?.Any() == true ? (double)t.Steps.Count(s => !string.IsNullOrEmpty(s.Error)) / t.Steps.Count : 0,
                Timestamp = t.StartTime
            }).ToList();
        }

        private BasicComparison CompareBasicMetrics(List<Trajectory> trajectories)
        {
            return new BasicComparison
            {
                ExecutionTimes = trajectories.Select(t => t.TotalExecutionTimeMs / 1000.0).ToList(),
                StepCounts = trajectories.Select(t => t.Steps?.Count ?? 0).ToList(),
                SuccessRates = trajectories.Select(t => t.Steps?.Any() == true ? (double)t.Steps.Count(s => s.Status == StepStatus.Completed) / t.Steps.Count : 0).ToList(),
                ErrorRates = trajectories.Select(t => t.Steps?.Any() == true ? (double)t.Steps.Count(s => !string.IsNullOrEmpty(s.Error)) / t.Steps.Count : 0).ToList()
            };
        }

        private PerformanceComparison ComparePerformance(List<Trajectory> trajectories)
        {
            return new PerformanceComparison
            {
                AverageExecutionTimes = trajectories.Select(t => t.Steps?.Any() == true ? t.Steps.Average(s => s.ExecutionTimeMs / 1000.0) : 0).ToList(),
                ThroughputRates = trajectories.Select(t => t.TotalExecutionTimeMs > 0 ? (t.Steps?.Count ?? 0) / (t.TotalExecutionTimeMs / 1000.0) : 0).ToList(),
                ResourceUtilization = trajectories.Select(t => CalculateResourceUtilization(t.Steps ?? new List<TrajectoryStep>())).ToList()
            };
        }

        private StepComparison CompareSteps(List<Trajectory> trajectories)
        {
            var allSteps = trajectories.SelectMany(t => t.Steps ?? new List<TrajectoryStep>()).ToList();
            
            return new StepComparison
            {
                StepTypeDistributions = trajectories.Select(t => 
                    (t.Steps ?? new List<TrajectoryStep>())
                    .GroupBy(s => s.Type)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
                ).ToList(),
                AverageStepDurations = trajectories.Select(t => 
                    t.Steps?.Any() == true ? t.Steps.Average(s => s.ExecutionTimeMs / 1000.0) : 0
                ).ToList()
            };
        }

        private SuccessRateComparison CompareSuccessRates(List<Trajectory> trajectories)
        {
            return new SuccessRateComparison
            {
                OverallSuccessRates = trajectories.Select(t => t.Result?.Success == true ? 1.0 : 0.0).ToList(),
                StepSuccessRates = trajectories.Select(t => 
                    t.Steps?.Any() == true ? (double)t.Steps.Count(s => s.Status == StepStatus.Completed) / t.Steps.Count : 0
                ).ToList(),
                ErrorRecoveryRates = trajectories.Select(t => CalculateErrorRecoveryRate(t)).ToList()
            };
        }

        private double CalculateErrorRecoveryRate(Trajectory trajectory)
        {
            var steps = trajectory.Steps ?? new List<TrajectoryStep>();
            var errorSteps = steps.Where(s => !string.IsNullOrEmpty(s.Error)).ToList();
            
            if (!errorSteps.Any()) return 1.0;
            
            // 简化的恢复率计算：错误后是否有成功的步骤
            var recoveredErrors = 0;
            foreach (var errorStep in errorSteps)
            {
                var subsequentSteps = steps.Where(s => s.StepNumber > errorStep.StepNumber).ToList();
                if (subsequentSteps.Any(s => s.Status == StepStatus.Completed))
                {
                    recoveredErrors++;
                }
            }
            
            return (double)recoveredErrors / errorSteps.Count;
        }
    }
}