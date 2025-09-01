using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;
using AceAgent.Infrastructure;
using Microsoft.Extensions.Logging;

namespace AceAgent.CLI.Services
{
    /// <summary>
    /// 轨迹管理服务
    /// </summary>
    public class TrajectoryService
    {
        private readonly ILogger<TrajectoryService> _logger;
        private ITrajectoryRecorder? _trajectoryRecorder;
        private readonly string _defaultDbPath;

        /// <summary>
        /// 初始化TrajectoryService实例
        /// </summary>
        /// <param name="logger">日志记录器</param>
        public TrajectoryService(ILogger<TrajectoryService> logger)
        {
            _logger = logger;
            _defaultDbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".aceagent", 
                "trajectories.db");
        }

        /// <summary>
        /// 获取轨迹记录器实例
        /// </summary>
        public ITrajectoryRecorder GetTrajectoryRecorder()
        {
            if (_trajectoryRecorder == null)
            {
                _trajectoryRecorder = new SqliteTrajectoryRecorder(_defaultDbPath);
                _logger.LogInformation($"轨迹记录器已初始化，数据库路径: {_defaultDbPath}");
            }
            return _trajectoryRecorder;
        }

        /// <summary>
        /// 列出执行轨迹
        /// </summary>
        public async Task ListTrajectoriesAsync(int limit, string? status)
        {
            try
            {
                var recorder = GetTrajectoryRecorder();
                var query = new TrajectorySearchQuery
                {
                    Limit = limit,
                    Status = !string.IsNullOrEmpty(status) && Enum.TryParse<TrajectoryStatus>(status, true, out var statusEnum) 
                        ? statusEnum 
                        : null
                };

                var trajectories = await recorder.SearchTrajectoriesAsync(query);
                
                if (!trajectories.Any())
                {
                    Console.WriteLine("未找到任何轨迹记录");
                    return;
                }

                Console.WriteLine($"找到 {trajectories.Count()} 条轨迹记录:");
                Console.WriteLine();
                Console.WriteLine($"{"ID",-36} {"状态",-10} {"开始时间",-20} {"描述",-30} {"步骤数",-8}");
                Console.WriteLine(new string('-', 110));

                foreach (var trajectory in trajectories)
                {
                    var statusText = GetStatusText(trajectory.Status);
                    var startTime = trajectory.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    var description = trajectory.Description?.Length > 27 
                        ? trajectory.Description[..27] + "..." 
                        : trajectory.Description ?? "";
                    
                    Console.WriteLine($"{trajectory.Id,-36} {statusText,-10} {startTime,-20} {description,-30} {trajectory.Steps?.Count ?? 0,-8}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "列出轨迹时发生错误");
                Console.WriteLine($"列出轨迹失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示轨迹详情
        /// </summary>
        public async Task ShowTrajectoryAsync(string id)
        {
            try
            {
                var recorder = GetTrajectoryRecorder();
                var trajectory = await recorder.GetTrajectoryAsync(id);
                
                if (trajectory == null)
                {
                    Console.WriteLine($"未找到ID为 {id} 的轨迹");
                    return;
                }

                Console.WriteLine($"轨迹详情: {trajectory.Id}");
                Console.WriteLine(new string('=', 50));
                Console.WriteLine($"会话ID: {trajectory.SessionId}");
                Console.WriteLine($"描述: {trajectory.Description}");
                Console.WriteLine($"状态: {GetStatusText(trajectory.Status)}");
                Console.WriteLine($"开始时间: {trajectory.StartTime:yyyy-MM-dd HH:mm:ss}");
                
                if (trajectory.EndTime != null)
                {
                    Console.WriteLine($"结束时间: {trajectory.EndTime:yyyy-MM-dd HH:mm:ss}");
                    var totalTime = TimeSpan.FromMilliseconds(trajectory.TotalExecutionTimeMs);
                    Console.WriteLine($"总耗时: {totalTime:hh\\:mm\\:ss\\.fff}");
                }

                if (trajectory.Result != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("执行结果:");
                    Console.WriteLine($"  成功: {(trajectory.Result.Success ? "是" : "否")}");
                    Console.WriteLine($"  消息: {trajectory.Result.Message}");
                    
                    if (trajectory.Result.Data != null && !string.IsNullOrEmpty(trajectory.Result.Data.ToString()))
                    {
                        Console.WriteLine($"  数据: {trajectory.Result.Data}");
                    }
                    
                    if (!string.IsNullOrEmpty(trajectory.Result.Error))
                    {
                        Console.WriteLine($"  错误: {trajectory.Result.Error}");
                    }

                    if (trajectory.Result.Statistics != null)
                    {
                        var stats = trajectory.Result.Statistics;
                        Console.WriteLine();
                        Console.WriteLine("统计信息:");
                        Console.WriteLine($"  总步骤数: {stats.TotalSteps}");
                        Console.WriteLine($"  成功步骤: {stats.SuccessfulSteps}");
                        Console.WriteLine($"  失败步骤: {stats.FailedSteps}");
                        Console.WriteLine($"  LLM调用次数: {stats.LLMCalls}");
                        Console.WriteLine($"  工具执行次数: {stats.ToolExecutions}");
                        Console.WriteLine($"  总Token使用: {stats.TotalTokenUsage}");
                    }
                }

                if (trajectory.Steps?.Any() == true)
                {
                    Console.WriteLine();
                    Console.WriteLine($"执行步骤 ({trajectory.Steps.Count} 个):");
                    Console.WriteLine(new string('-', 80));
                    
                    foreach (var step in trajectory.Steps.OrderBy(s => s.StepNumber))
                    {
                        Console.WriteLine($"步骤 {step.StepNumber}: {step.Name}");
                        Console.WriteLine($"  类型: {GetStepTypeText(step.Type)}");
                        Console.WriteLine($"  状态: {GetStepStatusText(step.Status)}");
                        Console.WriteLine($"  描述: {step.Description}");
                        
                        Console.WriteLine($"  开始: {step.StartTime:HH:mm:ss.fff}");
                        
                        if (step.EndTime.HasValue)
                        {
                            Console.WriteLine($"  结束: {step.EndTime.Value:HH:mm:ss.fff}");
                            var stepTime = TimeSpan.FromMilliseconds(step.ExecutionTimeMs);
                            Console.WriteLine($"  耗时: {stepTime:hh\\:mm\\:ss\\.fff}");
                        }
                        
                        if (!string.IsNullOrEmpty(step.InputData))
                        {
                            var input = step.InputData.Length > 100 ? step.InputData[..100] + "..." : step.InputData;
                            Console.WriteLine($"  输入: {input ?? "无"}");
                        }
                        
                        if (!string.IsNullOrEmpty(step.OutputData))
                        {
                            var output = step.OutputData.Length > 100 ? step.OutputData[..100] + "..." : step.OutputData;
                            Console.WriteLine($"  输出: {output}");
                        }
                        
                        if (!string.IsNullOrEmpty(step.Error))
                        {
                            Console.WriteLine($"  错误: {step.Error}");
                        }
                        
                        Console.WriteLine();
                    }
                }

                if (trajectory.Metadata?.Any() == true)
                {
                    Console.WriteLine("元数据:");
                    foreach (var kvp in trajectory.Metadata)
                    {
                        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "显示轨迹详情时发生错误");
                Console.WriteLine($"显示轨迹详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除轨迹
        /// </summary>
        public async Task DeleteTrajectoryAsync(string id)
        {
            try
            {
                var recorder = GetTrajectoryRecorder();
                var trajectory = await recorder.GetTrajectoryAsync(id);
                
                if (trajectory == null)
                {
                    Console.WriteLine($"未找到ID为 {id} 的轨迹");
                    return;
                }

                Console.Write($"确定要删除轨迹 '{trajectory.Description}' 吗？(y/N): ");
                var confirmation = Console.ReadLine();
                
                if (confirmation?.ToLower() != "y")
                {
                    Console.WriteLine("操作已取消");
                    return;
                }

                await recorder.DeleteTrajectoryAsync(id);
                Console.WriteLine($"轨迹 {id} 已删除");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除轨迹时发生错误");
                Console.WriteLine($"删除轨迹失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理旧轨迹
        /// </summary>
        public async Task CleanupOldTrajectoriesAsync(int daysToKeep = 30)
        {
            try
            {
                var recorder = GetTrajectoryRecorder();
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                
                var query = new TrajectorySearchQuery
                {
                    EndTimeBefore = cutoffDate,
                    Limit = 1000
                };

                var oldTrajectories = await recorder.SearchTrajectoriesAsync(query);
                
                if (!oldTrajectories.Any())
                {
                    Console.WriteLine($"没有找到 {daysToKeep} 天前的轨迹记录");
                    return;
                }

                Console.WriteLine($"找到 {oldTrajectories.Count()} 条超过 {daysToKeep} 天的轨迹记录");
                Console.Write("确定要删除这些记录吗？(y/N): ");
                
                var confirmation = Console.ReadLine();
                if (confirmation?.ToLower() != "y")
                {
                    Console.WriteLine("操作已取消");
                    return;
                }

                int deletedCount = 0;
                foreach (var trajectory in oldTrajectories)
                {
                    try
                    {
                        await recorder.DeleteTrajectoryAsync(trajectory.Id);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"删除轨迹 {trajectory.Id} 时发生错误");
                    }
                }

                Console.WriteLine($"已删除 {deletedCount} 条轨迹记录");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理旧轨迹时发生错误");
                Console.WriteLine($"清理旧轨迹失败: {ex.Message}");
            }
        }

        private static string GetStatusText(TrajectoryStatus status)
        {
            return status switch
            {
                TrajectoryStatus.InProgress => "进行中",
                TrajectoryStatus.Completed => "已完成",
                TrajectoryStatus.Failed => "失败",
                TrajectoryStatus.Cancelled => "已取消",
                _ => status.ToString()
            };
        }

        private static string GetStepTypeText(StepType type)
        {
            return type switch
            {
                StepType.LLMCall => "LLM调用",
                StepType.ToolExecution => "工具执行",
                StepType.UserInput => "用户输入",
                StepType.SystemOperation => "系统操作",
                _ => type.ToString()
            };
        }

        private static string GetStepStatusText(StepStatus status)
        {
            return status switch
            {
                StepStatus.InProgress => "进行中",
                StepStatus.Completed => "已完成",
                StepStatus.Failed => "失败",
                StepStatus.Skipped => "已跳过",
                _ => status.ToString()
            };
        }
    }
}