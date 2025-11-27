namespace MeetLive.Services.IService.Dtos.Outputs
{
    public class MeetingInfoDto
    {
        /// <summary>
        /// 会议id
        /// </summary>
        public long MeetingId { get; set; }
        /// <summary>
        /// 会议号
        /// </summary>
        public string MeetingNo { get; set; } = null!;
        /// <summary>
        /// 会议名称
        /// </summary>
        public string MeetingName { get; set; } = null!;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedUserId { get; set; }
        /// <summary>
        /// 加入类型:0-需要密码,1-不需要密码
        /// </summary>
        public int JoinType { get; set; }
        /// <summary>
        /// 加入的密码
        /// </summary>
        public string? JoinPassword { get; set; } = null;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 是否结束:0-结束,1-进行中
        /// </summary>
        public int Status { get; set; }
    }
}
