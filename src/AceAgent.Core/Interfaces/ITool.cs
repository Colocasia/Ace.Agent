using System;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Models;

namespace AceAgent.Core.Interfaces
{
    /// <summary>
    /// 工具接口，定义所有工具的基本契约
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// 工具名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 工具描述
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// 执行工具操作
        /// </summary>
        /// <param name="input">工具输入参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>工具执行结果</returns>
        Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 验证输入参数
        /// </summary>
        /// <param name="input">工具输入参数</param>
        /// <returns>验证结果</returns>
        Task<bool> ValidateInputAsync(ToolInput input);
    }
}