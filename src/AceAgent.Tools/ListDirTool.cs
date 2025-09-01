using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;

namespace AceAgent.Tools
{
    /// <summary>
    /// 目录列表工具
    /// 用于列出指定目录的内容
    /// </summary>
    public class ListDirTool : ITool
    {
        /// <summary>
        /// 工具名称
        /// </summary>
        public string Name => "list_dir";
        
        /// <summary>
        /// 工具描述
        /// </summary>
        public string Description => "列出指定目录的内容，包括文件和子目录";

        /// <summary>
        /// 执行目录列表工具
        /// </summary>
        /// <param name="input">工具输入</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>工具执行结果</returns>
        public async Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var directoryPath = input.GetParameter<string>("directory_path");
                var includeHidden = input.GetParameter<bool?>("include_hidden") ?? false;
                var recursive = input.GetParameter<bool?>("recursive") ?? false;
                var maxDepth = input.GetParameter<int?>("max_depth") ?? 1;
                var sortBy = input.GetParameter<string>("sort_by") ?? "name"; // name, size, date
                var sortOrder = input.GetParameter<string>("sort_order") ?? "asc"; // asc, desc

                if (string.IsNullOrWhiteSpace(directoryPath))
                    return ToolResult.Failure("目录路径不能为空");

                // 规范化路径
                directoryPath = Path.GetFullPath(directoryPath);

                if (!Directory.Exists(directoryPath))
                    return ToolResult.Failure($"目录不存在: {directoryPath}");

                var items = new List<DirectoryItem>();
                
                if (recursive)
                {
                    await ListDirectoryRecursiveAsync(directoryPath, items, includeHidden, maxDepth, 0, cancellationToken);
                }
                else
                {
                    await ListDirectoryAsync(directoryPath, items, includeHidden, cancellationToken);
                }

                // 排序
                items = SortItems(items, sortBy, sortOrder);
                
                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                var result = ToolResult.CreateSuccess(
                    $"成功列出目录内容，共 {items.Count} 项",
                    new
                    {
                        DirectoryPath = directoryPath,
                        ItemCount = items.Count,
                        Items = items.Select(item => new
                        {
                            Name = item.Name,
                            Type = item.Type,
                            Size = item.Size,
                            LastModified = item.LastModified,
                            FullPath = item.FullPath,
                            RelativePath = item.RelativePath,
                            IsHidden = item.IsHidden
                        })
                    }
                );
                
                result.ExecutionTimeMs = (long)executionTime;
                result.Metadata["operation"] = "list_directory";
                result.Metadata["directory_path"] = directoryPath;
                result.Metadata["item_count"] = items.Count;
                
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                return ToolResult.Failure($"访问被拒绝: {ex.Message}");
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
        public async Task<bool> ValidateInputAsync(ToolInput input)
        {
            await Task.CompletedTask;
            
            var directoryPath = input.GetParameter<string>("directory_path");
            var maxDepth = input.GetParameter<int?>("max_depth") ?? 1;

            return !string.IsNullOrWhiteSpace(directoryPath) && maxDepth > 0 && maxDepth <= 10;
        }

        private async Task ListDirectoryAsync(
            string directoryPath, 
            List<DirectoryItem> items, 
            bool includeHidden, 
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                
                // 列出子目录
                foreach (var dir in directoryInfo.GetDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (!includeHidden && IsHidden(dir))
                        continue;

                    items.Add(new DirectoryItem
                    {
                        Name = dir.Name,
                        Type = "directory",
                        Size = null,
                        LastModified = dir.LastWriteTime,
                        FullPath = dir.FullName,
                        RelativePath = Path.GetRelativePath(directoryPath, dir.FullName),
                        IsHidden = IsHidden(dir)
                    });
                }
                
                // 列出文件
                foreach (var file in directoryInfo.GetFiles())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (!includeHidden && IsHidden(file))
                        continue;

                    items.Add(new DirectoryItem
                    {
                        Name = file.Name,
                        Type = "file",
                        Size = file.Length,
                        LastModified = file.LastWriteTime,
                        FullPath = file.FullName,
                        RelativePath = Path.GetRelativePath(directoryPath, file.FullName),
                        IsHidden = IsHidden(file)
                    });
                }
            }, cancellationToken);
        }

        private async Task ListDirectoryRecursiveAsync(
            string directoryPath, 
            List<DirectoryItem> items, 
            bool includeHidden, 
            int maxDepth, 
            int currentDepth, 
            CancellationToken cancellationToken)
        {
            if (currentDepth >= maxDepth)
                return;

            await ListDirectoryAsync(directoryPath, items, includeHidden, cancellationToken);
            
            var directoryInfo = new DirectoryInfo(directoryPath);
            
            foreach (var subDir in directoryInfo.GetDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (!includeHidden && IsHidden(subDir))
                    continue;

                try
                {
                    await ListDirectoryRecursiveAsync(
                        subDir.FullName, 
                        items, 
                        includeHidden, 
                        maxDepth, 
                        currentDepth + 1, 
                        cancellationToken);
                }
                catch (UnauthorizedAccessException)
                {
                    // 跳过无权访问的目录
                    continue;
                }
            }
        }

        private List<DirectoryItem> SortItems(List<DirectoryItem> items, string sortBy, string sortOrder)
        {
            IEnumerable<DirectoryItem> sorted = sortBy.ToLower() switch
            {
                "size" => items.OrderBy(x => x.Size ?? 0),
                "date" => items.OrderBy(x => x.LastModified),
                "type" => items.OrderBy(x => x.Type).ThenBy(x => x.Name),
                _ => items.OrderBy(x => x.Name)
            };

            if (sortOrder.ToLower() == "desc")
            {
                sorted = sorted.Reverse();
            }

            return sorted.ToList();
        }

        private bool IsHidden(FileSystemInfo info)
        {
            return (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
                   info.Name.StartsWith(".");
        }
    }

    /// <summary>
    /// 目录项
    /// </summary>
    public class DirectoryItem
    {
        /// <summary>
        /// 文件或目录名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 类型（"file" 或 "directory"）
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// 文件大小（字节），目录为null
        /// </summary>
        public long? Size { get; set; }
        
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// 完整路径
        /// </summary>
        public string FullPath { get; set; } = string.Empty;
        
        /// <summary>
        /// 相对路径
        /// </summary>
        public string RelativePath { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否为隐藏文件或目录
        /// </summary>
        public bool IsHidden { get; set; }
    }
}