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
        Task<string> QuickMeetingAsync(QuickMeetingInput meetingInput);

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
        Task<string> PreJoinMeetingAsync(PreJoinMeetingInput meetingInput);
    }
}
