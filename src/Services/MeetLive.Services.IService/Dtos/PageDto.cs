namespace MeetLive.Services.IService.Dtos
{
    /// <summary>
    /// 分页返回参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageDto<T>
    {
        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public List<T> Data { get; set; }
    }
}
