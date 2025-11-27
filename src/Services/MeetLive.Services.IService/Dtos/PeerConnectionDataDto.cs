namespace MeetLive.Services.IService.Dtos
{
    /// <summary>
    /// 信令传递的信息
    /// </summary>
    public class PeerConnectionDataDto
    {
        public string Token { get; set; } = null!;
        public string? SendUserId { get; set; }
        public string? ReceiveUserId { get; set; }
        public string? SignalType { get; set; }
        public string? SignalData { get; set; }
    }
}
