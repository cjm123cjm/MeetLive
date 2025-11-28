using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Dtos
{
    /// <summary>
    /// 退出会议
    /// </summary>
    public class MeetingExitDto
    {
        /// <summary>
        /// 退出的用户
        /// </summary>
        public long ExitUserId { get; set; }
        /// <summary>
        /// 当前会议的人
        /// </summary>
        public List<MeetingMemberDto> MeetingMemberList { get; set; } = new();
        /// <summary>
        /// 退出状态
        /// </summary>
        public int ExitStatus { get; set; }
    }
}
