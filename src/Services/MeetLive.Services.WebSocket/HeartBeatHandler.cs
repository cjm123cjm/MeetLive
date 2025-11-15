using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;

namespace MeetLive.Services.WebSocket
{
    /// <summary>
    /// 心跳检测
    /// </summary>
    public class HeartBeatHandler : ChannelDuplexHandler
    {
        private readonly ILogger<HeartBeatHandler> _logger;

        public HeartBeatHandler(ILogger<HeartBeatHandler> logger)
        {
            _logger = logger;
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent)
            {
                IdleStateEvent idleStateEvent = (IdleStateEvent)evt;
                if (idleStateEvent.State == IdleState.ReaderIdle)
                {
                    context.WriteAndFlushAsync("心跳超时");
                    IAttribute<string> attribute = context.Channel.GetAttribute(AttributeKey<string>.ValueOf(context.Channel.Id.ToString()));
                    string userId = attribute.Get();

                    _logger.LogInformation($"用户<{userId}>断开链接");

                    context.CloseAsync();
                }
                else if (idleStateEvent.State == IdleState.WriterIdle)
                {
                    context.WriteAndFlushAsync("heart");
                }
            }
        }
    }
}
