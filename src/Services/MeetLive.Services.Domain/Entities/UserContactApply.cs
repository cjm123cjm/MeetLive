namespace MeetLive.Services.Domain.Entities
{
    /// <summary>
    /// 好友申请
    /// </summary>
    public class UserContactApply
    {
        /// <summary>
        /// 申请id
        /// </summary>
        public long ApplyId { get; set; }
        /// <summary>
        /// 申请人id
        /// </summary>
        public long ApplyUserId { get; set; }
        /// <summary>
        /// 接受人id
        /// </summary>
        public long ReceiveUserId { get; set; }
        /// <summary>
        /// 最后申请时间
        /// </summary>
        public DateTime LastApplyTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 状态:0-未处理,1-同意,2-拒绝,3-拉黑
        /// </summary>
        public int Status { get; set; }
    }
}
