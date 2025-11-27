using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using MeetLive.Services.Common.RedisUtil;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Enums;
using MeetLive.Services.WebSocket.Message;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MeetLive.Services.WebSocket
{
    /// <summary>
    /// WebSocketHandler
    /// </summary>
    public class WebSocketHandler : SimpleChannelInboundHandler<TextWebSocketFrame>
    {
        private readonly ILogger<WebSocketHandler> _logger;
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly IMessageHandler _messageHandler;

        public WebSocketHandler(
            ILogger<WebSocketHandler> logger,
            IUserInfoRepository userInfoRepository,
            IMessageHandler messageHandler)
        {
            _logger = logger;
            _userInfoRepository = userInfoRepository;
            _messageHandler = messageHandler;
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
            if (text == "Pind") return;

            _logger.LogInformation("收到消息：{0}", text);

            PeerConnectionDataDto? peerConnection = JsonConvert.DeserializeObject<PeerConnectionDataDto>(text);
            if (peerConnection == null) return;

            var userInfoDto = CacheManager.Get<UserInfoDto>(RedisKeyPrefix.Redis_Key_Ws_Token + peerConnection.Token);
            if (userInfoDto == null) return;

            PeerMessageDto messageDto = new PeerMessageDto
            {
                SignalType = peerConnection.SignalType,
                SignalData = peerConnection.SignalData
            };
            MessageSendDto<object> sendDto = new MessageSendDto<object>
            {
                MessageType = MessageTypeEnum.PEER,
                MessageContent = messageDto,
                MessageId = Convert.ToInt64(userInfoDto.CurrentMeetingId),
                SendUserId = userInfoDto.UserId.ToString(),
                ReceiveUserId = peerConnection.ReceiveUserId,
                MessageSendType = MessageSendTypeEnum.GROUP
            };

            _messageHandler.SendMessage(sendDto);
        }
    }
}
