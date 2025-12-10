using MeetLive.Services.Domain.Entities;

namespace MeetLive.Services.Domain.IRepository
{
    public interface IUserContactRepository : IBaseRepository<UserContact>
    {
        /// <summary>
        /// 根据用户id和联系人id查询数据
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="contactId"></param>
        /// <returns></returns>
        Task<UserContact?> SelectByUserAndContactIdAsync(long userId,long contactId);

        /// <summary>
        /// 添加联系人/修改状态
        /// </summary>
        /// <param name="userContact"></param>
        /// <returns></returns>
        Task UpdateOrAddUserContact(UserContact userContact);
    }
}
