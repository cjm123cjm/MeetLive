namespace MeetLive.Services.IService.Dtos.Outputs
{
    public class AppUpdateDto
    {
        public int Id { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; } = null!;
        /// <summary>
        /// 更新内容
        /// </summary>
        public string UpdateDesc { get; set; } = null!;
        /// <summary>
        /// 状态:0-未发布,1-灰度发布,2-全网发布
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 灰度用户id
        /// </summary>
        public string? GrayscaleId { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public int FileType { get; set; }
        /// <summary>
        /// 外链链接
        /// </summary>
        public string? OuterLink { get; set; }

        public int FileSize { get; set; }
        public string FileName { get; set; }
    }
}
