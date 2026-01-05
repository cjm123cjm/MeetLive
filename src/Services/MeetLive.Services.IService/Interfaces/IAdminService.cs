using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Interfaces
{
    /// <summary>
    /// 管理员接口
    /// </summary>
    public interface IAdminService
    {
        /// <summary>
        /// 查询用户列表
        /// </summary>
        /// <param name="queryInput"></param>
        /// <returns></returns>
        Task<PageDto<UserInfoDto>> LoadUserListAsync(UserInfoQueryInput queryInput);

        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="stateInput"></param>
        /// <returns></returns>
        Task<MessageSendDto<object>?> UpdateUserStateAsync(UpdateUserStateInput stateInput);

        /// <summary>
        /// 强制下线
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MessageSendDto<object>?> ForceOffLineAsync(long userId);

        /// <summary>
        /// 获取会议列表
        /// </summary>
        /// <param name="meetingQueryInput"></param>
        /// <returns></returns>
        Task<PageDto<MeetingInfoDto>> LoadMeetingListAsync(MeetingQueryInput meetingQueryInput);
    }
}
