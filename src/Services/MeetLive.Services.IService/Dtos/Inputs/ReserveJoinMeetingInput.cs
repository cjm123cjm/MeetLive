namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 预约参加会议输入参数
    /// </summary>
    public class ReserveJoinMeetingInput
    {
        /// <summary>
        /// 会议id
        /// </summary>
        public long MeetingId { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = null!;
        /// <summary>
        /// 入会密码
        /// </summary>
        public string? JoinPassword { get; set; }
    }
}
