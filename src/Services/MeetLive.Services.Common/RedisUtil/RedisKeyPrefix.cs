namespace MeetLive.Services.Common.RedisUtil
{
    /// <summary>
    /// 所有开发人员redis使用的Key前缀
    /// </summary>
    public static class RedisKeyPrefix
    {
        /// <summary>
        /// 会话
        /// </summary>
        public const string RedisSession = "RedisSession";
        /// <summary>
        /// 持久化
        /// </summary>
        public const string Permission = "Permission";

        /// <summary>
        /// 缓存及临时数据
        /// </summary>
        public const string RedisTempData = "RedisTempData";

        /// <summary>
        /// 无前缀
        /// </summary>
        public const string Empty = "";

        /// <summary>
        /// 前缀
        /// </summary>
        public const string InstanceName = "meetlive_";

        /// <summary>
        /// 登录注册验证码
        /// </summary>
        public const string VerifyCode = "VerifyCode_";

        /// <summary>
        /// 保存token对应的用户信息
        /// </summary>
        public const string Redis_Key_Ws_Token = "Redis_Key_Ws_Token_";
        /// <summary>
        /// 保存用户id对应的token
        /// </summary>
        public const string Redis_Key_Ws_Token_UserId = "Redis_Key_Ws_Token_UserId_";

        /// <summary>
        /// 会议房间
        /// </summary>
        public const string Redis_Key_Meeting_Root = "Redis_Key_Meeting_Root_";

    }
}
