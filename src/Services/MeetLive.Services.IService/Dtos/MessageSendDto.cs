using MeetLive.Services.IService.Enums;

namespace MeetLive.Services.IService.Dtos
{
    /// <summary>
    /// 发送消息Dto
    /// </summary>
    public class MessageSendDto<T>
    {
        //-------------------这里面是通用字段------------------------
        /// <summary>
        /// 0:给群组发消息,1:给人发消息
        /// </summary>
        public MessageSendTypeEnum MessageSendType { get; set; }
        /// <summary>
        /// 会议id
        /// </summary>
        public string? MeetingId { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageTypeEnum MessageType { get; set; }

        /// <summary>
        /// 发送人id
        /// </summary>
        public string? SendUserId { get; set; }
        /// <summary>
        /// 发送人昵称
        /// </summary>
        public string? SendUserNickName { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public T? MessageContent { get; set; }
        /// <summary>
        /// 收消息的用户id
        /// </summary>
        public string? ReceiveUserId { get; set; }


        //----------------下面是跟发消息相关的字段----------------
        /// <summary>
        /// 发送消息的时间(时间戳)
        /// </summary>
        public long SendTime { get; set; }
        /// <summary>
        /// 消息id
        /// </summary>
        public long MessageId { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 发送文件的名称
        /// </summary>
        public string? FileName { get; set; }
        /// <summary>
        /// 发送文件类型
        /// </summary>
        public int FileType { get; set; }
        /// <summary>
        /// 发送文件大小
        /// </summary>
        public long FileSize { get; set; }
    }

    public enum MessageSendTypeEnum
    {
        /// <summary>
        /// 个人
        /// </summary>
        USER = 0,

        /// <summary>
        /// 群组
        /// </summary>
        GROUP = 1
    }
}
