namespace MeetLive.Services.IService.Dtos.Outputs
{
    /// <summary>
    /// 登录返回参数
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public UserInfoDto User { get; set; } = new();
        /// <summary>
        /// token
        /// </summary>
        public string Token { get; set; } = null!;
    }
}
