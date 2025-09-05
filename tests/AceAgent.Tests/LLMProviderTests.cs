using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;
using AceAgent.LLM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AceAgent.Tests
{
    /// <summary>
    /// LLM提供商集成测试
    /// </summary>
    public class LLMProviderTests
    {
        private readonly Mock<ILogger<LLMProviderTests>> _mockLogger;

        public LLMProviderTests()
        {
            _mockLogger = new Mock<ILogger<LLMProviderTests>>();
        }

        [Fact]
        public void LLMProviderFactory_ShouldCreateOpenAIProvider()
        {
            // Arrange
            var factory = new LLMProviderFactory();
            var config = new LLMProviderConfig
            {
                ApiKey = "test-api-key",
                BaseUrl = "https://api.openai.com/v1",
                DefaultModel = "gpt-4"
            };

            // Act
            var provider = factory.CreateProvider("openai", config);

            // Assert
            provider.Should().NotBeNull();
            provider.Should().BeOfType<OpenAIProvider>();
            provider.ProviderName.Should().Be("OpenAI");
        }

        [Fact]
        public void LLMProviderFactory_ShouldCreateAnthropicProvider()
        {
            // Arrange
            var factory = new LLMProviderFactory();
            var config = new LLMProviderConfig
            {
                ApiKey = "test-api-key",
                BaseUrl = "https://api.anthropic.com",
                DefaultModel = "claude-3-sonnet-20240229"
            };

            // Act
            var provider = factory.CreateProvider("anthropic", config);

            // Assert
            provider.Should().NotBeNull();
            provider.Should().BeOfType<AnthropicProvider>();
            provider.ProviderName.Should().Be("Anthropic");
        }

        [Fact]
        public void LLMProviderFactory_ShouldCreateDoubaoProvider()
        {
            // Arrange
            var factory = new LLMProviderFactory();
            var config = new LLMProviderConfig
            {
                ApiKey = "test-api-key",
                BaseUrl = "https://ark.cn-beijing.volces.com/api/v3",
                DefaultModel = "doubao-seed-1.6"
            };

            // Act
            var provider = factory.CreateProvider("doubao", config);

            // Assert
            provider.Should().NotBeNull();
            provider.Should().BeOfType<DoubaoProvider>();
            provider.ProviderName.Should().Be("Doubao");
        }

        [Fact]
        public void LLMProviderFactory_ShouldThrowForUnsupportedProvider()
        {
            // Arrange
            var factory = new LLMProviderFactory();
            var config = new LLMProviderConfig { ApiKey = "test-key" };

            // Act & Assert
            Action act = () => factory.CreateProvider("unsupported", config);
            act.Should().Throw<NotSupportedException>()
                .WithMessage("*不支持的LLM提供商: unsupported*");
        }

        [Fact]
        public void LLMProviderFactory_ShouldThrowForNullConfig()
        {
            // Arrange
            var factory = new LLMProviderFactory();

            // Act & Assert
            Action act = () => factory.CreateProvider("openai", null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LLMProviderFactory_ShouldThrowForEmptyProviderName()
        {
            // Arrange
            var factory = new LLMProviderFactory();
            var config = new LLMProviderConfig { ApiKey = "test-key" };

            // Act & Assert
            Action act = () => factory.CreateProvider("", config);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void LLMProviderFactory_ShouldReturnSupportedProviders()
        {
            // Arrange
            var factory = new LLMProviderFactory();

            // Act
            var supportedProviders = factory.GetSupportedProviders().ToList();

            // Assert
            supportedProviders.Should().NotBeEmpty();
            supportedProviders.Should().Contain("openai");
            supportedProviders.Should().Contain("anthropic");
            supportedProviders.Should().Contain("doubao");
        }

        [Theory]
        [InlineData("openai", true)]
        [InlineData("anthropic", true)]
        [InlineData("doubao", true)]
        [InlineData("unsupported", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void LLMProviderFactory_ShouldCheckProviderSupport(string providerName, bool expectedSupported)
        {
            // Arrange
            var factory = new LLMProviderFactory();

            // Act
            var isSupported = factory.IsProviderSupported(providerName);

            // Assert
            isSupported.Should().Be(expectedSupported);
        }

        [Fact]
        public void LLMProviderFactory_ShouldRegisterCustomProvider()
        {
            // Arrange
            var factory = new LLMProviderFactory();
            var customProviderName = "custom";
            
            // Act
            factory.RegisterProvider(customProviderName, config => new OpenAIProvider(config.ApiKey, config.BaseUrl));

            // Assert
            factory.IsProviderSupported(customProviderName).Should().BeTrue();
            factory.GetSupportedProviders().Should().Contain(customProviderName);
        }

        [Fact]
        public void OpenAIProvider_ShouldHaveCorrectProviderName()
        {
            // Arrange & Act
            var provider = new OpenAIProvider("test-key");

            // Assert
            provider.ProviderName.Should().Be("OpenAI");
        }

        [Fact]
        public void OpenAIProvider_ShouldReturnSupportedModels()
        {
            // Arrange
            var provider = new OpenAIProvider("test-key");

            // Act
            var models = provider.GetSupportedModels().ToList();

            // Assert
            models.Should().NotBeEmpty();
            models.Should().Contain(model => model.Contains("gpt"));
        }

        [Fact]
        public void AnthropicProvider_ShouldHaveCorrectProviderName()
        {
            // Arrange & Act
            var provider = new AnthropicProvider("test-key");

            // Assert
            provider.ProviderName.Should().Be("Anthropic");
        }

        [Fact]
        public void AnthropicProvider_ShouldReturnSupportedModels()
        {
            // Arrange
            var provider = new AnthropicProvider("test-key");

            // Act
            var models = provider.GetSupportedModels().ToList();

            // Assert
            models.Should().NotBeEmpty();
            models.Should().Contain(model => model.Contains("claude"));
        }

        [Fact]
        public void DoubaoProvider_ShouldHaveCorrectProviderName()
        {
            // Arrange & Act
            var provider = new DoubaoProvider("test-key");

            // Assert
            provider.ProviderName.Should().Be("Doubao");
        }

        [Fact]
        public void DoubaoProvider_ShouldReturnSupportedModels()
        {
            // Arrange
            var provider = new DoubaoProvider("test-key");

            // Act
            var models = provider.GetSupportedModels().ToList();

            // Assert
            models.Should().NotBeEmpty();
            models.Should().Contain(model => model.Contains("doubao"));
        }

        [Fact]
        public void LLMProviderConfig_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var config = new LLMProviderConfig();

            // Assert
            config.ApiKey.Should().Be(string.Empty);
            config.BaseUrl.Should().BeNull();
            config.DefaultModel.Should().BeNull();
            config.TimeoutSeconds.Should().Be(60);
            config.MaxRetries.Should().Be(3);
            config.AdditionalConfig.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void LLMProviderConfig_ShouldAllowCustomValues()
        {
            // Arrange & Act
            var config = new LLMProviderConfig
            {
                ApiKey = "custom-key",
                BaseUrl = "https://custom.api.com",
                DefaultModel = "custom-model",
                TimeoutSeconds = 120,
                MaxRetries = 5,
                AdditionalConfig = new Dictionary<string, object> { ["custom"] = "value" }
            };

            // Assert
            config.ApiKey.Should().Be("custom-key");
            config.BaseUrl.Should().Be("https://custom.api.com");
            config.DefaultModel.Should().Be("custom-model");
            config.TimeoutSeconds.Should().Be(120);
            config.MaxRetries.Should().Be(5);
            config.AdditionalConfig.Should().ContainKey("custom").WhoseValue.Should().Be("value");
        }

        [Fact]
        public void OpenAIProvider_ShouldThrowForNullApiKey()
        {
            // Act & Assert
            Action act = () => new OpenAIProvider(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AnthropicProvider_ShouldThrowForNullApiKey()
        {
            // Act & Assert
            Action act = () => new AnthropicProvider(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DoubaoProvider_ShouldThrowForNullApiKey()
        {
            // Act & Assert
            Action act = () => new DoubaoProvider(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void OpenAIProvider_ShouldUseDefaultBaseUrl()
        {
            // Arrange & Act
            var provider = new OpenAIProvider("test-key");

            // Assert
            provider.Should().NotBeNull();
            provider.ProviderName.Should().Be("OpenAI");
        }

        [Fact]
        public void AnthropicProvider_ShouldUseDefaultBaseUrl()
        {
            // Arrange & Act
            var provider = new AnthropicProvider("test-key");

            // Assert
            provider.Should().NotBeNull();
            provider.ProviderName.Should().Be("Anthropic");
        }

        [Fact]
        public void DoubaoProvider_ShouldUseDefaultBaseUrl()
        {
            // Arrange & Act
            var provider = new DoubaoProvider("test-key");

            // Assert
            provider.Should().NotBeNull();
            provider.ProviderName.Should().Be("Doubao");
        }

        [Fact]
        public void OpenAIProvider_ShouldUseCustomBaseUrl()
        {
            // Arrange
            var customBaseUrl = "https://custom.openai.com/v1";

            // Act
            var provider = new OpenAIProvider("test-key", customBaseUrl);

            // Assert
            provider.Should().NotBeNull();
            provider.ProviderName.Should().Be("OpenAI");
        }

        [Fact]
        public void AnthropicProvider_ShouldUseCustomBaseUrl()
        {
            // Arrange
            var customBaseUrl = "https://custom.anthropic.com";

            // Act
            var provider = new AnthropicProvider("test-key", customBaseUrl);

            // Assert
            provider.Should().NotBeNull();
            provider.ProviderName.Should().Be("Anthropic");
        }

        [Fact]
        public void DoubaoProvider_ShouldUseCustomBaseUrl()
        {
            // Arrange
            var customBaseUrl = "https://custom.doubao.com/api/v3";

            // Act
            var provider = new DoubaoProvider("test-key", customBaseUrl);

            // Assert
            provider.Should().NotBeNull();
            provider.ProviderName.Should().Be("Doubao");
        }
    }
}