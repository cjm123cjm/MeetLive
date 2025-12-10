using MeetLive.Services.Domain.Entities;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Interfaces
{
    public interface IUserContactService
    {
        /// <summary>
        /// 根据邮箱查询联系人
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<SearchContactDto?> SearchContactAsync(string email);

        /// <summary>
        /// 申请好友
        /// </summary>
        /// <param name="userContactApply"></param>
        /// <returns></returns>
        Task<int> SaveUserContactAsync(UserContactApply userContactApply);

        /// <summary>
        /// 处理申请消息
        /// </summary>
        /// <param name="applyAsyncInput"></param>
        /// <returns></returns>
        Task DealWithApplyAsync(DealWithApplyAsyncInput applyAsyncInput);

        /// <summary>
        /// 加载联系人
        /// </summary>
        /// <returns></returns>
        Task<List<UserContactDto>> LoadContactUserAsync();

        /// <summary>
        /// 加载申请信息
        /// </summary>
        /// <returns></returns>
        Task<List<UserContactApplyDto>> LoadContactApplyAsync();

        /// <summary>
        /// 有多少条申请
        /// </summary>
        /// <returns></returns>
        Task<int> LoadContactApplyDealWithCountAsync();

        /// <summary>
        /// 删除/拉黑联系人
        /// </summary>
        /// <param name="deleteContactInput"></param>
        /// <returns></returns>
        Task DeleteContactAsync(DeleteContactInput deleteContactInput);
    }
}
