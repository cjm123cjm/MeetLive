using MeetLive.Services.Domain.Entities;

namespace MeetLive.Services.Domain.IRepository
{
    public interface IUserContactApplyRepository : IBaseRepository<UserContactApply>
    {
        /// <summary>
        /// 根据申请人id和接收人id查询数据
        /// </summary>
        /// <param name="applyUserId"></param>
        /// <param name="receiveUserId"></param>
        /// <returns></returns>
        Task<UserContactApply?> SelectByApplyUserIdAndReceiveUserIdAsync(long applyUserId,long receiveUserId);
    }
}
