using Microsoft.AspNetCore.Http;

namespace MeetLive.Services.IService.Dtos.Inputs
{
    public class UploadFileInput
    {
        /// <summary>
        /// 文件
        /// </summary>
        public IFormFile File { get; set; } = null!;
        /// <summary>
        /// 消息id
        /// </summary>
        public long MessageId { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }
    }
}
