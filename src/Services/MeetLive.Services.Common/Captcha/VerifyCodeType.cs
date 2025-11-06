using System.ComponentModel;

namespace MeetLive.Services.Common.Captcha
{
    /// <summary>
    /// 验证码类型
    /// </summary>
    public enum VerifyCodeType
    {
        [Description("纯数字验证码")]
        NUM = 0,
        [Description("数字加字母验证码")]
        CHAR = 1,
        [Description("数字运算验证码")]
        ARITH = 2,
    }
}
