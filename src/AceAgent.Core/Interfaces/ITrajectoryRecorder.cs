using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Models;

namespace AceAgent.Core.Interfaces
{
    /// <summary>
    /// 轨迹记录器接口
    /// </summary>
    public interface ITrajectoryRecorder
    {
        /// <summary>
        /// 开始新的轨迹记录
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <param name="metadata">元数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>轨迹ID</returns>
        Task<string> StartTrajectoryAsync(
            string sessionId, 
            Dictionary<string, object>? metadata = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 记录步骤
        /// </summary>
        /// <param name="trajectoryId">轨迹ID</param>
        /// <param name="step">执行步骤</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>步骤ID</returns>
        Task<string> RecordStepAsync(
            string trajectoryId, 
            TrajectoryStep step,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 完成轨迹记录
        /// </summary>
        /// <param name="trajectoryId">轨迹ID</param>
        /// <param name="result">最终结果</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>任务</returns>
        Task CompleteTrajectoryAsync(
            string trajectoryId, 
            TrajectoryResult result,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 获取轨迹
        /// </summary>
        /// <param name="trajectoryId">轨迹ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>轨迹信息</returns>
        Task<Trajectory?> GetTrajectoryAsync(
            string trajectoryId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 获取会话的所有轨迹
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>轨迹列表</returns>
        Task<IEnumerable<Trajectory>> GetSessionTrajectoriesAsync(
            string sessionId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 删除轨迹
        /// </summary>
        /// <param name="trajectoryId">轨迹ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>任务</returns>
        Task DeleteTrajectoryAsync(
            string trajectoryId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 搜索轨迹
        /// </summary>
        /// <param name="query">搜索查询</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>匹配的轨迹列表</returns>
        Task<IEnumerable<Trajectory>> SearchTrajectoriesAsync(
            TrajectorySearchQuery query,
            CancellationToken cancellationToken = default);
    }
}