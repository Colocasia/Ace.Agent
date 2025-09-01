using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using AceAgent.CLI.Services;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AceAgent.Tools.CKG.Services;
using AceAgent.Tools.CKG.Data;
using AceAgent.Tools.CKG;
using Microsoft.EntityFrameworkCore;

namespace AceAgent.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                
                var rootCommand = new RootCommand("AceAgent - AI驱动的智能代理工具")
                {
                    CreateChatCommand(host),
                    CreateExecuteCommand(host),
                    CreateConfigCommand(host),
                    CreateTrajectoryCommand(host),
                    CreateCkgCommand(host)
                };

                var parser = new CommandLineBuilder(rootCommand)
                    .UseDefaults()
                    .Build();

                return await parser.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"发生错误: {ex.Message}");
                return 1;
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ConfigurationService>();
                    services.AddSingleton<TrajectoryService>();
                    services.AddSingleton<AgentService>();
                    
                    // CKG服务注册
                    services.AddDbContext<CKGDbContext>(options =>
                        options.UseSqlite("Data Source=ckg.db"));
                    services.AddSingleton<TreeSitterService>();
                    services.AddScoped<CKGService>();
                    
                    services.AddLogging(builder => builder.AddConsole());
                });

        private static Command CreateChatCommand(IHost host)
        {
            var chatCommand = new Command("chat", "启动交互式聊天模式")
            {
                new Option<string>("--model", "指定使用的模型"),
                new Option<string>("--provider", "指定LLM提供商 (openai, anthropic, doubao)"),
                new Option<string>("--config", "指定配置文件路径"),
                new Option<bool>("--verbose", "启用详细输出")
            };

            var modelOption = chatCommand.Options.OfType<Option<string>>().First(o => o.Name == "model");
            var providerOption = chatCommand.Options.OfType<Option<string>>().First(o => o.Name == "provider");
            var configOption = chatCommand.Options.OfType<Option<string>>().First(o => o.Name == "config");
            var verboseOption = chatCommand.Options.OfType<Option<bool>>().First(o => o.Name == "verbose");
            
            chatCommand.SetHandler(async (string model, string provider, string config, bool verbose) =>
            {
                var agentService = host.Services.GetRequiredService<AgentService>();
                await agentService.StartChatAsync(model, provider, config, verbose);
            }, modelOption, providerOption, configOption, verboseOption);

            return chatCommand;
        }

        private static Command CreateExecuteCommand(IHost host)
        {
            var executeCommand = new Command("execute", "执行单个任务")
            {
                new Argument<string>("task", "要执行的任务描述"),
                new Option<string>("--model", "指定使用的模型"),
                new Option<string>("--provider", "指定LLM提供商"),
                new Option<string>("--config", "指定配置文件路径"),
                new Option<string>("--output", "输出文件路径"),
                new Option<bool>("--save-trajectory", "保存执行轨迹")
            };

            var taskArg = executeCommand.Arguments.OfType<Argument<string>>().First();
            var modelOpt = executeCommand.Options.OfType<Option<string>>().First(o => o.Name == "model");
            var providerOpt = executeCommand.Options.OfType<Option<string>>().First(o => o.Name == "provider");
            var configOpt = executeCommand.Options.OfType<Option<string>>().First(o => o.Name == "config");
            var outputOpt = executeCommand.Options.OfType<Option<string>>().First(o => o.Name == "output");
            var saveTrajectoryOpt = executeCommand.Options.OfType<Option<bool>>().First(o => o.Name == "save-trajectory");
            
            executeCommand.SetHandler(async (string task, string model, string provider, string config, string output, bool saveTrajectory) =>
            {
                var agentService = host.Services.GetRequiredService<AgentService>();
                await agentService.ExecuteTaskAsync(task, model, provider, config, output, saveTrajectory);
            }, taskArg, modelOpt, providerOpt, configOpt, outputOpt, saveTrajectoryOpt);

            return executeCommand;
        }

        private static Command CreateConfigCommand(IHost host)
        {
            var configCommand = new Command("config", "配置管理")
            {
                CreateConfigSetCommand(host),
                CreateConfigGetCommand(host),
                CreateConfigListCommand(host),
                CreateConfigRemoveCommand(host),
                CreateConfigResetCommand(host),
                CreateConfigValidateCommand(host),
                CreateConfigExportCommand(host),
                CreateConfigImportCommand(host),
                CreateConfigInitCommand(host)
            };

            return configCommand;
        }

        private static Command CreateConfigSetCommand(IHost host)
        {
            var setCommand = new Command("set", "设置配置项")
            {
                new Argument<string>("key", "配置键"),
                new Argument<string>("value", "配置值")
            };

            var keyArg = setCommand.Arguments.OfType<Argument<string>>().First();
            var valueArg = setCommand.Arguments.OfType<Argument<string>>().Last();
            
            setCommand.SetHandler(async (string key, string value) =>
            {
                var configService = host.Services.GetRequiredService<ConfigurationService>();
                await configService.SetConfigAsync(key, value);
                Console.WriteLine($"配置已设置: {key} = {value}");
            }, keyArg, valueArg);

            return setCommand;
        }

        private static Command CreateConfigGetCommand(IHost host)
        {
            var getCommand = new Command("get", "获取配置项")
            {
                new Argument<string>("key", "配置键")
            };

            var getKeyArg = getCommand.Arguments.OfType<Argument<string>>().First();
            
            getCommand.SetHandler(async (string key) =>
            {
                var configService = host.Services.GetRequiredService<ConfigurationService>();
                var value = await configService.GetConfigAsync(key);
                Console.WriteLine($"{key}: {value ?? "未设置"}");
            }, getKeyArg);

            return getCommand;
        }

        private static Command CreateConfigListCommand(IHost host)
        {
            var listCommand = new Command("list", "列出所有配置项");

            listCommand.SetHandler(async () =>
            {
                var configService = host.Services.GetRequiredService<ConfigurationService>();
                var configs = await configService.GetAllConfigsAsync();
                
                Console.WriteLine("当前配置:");
                foreach (var config in configs)
                {
                    Console.WriteLine($"  {config.Key}: {config.Value}");
                }
            });

            return listCommand;
        }

        private static Command CreateConfigRemoveCommand(IHost host)
        {
            var removeCommand = new Command("remove", "删除配置项")
            {
                new Argument<string>("key", "配置键")
            };

            var keyArg = removeCommand.Arguments.OfType<Argument<string>>().First();
            
            removeCommand.SetHandler(async (string key) =>
            {
                var configService = host.Services.GetRequiredService<ConfigurationService>();
                await configService.RemoveConfigAsync(key);
                Console.WriteLine($"配置已删除: {key}");
            }, keyArg);

            return removeCommand;
        }

        private static Command CreateConfigResetCommand(IHost host)
        {
            var resetCommand = new Command("reset", "重置配置到默认值");

            resetCommand.SetHandler(async () =>
            {
                var configService = host.Services.GetRequiredService<ConfigurationService>();
                await configService.ResetConfigAsync();
                Console.WriteLine("配置已重置为默认值");
            });

            return resetCommand;
        }

        private static Command CreateConfigValidateCommand(IHost host)
        {
            var validateCommand = new Command("validate", "验证配置");

            validateCommand.SetHandler(async () =>
            {
                var configService = host.Services.GetRequiredService<ConfigurationService>();
                var missingConfigs = await configService.ValidateRequiredConfigsAsync();
                
                if (missingConfigs.Count == 0)
                {
                    Console.WriteLine("✓ 配置验证通过");
                }
                else
                {
                    Console.WriteLine("✗ 配置验证失败，缺少以下配置:");
                    foreach (var config in missingConfigs)
                    {
                        Console.WriteLine($"  - {config}");
                    }
                }
            });

            return validateCommand;
        }

        private static Command CreateConfigExportCommand(IHost host)
        {
            var exportCommand = new Command("export", "导出配置到文件")
            {
                new Argument<string>("path", "导出文件路径")
            };

            var pathArg = exportCommand.Arguments.OfType<Argument<string>>().First();
            
            exportCommand.SetHandler(async (string path) =>
            {
                var configService = host.Services.GetRequiredService<ConfigurationService>();
                await configService.ExportConfigAsync(path);
                Console.WriteLine($"配置已导出到: {path}");
            }, pathArg);

            return exportCommand;
        }

        private static Command CreateConfigImportCommand(IHost host)
        {
            var importCommand = new Command("import", "从文件导入配置")
            {
                new Argument<string>("path", "导入文件路径")
            };

            var pathArg = importCommand.Arguments.OfType<Argument<string>>().First();
            
            importCommand.SetHandler(async (string path) =>
            {
                var configService = host.Services.GetRequiredService<ConfigurationService>();
                await configService.ImportConfigAsync(path);
                Console.WriteLine($"配置已从 {path} 导入");
            }, pathArg);

            return importCommand;
        }

        private static Command CreateConfigInitCommand(IHost host)
        {
            var initCommand = new Command("init", "初始化默认配置");

            initCommand.SetHandler(async () =>
            {
                var configService = host.Services.GetRequiredService<ConfigurationService>();
                await configService.InitializeDefaultConfigAsync();
                Console.WriteLine($"默认配置已初始化到: {configService.GetConfigPath()}");
            });

            return initCommand;
        }

        private static Command CreateTrajectoryCommand(IHost host)
        {
            var trajectoryCommand = new Command("trajectory", "轨迹管理")
            {
                CreateTrajectoryListCommand(host),
                CreateTrajectoryShowCommand(host),
                CreateTrajectoryDeleteCommand(host)
            };

            return trajectoryCommand;
        }

        private static Command CreateTrajectoryListCommand(IHost host)
        {
            var listCommand = new Command("list", "列出执行轨迹")
            {
                new Option<int>("--limit", () => 10, "限制结果数量"),
                new Option<string>("--status", "按状态过滤")
            };

            var limitOption = listCommand.Options.OfType<Option<int>>().First(o => o.Name == "limit");
            var statusOption = listCommand.Options.OfType<Option<string>>().First(o => o.Name == "status");
            
            listCommand.SetHandler(async (int limit, string status) =>
            {
                var trajectoryService = host.Services.GetRequiredService<TrajectoryService>();
                await trajectoryService.ListTrajectoriesAsync(limit, status);
            }, limitOption, statusOption);

            return listCommand;
        }

        private static Command CreateTrajectoryShowCommand(IHost host)
        {
            var showCommand = new Command("show", "显示轨迹详情")
            {
                new Argument<string>("id", "轨迹ID")
            };

            var showIdArg = showCommand.Arguments.OfType<Argument<string>>().First();
            
            showCommand.SetHandler(async (string id) =>
            {
                var trajectoryService = host.Services.GetRequiredService<TrajectoryService>();
                await trajectoryService.ShowTrajectoryAsync(id);
            }, showIdArg);

            return showCommand;
        }

        private static Command CreateTrajectoryDeleteCommand(IHost host)
        {
            var deleteCommand = new Command("delete", "删除轨迹")
            {
                new Argument<string>("id", "轨迹ID")
            };

            var deleteIdArg = deleteCommand.Arguments.OfType<Argument<string>>().First();
            
            deleteCommand.SetHandler(async (string id) =>
            {
                var trajectoryService = host.Services.GetRequiredService<TrajectoryService>();
                await trajectoryService.DeleteTrajectoryAsync(id);
            }, deleteIdArg);

            return deleteCommand;
        }

        private static Command CreateCkgCommand(IHost host)
        {
            var ckgCommand = new Command("ckg", "代码知识图谱工具")
            {
                CreateCkgAnalyzeCommand(host),
                CreateCkgQueryCommand(host),
                CreateCkgExportCommand(host),
                CreateCkgImportCommand(host)
            };

            return ckgCommand;
        }

        private static Command CreateCkgAnalyzeCommand(IHost host)
        {
            var analyzeCommand = new Command("analyze", "分析代码库并构建知识图谱")
            {
                new Argument<string>("path", "要分析的代码库路径"),
                new Option<string[]>("--languages", "指定要分析的编程语言"),
                new Option<string>("--output", "输出文件路径"),
                new Option<bool>("--verbose", "启用详细输出")
            };

            var pathArg = analyzeCommand.Arguments.OfType<Argument<string>>().First();
            var languagesOpt = analyzeCommand.Options.OfType<Option<string[]>>().First(o => o.Name == "languages");
            var outputOpt = analyzeCommand.Options.OfType<Option<string>>().First(o => o.Name == "output");
            var verboseOpt = analyzeCommand.Options.OfType<Option<bool>>().First(o => o.Name == "verbose");

            analyzeCommand.SetHandler(async (string path, string[] languages, string output, bool verbose) =>
            {
                using var scope = host.Services.CreateScope();
                var codeParsingService = scope.ServiceProvider.GetRequiredService<CKGService>();
                
                // 检查路径是文件还是目录
                if (File.Exists(path))
                {
                    Console.WriteLine($"分析单个文件: {path}");
                    await codeParsingService.AnalyzeFileAndSaveAsync(path);
                    Console.WriteLine("单文件分析完成");
                }
                else if (Directory.Exists(path))
                {
                    Console.WriteLine($"分析目录: {path}");
                    await codeParsingService.AnalyzeRepositoryAsync(path, languages, output, verbose);
                }
                else
                {
                    Console.WriteLine($"错误: 路径不存在: {path}");
                }
            }, pathArg, languagesOpt, outputOpt, verboseOpt);

            return analyzeCommand;
        }

        private static Command CreateCkgQueryCommand(IHost host)
        {
            var queryCommand = new Command("query", "查询代码知识图谱")
            {
                new Argument<string>("query", "查询语句"),
                new Option<string>("--database", "数据库文件路径"),
                new Option<string>("--format", () => "table", "输出格式 (table, json, csv)")
            };

            var queryArg = queryCommand.Arguments.OfType<Argument<string>>().First();
            var databaseOpt = queryCommand.Options.OfType<Option<string>>().First(o => o.Name == "database");
            var formatOpt = queryCommand.Options.OfType<Option<string>>().First(o => o.Name == "format");

            queryCommand.SetHandler(async (string query, string database, string format) =>
            {
                using var scope = host.Services.CreateScope();
                var codeParsingService = scope.ServiceProvider.GetRequiredService<CKGService>();
                    await codeParsingService.QueryKnowledgeGraphAsync(query, database, format);
            }, queryArg, databaseOpt, formatOpt);

            return queryCommand;
        }

        private static Command CreateCkgExportCommand(IHost host)
        {
            var exportCommand = new Command("export", "导出知识图谱数据")
            {
                new Argument<string>("output", "输出文件路径"),
                new Option<string>("--database", "数据库文件路径"),
                new Option<string>("--format", () => "json", "导出格式 (json, csv, graphml)")
            };

            var outputArg = exportCommand.Arguments.OfType<Argument<string>>().First();
            var databaseOpt = exportCommand.Options.OfType<Option<string>>().First(o => o.Name == "database");
            var formatOpt = exportCommand.Options.OfType<Option<string>>().First(o => o.Name == "format");

            exportCommand.SetHandler(async (string output, string database, string format) =>
            {
                using var scope = host.Services.CreateScope();
                var codeParsingService = scope.ServiceProvider.GetRequiredService<CKGService>();
                    await codeParsingService.ExportKnowledgeGraphAsync(output, database, format);
            }, outputArg, databaseOpt, formatOpt);

            return exportCommand;
        }

        private static Command CreateCkgImportCommand(IHost host)
        {
            var importCommand = new Command("import", "导入知识图谱数据")
            {
                new Argument<string>("input", "输入文件路径"),
                new Option<string>("--database", "数据库文件路径"),
                new Option<bool>("--merge", "合并到现有数据")
            };

            var inputArg = importCommand.Arguments.OfType<Argument<string>>().First();
            var databaseOpt = importCommand.Options.OfType<Option<string>>().First(o => o.Name == "database");
            var mergeOpt = importCommand.Options.OfType<Option<bool>>().First(o => o.Name == "merge");

            importCommand.SetHandler(async (string input, string database, bool merge) =>
            {
                using var scope = host.Services.CreateScope();
                var codeParsingService = scope.ServiceProvider.GetRequiredService<CKGService>();
                    await codeParsingService.ImportKnowledgeGraphAsync(input, database, merge);
            }, inputArg, databaseOpt, mergeOpt);

            return importCommand;
        }
    }
}
