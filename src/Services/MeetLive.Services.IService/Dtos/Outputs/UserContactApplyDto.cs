namespace MeetLive.Services.IService.Dtos.Outputs
{
    /// <summary>
    /// 申请输出参数
    /// </summary>
    public class UserContactApplyDto
    {
        /// <summary>
        /// 申请人id
        /// </summary>
        public long ApplyUserId { get; set; }
        /// <summary>
        /// 申请人昵称
        /// </summary>
        public string ApplyNickName { get; set; }
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
