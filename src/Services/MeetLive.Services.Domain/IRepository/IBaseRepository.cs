using System.Linq.Expressions;

namespace MeetLive.Services.Domain.IRepository
{
    public interface IBaseRepository<Entity> where Entity : class
    {
        /// <summary>
        /// 主键查询数据
        /// </summary>
        /// <returns></returns>
        Task<Entity?> GetByIdAsync(long id);

        /// <summary>
        /// 根据条件查询数据
        /// </summary>
        /// <param name="expression">条件表达式</param>
        /// <param name="isNoTracking">是否跟踪</param>
        /// <returns></returns>
        IQueryable<Entity> QueryWhere(Expression<Func<Entity, bool>>? expression, bool isNoTracking = false);

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        IQueryable<Entity> Query();

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task AddAsync(params Entity[] entities);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        void Update(params Entity[] entities);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        void Delete(params Entity[] entities);

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<Entity, bool>> expression, CancellationToken cancellationToken = default);
    }
}
