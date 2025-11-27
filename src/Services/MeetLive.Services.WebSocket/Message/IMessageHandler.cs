using MeetLive.Services.IService.Dtos;

namespace MeetLive.Services.WebSocket.Message
{
    public interface IMessageHandler
    {
        /// <summary>
        /// 监听消息
        /// </summary>
        void ListenMessage();

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageSendDto"></param>
        void SendMessage(MessageSendDto<object> messageSendDto);

        /// <summary>
        /// 销毁
        /// </summary>
        void Destroy();
    }
}
