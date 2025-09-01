using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;

namespace AceAgent.Tools
{
    /// <summary>
    /// 文件查看工具
    /// 用于查看文件内容，支持多种格式和编码
    /// </summary>
    public class ViewFilesTool : ITool
    {
        private static readonly string[] TextExtensions = 
        {
            ".txt", ".md", ".json", ".xml", ".yaml", ".yml", ".csv", ".log",
            ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h", ".hpp",
            ".html", ".css", ".scss", ".less", ".sql", ".sh", ".bat", ".ps1",
            ".php", ".rb", ".go", ".rs", ".swift", ".kt", ".scala", ".clj",
            ".dockerfile", ".gitignore", ".gitattributes", ".editorconfig",
            ".config", ".conf", ".ini", ".properties", ".toml"
        };

        /// <summary>
        /// 工具名称
        /// </summary>
        public string Name => "view_files";
        
        /// <summary>
        /// 工具描述
        /// </summary>
        public string Description => "查看文件内容，支持指定行范围和编码";

        /// <summary>
        /// 执行文件查看工具
        /// </summary>
        /// <param name="input">工具输入参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>文件内容结果</returns>
        public async Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var filePaths = input.GetParameter<string[]>("file_paths") ?? new string[0];
                var encoding = input.GetParameter<string>("encoding") ?? "utf-8";
                var maxLines = input.GetParameter<int?>("max_lines") ?? 1000;
                var startLine = input.GetParameter<int?>("start_line") ?? 1;
                var endLine = input.GetParameter<int?>("end_line");
                var showLineNumbers = input.GetParameter<bool?>("show_line_numbers") ?? true;
                var maxFileSize = input.GetParameter<long?>("max_file_size") ?? 10 * 1024 * 1024; // 10MB

                if (filePaths.Length == 0)
                    return ToolResult.Failure("文件路径列表不能为空");

                if (filePaths.Length > 10)
                    return ToolResult.Failure("一次最多只能查看10个文件");

                if (startLine < 1)
                    return ToolResult.Failure("起始行号必须大于0");

                if (maxLines < 1 || maxLines > 10000)
                    return ToolResult.Failure("最大行数必须在1-10000之间");

                var fileContents = new List<FileContent>();
                var encodingObj = GetEncoding(encoding);

                foreach (var filePath in filePaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var normalizedPath = Path.GetFullPath(filePath);
                    
                    if (!File.Exists(normalizedPath))
                    {
                        fileContents.Add(new FileContent
                        {
                            FilePath = normalizedPath,
                            Error = $"文件不存在: {normalizedPath}"
                        });
                        continue;
                    }

                    var fileInfo = new FileInfo(normalizedPath);
                    
                    if (fileInfo.Length > maxFileSize)
                    {
                        fileContents.Add(new FileContent
                        {
                            FilePath = normalizedPath,
                            Error = $"文件太大 ({fileInfo.Length:N0} 字节)，超过限制 ({maxFileSize:N0} 字节)"
                        });
                        continue;
                    }

                    if (!IsTextFile(normalizedPath))
                    {
                        fileContents.Add(new FileContent
                        {
                            FilePath = normalizedPath,
                            Error = "不支持的文件类型，仅支持文本文件",
                            FileSize = fileInfo.Length,
                            LastModified = fileInfo.LastWriteTime
                        });
                        continue;
                    }

                    try
                    {
                        var content = await ReadFileContentAsync(
                            normalizedPath, 
                            encodingObj, 
                            startLine, 
                            endLine, 
                            maxLines, 
                            showLineNumbers, 
                            cancellationToken);
                        
                        fileContents.Add(content);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        fileContents.Add(new FileContent
                        {
                            FilePath = normalizedPath,
                            Error = "访问被拒绝"
                        });
                    }
                    catch (Exception ex)
                    {
                        fileContents.Add(new FileContent
                        {
                            FilePath = normalizedPath,
                            Error = $"读取文件时出错: {ex.Message}"
                        });
                    }
                }
                
                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var successCount = fileContents.Count(f => string.IsNullOrEmpty(f.Error));
                
                var result = ToolResult.CreateSuccess(
                    $"文件查看完成，成功读取 {successCount}/{filePaths.Length} 个文件",
                    new
                    {
                        FileCount = filePaths.Length,
                        SuccessCount = successCount,
                        Files = fileContents
                    }
                );
                
                result.ExecutionTimeMs = (long)executionTime;
                result.Metadata["operation"] = "view_files";
                result.Metadata["file_count"] = filePaths.Length;
                result.Metadata["success_count"] = successCount;
                
