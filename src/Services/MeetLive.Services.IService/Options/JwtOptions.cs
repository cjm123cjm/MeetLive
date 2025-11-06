namespace MeetLive.Services.IService.Options
{
    /// <summary>
    /// jwt配置
    /// </summary>
    public class JwtOptions
    {
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        /// <summary>
        /// 密钥
        /// </summary>
        public string Secret { get; set; } = null!;
        /// <summary>
        /// 过期时间/min
        /// </summary>
        public double Expires { get; set; }
    }
}
