using MeetLive.Services.Domain.Entities;

namespace MeetLive.Services.Domain.IRepository
{
    public interface IUserInfoRepository : IBaseRepository<UserInfo>
    {
        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<UserInfo?> SelectByEmailAsync(string email);

        /// <summary>
        /// 更新最后登陆时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        Task UpdateLastLoginTimeAsync(long userId, DateTime time);

        /// <summary>
        /// 更新最后离开时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        Task UpdateLastOffTimeAsync(long userId, DateTime time);
    }
}
