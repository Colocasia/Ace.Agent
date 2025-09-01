using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;
using AceAgent.LLM;
using AceAgent.Tools;
using AceAgent.Tools.CKG;
using AceAgent.Tools.CKG.Data;
using AceAgent.Tools.CKG.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AceAgent.CLI.Services
{
    /// <summary>
    /// 代理服务，负责协调LLM、工具和轨迹记录
    /// </summary>
    public class AgentService
    {
        private readonly ILogger<AgentService> _logger;
        private readonly ConfigurationService _configService;
        private readonly TrajectoryService _trajectoryService;
        private readonly Dictionary<string, ITool> _tools;
        private ILLMProvider? _currentProvider;
        private ITrajectoryRecorder? _trajectoryRecorder;

        /// <summary>
        /// 初始化AgentService实例
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="configService">配置服务</param>
        /// <param name="trajectoryService">轨迹服务</param>
        public AgentService(
            ILogger<AgentService> logger,
            ConfigurationService configService,
            TrajectoryService trajectoryService)
        {
            _logger = logger;
            _configService = configService;
            _trajectoryService = trajectoryService;
            _tools = InitializeTools();
        }

        /// <summary>
        /// 启动交互式聊天模式
        /// </summary>
        public async Task StartChatAsync(string? model, string? provider, string? configPath, bool verbose)
        {
            try
            {
                await InitializeAsync(model, provider, configPath);
                
                Console.WriteLine("=== AceAgent 交互式聊天模式 ===");
                Console.WriteLine("输入 'exit' 或 'quit' 退出，输入 'help' 查看帮助");
                Console.WriteLine($"当前模型: {_currentProvider?.ProviderName} - {model ?? "默认"}");
                Console.WriteLine();

                var sessionId = Guid.NewGuid().ToString();
                var messages = new List<Message>();
                var trajectoryId = _trajectoryRecorder != null ? await _trajectoryRecorder.StartTrajectoryAsync(sessionId, new Dictionary<string, object> { ["description"] = "Interactive Chat" }) : string.Empty;

                while (true)
                {
                    Console.Write("用户: ");
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    if (input.ToLower() is "exit" or "quit")
                    {
                        Console.WriteLine("再见！");
                        break;
                    }

                    if (input.ToLower() == "help")
                    {
                        ShowHelp();
                        continue;
                    }

                    if (input.ToLower() == "clear")
                    {
                        messages.Clear();
                        Console.Clear();
                        Console.WriteLine("对话历史已清除");
                        continue;
                    }

                    messages.Add(Message.User(input));

                    try
                    {
                        var response = await ProcessMessageAsync(messages, verbose);
                        Console.WriteLine($"助手: {response.Content}");
                        
                        messages.Add(Message.Assistant(response.Content));

                        // 处理工具调用
                        if (response.ToolCalls?.Any() == true)
                        {
                            await ProcessToolCallsAsync(response.ToolCalls, messages, verbose);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"错误: {ex.Message}");
                        if (verbose)
                        {
                            Console.WriteLine($"详细信息: {ex}");
                        }
                    }

                    Console.WriteLine();
                }

                if (!string.IsNullOrEmpty(trajectoryId) && _trajectoryRecorder != null)
                {
                    await _trajectoryRecorder.CompleteTrajectoryAsync(trajectoryId, new TrajectoryResult
                    {
                        Success = true,
                        Message = "Chat session completed",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动聊天模式时发生错误");
                Console.WriteLine($"启动失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行单个任务
        /// </summary>
        public async Task ExecuteTaskAsync(string task, string? model, string? provider, string? configPath, string? outputPath, bool saveTrajectory)
        {
            try
            {
                await InitializeAsync(model, provider, configPath);
                
                Console.WriteLine($"执行任务: {task}");
                Console.WriteLine($"使用模型: {_currentProvider?.ProviderName} - {model ?? "默认"}");
                Console.WriteLine();

                var sessionId = Guid.NewGuid().ToString();
                var messages = new List<Message>
                {
                    Message.User(task)
                };

                string? trajectoryId = null;
                if (saveTrajectory)
                {
                    trajectoryId = _trajectoryRecorder != null ? await _trajectoryRecorder.StartTrajectoryAsync(sessionId, new Dictionary<string, object> { ["description"] = task }) : string.Empty;
                }

                try
                {
                    var response = await ProcessMessageAsync(messages, true);
                    Console.WriteLine($"结果: {response.Content}");

                    // 处理工具调用
                    if (response.ToolCalls?.Any() == true)
                    {
                        await ProcessToolCallsAsync(response.ToolCalls, messages, true);
                    }

                    // 保存输出到文件
                    if (!string.IsNullOrEmpty(outputPath))
                    {
                        await File.WriteAllTextAsync(outputPath, response.Content);
                        Console.WriteLine($"结果已保存到: {outputPath}");
                    }

                    if (!string.IsNullOrEmpty(trajectoryId) && _trajectoryRecorder != null)
                    {
                        await _trajectoryRecorder.CompleteTrajectoryAsync(trajectoryId, new TrajectoryResult
                        {
                            Success = true,
                            Message = "Task completed successfully",
                            Data = response.Content
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"执行失败: {ex.Message}");
                    
                    if (!string.IsNullOrEmpty(trajectoryId) && _trajectoryRecorder != null)
                    {
                        await _trajectoryRecorder.CompleteTrajectoryAsync(trajectoryId, new TrajectoryResult
                        {
                            Success = false,
                            Message = ex.Message,
                            Data = null,
                            Error = ex.ToString()
                        });
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行任务时发生错误");
                Console.WriteLine($"任务执行失败: {ex.Message}");
            }
        }

        private async Task InitializeAsync(string? model, string? provider, string? configPath)
        {
            // 加载配置
            if (!string.IsNullOrEmpty(configPath))
            {
                await _configService.LoadConfigAsync(configPath);
            }

            // 初始化LLM提供商
            provider ??= await _configService.GetConfigAsync("default_provider") ?? "openai";
            model ??= await _configService.GetConfigAsync($"{provider}_default_model");

            var apiKey = await _configService.GetConfigAsync($"{provider}_api_key");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException($"未找到 {provider} 的API密钥，请先配置");
            }

            var baseUrl = await _configService.GetConfigAsync($"{provider}_base_url");
            var config = new LLMProviderConfig
            {
                ApiKey = apiKey,
                BaseUrl = baseUrl
            };

            var factory = new LLMProviderFactory();
            _currentProvider = factory.CreateProvider(provider, config);
            
            // 验证配置
            if (!await _currentProvider.ValidateConfigurationAsync())
            {
                throw new InvalidOperationException($"LLM提供商 {provider} 配置验证失败");
            }

            // 初始化轨迹记录器
            _trajectoryRecorder = _trajectoryService.GetTrajectoryRecorder();

            _logger.LogInformation($"已初始化 {provider} 提供商，模型: {model}");
        }

        private async Task<ModelResponse> ProcessMessageAsync(List<Message> messages, bool verbose)
        {
            if (_currentProvider == null)
                throw new InvalidOperationException("LLM提供商未初始化");

            var options = new LLMOptions
            {
                Model = await _configService.GetConfigAsync("model"),
                Temperature = float.TryParse(await _configService.GetConfigAsync("temperature"), out var temp) ? temp : 0.7f,
                MaxTokens = int.TryParse(await _configService.GetConfigAsync("max_tokens"), out var maxTokens) ? maxTokens : 2000,
                Tools = GetAvailableTools()
            };

            if (verbose)
            {
                Console.WriteLine($"发送消息到 {_currentProvider.ProviderName}...");
            }

            var response = await _currentProvider.GenerateResponseAsync(messages, options);
            
            if (verbose && response.Usage != null)
            {
                Console.WriteLine($"Token使用: {response.Usage.PromptTokens} + {response.Usage.CompletionTokens} = {response.Usage.TotalTokens}");
            }

            return response;
        }

        private async Task ProcessToolCallsAsync(List<ToolCall> toolCalls, List<Message> messages, bool verbose)
        {
            foreach (var toolCall in toolCalls)
            {
                if (verbose)
                {
                    Console.WriteLine($"执行工具: {toolCall.Name}");
                }

                if (_tools.TryGetValue(toolCall.Name, out var tool))
                {
                    try
                    {
                        var input = new ToolInput
                        {
                            RawInput = toolCall.Arguments,
                            WorkingDirectory = Environment.CurrentDirectory
                        };

                        var result = await tool.ExecuteAsync(input);
                        
                        var resultMessage = result.Success 
                            ? $"工具执行成功: {result.Message}\n{result.Data}"
                            : $"工具执行失败: {result.Error}";

                        messages.Add(Message.Tool(resultMessage, toolCall.Id, toolCall.Name));
                        
                        if (verbose)
                        {
                            Console.WriteLine($"工具结果: {resultMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"工具执行异常: {ex.Message}";
                        messages.Add(Message.Tool(errorMessage, toolCall.Id, toolCall.Name));
                        
                        if (verbose)
                        {
                            Console.WriteLine(errorMessage);
                        }
                    }
                }
                else
                {
                    var errorMessage = $"未找到工具: {toolCall.Name}";
                    messages.Add(Message.Tool(errorMessage, toolCall.Id, toolCall.Name));
                    
                    if (verbose)
                    {
                        Console.WriteLine(errorMessage);
                    }
                }
            }
        }

        private Dictionary<string, ITool> InitializeTools()
        {
            // 创建CKG相关服务实例
            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
            var treeSitterLogger = loggerFactory.CreateLogger<TreeSitterService>();
            var ckgServiceLogger = loggerFactory.CreateLogger<CKGService>();
            var ckgToolLogger = loggerFactory.CreateLogger<CKGTool>();
            
            var treeSitterService = new TreeSitterService(treeSitterLogger);
            
            // 配置SQLite数据库
            var options = new DbContextOptionsBuilder<CKGDbContext>()
                .UseSqlite("Data Source=ckg.db")
                .Options;
            var dbContext = new CKGDbContext(options);
            
            var ckgService = new CKGService(treeSitterService, dbContext, ckgServiceLogger);
            
            return new Dictionary<string, ITool>
            {
                ["bash"] = new BashTool(),
                ["file_edit_tool"] = new FileEditTool(),
                ["sequentialthinking"] = new SequentialThinkingTool(),
                ["task_done"] = new TaskDoneTool(),
                ["web_search"] = new WebSearchTool(),
                ["list_dir"] = new ListDirTool(),
                ["view_files"] = new ViewFilesTool(),
                ["ckg"] = new CKGTool(ckgToolLogger, ckgService)
            };
        }

        private List<ToolDefinition> GetAvailableTools()
        {
            return _tools.Values.Select(tool => new ToolDefinition
            {
                Type = "function",
                Function = new FunctionDefinition
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    Parameters = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>(),
                        ["required"] = new string[0]
                    }
                }
            }).ToList();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("可用命令:");
            Console.WriteLine("  help  - 显示此帮助信息");
            Console.WriteLine("  clear - 清除对话历史");
            Console.WriteLine("  exit  - 退出程序");
            Console.WriteLine("  quit  - 退出程序");
            Console.WriteLine();
            Console.WriteLine("可用工具:");
            Console.WriteLine("  bash - 跨平台命令执行工具");
            Console.WriteLine("  file_edit_tool - 基于字符串替换的文件编辑工具");
            Console.WriteLine("  sequentialthinking - 结构化推理工具");
            Console.WriteLine("  task_done - 任务完成工具");
            Console.WriteLine("  web_search - 网络搜索工具");
            Console.WriteLine("  list_dir - 目录列表工具");
            Console.WriteLine("  view_files - 文件查看工具");
        }
    }
}