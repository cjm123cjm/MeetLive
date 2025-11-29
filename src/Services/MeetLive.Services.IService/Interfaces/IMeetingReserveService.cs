using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Interfaces
{
    public interface IMeetingReserveService
    {
        /// <summary>
        /// 加载我的预约会议
        /// </summary>
        /// <returns></returns>
        Task<List<MeetingReserveDto>> LoadMyMeetingReserveAsync();

        /// <summary>
        /// 创建预约会议
        /// </summary>
        /// <param name="meetingReserveInput"></param>
        /// <returns></returns>
        Task CreateMeetingReserveAsync(MeetingReserveInput meetingReserveInput);

        /// <summary>
        /// 删除预约会议(创建人删除会议)
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        Task DeleteMeetingReserveAsync(string meetingId);

        /// <summary>
        /// 参加人删除会议
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        Task DeleteMeetingReserveByUserAsync(string meetingId);

        /// <summary>
        /// 加载今天要参加的会议
        /// </summary>
        /// <returns></returns>
        Task<List<MeetingReserveDto>> LoadTodayMeetingAsync();
    }
}
