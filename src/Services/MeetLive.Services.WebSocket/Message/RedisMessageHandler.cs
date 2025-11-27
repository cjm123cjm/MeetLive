using MeetLive.Services.Common.RedisUtil;
using MeetLive.Services.IService.Dtos;
using Newtonsoft.Json;

namespace MeetLive.Services.WebSocket.Message
{
    public class RedisMessageHandler : IMessageHandler, IDisposable
    {
        private const string MESSAGE_TOPIC = "message.topic";
        private readonly ChannelContextUtils _channelContextUtils;
        private bool _isListening = false;

        public RedisMessageHandler(ChannelContextUtils channelContextUtils)
        {
            _channelContextUtils = channelContextUtils;
        }

        /// <summary>
        /// 监听消息
        /// </summary>
        public void ListenMessage()
        {
            if (_isListening) return;

            CacheManager.Subscribe<MessageSendDto<object>>(MESSAGE_TOPIC, (channel, message) =>
            {
                Console.WriteLine($"Redis收到消息: {JsonConvert.SerializeObject(message)}");

                _channelContextUtils.SendMessage(message);
            });

            _isListening = true;

            Console.WriteLine($"开始监听Redis频道: {MESSAGE_TOPIC}");
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageSendDto"></param>
        public void SendMessage(MessageSendDto<object> messageSendDto)
        {
            CacheManager.Publish(MESSAGE_TOPIC, messageSendDto);

            Console.WriteLine($"消息已发送到Redis频道: {MESSAGE_TOPIC}");
        }

        /// <summary>
        /// 取消订阅并清理资源
        /// </summary>
        public void Destroy()
        {
            CacheManager.Unsubscribe(MESSAGE_TOPIC);
            _isListening = false;
            Console.WriteLine($"已取消订阅Redis频道: {MESSAGE_TOPIC}");
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
