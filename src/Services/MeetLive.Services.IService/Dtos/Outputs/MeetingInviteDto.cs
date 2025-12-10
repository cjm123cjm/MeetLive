namespace MeetLive.Services.IService.Dtos.Outputs
{
    /// <summary>
    /// 邀请入会输出参数
    /// </summary>
    public class MeetingInviteDto
    {
        /// <summary>
        /// 会议名称
        /// </summary>
        public string MeetingName { get; set; } = null!;
        /// <summary>
        /// 邀请人昵称
        /// </summary>
        public string InviteUserName { get; set; } = null!;
        /// <summary>
        /// 会议id
        /// </summary>
        public string MeetingId { get; set; } = null!;
    }
}
