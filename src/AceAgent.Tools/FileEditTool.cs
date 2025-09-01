using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;

namespace AceAgent.Tools
{
    /// <summary>
    /// 基于字符串替换的文件编辑工具
    /// </summary>
    public class FileEditTool : ITool
    {
        /// <summary>
        /// 工具名称
        /// </summary>
        public string Name => "file_edit_tool";
        
        /// <summary>
        /// 工具描述
        /// </summary>
        public string Description => "基于字符串替换的文件编辑工具，支持安全的文件修改操作";

        /// <summary>
        /// 执行文件编辑工具
        /// </summary>
        /// <param name="input">工具输入</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>工具执行结果</returns>
        public async Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var filePath = input.GetParameter<string>("file_path");
                var searchText = input.GetParameter<string>("search_text");
                var replaceText = input.GetParameter<string>("replace_text");
                var createBackup = input.GetParameter<bool?>("create_backup") ?? true;
                var encoding = input.GetParameter<string>("encoding") ?? "utf-8";

                if (string.IsNullOrEmpty(filePath))
                    return ToolResult.Failure("文件路径不能为空");

                if (string.IsNullOrEmpty(searchText))
                    return ToolResult.Failure("搜索文本不能为空");

                if (replaceText == null)
                    replaceText = string.Empty;

                // 验证文件路径
                if (!File.Exists(filePath))
                    return ToolResult.Failure($"文件不存在: {filePath}");

                // 检查文件权限
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.IsReadOnly)
                    return ToolResult.Failure($"文件为只读: {filePath}");

                // 读取文件内容
                var encodingObj = GetEncoding(encoding);
                var originalContent = await File.ReadAllTextAsync(filePath, encodingObj, cancellationToken);

                // 检查搜索文本是否存在
                if (!originalContent.Contains(searchText))
                    return ToolResult.Failure($"在文件中未找到搜索文本: {searchText}");

                // 创建备份
                string? backupPath = null;
                if (createBackup)
                {
                    backupPath = $"{filePath}.backup.{DateTime.Now:yyyyMMddHHmmss}";
                    await File.WriteAllTextAsync(backupPath, originalContent, encodingObj, cancellationToken);
                }

                // 执行替换
                var newContent = originalContent.Replace(searchText, replaceText);
                
                // 计算变更统计
                var searchCount = (originalContent.Length - originalContent.Replace(searchText, "").Length) / searchText.Length;
                
                // 写入新内容
                await File.WriteAllTextAsync(filePath, newContent, encodingObj, cancellationToken);

                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                var result = ToolResult.CreateSuccess($"文件编辑完成，替换了 {searchCount} 处匹配项", new
                {
                    FilePath = filePath,
                    BackupPath = backupPath,
                    ReplacementCount = searchCount,
                    OriginalSize = originalContent.Length,
                    NewSize = newContent.Length,
                    SizeDifference = newContent.Length - originalContent.Length
                });
                
                result.ExecutionTimeMs = (long)executionTime;
                result.Metadata["operation"] = "file_edit";
                result.Metadata["file_path"] = filePath;
                
                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                return ToolResult.Failure($"访问文件被拒绝: {ex.Message}");
            }
            catch (IOException ex)
            {
                return ToolResult.Failure($"文件IO错误: {ex.Message}");
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
            
            var filePath = input.GetParameter<string>("file_path");
            var searchText = input.GetParameter<string>("search_text");

            if (string.IsNullOrEmpty(filePath))
                return false;

            if (string.IsNullOrEmpty(searchText))
                return false;

            // 验证文件路径格式
            try
            {
                Path.GetFullPath(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Encoding GetEncoding(string encodingName)
        {
            return encodingName.ToLowerInvariant() switch
            {
                "utf-8" or "utf8" => Encoding.UTF8,
                "utf-16" or "utf16" => Encoding.Unicode,
                "ascii" => Encoding.ASCII,
                "gb2312" or "gbk" => Encoding.GetEncoding("GB2312"),
                _ => Encoding.UTF8
            };
        }
    }

    /// <summary>
    /// 文件编辑工具输入参数
    /// </summary>
    public class FileEditInput
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// 搜索文本
        /// </summary>
        public string SearchText { get; set; } = string.Empty;
        
        /// <summary>
        /// 替换文本
        /// </summary>
        public string ReplaceText { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否创建备份
        /// </summary>
        public bool CreateBackup { get; set; } = true;
        
        /// <summary>
        /// 文件编码
        /// </summary>
        public string Encoding { get; set; } = "utf-8";
    }
}