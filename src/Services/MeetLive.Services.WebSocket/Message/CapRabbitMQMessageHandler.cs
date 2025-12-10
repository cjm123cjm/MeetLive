using DotNetCore.CAP;
using DotNetCore.CAP.Messages;
using MeetLive.Services.IService.Dtos;
using Microsoft.Extensions.Logging;

namespace MeetLive.Services.WebSocket.Message
{
    public class CapRabbitMQMessageHandler : IMessageHandler, IDisposable
    {
        private const string TOPIC_NAME = "meetlive.websocket.messages";

        private readonly ChannelContextUtils _channelContextUtils;
        private readonly ICapPublisher _capPublisher;
        private readonly ILogger<CapRabbitMQMessageHandler> _logger;
        private bool _isSubscribed = false;

        public CapRabbitMQMessageHandler(
            ChannelContextUtils channelContextUtils,
            ICapPublisher capPublisher,
            ILogger<CapRabbitMQMessageHandler> logger)
        {
            _channelContextUtils = channelContextUtils;
            _capPublisher = capPublisher;
            _logger = logger;
        }

        /// <summary>
        /// 监听 - CAP 会自动处理订阅
        /// </summary>
        public void ListenMessage()
        {
            if (_isSubscribed) return;

            // CAP 的订阅通常在服务启动时自动注册
            // 这里主要标记状态
            _isSubscribed = true;
            _logger.LogInformation("CAP RabbitMQ 消息监听已准备就绪");
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageSendDto"></param>
        /// <exception cref="NotImplementedException"></exception>
        public async void SendMessage(MessageSendDto<object> messageSendDto)
        {
            try
            {
                await _capPublisher.PublishAsync(TOPIC_NAME, messageSendDto);

                _logger.LogInformation($"消息已通过CAP发送到RabbitMQ: {messageSendDto.MessageType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通过CAP发送消息到RabbitMQ失败");
                throw;
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            _isSubscribed = false;
            _logger.LogInformation("CAP RabbitMQ 消息处理器已销毁");
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
