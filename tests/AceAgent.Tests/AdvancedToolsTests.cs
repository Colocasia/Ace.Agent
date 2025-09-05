using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;
using AceAgent.Tools;
using AceAgent.Tools.CKG;
using AceAgent.Tools.CKG.Data;
using AceAgent.Tools.CKG.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AceAgent.Tests
{
    /// <summary>
    /// 高级工具测试
    /// </summary>
    public class AdvancedToolsTests
    {
        private readonly Mock<ILogger<CKGTool>> _mockCKGLogger;

        public AdvancedToolsTests()
        {
            _mockCKGLogger = new Mock<ILogger<CKGTool>>();
        }

        #region CKGTool Tests

        [Fact]
        public void CKGTool_ShouldHaveCorrectToolType()
        {
            // Act & Assert
            Assert.True(typeof(CKGTool).IsAssignableTo(typeof(ITool)));
        }

        [Fact]
        public void CKGTool_ShouldBeInCorrectNamespace()
        {
            // Act & Assert
            Assert.Equal("AceAgent.Tools", typeof(CKGTool).Namespace);
        }

        #endregion

        #region WebSearchTool Tests

        [Fact]
        public void WebSearchTool_ShouldHaveCorrectNameAndDescription()
        {
            // Arrange
            var tool = new WebSearchTool();

            // Act & Assert
            tool.Name.Should().Be("web_search");
            tool.Description.Should().Contain("搜索互联网");
        }

        [Fact]
        public async Task WebSearchTool_ShouldFailWithEmptyQuery()
        {
            // Arrange
            var tool = new WebSearchTool();
            var input = new ToolInput
            {
                Parameters = new Dictionary<string, object>
                {
                    ["query"] = ""
                }
            };

            // Act
            var result = await tool.ExecuteAsync(input);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("搜索查询不能为空");
        }

        [Fact]
        public async Task WebSearchTool_ShouldValidateInput()
        {
            // Arrange
            var tool = new WebSearchTool();
            var validInput = new ToolInput
            {
                Parameters = new Dictionary<string, object>
                {
                    ["query"] = "test query"
                }
            };
            var invalidInput = new ToolInput
            {
                Parameters = new Dictionary<string, object>
                {
                    ["query"] = ""
                }
            };

            // Act
            var validResult = await tool.ValidateInputAsync(validInput);
            var invalidResult = await tool.ValidateInputAsync(invalidInput);

            // Assert
            validResult.Should().BeTrue();
            invalidResult.Should().BeFalse();
        }

        #endregion

        #region SequentialThinkingTool Tests

        [Fact]
        public void SequentialThinkingTool_ShouldHaveCorrectNameAndDescription()
        {
            // Arrange
            var tool = new SequentialThinkingTool();

            // Act & Assert
            tool.Name.Should().Be("sequentialthinking");
            tool.Description.Should().Contain("顺序思维推理");
        }

        [Fact]
        public async Task SequentialThinkingTool_ShouldFailWithEmptyProblem()
        {
            // Arrange
            var tool = new SequentialThinkingTool();
            var input = new ToolInput
            {
                Parameters = new Dictionary<string, object>
                {
                    ["problem"] = ""
                }
            };

            // Act
            var result = await tool.ExecuteAsync(input);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("问题描述不能为空");
        }

        [Fact]
        public async Task SequentialThinkingTool_ShouldProcessValidProblem()
        {
            // Arrange
            var tool = new SequentialThinkingTool();
            var input = new ToolInput
            {
                Parameters = new Dictionary<string, object>
                {
                    ["problem"] = "如何优化代码性能？",
                    ["max_steps"] = 5
                }
            };

            // Act
            var result = await tool.ExecuteAsync(input);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task SequentialThinkingTool_ShouldValidateInput()
        {
            // Arrange
            var tool = new SequentialThinkingTool();
            var validInput = new ToolInput
            {
                Parameters = new Dictionary<string, object>
                {
                    ["problem"] = "test problem"
                }
            };
            var invalidInput = new ToolInput
            {
                Parameters = new Dictionary<string, object>
                {
                    ["problem"] = ""
                }
            };

            // Act
            var validResult = await tool.ValidateInputAsync(validInput);
            var invalidResult = await tool.ValidateInputAsync(invalidInput);

            // Assert
            validResult.Should().BeTrue();
            invalidResult.Should().BeFalse();
        }

        #endregion

        private void Cleanup()
        {
            // 清理测试资源
        }

        ~AdvancedToolsTests()
        {
            Cleanup();
        }
    }
}