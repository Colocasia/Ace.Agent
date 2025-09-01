using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;
using Microsoft.Data.Sqlite;

namespace AceAgent.Infrastructure
{
    /// <summary>
    /// SQLite轨迹记录器实现
    /// </summary>
    public class SqliteTrajectoryRecorder : ITrajectoryRecorder, IDisposable
    {
        private readonly string _connectionString;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public SqliteTrajectoryRecorder(string databasePath)
        {
            // 确保数据库目录存在
            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = $"Data Source={databasePath}";
            _semaphore = new SemaphoreSlim(1, 1);
            
            // 初始化数据库
            _ = Task.Run(InitializeDatabaseAsync);
        }

        public async Task<string> StartTrajectoryAsync(string sessionId, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                var trajectory = new Trajectory
                {
                    Id = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    Description = "轨迹记录",
                    Status = TrajectoryStatus.InProgress,
                    StartTime = DateTime.UtcNow,
                    Steps = new List<TrajectoryStep>(),
                    Metadata = metadata ?? new Dictionary<string, object>()
                };

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    INSERT INTO trajectories (id, session_id, description, status, start_time, metadata)
                    VALUES (@id, @sessionId, @description, @status, @startTime, @metadata)";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@id", trajectory.Id);
                command.Parameters.AddWithValue("@sessionId", trajectory.SessionId);
                command.Parameters.AddWithValue("@description", trajectory.Description ?? string.Empty);
                command.Parameters.AddWithValue("@status", trajectory.Status.ToString());
                command.Parameters.AddWithValue("@startTime", trajectory.StartTime.ToString("O"));
                command.Parameters.AddWithValue("@metadata", JsonSerializer.Serialize(trajectory.Metadata));

