using System.ComponentModel.DataAnnotations;

namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 快速会议输入参数
    /// </summary>
    public class QuickMeetingInput
    {
        /// <summary>
        /// 0-自己的会议号,1-随机生成10位数的会议号
        /// </summary>
        public int MeetingType { get; set; }

        /// <summary>
        /// 会议吗
        /// </summary>
        [MaxLength(100)]
        public string MeetingName { get; set; } = null!;

        /// <summary>
        /// 加入类型:0-需要密码,1-不需要密码
        /// </summary>
        public int JoinType { get; set; }
        /// <summary>
        /// 会议密码
        /// </summary>
        [MaxLength(5)]
        public string? JoinPassword { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = null!;
    }
}
