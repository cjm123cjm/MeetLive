using MeetLive.Services.WebSocket;

namespace MeetLive.Services.Api.Extensions
{
    public class MessageHostedService : IHostedService
    {
        private readonly NettyWebScoketServer _nettyWebScoketServer;
        private readonly ILogger<MessageHostedService> _logger;

        public MessageHostedService(
            NettyWebScoketServer nettyWebScoketServer, 
            ILogger<MessageHostedService> logger)
        {
            _nettyWebScoketServer = nettyWebScoketServer;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WebSocket服务器正在启动...");

            try
            {
                // 这里使用 await 是安全的，因为 RunStartAsync 不会阻塞
                await _nettyWebScoketServer.RunStartAsync();
                _logger.LogInformation("WebSocket服务器启动成功，端口: 2222");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket服务器启动失败");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WebSocket服务器正在关闭...");

            try
            {
                await _nettyWebScoketServer.ShutdownAsync();
                _logger.LogInformation("WebSocket服务器关闭成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket服务器关闭过程中发生错误");
            }
        }
    }
}
