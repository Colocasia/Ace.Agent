using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;

namespace AceAgent.Tools
{
    /// <summary>
    /// 任务完成工具
    /// 用于跟踪、管理和完成复杂任务的执行状态
    /// </summary>
    public class TaskDoneTool : ITool
    {
        private readonly Dictionary<string, TaskExecution> _activeTasks = new();
        private readonly object _lock = new();

        /// <summary>
        /// 工具名称
        /// </summary>
        public string Name => "task_done";
        
        /// <summary>
        /// 工具描述
        /// </summary>
        public string Description => "任务完成工具，用于跟踪、管理和完成复杂任务的执行状态";

        /// <summary>
        /// 执行任务完成工具
        /// </summary>
        /// <param name="input">工具输入</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>工具执行结果</returns>
        public async Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken = default)
        {
            try
            {
                var action = input.GetParameter<string>("action");
                
                return action?.ToLower() switch
                {
                    "create" => await CreateTaskAsync(input),
                    "update" => await UpdateTaskAsync(input),
                    "complete" => await CompleteTaskAsync(input),
                    "cancel" => await CancelTaskAsync(input),
                    "get" => await GetTaskAsync(input),
                    "list" => await ListTasksAsync(input),
                    "progress" => await UpdateProgressAsync(input),
                    "add_subtask" => await AddSubtaskAsync(input),
                    "remove_subtask" => await RemoveSubtaskAsync(input),
                    "get_statistics" => await GetStatisticsAsync(input),
                    _ => ToolResult.Failure($"不支持的操作：{action}")
                };
            }
            catch (Exception ex)
            {
                return ToolResult.FromException(ex);
            }
        }

        /// <summary>
        /// 验证输入参数
        /// </summary>
        /// <param name="input">工具输入</param>
        /// <returns>验证结果</returns>
        public Task<bool> ValidateInputAsync(ToolInput input)
        {
            var action = input.GetParameter<string>("action");
            var validActions = new[] { "create", "update", "complete", "cancel", "get", "list", "progress", "add_subtask", "remove_subtask", "get_statistics" };
            
            var isValid = !string.IsNullOrWhiteSpace(action) && validActions.Contains(action.ToLower());
            return Task.FromResult(isValid);
        }

        private async Task<ToolResult> CreateTaskAsync(ToolInput input)
        {
            var taskId = input.GetParameter<string>("task_id") ?? Guid.NewGuid().ToString();
            var title = input.GetParameter<string>("title") ?? "";
            var description = input.GetParameter<string>("description") ?? "";
            var priority = input.GetParameter<string>("priority") ?? "medium";
            var estimatedDuration = input.GetParameter<int?>("estimated_duration_minutes") ?? 60;
            var tags = input.GetParameter<List<string>>("tags") ?? new List<string>();
            var dependencies = input.GetParameter<List<string>>("dependencies") ?? new List<string>();

            if (string.IsNullOrWhiteSpace(title))
            {
                return ToolResult.Failure("任务标题不能为空");
            }

            lock (_lock)
            {
                if (_activeTasks.ContainsKey(taskId))
                {
                    return ToolResult.Failure($"任务ID {taskId} 已存在");
                }

                var task = new TaskExecution
                {
                    Id = taskId,
                    Title = title,
                    Description = description,
                    Status = TaskStatus.Created,
                    Priority = ParsePriority(priority),
                    CreatedAt = DateTime.UtcNow,
                    EstimatedDuration = TimeSpan.FromMinutes(estimatedDuration),
                    Tags = tags,
                    Dependencies = dependencies,
                    Progress = 0.0,
                    Subtasks = new List<Subtask>(),
                    StatusHistory = new List<TaskStatusChange>
                    {
                        new TaskStatusChange
                        {
                            Status = TaskStatus.Created,
                            Timestamp = DateTime.UtcNow,
                            Reason = "任务创建"
                        }
                    },
                    Metadata = new Dictionary<string, object>()
                };

                _activeTasks[taskId] = task;
            }

            await Task.Delay(1); // 模拟异步操作

            var result = ToolResult.CreateSuccess(
                $"任务 '{title}' 创建成功",
                new { task_id = taskId, status = "created" }
            );
            result.Metadata["task_id"] = taskId;
            return result;
        }

        private async Task<ToolResult> UpdateTaskAsync(ToolInput input)
        {
            var taskId = input.GetParameter<string>("task_id");
            
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return ToolResult.Failure("任务ID不能为空");
            }

