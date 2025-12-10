using System.Data;

namespace MeetLive.Services.Domain.IRepository
{
    public interface IDapperRepository
    {
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
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

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
        Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// 查询严格单个记录（必须只有一条）
        /// </summary>
        /// <typeparam name="T">目标类</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="transaction">事务</param>
        /// <param name="param">参数对象</param>
        /// <param name="commandTimeout">命令超时时间（秒）</param>
        /// <param name="commandType">命令类型（Text 或 StoredProcedure）</param>
        /// <returns></returns>
        Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

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
        Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    }
}
