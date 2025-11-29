namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 创建会议输入参数
    /// </summary>
    public class MeetingReserveInput
    {
        /// <summary>
        /// 会议号
        /// </summary>
        public string MeetingNo { get; set; } = null!;
        /// <summary>
        /// 会议名称
        /// </summary>
        public string MeetingName { get; set; } = null!;
        /// <summary>
        /// 会议开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 加入类型:0-需要密码,1-不需要密码
        /// </summary>
        public int JoinType { get; set; }
        /// <summary>
        /// 入会密码
        /// </summary>
        public string? JoinPassword { get; set; } = null;
        /// <summary>
        /// 持续时间
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// 邀请人ids
        /// </summary>
        public string? InviteUserIds { get; set; }
    }
}
