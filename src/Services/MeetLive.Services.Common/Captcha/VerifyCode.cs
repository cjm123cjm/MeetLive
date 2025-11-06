namespace MeetLive.Services.Common.Captcha
{
    /// <summary>
    /// 验证码信息
    /// </summary>
    public class VerifyCode
    {
        /// <summary>
        /// 验证码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 验证码数据流
        /// </summary>
        public byte[] Image { get; set; }

        /// <summary>
        /// 验证码Key
        /// </summary>
        public Guid CodeKey { get; set; } = Guid.NewGuid();
    }
}
