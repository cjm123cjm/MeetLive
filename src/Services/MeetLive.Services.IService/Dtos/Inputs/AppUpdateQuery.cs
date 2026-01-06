namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 应用更新查询参数
    /// </summary>
    public class AppUpdateQuery : PageInput
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
