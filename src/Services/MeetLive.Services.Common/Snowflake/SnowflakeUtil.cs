using MeetLive.Services.Common.RedisUtil;

namespace MeetLive.Services.Common.Snowflake
{
    public static class SnowflakeUtil
    {
        private static readonly object LockMac = new object();
        /// <summary>
        /// 过期秒数
        /// </summary>
        private static long _workId;
        private static IdWorker? _idWorker;


        /// <summary>
        /// 单例模式，创建IdWorker
        /// </summary>
        /// <returns>IdWorker实例</returns>
        public static IdWorker CreateIdWorker()
        {
            if (_idWorker == null)
            {
                lock (LockMac)
                {
                    if (_idWorker == null)
                    {
                        _workId = GetWorkId();
                        _idWorker = new IdWorker(_workId, 0);

                    }
                }
            }
            return _idWorker;
        }

        // redis 中存储当前最大的workerId值，以解决workerId重复问题
        private static int GetWorkId()
        {
            int workerId = 0;
            string maxWorkerId = CacheManager.Get<string>("max_worker_id");
            if (!string.IsNullOrWhiteSpace(maxWorkerId))
            {
                workerId = Convert.ToInt32(maxWorkerId) + 1;
                if (workerId >= 1023)
                {
                    workerId = 0;
                }
            }

            CacheManager.Set("max_worker_id", workerId.ToString());

            return workerId;
        }
    }
}
