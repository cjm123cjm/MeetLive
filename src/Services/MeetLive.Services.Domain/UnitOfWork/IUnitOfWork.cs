using System.Data;

namespace MeetLive.Services.Domain.UnitOfWork
{
    /// <summary>
    /// 工作单元接口
    /// </summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        // 事务管理
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        // 高性能事务方法（表达式树实现）
        Task ExecuteInTransactionAsync(Func<Task> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

        // 保存更改
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // 状态检查
        bool HasActiveTransaction { get; }
        string TransactionId { get; }
    }
}
