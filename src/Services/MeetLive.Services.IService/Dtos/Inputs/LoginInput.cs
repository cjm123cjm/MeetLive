namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 登录输入参数
    /// </summary>
    public class LoginInput
    {
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = null!;
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = null!;
        /// <summary>
        /// 验证码key
        /// </summary>
        public string CodeKey { get; set; } = null!;
        /// <summary>
        /// 验证码
        /// </summary>
        public string Code { get; set; } = null!;
    }
}
