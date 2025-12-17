namespace MeetLive.Services.IService.Dtos.Inputs
{
    public class ChatMessageQuery : PageInput
    {
        /// <summary>
        /// 最大的消息id
        /// </summary>
        public long? MaxMessageId { get; set; }
        /// <summary>
        /// 会议id
        /// </summary>
        public long? MeetingId { get; set; }
    }
}
