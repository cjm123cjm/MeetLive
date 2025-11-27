using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MeetLive.Services.Domain.Repository
{
    public class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
    {
        public UserInfoRepository(MeetLiveDbContext context) : base(context)
        {
        }
        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<UserInfo?> SelectByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Email == email);
        }

        /// <summary>
        /// 更新最后登陆时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public async Task UpdateLastLoginTimeAsync(long userId, DateTime time)
        {
            var sql = "UPDATE UserInfos SET LastLoginTime = {0} WHERE UserId = {1}";
            await _context.Database.ExecuteSqlRawAsync(sql, time, userId);
        }

        /// <summary>
        /// 更新最后离开时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public async Task UpdateLastOffTimeAsync(long userId, DateTime time)
        {
            var sql = "UPDATE UserInfos SET LastOffTime = {0} WHERE UserId = {1}";
            await _context.Database.ExecuteSqlRawAsync(sql, time, userId);
        }
    }
}
