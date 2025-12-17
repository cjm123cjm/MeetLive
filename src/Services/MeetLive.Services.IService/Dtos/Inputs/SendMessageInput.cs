namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 发送聊天消息输入参数
    /// </summary>
    public class SendMessageInput
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; } = null!;
        /// <summary>
        /// 消息类型
        /// </summary>
        public int MessageType { get; set; }
        /// <summary>
        /// 接受人id,0-发给全员,1-发给个人
        /// </summary>
        public long ReceiveUserId { get; set; }
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
    }
}
