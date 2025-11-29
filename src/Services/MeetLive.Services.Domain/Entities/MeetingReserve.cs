using System.ComponentModel.DataAnnotations;

namespace MeetLive.Services.Domain.Entities
{
    /// <summary>
    /// 预约会议
    /// </summary>
    public class MeetingReserve
    {
        public long MeetingId { get; set; }
        /// <summary>
        /// 会议号
        /// </summary>
        [MaxLength(10)]
        public string MeetingNo { get; set; } = null!;
        /// <summary>
        /// 会议名称
        /// </summary>
        [MaxLength(20)]
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
        [MaxLength(10)]
        public string? JoinPassword { get; set; } = null;
        /// <summary>
        /// 持续时间
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// 是否结束:0-未开始,1-已结束
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedUserId { get; set; }
    }
}