                await command.ExecuteNonQueryAsync();
                return trajectory.Id;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string> RecordStepAsync(string trajectoryId, TrajectoryStep step, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    INSERT INTO trajectory_steps 
                    (id, trajectory_id, step_number, type, name, description, status, start_time, end_time, 
                     input_data, output_data, error, metadata, execution_time)
                    VALUES 
                    (@id, @trajectoryId, @stepNumber, @type, @name, @description, @status, @startTime, @endTime,
                     @inputData, @outputData, @error, @metadata, @executionTime)";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@id", step.Id);
                command.Parameters.AddWithValue("@trajectoryId", trajectoryId);
                command.Parameters.AddWithValue("@stepNumber", step.StepNumber);
                command.Parameters.AddWithValue("@type", step.Type.ToString());
                command.Parameters.AddWithValue("@name", step.Name ?? string.Empty);
                command.Parameters.AddWithValue("@description", step.Description ?? string.Empty);
                command.Parameters.AddWithValue("@status", step.Status.ToString());
                command.Parameters.AddWithValue("@startTime", step.StartTime.ToString("O"));
                command.Parameters.AddWithValue("@endTime", step.EndTime?.ToString("O") ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@inputData", step.InputData ?? string.Empty);
                command.Parameters.AddWithValue("@outputData", step.OutputData ?? string.Empty);
                command.Parameters.AddWithValue("@error", step.Error ?? string.Empty);
                command.Parameters.AddWithValue("@metadata", JsonSerializer.Serialize(step.Metadata ?? new Dictionary<string, object>()));
                command.Parameters.AddWithValue("@executionTime", step.ExecutionTimeMs);

                await command.ExecuteNonQueryAsync();
                return step.Id;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CompleteTrajectoryAsync(string trajectoryId, TrajectoryResult result, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    UPDATE trajectories 
                    SET status = @status, end_time = @endTime, result = @result
                    WHERE id = @id";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@id", trajectoryId);
                command.Parameters.AddWithValue("@status", result.Success ? TrajectoryStatus.Completed.ToString() : TrajectoryStatus.Failed.ToString());
                command.Parameters.AddWithValue("@endTime", DateTime.UtcNow.ToString("O"));
                command.Parameters.AddWithValue("@result", JsonSerializer.Serialize(result));

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Trajectory?> GetTrajectoryAsync(string trajectoryId, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // 获取轨迹基本信息
                const string trajectorySql = @"
                    SELECT id, session_id, description, status, start_time, end_time, result, metadata
                    FROM trajectories WHERE id = @id";

                using var trajectoryCommand = new SqliteCommand(trajectorySql, connection);
                trajectoryCommand.Parameters.AddWithValue("@id", trajectoryId);

                using var trajectoryReader = await trajectoryCommand.ExecuteReaderAsync();
                if (!await trajectoryReader.ReadAsync())
                {
                    return null;
                }

                var trajectory = new Trajectory
                {
                    Id = trajectoryReader.GetString("id"),
                    SessionId = trajectoryReader.GetString("session_id"),
                    Description = trajectoryReader.IsDBNull("description") ? null : trajectoryReader.GetString("description"),
                    Status = Enum.Parse<TrajectoryStatus>(trajectoryReader.GetString("status")),
                    StartTime = DateTime.Parse(trajectoryReader.GetString("start_time")),
                    EndTime = trajectoryReader.IsDBNull("end_time") ? null : DateTime.Parse(trajectoryReader.GetString("end_time")),
                    Metadata = trajectoryReader.IsDBNull("metadata") 
                        ? new Dictionary<string, object>() 
                        : JsonSerializer.Deserialize<Dictionary<string, object>>(trajectoryReader.GetString("metadata")) ?? new Dictionary<string, object>()
                };

                if (!trajectoryReader.IsDBNull("result"))
                {
                    trajectory.Result = JsonSerializer.Deserialize<TrajectoryResult>(trajectoryReader.GetString("result"));
                }

                await trajectoryReader.CloseAsync();

                // 获取步骤信息
                const string stepsSql = @"
                    SELECT id, step_number, type, name, description, status, start_time, end_time,
                           input_data, output_data, error, metadata, execution_time
                    FROM trajectory_steps WHERE trajectory_id = @trajectoryId
                    ORDER BY step_number";

                using var stepsCommand = new SqliteCommand(stepsSql, connection);
                stepsCommand.Parameters.AddWithValue("@trajectoryId", trajectoryId);

                using var stepsReader = await stepsCommand.ExecuteReaderAsync();
                var steps = new List<TrajectoryStep>();

                while (await stepsReader.ReadAsync())
                {
                    var step = new TrajectoryStep
                    {
                        Id = stepsReader.GetString("id"),
                        StepNumber = stepsReader.GetInt32("step_number"),
                        Type = Enum.Parse<StepType>(stepsReader.GetString("type")),
                        Name = stepsReader.IsDBNull("name") ? string.Empty : stepsReader.GetString("name"),
                        Description = stepsReader.IsDBNull("description") ? string.Empty : stepsReader.GetString("description"),
                        Status = Enum.Parse<StepStatus>(stepsReader.GetString("status")),
                        StartTime = DateTime.Parse(stepsReader.GetString("start_time")),
                        EndTime = stepsReader.IsDBNull("end_time") ? null : DateTime.Parse(stepsReader.GetString("end_time")),
                        InputData = stepsReader.IsDBNull("input_data") ? null : stepsReader.GetString("input_data"),
                        OutputData = stepsReader.IsDBNull("output_data") ? null : stepsReader.GetString("output_data"),
                        Error = stepsReader.IsDBNull("error") ? null : stepsReader.GetString("error"),
                        Metadata = stepsReader.IsDBNull("metadata") 
                            ? new Dictionary<string, object>() 
                            : JsonSerializer.Deserialize<Dictionary<string, object>>(stepsReader.GetString("metadata")) ?? new Dictionary<string, object>()
                    };

                    // ExecutionTimeMs is calculated property, no need to set it

                    steps.Add(step);
                }

                trajectory.Steps = steps;
                return trajectory;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<Trajectory>> GetSessionTrajectoriesAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    SELECT id, session_id, description, status, start_time, end_time, result, metadata
                    FROM trajectories WHERE session_id = @sessionId
                    ORDER BY start_time DESC";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@sessionId", sessionId);

                using var reader = await command.ExecuteReaderAsync();
                var trajectories = new List<Trajectory>();

                while (await reader.ReadAsync())
                {
                    var trajectory = new Trajectory
                    {
                        Id = reader.GetString("id"),
                        SessionId = reader.GetString("session_id"),
                        Description = reader.IsDBNull("description") ? string.Empty : reader.GetString("description"),
                        Status = Enum.Parse<TrajectoryStatus>(reader.GetString("status")),
                        StartTime = DateTime.Parse(reader.GetString("start_time")),
                        EndTime = reader.IsDBNull("end_time") ? null : DateTime.Parse(reader.GetString("end_time")),
                        Metadata = reader.IsDBNull("metadata") 
                            ? new Dictionary<string, object>() 
                            : JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString("metadata")) ?? new Dictionary<string, object>()
                    };

                    if (!reader.IsDBNull("result"))
                    {
                        trajectory.Result = JsonSerializer.Deserialize<TrajectoryResult>(reader.GetString("result"));
                    }

                    // 获取步骤数量
                    trajectory.Steps = await GetStepCountAsync(connection, trajectory.Id);

                    trajectories.Add(trajectory);
                }

