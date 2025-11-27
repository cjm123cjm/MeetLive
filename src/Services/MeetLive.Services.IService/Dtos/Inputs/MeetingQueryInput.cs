namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 会议查询输入参数
    /// </summary>
    public class MeetingQueryInput : PageInput
    {
        /// <summary>
        /// 是否结束:0-结束,1-进行中
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 会议号
        /// </summary>
        public string? MeetingNo { get; set; }
        /// <summary>
        /// 会议名称
        /// </summary>
        public string? MeetingName { get; set; }
    }
}
