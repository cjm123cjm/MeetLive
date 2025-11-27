using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Dtos
{
    /// <summary>
    /// 会议加入消息
    /// </summary>
    public class MeetingJoinDto
    {
        /// <summary>
        /// 新加人员
        /// </summary>
        public MeetingMemberDto NewMember { get; set; } = new();
        /// <summary>
        /// 会议人员集合
        /// </summary>
        public List<MeetingMemberDto> MeetingMemberList { get; set; } = new();
    }
}
