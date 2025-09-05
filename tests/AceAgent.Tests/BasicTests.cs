using Xunit;
using FluentAssertions;
using AceAgent.Core.Models;

namespace AceAgent.Tests
{
    /// <summary>
    /// 基础功能测试
    /// </summary>
    public class BasicTests
    {
        [Fact]
        public void ToolInput_ShouldCreateWithValidData()
        {
            // Arrange
            var rawInput = "test input";
            
            // Act
            var toolInput = new ToolInput { RawInput = rawInput };
            
            // Assert
            toolInput.Should().NotBeNull();
            toolInput.RawInput.Should().Be(rawInput);
        }
        
        [Fact]
        public void ToolResult_ShouldCreateSuccessResult()
        {
            // Arrange
            var message = "Operation completed successfully";
            var data = new { result = "test" };
            
            // Act
            var result = ToolResult.CreateSuccess(message, data);
            
            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be(message);
            result.Data.Should().Be(data);
            result.Error.Should().BeNull();
        }
        
        [Fact]
        public void ToolResult_ShouldCreateFailureResult()
        {
            // Arrange
            var message = "Operation failed";
            var error = "Detailed error message";
            
            // Act
            var result = ToolResult.Failure(message, error);
            
            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be(message);
            result.Error.Should().Be(error);
        }
        
        [Fact]
        public void LLMOptions_ShouldValidateRequiredFields()
        {
            // Arrange & Act
            var options = new LLMOptions
            {
                Model = "gpt-4",
                Temperature = 0.7,
                MaxTokens = 2000
            };
            
            // Assert
            options.Should().NotBeNull();
            options.Model.Should().Be("gpt-4");
            options.Temperature.Should().Be(0.7);
            options.MaxTokens.Should().Be(2000);
        }
        
        [Theory]
        [InlineData("gpt-4", 0.7)]
        [InlineData("claude-3", 0.5)]
        [InlineData("doubao-pro", 0.9)]
        public void LLMOptions_ShouldSupportMultipleModels(string model, double temperature)
        {
            // Arrange & Act
            var options = new LLMOptions
            {
                Model = model,
                Temperature = temperature,
                MaxTokens = 1000
            };
            
            // Assert
            options.Model.Should().Be(model);
            options.Temperature.Should().Be(temperature);
        }
    }
}