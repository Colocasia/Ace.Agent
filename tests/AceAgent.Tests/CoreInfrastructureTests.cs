using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AceAgent.CLI.Services;
using AceAgent.Core.Models;
using System.IO;
using System.Threading.Tasks;

namespace AceAgent.Tests
{
    /// <summary>
    /// 核心基础设施测试
    /// </summary>
    public class CoreInfrastructureTests
    {
        /// <summary>
        /// 测试依赖注入容器配置
        /// </summary>
        [Fact]
        public void DependencyInjection_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddLogging();
            services.AddSingleton<ConfigurationService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Assert
            var logger = serviceProvider.GetService<ILogger<CoreInfrastructureTests>>();
            var configService = serviceProvider.GetService<ConfigurationService>();
            
            logger.Should().NotBeNull();
            configService.Should().NotBeNull();
        }
        
        /// <summary>
        /// 测试日志记录功能
        /// </summary>
        [Fact]
        public void Logging_ShouldCreateLogger()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Act
            var logger = serviceProvider.GetRequiredService<ILogger<CoreInfrastructureTests>>();
            
            // Assert
            logger.Should().NotBeNull();
            logger.IsEnabled(LogLevel.Information).Should().BeTrue();
        }
        
        /// <summary>
        /// 测试配置服务初始化
        /// </summary>
        [Fact]
        public async Task ConfigurationService_ShouldInitialize()
        {
            // Arrange
            var tempConfigPath = Path.GetTempFileName();
            var services = new ServiceCollection();
            services.AddLogging();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConfigPath"] = tempConfigPath
                })
                .Build();
                
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ConfigurationService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            try
            {
                // Act
                var configService = serviceProvider.GetRequiredService<ConfigurationService>();
                
                // Assert
                configService.Should().NotBeNull();
                
                // Test basic configuration operations
                await configService.SetConfigAsync("test_key", "test_value");
                var value = await configService.GetConfigAsync("test_key");
                value.Should().Be("test_value");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempConfigPath))
                {
                    File.Delete(tempConfigPath);
                }
            }
        }
        
        /// <summary>
        /// 测试配置验证
        /// </summary>
        [Theory]
        [InlineData("openai", "gpt-4", "test-key-1")]
        [InlineData("anthropic", "claude-3-sonnet-20240229", "test-key-2")]
        [InlineData("doubao", "doubao-pro", "test-key-3")]
        public async Task Configuration_ShouldValidateProviderSettings(string provider, string model, string apiKey)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ConfigurationService>>();
            var testConfigFile = $"test_config_{Guid.NewGuid()}.yaml";
            
            try
            {
                // Act - Create a new ConfigurationService instance for each test
                var configService = new ConfigurationService(mockLogger.Object);
                
                // Clear any existing configuration first
                await configService.SetConfigAsync("default_provider", provider);
                await configService.SetConfigAsync($"{provider}_default_model", model);
                await configService.SetConfigAsync($"{provider}_api_key", apiKey);
                
                // Wait a bit to ensure the configuration is saved
                await Task.Delay(10);
                
                // Create a fresh instance to read the configuration
                var freshConfigService = new ConfigurationService(mockLogger.Object);
                
                // Assert
                var savedProvider = await freshConfigService.GetConfigAsync("default_provider");
                var savedModel = await freshConfigService.GetConfigAsync($"{provider}_default_model");
                var savedApiKey = await freshConfigService.GetConfigAsync($"{provider}_api_key");
                
                savedProvider.Should().Be(provider);
                savedModel.Should().Be(model);
                savedApiKey.Should().Be(apiKey);
            }
            finally
            {
                // Cleanup - Reset to default state
                var cleanupService = new ConfigurationService(mockLogger.Object);
                await cleanupService.SetConfigAsync("default_provider", "openai");
                
                // Clean up test config file if it exists
                if (File.Exists(testConfigFile))
                {
                    File.Delete(testConfigFile);
                }
            }
        }
        
        /// <summary>
        /// 测试LLM选项验证
        /// </summary>
        [Fact]
        public void LLMOptions_ShouldValidateParameters()
        {
            // Arrange & Act
            var options = new LLMOptions
            {
                Model = "gpt-4",
                Temperature = 0.7,
                MaxTokens = 2000,
                TopP = 0.9,
                FrequencyPenalty = 0.1,
                PresencePenalty = 0.1
            };
            
            // Assert
            options.Should().NotBeNull();
            options.Model.Should().Be("gpt-4");
            options.Temperature.Should().Be(0.7);
            options.MaxTokens.Should().Be(2000);
            options.TopP.Should().Be(0.9);
            options.FrequencyPenalty.Should().Be(0.1);
            options.PresencePenalty.Should().Be(0.1);
        }
        
        /// <summary>
        /// 测试工具定义创建
        /// </summary>
        [Fact]
        public void ToolDefinition_ShouldCreateCorrectly()
        {
            // Arrange & Act
            var toolDef = new ToolDefinition
            {
                Type = "function",
                Function = new FunctionDefinition
                {
                    Name = "test_function",
                    Description = "A test function",
                    Parameters = new { type = "object", properties = new { } }
                }
            };
            
            // Assert
            toolDef.Should().NotBeNull();
            toolDef.Type.Should().Be("function");
            toolDef.Function.Should().NotBeNull();
            toolDef.Function!.Name.Should().Be("test_function");
            toolDef.Function.Description.Should().Be("A test function");
            toolDef.Function.Parameters.Should().NotBeNull();
        }
    }
}