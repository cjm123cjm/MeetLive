namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 预览资源输入参数
    /// </summary>
    public class ResourceInput
    {
        /// <summary>
        /// 会议id
        /// </summary>
        public long MeetingId { get; set; }
        /// <summary>
        /// 消息id
        /// </summary>
        public long MessageId { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }
        /// <summary>
        /// 文件类型0-图片,1-视频,2-其它
        /// </summary>
        public int FileType { get; set; }
        /// <summary>
        /// 是否显示缩略图
        /// </summary>
        public bool Thumbnail { get; set; }
    }
}
