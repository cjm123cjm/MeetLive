using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MeetLive.Services.Domain.Repository
{
    public class UserContactApplyRepository : BaseRepository<UserContactApply>, IUserContactApplyRepository
    {
        public UserContactApplyRepository(MeetLiveDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 根据申请人id和接收人id查询数据
        /// </summary>
        /// <param name="applyUserId"></param>
        /// <param name="receiveUserId"></param>
        /// <returns></returns>
        public async Task<UserContactApply?> SelectByApplyUserIdAndReceiveUserIdAsync(long applyUserId, long receiveUserId)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.ApplyUserId == applyUserId && t.ReceiveUserId == receiveUserId);
        }
    }
}