            lock (_lock)
            {
                if (!_activeTasks.TryGetValue(taskId, out var task))
                {
                    return ToolResult.Failure($"任务 {taskId} 不存在");
                }

                // 更新可选字段
                if (input.Parameters.ContainsKey("title"))
                {
                    task.Title = input.GetParameter<string>("title") ?? "";
                }
                
                if (input.Parameters.ContainsKey("description"))
                {
                    task.Description = input.GetParameter<string>("description") ?? "";
                }
                
                if (input.Parameters.ContainsKey("priority"))
                {
                    var priorityParam = input.GetParameter<string>("priority") ?? "medium";
                    task.Priority = ParsePriority(priorityParam);
                }
                
                if (input.Parameters.ContainsKey("estimated_duration_minutes"))
                {
                    task.EstimatedDuration = TimeSpan.FromMinutes(input.GetParameter<int>("estimated_duration_minutes"));
                }
                
                if (input.Parameters.ContainsKey("tags"))
                {
                    task.Tags = input.GetParameter<List<string>>("tags") ?? new List<string>();
                }

                task.UpdatedAt = DateTime.UtcNow;
            }

            await Task.Delay(1);

            return ToolResult.CreateSuccess(
                $"任务 {taskId} 更新成功",
                new { task_id = taskId, status = "updated" }
            );
        }

        private async Task<ToolResult> CompleteTaskAsync(ToolInput input)
        {
            var taskId = input.GetParameter<string>("task_id");
            var result = input.GetParameter<string>("result") ?? "";
            var notes = input.GetParameter<string>("notes") ?? "";

            if (string.IsNullOrWhiteSpace(taskId))
            {
                return ToolResult.Failure("任务ID不能为空");
            }

            lock (_lock)
            {
                if (!_activeTasks.TryGetValue(taskId, out var task))
                {
                    return ToolResult.Failure($"任务 {taskId} 不存在");
                }

                if (task.Status == TaskStatus.Completed)
                {
                    return ToolResult.Failure($"任务 {taskId} 已经完成");
                }

                if (task.Status == TaskStatus.Cancelled)
                {
                    return ToolResult.Failure($"任务 {taskId} 已被取消");
                }

                // 检查依赖任务是否完成
                var incompleteDependencies = task.Dependencies
                    .Where(depId => _activeTasks.ContainsKey(depId) && _activeTasks[depId].Status != TaskStatus.Completed)
                    .ToList();

                if (incompleteDependencies.Any())
                {
                    return ToolResult.Failure($"依赖任务尚未完成：{string.Join(", ", incompleteDependencies)}");
                }

                // 检查子任务是否全部完成
                var incompleteSubtasks = task.Subtasks.Where(st => st.Status != SubtaskStatus.Completed).ToList();
                if (incompleteSubtasks.Any())
                {
                    return ToolResult.Failure($"子任务尚未完成：{string.Join(", ", incompleteSubtasks.Select(st => st.Title))}");
                }

                task.Status = TaskStatus.Completed;
                task.CompletedAt = DateTime.UtcNow;
                task.Progress = 100.0;
                task.Result = result;
                task.Notes = notes;
                task.ActualDuration = task.CompletedAt - task.StartedAt;

                task.StatusHistory.Add(new TaskStatusChange
                {
                    Status = TaskStatus.Completed,
                    Timestamp = DateTime.UtcNow,
                    Reason = "任务完成",
                    Notes = notes
                });
            }

            await Task.Delay(1);

            var toolResult = ToolResult.CreateSuccess(
                $"任务 {taskId} 完成",
                new { task_id = taskId, status = "completed", result }
            );
            toolResult.Metadata["completion_time"] = DateTime.UtcNow;
            toolResult.Metadata["result"] = result;
            return toolResult;
        }

        private async Task<ToolResult> CancelTaskAsync(ToolInput input)
        {
            var taskId = input.GetParameter<string>("task_id");
            var reason = input.GetParameter<string>("reason") ?? "用户取消";

            if (string.IsNullOrWhiteSpace(taskId))
            {
                return ToolResult.Failure("任务ID不能为空");
            }

            lock (_lock)
            {
                if (!_activeTasks.TryGetValue(taskId, out var task))
                {
                    return ToolResult.Failure($"任务 {taskId} 不存在");
                }

                if (task.Status == TaskStatus.Completed)
                {
                    return ToolResult.Failure($"任务 {taskId} 已完成，无法取消");
                }

                if (task.Status == TaskStatus.Cancelled)
                {
                    return ToolResult.Failure($"任务 {taskId} 已被取消");
                }

                task.Status = TaskStatus.Cancelled;
                task.CancelledAt = DateTime.UtcNow;
                task.CancellationReason = reason;

                task.StatusHistory.Add(new TaskStatusChange
                {
                    Status = TaskStatus.Cancelled,
                    Timestamp = DateTime.UtcNow,
                    Reason = reason
                });

                // 取消所有子任务
                foreach (var subtask in task.Subtasks.Where(st => st.Status != SubtaskStatus.Completed))
                {
                    subtask.Status = SubtaskStatus.Cancelled;
                    subtask.CompletedAt = DateTime.UtcNow;
                }
            }

            await Task.Delay(1);

            return ToolResult.CreateSuccess(
                $"任务 {taskId} 已取消",
                new { task_id = taskId, status = "cancelled", reason }
            );
        }

        private async Task<ToolResult> GetTaskAsync(ToolInput input)
        {
            var taskId = input.GetParameter<string>("task_id");

            if (string.IsNullOrWhiteSpace(taskId))
            {
                return ToolResult.Failure("任务ID不能为空");
            }

            TaskExecution? task;
            lock (_lock)
            {
                if (!_activeTasks.TryGetValue(taskId, out task))
                {
                    return ToolResult.Failure($"任务 {taskId} 不存在");
                }
            }

            await Task.Delay(1);

            var result = ToolResult.CreateSuccess(
                $"获取任务 {taskId} 信息",
                task
            );
            result.Metadata["task_id"] = taskId;
            return result;
        }

        private async Task<ToolResult> ListTasksAsync(ToolInput input)
        {
            var status = input.GetParameter<string>("status") ?? "";
            var priority = input.GetParameter<string>("priority") ?? "";
            var tag = input.GetParameter<string>("tag") ?? "";
            var limit = input.GetParameter<int?>("limit") ?? 50;

            List<TaskExecution> tasks;
            lock (_lock)
            {
                tasks = _activeTasks.Values.ToList();
            }

            // 应用过滤器
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TaskStatus>(status, true, out var statusFilter))
            {
                tasks = tasks.Where(t => t.Status == statusFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TaskPriority>(priority, true, out var priorityFilter))
            {
                tasks = tasks.Where(t => t.Priority == priorityFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                tasks = tasks.Where(t => t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            // 排序和限制
            tasks = tasks
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .Take(limit)
                .ToList();

            await Task.Delay(1);

            var result = ToolResult.CreateSuccess(
                $"找到 {tasks.Count} 个任务",
                tasks.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Status,
                    t.Priority,
                    t.Progress,
                    t.CreatedAt,
                    SubtaskCount = t.Subtasks.Count,
                    CompletedSubtasks = t.Subtasks.Count(st => st.Status == SubtaskStatus.Completed)
                }).ToList()
            );
            result.Metadata["total_count"] = tasks.Count;
            return result;
        }

        private async Task<ToolResult> UpdateProgressAsync(ToolInput input)
        {
            var taskId = input.GetParameter<string>("task_id");
            var progress = input.GetParameter<double>("progress");
            var notes = input.GetParameter<string>("notes") ?? "";

            if (string.IsNullOrWhiteSpace(taskId))
            {
                return ToolResult.Failure("任务ID不能为空");
            }

            if (progress < 0 || progress > 100)
            {
                return ToolResult.Failure("进度值必须在0-100之间");
            }

            lock (_lock)
            {
                if (!_activeTasks.TryGetValue(taskId, out var task))
                {
                    return ToolResult.Failure($"任务 {taskId} 不存在");
                }

                if (task.Status == TaskStatus.Completed || task.Status == TaskStatus.Cancelled)
                {
                    return ToolResult.Failure($"任务 {taskId} 已结束，无法更新进度");
                }

                var oldProgress = task.Progress;
                task.Progress = progress;
                task.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(notes))
                {
                    task.Notes = notes;
                }

                // 如果是第一次更新进度，标记为进行中
                if (task.Status == TaskStatus.Created && progress > 0)
                {
                    task.Status = TaskStatus.InProgress;
                    task.StartedAt = DateTime.UtcNow;
                    
                    task.StatusHistory.Add(new TaskStatusChange
                    {
                        Status = TaskStatus.InProgress,
                        Timestamp = DateTime.UtcNow,
                        Reason = "开始执行任务"
                    });
                }

                // 记录进度变化
                task.ProgressHistory.Add(new ProgressUpdate
                {
                    Timestamp = DateTime.UtcNow,
                    OldProgress = oldProgress,
                    NewProgress = progress,
                    Notes = notes
                });
            }

            await Task.Delay(1);

            var result = ToolResult.CreateSuccess(
                $"任务 {taskId} 进度更新为 {progress:F1}%",
                new { task_id = taskId, progress, notes }
            );
            result.Metadata["progress"] = progress;
            return result;
        }

        private async Task<ToolResult> AddSubtaskAsync(ToolInput input)
        {
            var taskId = input.GetParameter<string>("task_id");
            var subtaskTitle = input.GetParameter<string>("subtask_title");
            var subtaskDescription = input.GetParameter<string>("subtask_description") ?? "";
            var estimatedMinutes = input.GetParameter<int?>("estimated_minutes") ?? 30;

            if (string.IsNullOrWhiteSpace(taskId) || string.IsNullOrWhiteSpace(subtaskTitle))
            {
                return ToolResult.Failure("任务ID和子任务标题不能为空");
            }

            lock (_lock)
            {
                if (!_activeTasks.TryGetValue(taskId, out var task))
                {
                    return ToolResult.Failure($"任务 {taskId} 不存在");
                }

                var subtask = new Subtask
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = subtaskTitle,
                    Description = subtaskDescription,
                    Status = SubtaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    EstimatedDuration = TimeSpan.FromMinutes(estimatedMinutes)
                };

                task.Subtasks.Add(subtask);
                task.UpdatedAt = DateTime.UtcNow;
            }

            await Task.Delay(1);

            return ToolResult.CreateSuccess(
                $"子任务 '{subtaskTitle}' 添加成功",
                new { task_id = taskId, subtask_title = subtaskTitle }
            );
        }

        private async Task<ToolResult> RemoveSubtaskAsync(ToolInput input)
        {
            var taskId = input.GetParameter<string>("task_id");
            var subtaskId = input.GetParameter<string>("subtask_id");

            if (string.IsNullOrWhiteSpace(taskId) || string.IsNullOrWhiteSpace(subtaskId))
            {
                return ToolResult.Failure("任务ID和子任务ID不能为空");
            }

            lock (_lock)
            {
                if (!_activeTasks.TryGetValue(taskId, out var task))
                {
                    return ToolResult.Failure($"任务 {taskId} 不存在");
                }

                var subtask = task.Subtasks.FirstOrDefault(st => st.Id == subtaskId);
                if (subtask == null)
                {
                    return ToolResult.Failure($"子任务 {subtaskId} 不存在");
                }

                task.Subtasks.Remove(subtask);
                task.UpdatedAt = DateTime.UtcNow;
            }

            await Task.Delay(1);

            return ToolResult.CreateSuccess(
                $"子任务 {subtaskId} 移除成功",
                new { task_id = taskId, subtask_id = subtaskId }
            );
        }

        private async Task<ToolResult> GetStatisticsAsync(ToolInput input)
        {
            Dictionary<TaskStatus, int> statusCounts;
            Dictionary<TaskPriority, int> priorityCounts;
            int totalTasks;
            double avgProgress;
            TimeSpan totalEstimatedTime;
            TimeSpan totalActualTime;

            lock (_lock)
            {
                var tasks = _activeTasks.Values.ToList();
                totalTasks = tasks.Count;
                
                statusCounts = tasks
                    .GroupBy(t => t.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                priorityCounts = tasks
                    .GroupBy(t => t.Priority)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                avgProgress = tasks.Any() ? tasks.Average(t => t.Progress) : 0;
                totalEstimatedTime = TimeSpan.FromTicks(tasks.Sum(t => t.EstimatedDuration.Ticks));
                totalActualTime = TimeSpan.FromTicks(tasks.Where(t => t.ActualDuration.HasValue).Sum(t => t.ActualDuration!.Value.Ticks));
            }

            await Task.Delay(1);

            var statistics = new
            {
                total_tasks = totalTasks,
                status_distribution = statusCounts,
                priority_distribution = priorityCounts,
                average_progress = Math.Round(avgProgress, 2),
                total_estimated_time_hours = Math.Round(totalEstimatedTime.TotalHours, 2),
                total_actual_time_hours = Math.Round(totalActualTime.TotalHours, 2),
                efficiency_ratio = totalEstimatedTime.TotalHours > 0 ? Math.Round(totalActualTime.TotalHours / totalEstimatedTime.TotalHours, 2) : 0
            };

            var result = ToolResult.CreateSuccess(
                "任务统计信息",
                statistics
            );
            result.Metadata["generated_at"] = DateTime.UtcNow;
            return result;
        }

        private TaskPriority ParsePriority(string priority)
        {
            return priority?.ToLower() switch
            {
                "low" => TaskPriority.Low,
                "medium" => TaskPriority.Medium,
                "high" => TaskPriority.High,
                "urgent" => TaskPriority.Urgent,
                _ => TaskPriority.Medium
            };
        }
    }

    /// <summary>
    /// 任务执行相关的数据模型
    /// </summary>
    public class TaskExecution
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 任务标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus Status { get; set; }
        
        /// <summary>
        /// 任务优先级
        /// </summary>
        public TaskPriority Priority { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartedAt { get; set; }
        
        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// 取消时间
        /// </summary>
        public DateTime? CancelledAt { get; set; }
        
        /// <summary>
        /// 取消原因
        /// </summary>
        public string? CancellationReason { get; set; }
        
        /// <summary>
        /// 预估持续时间
        /// </summary>
        public TimeSpan EstimatedDuration { get; set; }
        
        /// <summary>
        /// 实际持续时间
        /// </summary>
        public TimeSpan? ActualDuration { get; set; }
        
        /// <summary>
        /// 进度百分比
        /// </summary>
        public double Progress { get; set; }
        
        /// <summary>
        /// 任务结果
        /// </summary>
        public string? Result { get; set; }
        
        /// <summary>
        /// 备注
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// 标签列表
        /// </summary>
        public List<string> Tags { get; set; } = new();
        
        /// <summary>
        /// 依赖任务列表
        /// </summary>
        public List<string> Dependencies { get; set; } = new();
        
        /// <summary>
        /// 子任务列表
        /// </summary>
        public List<Subtask> Subtasks { get; set; } = new();
        
        /// <summary>
        /// 状态变更历史
        /// </summary>
        public List<TaskStatusChange> StatusHistory { get; set; } = new();
        
        /// <summary>
        /// 进度更新历史
        /// </summary>
        public List<ProgressUpdate> ProgressHistory { get; set; } = new();
        
        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// 子任务
    /// </summary>
    public class Subtask
    {
        /// <summary>
        /// 子任务ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 子任务标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 子任务描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 子任务状态
        /// </summary>
        public SubtaskStatus Status { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// 预估持续时间
        /// </summary>
        public TimeSpan EstimatedDuration { get; set; }
        
        /// <summary>
        /// 实际持续时间
        /// </summary>
        public TimeSpan? ActualDuration { get; set; }
        
        /// <summary>
        /// 备注
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 任务状态变更记录
    /// </summary>
    public class TaskStatusChange
    {
        /// <summary>
        /// 状态
        /// </summary>
        public TaskStatus Status { get; set; }
        
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 变更原因
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// 备注
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 进度更新记录
    /// </summary>
    public class ProgressUpdate
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 旧进度
        /// </summary>
        public double OldProgress { get; set; }
        
        /// <summary>
        /// 新进度
        /// </summary>
        public double NewProgress { get; set; }
        
        /// <summary>
        /// 备注
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 任务状态
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 已创建
        /// </summary>
        Created,
        
        /// <summary>
        /// 进行中
        /// </summary>
        InProgress,
        
        /// <summary>
        /// 已暂停
        /// </summary>
        Paused,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        
        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// 已失败
        /// </summary>
        Failed
    }

    /// <summary>
    /// 任务优先级
    /// </summary>
    public enum TaskPriority
    {
        /// <summary>
        /// 低优先级
        /// </summary>
        Low,
        
        /// <summary>
        /// 中等优先级
        /// </summary>
        Medium,
        
        /// <summary>
        /// 高优先级
        /// </summary>
        High,
        
        /// <summary>
        /// 紧急
        /// </summary>
        Urgent
    }

    /// <summary>
    /// 子任务状态
    /// </summary>
    public enum SubtaskStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        Pending,
        
        /// <summary>
        /// 进行中
        /// </summary>
        InProgress,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        
        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled
    }
}