using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System.Net;
using DotNetty.Codecs.Http;
using DotNetty.Handlers.Timeout;
using DotNetty.Codecs.Http.WebSockets;

namespace MeetLive.Services.WebSocket
{
    /// <summary>
    /// dotnetty服务
    /// </summary>
    public class NettyWebScoketServer
    {
        //boss线程,用户处理链接
        private readonly IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
        //work线程,用于处理消息
        private readonly IEventLoopGroup workGroup = new MultithreadEventLoopGroup();
        private IChannel? bootstrapChannel;
        private readonly ILogger<NettyWebScoketServer> _logger;
        private readonly HeartBeatHandler _heartBeatHandler;
        private readonly TokenValidationHandler _tokenValidationHandler;
        private readonly WebSocketHandler _webSocketHandler;

        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serviceProvider"></param>
        public NettyWebScoketServer(
            ILogger<NettyWebScoketServer> logger,
            IServiceProvider serviceProvider,
            HeartBeatHandler heartBeatHandler,
            TokenValidationHandler tokenValidationHandler,
            WebSocketHandler webSocketHandler)
        {
            _logger = logger;
            _heartBeatHandler = heartBeatHandler;
            bootstrapChannel = null;
            _tokenValidationHandler = tokenValidationHandler;
            _webSocketHandler = webSocketHandler;
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        public async Task RunStartAsync()
        {
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workGroup);

                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                       .ChildOption(ChannelOption.SoKeepalive, true)
                       .Handler(new LoggingHandler("SRV-LSTN"))
                       .Option(ChannelOption.SoBacklog, 8192)
                       .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                       {
                           IChannelPipeline pipeline = channel.Pipeline;
                           //对http协议的支持，使用http的编码器和解码器
                           pipeline.AddLast(new HttpServerCodec());

                           //保证接收http的完整性(Http聚合器,将分片的Http消息聚合成完成的FullHttpRequest FullHttpResponse)
                           pipeline.AddLast(new HttpObjectAggregator(64 * 1024));

                           //读超时时间，写超时时间，所有类型的超时时间
                           /*
                            * readerIdleTimeSeconds:一段时间未收到客户端消息(6s没有收到客户端消息就断掉)--读[前端设置的是5s发一次ws请求]
                            * writerIdleTimeSeconds:一段时间未向客户端发送消息
                            * allIdleTimeSeconds:读写都没有活动
                            */
                           pipeline.AddLast(new IdleStateHandler(readerIdleTimeSeconds: 6, writerIdleTimeSeconds: 0, allIdleTimeSeconds: 0));
                           
                           //心跳检测
                           pipeline.AddLast(_heartBeatHandler);
                           //token拦截
                           pipeline.AddLast(_tokenValidationHandler);

                           /*
                            * webSocket协议处理器
                            * websocketPath: 路径
                            * subprotocols：指定支持的子协议
                            * allowExtensions：是否允许websocket扩展
                            * maxFrameSize：最大帧数 6553 
                            * allowMaskMismatch：是否允许掩码不匹配
                            * checkStartsWith：是否严格检查路径头
                            */
                           pipeline.AddLast(new WebSocketServerProtocolHandler(
                               websocketPath: "/ws", 
                               subprotocols: null, 
                               allowExtensions: true, 
                               maxFrameSize: 65536, 
                               allowMaskMismatch: true, 
                               checkStartsWith: true));

                           pipeline.AddLast(_webSocketHandler);
                       }));
                int port = 2222;
                bootstrapChannel = await bootstrap.BindAsync(IPAddress.Loopback, port);

                Console.WriteLine("Listening on "
                    + $"ws://127.0.0.1:{port}/websocket");
            }
            catch (Exception ex)
            {
                await ShutdownAsync();
                _logger.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public async Task ShutdownAsync()
        {
            try
            {
                if (bootstrapChannel != null)
                {
                    await bootstrapChannel.CloseAsync();
                }
            }
            finally
            {
                await Task.WhenAll(
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    workGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
                );
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            ShutdownAsync().GetAwaiter().GetResult();
        }
    }
}