                return result;
            }
            catch (Exception ex)
            {
                return ToolResult.FromException(ex);
            }
        }

        /// <summary>
        /// 验证输入参数
        /// </summary>
        /// <param name="input">工具输入参数</param>
        /// <returns>验证结果</returns>
        public async Task<bool> ValidateInputAsync(ToolInput input)
        {
            await Task.CompletedTask;
            
            var filePaths = input.GetParameter<string[]>("file_paths") ?? new string[0];
            var maxLines = input.GetParameter<int?>("max_lines") ?? 1000;
            var startLine = input.GetParameter<int?>("start_line") ?? 1;

            return filePaths.Length > 0 && 
                   filePaths.Length <= 10 && 
                   maxLines > 0 && 
                   maxLines <= 10000 && 
                   startLine > 0;
        }

        private async Task<FileContent> ReadFileContentAsync(
            string filePath, 
            Encoding encoding, 
            int startLine, 
            int? endLine, 
            int maxLines, 
            bool showLineNumbers, 
            CancellationToken cancellationToken)
        {
            var fileInfo = new FileInfo(filePath);
            var lines = await File.ReadAllLinesAsync(filePath, encoding, cancellationToken);
            
            var totalLines = lines.Length;
            var actualStartLine = Math.Max(1, startLine);
            var actualEndLine = endLine ?? Math.Min(totalLines, actualStartLine + maxLines - 1);
            
            // 确保不超过最大行数限制
            if (actualEndLine - actualStartLine + 1 > maxLines)
            {
                actualEndLine = actualStartLine + maxLines - 1;
            }
            
            var selectedLines = lines
                .Skip(actualStartLine - 1)
                .Take(actualEndLine - actualStartLine + 1)
                .ToArray();

            var contentBuilder = new StringBuilder();
            
            if (showLineNumbers)
            {
                var lineNumberWidth = actualEndLine.ToString().Length;
                
                for (int i = 0; i < selectedLines.Length; i++)
                {
                    var lineNumber = actualStartLine + i;
                    contentBuilder.AppendLine($"{lineNumber.ToString().PadLeft(lineNumberWidth)}: {selectedLines[i]}");
                }
            }
            else
            {
                contentBuilder.AppendLine(string.Join(Environment.NewLine, selectedLines));
            }

            return new FileContent
            {
                FilePath = filePath,
                Content = contentBuilder.ToString().TrimEnd(),
                TotalLines = totalLines,
                DisplayedLines = selectedLines.Length,
                StartLine = actualStartLine,
                EndLine = actualEndLine,
                FileSize = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime,
                Encoding = encoding.EncodingName
            };
        }

        private bool IsTextFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            // 检查已知的文本文件扩展名
            if (TextExtensions.Contains(extension))
                return true;
            
            // 检查没有扩展名的常见文本文件
            var fileName = Path.GetFileName(filePath).ToLowerInvariant();
            var textFileNames = new[] { "readme", "license", "changelog", "makefile", "dockerfile" };
            
            if (textFileNames.Any(name => fileName.StartsWith(name)))
                return true;
            
            // 对于未知扩展名，尝试读取前几个字节来判断
            try
            {
                var buffer = new byte[1024];
                using var stream = File.OpenRead(filePath);
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                
                // 检查是否包含过多的二进制字符
                var binaryCount = 0;
                for (int i = 0; i < bytesRead; i++)
                {
                    var b = buffer[i];
                    if (b == 0 || (b < 32 && b != 9 && b != 10 && b != 13))
                    {
                        binaryCount++;
                    }
                }
                
                // 如果二进制字符超过5%，认为是二进制文件
                return (double)binaryCount / bytesRead < 0.05;
            }
            catch
            {
                return false;
            }
        }

        private Encoding GetEncoding(string encodingName)
        {
            return encodingName.ToLowerInvariant() switch
            {
                "utf-8" or "utf8" => Encoding.UTF8,
                "utf-16" or "utf16" => Encoding.Unicode,
                "ascii" => Encoding.ASCII,
                "gb2312" or "gbk" => Encoding.GetEncoding("GB2312"),
                "big5" => Encoding.GetEncoding("Big5"),
                _ => Encoding.UTF8
            };
        }
    }

    /// <summary>
    /// 文件内容
    /// </summary>
    public class FileContent
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// 文件内容
        /// </summary>
        public string? Content { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; set; }
        
        /// <summary>
        /// 文件总行数
        /// </summary>
        public int TotalLines { get; set; }
        
        /// <summary>
        /// 显示的行数
        /// </summary>
        public int DisplayedLines { get; set; }
        
        /// <summary>
        /// 起始行号
        /// </summary>
        public int StartLine { get; set; }
        
        /// <summary>
        /// 结束行号
        /// </summary>
        public int EndLine { get; set; }
        
        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// 文件编码
        /// </summary>
        public string? Encoding { get; set; }
    }
}