namespace MeetLive.Services.Common
{
    /// <summary>
    /// 时间工具类
    /// </summary>
    public static class DateTimeUtil
    {
        /// <summary>
        /// 获取某个时间的时间戳
        /// </summary>
        /// <param name="dateTime">如果为null,则获取当前时间的时间戳</param>
        /// <returns></returns>
        public static long GetTimestamp(DateTime? dateTime = null)
        {
            if (dateTime == null)
            {
                dateTime = DateTime.Now;
            }

            return new DateTimeOffset(dateTime.Value.ToUniversalTime()).ToUnixTimeSeconds();
        }

        /// <summary>
        /// 时间戳转换为时间
        /// </summary>
        /// <param name="timestamp">需要转换的时间戳</param>
        /// <returns></returns>
        public static DateTime GetDataTime(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
        }
    }
}
