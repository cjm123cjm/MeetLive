using MeetLive.Services.Domain.Entities;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Interfaces
{
    public interface IMeetingChatMessageService
    {
        /// <summary>
        /// 加载聊天消息
        /// </summary>
        /// <param name="chatMessageQuery"></param>
        /// <returns></returns>
        Task<List<MeetingChatMessageDto>> LoadChatMessagesAsync(ChatMessageQuery chatMessageQuery);

        /// <summary>
        /// 加载历史消息
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        Task<List<MeetingChatMessageDto>> LoadHistoryMessagesAsync(ChatMessageQuery chatMessageQuery);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="sendMessageInput"></param>
        /// <returns></returns>
        Task<(MeetingChatMessage, List<MessageSendDto<object>>)> SendMessageAsync(SendMessageInput sendMessageInput);

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="uploadFileInput"></param>
        /// <returns></returns>
        Task<MessageSendDto<object>> UploadFileAsync(UploadFileInput uploadFileInput);
    }
}
