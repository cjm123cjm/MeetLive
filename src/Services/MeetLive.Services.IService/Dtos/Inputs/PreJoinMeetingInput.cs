namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 预加入会议输入参数
    /// </summary>
    public class PreJoinMeetingInput
    {
        /// <summary>
        /// 会议号
        /// </summary>
        public string MeetingNo { get; set; } = null!;
        /// <summary>
        /// 会议昵称
        /// </summary>
        public string NickName { get; set; } = null!;
        /// <summary>
        /// 会议密码
        /// </summary>
        public string? JoinPassword { get; set; }
    }
}
