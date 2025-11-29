namespace MeetLive.Services.Domain.Entities
{
    /// <summary>
    /// 预约会议邀请人
    /// </summary>
    public class MeetingReserveMember
    {
        /// <summary>
        /// 预约会议id
        /// </summary>
        public long MeetingId { get; set; }
        /// <summary>
        /// 被预约人
        /// </summary>
        public long InviteUserId { get; set; }
    }
}
