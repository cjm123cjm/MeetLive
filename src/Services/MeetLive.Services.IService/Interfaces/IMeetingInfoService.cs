using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Interfaces
{
    public interface IMeetingInfoService
    {
        /// <summary>
        /// 查询会议
        /// </summary>
        /// <param name="queryInput"></param>
        /// <returns></returns>
        Task<PageDto<MeetingInfoDto>> LoadMeetingAsync(MeetingQueryInput queryInput);

        /// <summary>
        /// 创建快速会议
        /// </summary>
        /// <param name="meetingInput"></param>
        /// <returns></returns>
        Task QuickMeetingAsync(QuickMeetingInput meetingInput);

        /// <summary>
        /// 加入会议
        /// </summary>
        /// <param name="videoOpen">是否打开摄像头</param>
        /// <returns></returns>
        Task<MessageSendDto<object>> JoinMeetingAsync(bool videoOpen);

        /// <summary>
        /// 预加入会议
        /// </summary>
        /// <param name="meetingInput"></param>
        /// <returns></returns>
        Task PreJoinMeetingAsync(PreJoinMeetingInput meetingInput);

        /// <summary>
        /// 退出会议
        /// </summary>
        /// <param name="type">2-退出会议,3-被踢出会议,4-被拉黑</param>
        /// <returns></returns>
        Task<MessageSendDto<object>?> ExitMeetingAsync(UserInfoDto userDto, int type);

        /// <summary>
        /// 被踢出会议/被拉黑
        /// </summary>
        /// <param name="type">2-退出会议,3-被踢出会议,4-被拉黑</param>
        /// <param name="userId">被提出的用户id</param>
        /// <returns></returns>
        Task<MessageSendDto<object>?> ForceExitMeetingAsync(int type, string userId);

        /// <summary>
        /// 获取正在进行的会议
        /// </summary>
        /// <returns></returns>
        Task<MeetingInfoDto> GetCurrentMeetingAsync();

        /// <summary>
        /// 结束会议
        /// </summary>
        /// <param name="meetingId">会议id</param>
        /// <returns></returns>
        Task<MessageSendDto<object>> FinishMeetingAsync(string meetingId);

        /// <summary>
        /// 删除会议记录
        /// </summary>
        /// <param name="meetingId">会议id</param>
        /// <returns></returns>
        Task DeleteMeetingRecordAsync(string meetingId);

        /// <summary>
        /// 加载参加会议的成员
        /// </summary>
        /// <param name="meetingId">会议id</param>
        /// <returns></returns>
        Task<List<MeetingMemberDto>> LoadMeetingMemberAsync(string meetingId);

        /// <summary>
        /// 参加预约会议
        /// </summary>
        /// <param name="reserveJoinMeetingInput"></param>
        /// <returns></returns>
        Task ReserveJoinMeetingAsync(ReserveJoinMeetingInput reserveJoinMeetingInput);

        /// <summary>
        /// 邀请入会
        /// </summary>
        /// <param name="selectContactIds">邀请人ids</param>
        /// <returns></returns>
        Task<List<MessageSendDto<object>>> InviteMemberAsync(string selectContactIds);

        /// <summary>
        /// 接受邀请
        /// </summary>
        /// <param name="meetingId"></param>
        void AcceptInviteAsync(long meetingId);

        /// <summary>
        /// 开启/关闭摄像头
        /// </summary>
        /// <param name="openVideo"></param>
        /// <returns></returns>
        MessageSendDto<object> UpdateMemberOpenVideoAsync(bool openVideo);
    }
}
