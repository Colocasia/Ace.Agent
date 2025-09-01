using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;

namespace AceAgent.Tools
{
    /// <summary>
    /// Bash工具 - 安全的跨平台命令执行
    /// </summary>
    public class BashTool : ITool
    {
        private readonly HashSet<string> _allowedCommands;
        private readonly HashSet<string> _blockedCommands;
        private readonly int _timeoutSeconds;

        /// <summary>
        /// 工具名称
        /// </summary>
        public string Name => "bash";
        
        /// <summary>
        /// 工具描述
        /// </summary>
        public string Description => "安全的跨平台命令执行工具，支持命令白名单和黑名单";

        /// <summary>
        /// 构造函数
        /// </summary>
        public BashTool()
        {
            _timeoutSeconds = 300; // 默认5分钟超时
            
            // 默认允许的安全命令
            _allowedCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ls", "dir", "pwd", "cd", "cat", "type", "echo", "find", "grep",
                "git", "npm", "yarn", "dotnet", "python", "pip", "node",
                "curl", "wget", "ping", "nslookup", "which", "where",
                "mkdir", "rmdir", "cp", "copy", "mv", "move", "touch",
                "head", "tail", "wc", "sort", "uniq", "diff"
            };

            // 危险命令黑名单
            _blockedCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "rm", "del", "format", "fdisk", "mkfs", "dd",
                "shutdown", "reboot", "halt", "poweroff",
                "su", "sudo", "passwd", "chown", "chmod",
                "kill", "killall", "pkill", "taskkill"
            };
        }

        /// <summary>
        /// 执行Bash命令工具
        /// </summary>
        /// <param name="input">工具输入</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>工具执行结果</returns>
        public async Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var command = input.GetParameter<string>("command");
                var workingDirectory = input.GetParameter<string>("working_directory") ?? Environment.CurrentDirectory;
                var timeoutSeconds = input.GetParameter<int?>("timeout_seconds") ?? _timeoutSeconds;
                var captureOutput = input.GetParameter<bool?>("capture_output") ?? true;
                var allowUnsafe = input.GetParameter<bool?>("allow_unsafe") ?? false;

                if (string.IsNullOrWhiteSpace(command))
                    return ToolResult.Failure("命令不能为空");

                // 安全检查
                if (!allowUnsafe && !IsCommandSafe(command))
                    return ToolResult.Failure($"命令被安全策略阻止: {command}");

                // 验证工作目录
                if (!Directory.Exists(workingDirectory))
                    return ToolResult.Failure($"工作目录不存在: {workingDirectory}");

                // 解析命令和参数
                var (fileName, arguments) = ParseCommand(command);

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = captureOutput,
                    RedirectStandardError = captureOutput,
                    RedirectStandardInput = false,
                    CreateNoWindow = true
                };

                var output = new StringBuilder();
                var error = new StringBuilder();
                int exitCode;

                using var process = new Process { StartInfo = processStartInfo };
                
                if (captureOutput)
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            output.AppendLine(e.Data);
                    };
                    
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            error.AppendLine(e.Data);
                    };
                }

                process.Start();
                
                if (captureOutput)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                // 等待进程完成或超时
                var completed = await WaitForExitAsync(process, TimeSpan.FromSeconds(timeoutSeconds), cancellationToken);
                
                if (!completed)
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch { }
                    return ToolResult.Failure($"命令执行超时 ({timeoutSeconds}秒)");
                }

                exitCode = process.ExitCode;
                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var result = new ToolResult
                {
                    Success = exitCode == 0,
                    Message = exitCode == 0 ? "命令执行成功" : $"命令执行失败，退出码: {exitCode}",
                    Data = new
                    {
                        Command = command,
                        ExitCode = exitCode,
                        Output = output.ToString(),
                        Error = error.ToString(),
                        WorkingDirectory = workingDirectory
                    },
                    ExecutionTimeMs = (long)executionTime
                };

                result.Metadata["operation"] = "command_execution";
                result.Metadata["command"] = command;
                result.Metadata["exit_code"] = exitCode;
                
                if (exitCode != 0)
                {
                    result.Error = error.ToString();
                }

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
        /// <param name="input">工具输入</param>
        /// <returns>验证结果</returns>
        public async Task<bool> ValidateInputAsync(ToolInput input)
        {
            await Task.CompletedTask;
            
            var command = input.GetParameter<string>("command");
            var workingDirectory = input.GetParameter<string>("working_directory") ?? Environment.CurrentDirectory;

            if (string.IsNullOrWhiteSpace(command))
                return false;

            if (!Directory.Exists(workingDirectory))
                return false;

            return true;
        }

        /// <summary>
        /// 检查命令是否安全
        /// </summary>
        /// <param name="command">要检查的命令</param>
        /// <returns>是否安全</returns>
        private bool IsCommandSafe(string command)
        {
            var commandParts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (commandParts.Length == 0)
                return false;

            var baseCommand = Path.GetFileNameWithoutExtension(commandParts[0]);
            
            // 检查黑名单
            if (_blockedCommands.Contains(baseCommand))
                return false;

            // 检查白名单（如果配置了白名单）
            if (_allowedCommands.Count > 0 && !_allowedCommands.Contains(baseCommand))
                return false;

            // 检查危险模式
            if (command.Contains("rm -rf") || command.Contains("del /s") || command.Contains("format"))
                return false;

            return true;
        }

        /// <summary>
        /// 解析命令为文件名和参数
        /// </summary>
        /// <param name="command">要解析的命令</param>
        /// <returns>文件名和参数</returns>
        private static (string fileName, string arguments) ParseCommand(string command)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ("cmd.exe", $"/c {command}");
            }
            else
            {
                return ("/bin/bash", $"-c \"{command.Replace("\"", "\\\"")}\"");
            }
        }

        /// <summary>
        /// 异步等待进程退出
        /// </summary>
        /// <param name="process">进程</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否在超时前完成</returns>
        private static async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using var timeoutCts = new CancellationTokenSource(timeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            try
            {
                await process.WaitForExitAsync(combinedCts.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 命令执行工具输入参数
    /// </summary>
    public class CommandExecutorInput
    {
        /// <summary>
        /// 要执行的命令
        /// </summary>
        public string Command { get; set; } = string.Empty;
        
        /// <summary>
        /// 工作目录
        /// </summary>
        public string? WorkingDirectory { get; set; }
        
        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;
        
        /// <summary>
        /// 是否捕获输出
        /// </summary>
        public bool CaptureOutput { get; set; } = true;
        
        /// <summary>
        /// 是否允许不安全命令
        /// </summary>
        public bool AllowUnsafe { get; set; } = false;
    }
}