namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 发布
    /// </summary>
    public class PostUpdateAppStatusInput
    {
        public int Id { get; set; }
        /// <summary>
        /// 状态:0-未发布,1-灰度发布,2-全网发布
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 灰度用户id
        /// </summary>
        public string? GrayscaleId { get; set; }
    }
}
