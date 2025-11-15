using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MeetLive.Services.WebSocket
{
    /// <summary>
    /// WebSocketHandler
    /// </summary>
    public class WebSocketHandler : SimpleChannelInboundHandler<TextWebSocketFrame>
    {
        private readonly ILogger<WebSocketHandler> _logger;

        public WebSocketHandler(ILogger<WebSocketHandler> logger)
        {
            _logger = logger;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, TextWebSocketFrame msg)
        {
            throw new NotImplementedException();
        }
    }
}
