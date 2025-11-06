using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MeetLive.Services.Domain.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MeetLiveDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;

        private IDbContextTransaction _currentTransaction;

        public bool HasActiveTransaction => _currentTransaction != null;
        public string TransactionId => _currentTransaction?.TransactionId.ToString() ?? "No-Transaction";

        public UnitOfWork(
                MeetLiveDbContext context,
                ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("已有活动的事务");

            _currentTransaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            _logger.LogInformation("开始事务: {TransactionId}", _currentTransaction.TransactionId);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("没有活动的事务");

            try
            {
                await SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
                _logger.LogInformation("事务提交成功: {TransactionId}", _currentTransaction.TransactionId);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("没有活动的事务");

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("事务回滚: {TransactionId}", _currentTransaction.TransactionId);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
            try
            {
                await operation();
                await SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("自动事务执行成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "自动事务执行失败");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("保存 {Count} 条更改", result);
            return result;
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
            try
            {
                var result = await operation();
                await SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("自动事务执行成功");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "自动事务执行失败");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
            }
        }
    }
}
