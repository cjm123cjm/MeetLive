namespace MeetLive.Services.Domain.Entities
{
    /// <summary>
    /// 联系人
    /// </summary>
    public class UserContact
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 联系人id
        /// </summary>
        public long ContactId { get; set; }
        /// <summary>
        /// 状态:1-好友,2-已删除好友,3-已拉黑好友
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
    }
}
