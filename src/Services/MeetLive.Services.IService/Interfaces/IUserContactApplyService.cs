using MeetLive.Services.Domain.Entities;

namespace MeetLive.Services.IService.Interfaces
{
    public interface IUserContactApplyService
    {
        /// <summary>
        /// 申请好友
        /// </summary>
        /// <param name="userContactApply"></param>
        /// <returns></returns>
        Task SaveUserContactAsync(UserContactApply userContactApply);
    }
}
