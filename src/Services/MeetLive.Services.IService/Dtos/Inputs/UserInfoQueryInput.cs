namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 用户查询输入参数
    /// </summary>
    public class UserInfoQueryInput : PageInput
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string? NickName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }
    }
}
