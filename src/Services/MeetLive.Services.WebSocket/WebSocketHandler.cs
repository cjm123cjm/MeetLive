using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using MeetLive.Services.Domain.IRepository;
using Microsoft.Extensions.Logging;

namespace MeetLive.Services.WebSocket
{
    /// <summary>
    /// WebSocketHandler
    /// </summary>
    public class WebSocketHandler : SimpleChannelInboundHandler<TextWebSocketFrame>
    {
        private readonly ILogger<WebSocketHandler> _logger;
        private readonly IUserInfoRepository _userInfoRepository;

        public WebSocketHandler(
            ILogger<WebSocketHandler> logger,
            IUserInfoRepository userInfoRepository)
        {
            _logger = logger;
            _userInfoRepository = userInfoRepository;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _logger.LogInformation("有新的链接加入...");
        }

        public override async void ChannelInactive(IChannelHandlerContext context)
        {
            _logger.LogInformation("有链接断开...");

            string channelId = context.Channel.Id.ToString()!;

            var attribute = context.Channel.GetAttribute(AttributeKey<string>.ValueOf(channelId));
            string userId = attribute.Get();

            //更新用户下线时间
            await _userInfoRepository.UpdateLastOffTimeAsync(Convert.ToInt64(userId), DateTime.Now);
        }
        protected override void ChannelRead0(IChannelHandlerContext ctx, TextWebSocketFrame msg)
        {
            string text = msg.Text();

            _logger.LogInformation("收到消息：{0}", text);
        }
    }
}
