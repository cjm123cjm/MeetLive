using MeetLive.Services.Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace MeetLive.Services.Domain.Repository
{
    public class BaseRepository<Entity> : IBaseRepository<Entity> where Entity : class
    {
        protected readonly MeetLiveDbContext _context;
        protected readonly DbSet<Entity> _dbSet;
        protected readonly ILogger _logger;

        protected BaseRepository(MeetLiveDbContext context, ILogger logger)
        {
            _context = context;
            _dbSet = context.Set<Entity>();
            _logger = logger;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task AddAsync(params Entity[] entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(Expression<Func<Entity, bool>> expression, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().Where(expression).CountAsync(cancellationToken);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(params Entity[] entities)
        {
            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// 根据id查询数据
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public async Task<Entity?> GetByIdAsync(long id)
        {
            return await _dbSet.FindAsync(new[] { id });
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public IQueryable<Entity> Query()
        {
            return _dbSet;
        }

        /// <summary>
        /// 根据条件查询数据
        /// </summary>
        /// <param name="expression">条件表达式</param>
        /// <param name="isNoTracking">是否跟踪</param>
        /// <returns></returns>
        public IQueryable<Entity> QueryWhere(Expression<Func<Entity, bool>>? expression, bool isNoTracking = false)
        {
            var query = !isNoTracking ? _dbSet.AsNoTracking() : _dbSet.AsQueryable();

            if (expression != null)
                query = query.Where(expression);

            return query;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        public void Update(params Entity[] entities)
        {
            _dbSet.UpdateRange(entities);
        }
    }
}