                return trajectories;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<Trajectory>> SearchTrajectoriesAsync(TrajectorySearchQuery query, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var sql = "SELECT id, session_id, description, status, start_time, end_time, result, metadata FROM trajectories WHERE 1=1";
                var parameters = new List<SqliteParameter>();

                if (!string.IsNullOrEmpty(query.SessionId))
                {
                    sql += " AND session_id = @sessionId";
                    parameters.Add(new SqliteParameter("@sessionId", query.SessionId));
                }

                if (query.Status.HasValue)
                {
                    sql += " AND status = @status";
                    parameters.Add(new SqliteParameter("@status", query.Status.Value.ToString()));
                }

                if (query.StartTimeAfter.HasValue)
                {
                    sql += " AND start_time >= @startTimeAfter";
                    parameters.Add(new SqliteParameter("@startTimeAfter", query.StartTimeAfter.Value.ToString("O")));
                }

                if (query.StartTimeBefore.HasValue)
                {
                    sql += " AND start_time <= @startTimeBefore";
                    parameters.Add(new SqliteParameter("@startTimeBefore", query.StartTimeBefore.Value.ToString("O")));
                }

                if (query.EndTimeAfter.HasValue)
                {
                    sql += " AND end_time >= @endTimeAfter";
                    parameters.Add(new SqliteParameter("@endTimeAfter", query.EndTimeAfter.Value.ToString("O")));
                }

                if (query.EndTimeBefore.HasValue)
                {
                    sql += " AND end_time <= @endTimeBefore";
                    parameters.Add(new SqliteParameter("@endTimeBefore", query.EndTimeBefore.Value.ToString("O")));
                }

                if (!string.IsNullOrEmpty(query.Keywords))
                {
                    sql += " AND (description LIKE @keywords OR result LIKE @keywords)";
                    parameters.Add(new SqliteParameter("@keywords", $"%{query.Keywords}%"));
                }

                sql += " ORDER BY start_time DESC";

                if (query.Limit > 0)
                {
                    sql += " LIMIT @limit";
                    parameters.Add(new SqliteParameter("@limit", query.Limit));
                }

                if (query.Offset > 0)
                {
                    sql += " OFFSET @offset";
                    parameters.Add(new SqliteParameter("@offset", query.Offset));
                }

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddRange(parameters.ToArray());

                using var reader = await command.ExecuteReaderAsync();
                var trajectories = new List<Trajectory>();

                while (await reader.ReadAsync())
                {
                    var trajectory = new Trajectory
                    {
                        Id = reader.GetString("id"),
                        SessionId = reader.GetString("session_id"),
                        Description = reader.IsDBNull("description") ? string.Empty : reader.GetString("description"),
                        Status = Enum.Parse<TrajectoryStatus>(reader.GetString("status")),
                        StartTime = DateTime.Parse(reader.GetString("start_time")),
                        EndTime = reader.IsDBNull("end_time") ? null : DateTime.Parse(reader.GetString("end_time")),
                        Metadata = reader.IsDBNull("metadata") 
                            ? new Dictionary<string, object>() 
                            : JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString("metadata")) ?? new Dictionary<string, object>()
                    };

                    if (!reader.IsDBNull("result"))
                    {
                        trajectory.Result = JsonSerializer.Deserialize<TrajectoryResult>(reader.GetString("result"));
                    }

                    // 获取步骤数量
                    trajectory.Steps = await GetStepCountAsync(connection, trajectory.Id);

                    trajectories.Add(trajectory);
                }

