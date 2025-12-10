using Dapper;
using MeetLive.Services.Domain.CustomerException;
using MeetLive.Services.Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Data;

namespace MeetLive.Services.Domain.Repository
{
    public class DapperRepository : IDapperRepository
    {
        private readonly MeetLiveDbContext _context;
        private readonly ILogger<DapperRepository> _logger;
        private readonly IDapperOptions _options;
        private readonly string _connectionString;
        private bool _disposed = false;

        public DapperRepository(
            MeetLiveDbContext context,
            ILogger<DapperRepository> logger,
            IOptions<DapperOptions> options = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? new DapperOptions();
            _connectionString = _context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("无法获取数据库连接字符串");
        }

        #region 连接管理

        /// <summary>
        /// EF Core 共享连接（用于事务性操作）
        /// </summary>
        private IDbConnection SharedConnection => _context.Database.GetDbConnection();

        /// <summary>
        /// 当前 EF Core 事务
        /// </summary>
        private IDbTransaction CurrentTransaction => _context.Database.CurrentTransaction?.GetDbTransaction();

        /// <summary>
        /// 创建独立连接（用于只读操作，自动管理生命周期）
        /// </summary>
        private IDbConnection CreateIndependentConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// 确保共享连接已打开
        /// </summary>
        private void EnsureSharedConnectionOpen()
        {
            if (SharedConnection.State != ConnectionState.Open)
            {
                SharedConnection.Open();
                _logger.LogDebug("已打开共享数据库连接");
            }
        }

        /// <summary>
        /// 判断是否应该使用独立连接
        /// </summary>
        private bool ShouldUseIndependentConnection(IDbTransaction transaction)
        {
            // 如果有事务，必须使用共享连接以保证事务一致性
            if (transaction != null || CurrentTransaction != null)
                return false;

            // 根据配置决定是否对查询使用独立连接
            return _options.UseIndependentConnectionForQueries;
        }

        #endregion

        #region 工具方法

        private void ValidateParameters(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL 语句不能为空", nameof(sql));
        }

