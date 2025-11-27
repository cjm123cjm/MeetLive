namespace MeetLive.Services.Domain.Entities
{
    /// <summary>
    /// 会议-成员表
    /// </summary>
    public class MeetingMember
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
        /// 会议状态(MeetingInfo里的Status冗余字段)：0-结束,1-进行中
        /// </summary>
        public int MeetingStatus { get; set; }
    }
}
