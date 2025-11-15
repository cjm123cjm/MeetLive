using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Channels;
using MeetLive.Services.Common.RedisUtil;
using MeetLive.Services.IService.Dtos.Outputs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace MeetLive.Services.WebSocket
{
    /// <summary>
    /// token校验
    /// </summary>
    public class TokenValidationHandler : SimpleChannelInboundHandler<IFullHttpRequest>
    {
        private readonly ILogger<TokenValidationHandler> _logger;
        private readonly ChannelContextUtils _channelContextUtils;

        public TokenValidationHandler(
            ILogger<TokenValidationHandler> logger, 
            ChannelContextUtils channelContextUtils)
        {
            _logger = logger;
            _channelContextUtils = channelContextUtils;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
        {
            string uri = msg.Uri;
            QueryStringDecoder queryDecoder = new QueryStringDecoder(uri);
            var exitToken = queryDecoder.Parameters.TryGetValue("token", out var tokens);
            if (!exitToken || tokens == null)
            {
                _ = SendErrorResponse(ctx);
                return;
            }
            var token = tokens[0];

            var userInfoDto = CheckToken(token);
            if (userInfoDto == null)
            {
                _logger.LogInformation($"token<{token}>校验失败");
                _ = SendErrorResponse(ctx);
                return;
            }

            ctx.FireChannelRead(msg.Retain());

            _channelContextUtils.AddContext(userInfoDto.UserId.ToString(), ctx.Channel);
        }
        private async Task SendErrorResponse(IChannelHandlerContext ctx)
        {
            IFullHttpResponse httpResponse = new DefaultFullHttpResponse(
                                            HttpVersion.Http11,
                                            HttpResponseStatus.Forbidden,
                                            Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("token无效")));

            httpResponse.Headers.Set(HttpHeaderNames.ContentType, "text/plain; charset=utf-8");
            httpResponse.Headers.Set(HttpHeaderNames.ContentLength, httpResponse.Content.ReadableBytes);
            httpResponse.Headers.Set(HttpHeaderNames.Connection, HttpHeaderValues.Close);

            // 发送响应
            await ctx.WriteAndFlushAsync(httpResponse);

            await ctx.CloseAsync();
        }
        private UserInfoDto? CheckToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            return CacheManager.Get<UserInfoDto?>(RedisKeyPrefix.Redis_Key_Ws_Token + token);
        }
    }
}