        private void LogOperation(string operation, string sql, object param)
        {
            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug("Dapper {Operation}: {Sql} | 参数: {Parameters}",
                    operation, sql, param ?? "无");
            }
            else
            {
                _logger.LogDebug("Dapper {Operation}: {Sql}", operation, sql);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DapperRepository));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                // 注意：这里不关闭共享连接，由 EF Core 管理
                _disposed = true;
                _logger.LogDebug("DapperRepository 已释放");
            }
        }

        #endregion


        #region 执行策略

        /// <summary>
        /// 使用共享连接执行（由 EF Core 管理生命周期）
        /// </summary>
        private async Task<T> ExecuteWithSharedConnection<T>(Func<Task<T>> operation)
        {
            ThrowIfDisposed();
            EnsureSharedConnectionOpen();

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = await operation();
                stopwatch.Stop();

                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug("共享连接操作完成，耗时: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                }

                return result;
            }
            catch (MySqlException sqlEx)
            {
                _logger.LogError(sqlEx, "数据库操作失败 (SQL错误: {ErrorNumber})", sqlEx.Number);
                throw new DapperRepositoryException($"数据库操作失败: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "共享连接操作失败");
                throw new DapperRepositoryException("数据访问操作失败", ex);
            }
        }

        /// <summary>
        /// 使用独立连接执行（自动管理生命周期）
        /// </summary>
        private async Task<T> ExecuteWithIndependentConnection<T>(Func<IDbConnection, Task<T>> operation)
        {
            ThrowIfDisposed();

            using (var connection = CreateIndependentConnection())
            {
                // 连接验证
                if (connection == null)
                    throw new InvalidOperationException("无法创建数据库连接");

                connection.Open();

                try
                {
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var result = await operation(connection);
                    stopwatch.Stop();

                    if (_options.EnableDetailedLogging)
                    {
                        _logger.LogDebug("独立连接操作完成，耗时: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                    }

                    return result;
                }
                catch (MySqlException sqlEx)
                {
                    _logger.LogError(sqlEx, "数据库操作失败 (SQL错误: {ErrorNumber})", sqlEx.Number);
                    throw new DapperRepositoryException($"数据库操作失败: {sqlEx.Message}", sqlEx);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "独立连接操作失败");
                    throw new DapperRepositoryException("数据访问操作失败", ex);
                }
            }
        }

        #endregion


        /// <summary>
        /// 查询多个记录
        /// </summary>
        /// <typeparam name="T">目标类</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="transaction">事务</param>
        /// <param name="param">参数对象</param>
        /// <param name="commandTimeout">命令超时时间（秒）</param>
        /// <param name="commandType">命令类型（Text 或 StoredProcedure）</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            ValidateParameters(sql);
            LogOperation("查询", sql, param);

            if (ShouldUseIndependentConnection(transaction))
            {
                return await ExecuteWithIndependentConnection(async (conn) =>
                    await conn.QueryAsync<T>(sql, param, transaction, commandTimeout ?? _options.DefaultCommandTimeout, commandType));
            }
            else
            {
                return await ExecuteWithSharedConnection(async () =>
                    await SharedConnection.QueryAsync<T>(sql, param, transaction ?? CurrentTransaction, commandTimeout ?? _options.DefaultCommandTimeout, commandType));
            }
        }

        /// <summary>
        /// 查询单个记录（第一条或默认值）
        /// </summary>
        /// <typeparam name="T">目标类</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数对象</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间（秒）</param>
        /// <param name="commandType">命令类型（Text 或 StoredProcedure）</param>
        /// <returns></returns>
        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            ValidateParameters(sql);
            LogOperation("查询第一条", sql, param);

            if (ShouldUseIndependentConnection(transaction))
            {
                return await ExecuteWithIndependentConnection(async (conn) =>
                    await conn.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout ?? _options.DefaultCommandTimeout, commandType));
            }
            else
            {
                return await ExecuteWithSharedConnection(async () =>
                    await SharedConnection.QueryFirstOrDefaultAsync<T>(sql, param, transaction ?? CurrentTransaction, commandTimeout ?? _options.DefaultCommandTimeout, commandType));
            }
        }

        /// <summary>
        /// 查询单个记录（第一条或默认值）
        /// </summary>
        /// <typeparam name="T">目标类</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数对象</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间（秒）</param>
        /// <param name="commandType">命令类型（Text 或 StoredProcedure）</param>
        /// <returns></returns>
        public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            ValidateParameters(sql);
            LogOperation("查询单条", sql, param);

            if (ShouldUseIndependentConnection(transaction))
            {
                return await ExecuteWithIndependentConnection(async (conn) =>
                    await conn.QuerySingleOrDefaultAsync<T>(sql, param, transaction, commandTimeout ?? _options.DefaultCommandTimeout, commandType));
            }
            else
            {
                return await ExecuteWithSharedConnection(async () =>
                    await SharedConnection.QuerySingleOrDefaultAsync<T>(sql, param, transaction ?? CurrentTransaction, commandTimeout ?? _options.DefaultCommandTimeout, commandType));
            }
        }

        /// <summary>
        /// 执行增删改操作
        /// </summary>
        /// <typeparam name="T">目标类</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数对象</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间（秒）</param>
        /// <param name="commandType">命令类型（Text 或 StoredProcedure）</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            ValidateParameters(sql);
            LogOperation("执行", sql, param);

            // 写操作通常需要在事务中，使用共享连接
            var result = await ExecuteWithSharedConnection(async () =>
                await SharedConnection.ExecuteAsync(sql, param, transaction ?? CurrentTransaction, commandTimeout ?? _options.DefaultCommandTimeout, commandType));

            _logger.LogDebug("执行影响 {Count} 行", result);
            return result;
        }
    }
}