                return trajectories;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteTrajectoryAsync(string trajectoryId, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();
                try
                {
                    // 删除步骤
                    const string deleteStepsSql = "DELETE FROM trajectory_steps WHERE trajectory_id = @trajectoryId";
                    using var deleteStepsCommand = new SqliteCommand(deleteStepsSql, connection, transaction);
                    deleteStepsCommand.Parameters.AddWithValue("@trajectoryId", trajectoryId);
                    await deleteStepsCommand.ExecuteNonQueryAsync();

                    // 删除轨迹
                    const string deleteTrajectorySql = "DELETE FROM trajectories WHERE id = @id";
                    using var deleteTrajectoryCommand = new SqliteCommand(deleteTrajectorySql, connection, transaction);
                    deleteTrajectoryCommand.Parameters.AddWithValue("@id", trajectoryId);
                    await deleteTrajectoryCommand.ExecuteNonQueryAsync();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<List<TrajectoryStep>> GetStepCountAsync(SqliteConnection connection, string trajectoryId)
        {
            const string sql = "SELECT COUNT(*) FROM trajectory_steps WHERE trajectory_id = @trajectoryId";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@trajectoryId", trajectoryId);
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return new List<TrajectoryStep>(count);
        }

        private async Task InitializeDatabaseAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // 创建轨迹表
            const string createTrajectoriesTable = @"
                CREATE TABLE IF NOT EXISTS trajectories (
                    id TEXT PRIMARY KEY,
                    session_id TEXT NOT NULL,
                    description TEXT,
                    status TEXT NOT NULL,
                    start_time TEXT NOT NULL,
                    end_time TEXT,
                    result TEXT,
                    metadata TEXT,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            using var createTrajectoriesCommand = new SqliteCommand(createTrajectoriesTable, connection);
            await createTrajectoriesCommand.ExecuteNonQueryAsync();

            // 创建步骤表
            const string createStepsTable = @"
                CREATE TABLE IF NOT EXISTS trajectory_steps (
                    id TEXT PRIMARY KEY,
                    trajectory_id TEXT NOT NULL,
                    step_number INTEGER NOT NULL,
                    type TEXT NOT NULL,
                    name TEXT,
                    description TEXT,
                    status TEXT NOT NULL,
                    start_time TEXT,
                    end_time TEXT,
                    input_data TEXT,
                    output_data TEXT,
                    error TEXT,
                    metadata TEXT,
                    execution_time REAL,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (trajectory_id) REFERENCES trajectories (id) ON DELETE CASCADE
                )";

            using var createStepsCommand = new SqliteCommand(createStepsTable, connection);
            await createStepsCommand.ExecuteNonQueryAsync();

            // 创建索引
            const string createIndexes = @"
                CREATE INDEX IF NOT EXISTS idx_trajectories_session_id ON trajectories (session_id);
                CREATE INDEX IF NOT EXISTS idx_trajectories_status ON trajectories (status);
                CREATE INDEX IF NOT EXISTS idx_trajectories_start_time ON trajectories (start_time);
                CREATE INDEX IF NOT EXISTS idx_trajectory_steps_trajectory_id ON trajectory_steps (trajectory_id);
                CREATE INDEX IF NOT EXISTS idx_trajectory_steps_step_number ON trajectory_steps (step_number);";

            using var createIndexesCommand = new SqliteCommand(createIndexes, connection);
            await createIndexesCommand.ExecuteNonQueryAsync();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore?.Dispose();
                _disposed = true;
            }
        }
    }
}