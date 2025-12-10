namespace MeetLive.Services.Domain.Entities
{
    /// <summary>
    /// 会议消息表
    /// </summary>
    public class MeetingChatMessage
    {
        public long MessageId { get; set; }
        /// <summary>
        /// 会议id
        /// </summary>
        public long MeetingId { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public int MessageType { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string MessageContent { get; set; } = null!;
        /// <summary>
        /// 发送人id
        /// </summary>
        public long SendUserId { get; set; }
        /// <summary>
        /// 发送人昵称
        /// </summary>
        public string SendUserName { get; set; } = null!;

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 接收类型:0-发给全员,1-发给指定的人
        /// </summary>
        public int Receive_type { get; set; }

        /// <summary>
        /// 接收人
        /// </summary>
        public long? ReceiveUserId { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public int? FileSize { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string? FileName { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public int? FileType { get; set; }
        /// <summary>
        /// 文件后缀
        /// </summary>
        public string? FileSuffix { get; set; }
        /// <summary>
        /// 状态:是否发送完成,0-未完成,1-已完成
        /// </summary>
        public int Status { get; set; }
    }
}
