namespace MeetLive.Services.IService.Dtos.Outputs
{
    public class UserInfoDto
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = null!;
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = null!;
        /// <summary>
        /// 0:女,1:男
        /// </summary>
        public int? Sex { get; set; }
        /// <summary>
        /// 是否是超级管理员
        /// </summary>
        public bool IsAdmin { get; set; }
        /// <summary>
        /// 个人会议号
        /// </summary>
        public string? MeetingNo { get; set; }
        /// <summary>
        /// 当前正在参加的会议号
        /// </summary>
        public string? CurrentMeetingId { get; set; } = null;
        public string? Token { get; set; } = null;
    }
}
