using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MeetLive.Services.Domain.Repository
{
    public class UserContactRepository : BaseRepository<UserContact>, IUserContactRepository
    {
        public UserContactRepository(MeetLiveDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 根据用户id和联系人id查询数据
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public async Task<UserContact?> SelectByUserAndContactIdAsync(long userId, long contactId)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.UserId == userId && t.ContactId == contactId);
        }

        /// <summary>
        /// 添加联系人/修改状态
        /// </summary>
        /// <param name="userContact"></param>
        /// <returns></returns>
        public async Task UpdateOrAddUserContact(UserContact userContact)
        {
            var existing = await _dbSet.FirstOrDefaultAsync(x => x.UserId == userContact.UserId &&
                                                             x.ContactId == userContact.ContactId);
            if (existing == null)
            {
                await AddAsync(userContact);
            }
            else
            {
                existing.Status = userContact.Status;
                existing.LastUpdateTime = DateTime.Now;
            }
        }
    }
}
