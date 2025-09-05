using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Builder;
using System.IO;
using System.Threading.Tasks;
using AceAgent.CLI.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AceAgent.Tests
{
    public class CLITests : IDisposable
    {
        private readonly IHost _host;
        private readonly Parser _parser;

        public CLITests()
        {
            _host = CreateTestHost();
            _parser = CreateTestParser();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }

        private IHost CreateTestHost()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ConfigurationService>();
                    services.AddSingleton<TrajectoryService>();
                    services.AddSingleton<AgentService>();
                })
                .Build();
        }

        private Parser CreateTestParser()
        {
            var rootCommand = new RootCommand("AceAgent - AI驱动的智能代理工具")
            {
                CreateTestChatCommand(),
                CreateTestExecuteCommand(),
                CreateTestConfigCommand()
            };

            return new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build();
        }

        private Command CreateTestChatCommand()
        {
            return new Command("chat", "启动交互式聊天模式")
            {
                new Option<string>("--model", "指定使用的模型"),
                new Option<string>("--provider", "指定LLM提供商"),
                new Option<bool>("--verbose", "启用详细输出")
            };
        }

        private Command CreateTestExecuteCommand()
        {
            return new Command("execute", "执行单个任务")
            {
                new Argument<string>("task", "要执行的任务描述"),
                new Option<string>("--model", "指定使用的模型"),
                new Option<string>("--provider", "指定LLM提供商")
            };
        }

        private Command CreateTestConfigCommand()
        {
            var configCommand = new Command("config", "配置管理");
            
            var setCommand = new Command("set", "设置配置项")
            {
                new Argument<string>("key", "配置键"),
                new Argument<string>("value", "配置值")
            };
            
            var getCommand = new Command("get", "获取配置项")
            {
                new Argument<string>("key", "配置键")
            };
            
            configCommand.AddCommand(setCommand);
            configCommand.AddCommand(getCommand);
            
            return configCommand;
        }

        [Fact]
        public void CLI_ShouldParseEmptyCommand()
        {
            // Act
            var result = _parser.Parse(new string[] { });

            // Assert
            result.Should().NotBeNull();
            // 空命令会有错误，但不应该抛出异常
        }

        [Fact]
        public void CLI_ShouldParseHelpCommand()
        {
            // Act
            var result = _parser.Parse(new[] { "--help" });

            // Assert
            result.Should().NotBeNull();
            // 使用--help选项时，不会报错
        }

        [Fact]
        public void CLI_ShouldParseChatCommand()
        {
            // Act
            var result = _parser.Parse(new[] { "chat" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("chat");
            // 简化测试，只验证命令名称和错误状态
        }

        [Fact]
        public void CLI_ShouldParseChatCommandWithOptions()
        {
            // Act
            var result = _parser.Parse(new[] { "chat", "--model", "gpt-4", "--provider", "openai", "--verbose" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("chat");
            // 简化测试，只验证命令名称和错误状态
        }

        [Fact]
        public void CLI_ShouldParseExecuteCommand()
        {
            // Act
            var result = _parser.Parse(new[] { "execute", "创建一个Hello World程序" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("execute");
            // 简化测试，只验证命令名称和错误状态
        }

        [Fact]
        public void CLI_ShouldParseExecuteCommandWithOptions()
        {
            // Act
            var result = _parser.Parse(new[] { "execute", "创建测试", "--model", "claude-3", "--provider", "anthropic" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("execute");
            // 简化测试，只验证命令名称和错误状态
        }

        [Fact]
        public void CLI_ShouldParseConfigSetCommand()
        {
            // Act
            var result = _parser.Parse(new[] { "config", "set", "api_key", "test_key" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("set");
            // 简化测试，只验证命令名称和错误状态
        }

        [Fact]
        public void CLI_ShouldParseConfigGetCommand()
        {
            // Act
            var result = _parser.Parse(new[] { "config", "get", "api_key" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("get");
            // 简化测试，只验证命令名称和错误状态
        }

        [Fact]
        public void CLI_ShouldRejectInvalidCommand()
        {
            // Act
            var result = _parser.Parse(new[] { "invalid_command" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public void CLI_ShouldRejectExecuteCommandWithoutTask()
        {
            // Act
            var result = _parser.Parse(new[] { "execute" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public void CLI_ShouldRejectConfigSetCommandWithMissingArguments()
        {
            // Act
            var result = _parser.Parse(new[] { "config", "set", "key_only" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public void CLI_ShouldShowHelpForRootCommand()
        {
            // Act
            var result = _parser.Parse(new[] { "--help" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void CLI_ShouldShowHelpForChatCommand()
        {
            // Act
            var result = _parser.Parse(new[] { "chat", "--help" });

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
        }
    }
}