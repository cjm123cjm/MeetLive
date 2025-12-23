namespace MeetLive.Services.IService.Dtos
{
    /// <summary>
    /// 系统设置Dto
    /// </summary>
    public class SysSettingDto
    {
        /// <summary>
        /// 最大照片size 单位:M
        /// </summary>
        public int MaxImageSize { get; set; } = 2;
        /// <summary>
        /// 最大视频size 单位:M
        /// </summary>
        public int MaxVideoSize { get; set; } = 5;
        /// <summary>
        /// 最大文件size 单位:M
        /// </summary>
        public int MaxFileSize { get; set; } = 5;
    }
}
