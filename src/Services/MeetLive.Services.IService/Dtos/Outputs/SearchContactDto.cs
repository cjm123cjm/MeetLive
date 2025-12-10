namespace MeetLive.Services.IService.Dtos.Outputs
{
    /// <summary>
    /// 搜索联系人输出参数
    /// </summary>
    public class SearchContactDto
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = null!;
        /// <summary>
        /// 状态:0-待处理,1-同意,2-拒绝,3-拉黑,-1是自己
        /// </summary>
        public int Status { get; set; }
    }
}
