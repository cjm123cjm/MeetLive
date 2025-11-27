using DotNetCore.CAP;
using MeetLive.Services.IService.Dtos;
using Microsoft.Extensions.Logging;

namespace MeetLive.Services.WebSocket.Message
{
    public class WebSocketMessageConsumer : ICapSubscribe
    {
        private const string TOPIC_NAME = "meetlive.websocket.messages";

        private readonly ChannelContextUtils _channelContextUtils;
        private readonly ILogger<WebSocketMessageConsumer> _logger;

        public WebSocketMessageConsumer(
            ChannelContextUtils channelContextUtils,
            ILogger<WebSocketMessageConsumer> logger)
        {
            _channelContextUtils = channelContextUtils;
            _logger = logger;
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="messageSendDto"></param>
        /// <returns></returns>
        [CapSubscribe(TOPIC_NAME)]
        public void HandleMessage(MessageSendDto<object> messageSendDto)
        {
            try
            {
                _logger.LogInformation($"CAP收到消息: {System.Text.Json.JsonSerializer.Serialize(messageSendDto)}");

                // 处理消息
                _channelContextUtils.SendMessage(messageSendDto);

                _logger.LogInformation("CAP消息处理成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CAP处理消息时发生异常");

                throw; // 抛出异常让CAP知道处理失败
            }
        }
    }
}
