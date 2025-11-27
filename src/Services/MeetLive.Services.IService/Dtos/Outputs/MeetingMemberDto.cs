namespace MeetLive.Services.IService.Dtos.Outputs
{
    public class MeetingMemberDto
    {
        /// <summary>
        /// 会议id
        /// </summary>
        public long MeetingId { get; set; }
        /// <summary>
        /// 成员id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = null!;
        /// <summary>
        /// 性别
        /// </summary>
        public int? Sex { get; set; }
        /// <summary>
        /// 最后加入时间
        /// </summary>
        public DateTime? LastJoinTime { get; set; }
        /// <summary>
        /// 状态:0-删除会议,1-正常,2-退出会议,3-被踢出会议,4-被拉黑
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 成员类型:0-主持人,1-普通成员
        /// </summary>
        public int MemberType { get; set; }
        /// <summary>
        /// 是否打开摄像头
        /// </summary>
        public bool VideoOpen { get; set; }
    }
}
