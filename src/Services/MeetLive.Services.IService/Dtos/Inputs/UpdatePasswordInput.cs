namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 修改密码输入参数
    /// </summary>
    public class UpdatePasswordInput
    {
        /// <summary>
        /// 旧密码
        /// </summary>
        public string OldPassword { get; set; } = null!;
        /// <summary>
        /// 新密码
        /// </summary>
        public string NewPassword { get; set; } = null!;
    }
}
