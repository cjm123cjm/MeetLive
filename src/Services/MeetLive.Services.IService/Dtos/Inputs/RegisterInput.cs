using System.ComponentModel.DataAnnotations;

namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 注册输入参数
    /// </summary>
    public class RegisterInput
    {
        /// <summary>
        /// 邮箱
        /// </summary>
        [EmailAddress]
        public string Email { get; set; } = null!;
        /// <summary>
        /// 昵称
        /// </summary>
        [MaxLength(20)]
        public string NickName { get; set; } = null!;
        /// <summary>
        /// 密码
        /// </summary>
        [MaxLength(20)]
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
