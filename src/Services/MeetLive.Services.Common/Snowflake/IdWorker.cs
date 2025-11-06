using System.Net.NetworkInformation;

namespace MeetLive.Services.Common.Snowflake
{
    public class IdWorker
    {
        /// <summary>
        /// 当前时间戳，你可以替换成你系统上线时的时间戳
        /// </summary>
        public const long Twepoch = 1655824116834L;

        /// <summary>
        /// 机器id所占的位数
        /// </summary>
        const int WorkerIdBits = 10;
        /// <summary>
        /// 序列在id中占的位数
        /// </summary>
        const int SequenceBits = 12;
        /// <summary>
        /// 支持的最大机器id，结果是31 (这个移位算法可以很快的计算出几位二进制数所能表示的最大十进制数)
        /// </summary>
        public const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
        /// <summary>
        /// 机器ID向左移12位
        /// </summary>
        private const int WorkerIdShift = SequenceBits;
        /// <summary>
        /// 时间截向左移22位(10+12)
        /// </summary>
        public const int TimestampLeftShift = WorkerIdShift + WorkerIdBits;
        /// <summary>
        /// 生成序列的掩码，这里为4095 (0b111111111111=0xfff=4095)
        /// </summary>
        private const long SequenceMask = -1L ^ (-1L << SequenceBits);

        private long _sequence = 0L;
        private long _lastTimestamp = -1L;

        ///  <summary>
        /// 同一库表由不同的IdWorker对象生成主键时,需要区分参数
        ///  </summary>
        ///  <param name="workerId"></param>
        ///  <param name="sequence"></param>
        public IdWorker(long workerId, long sequence)
        {
            WorkerId = workerId;
            _sequence = sequence;
            // sanity check for workerId
            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException(
                    $"workerId 不能比最大值 {MaxWorkerId} 大 或者比 0 小");
            }
        }

        public long WorkerId { get; protected set; }

        public long Sequence
        {
            get { return _sequence; }
            internal set { _sequence = value; }
        }

        // def get_timestamp() = System.currentTimeMillis

        readonly object _lock = new();

        public virtual long NextId()
        {
            lock (_lock)
            {
                var timestamp = TimeGen();
                // 如果当前时间小于上一次ID生成的时间戳，说明系统时钟回退过这个时候应当抛出异常
                if (timestamp < _lastTimestamp)
                {
                    //exceptionCounter.incr(1);
                    //log.Error("clock is moving backwards.  Rejecting requests until %d.", _lastTimestamp);
                    throw new Exception(
                        $"系统发生了时钟回拨.  {_lastTimestamp - timestamp} 毫秒后可继续生产ID");
                }
                // 如果是同一时间生成的，则进行毫秒内序列
                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & SequenceMask;
                    // 毫秒内序列溢出
                    if (_sequence == 0)
                    {
                        //阻塞到下一个毫秒,获得新的时间戳
                        timestamp = TilNextMillis(_lastTimestamp);
                        // 你在这里可以换成记录日志的方式
                        Console.WriteLine(
                            $"{nameof(IdWorker)}:{GetLocalMacAddress()}序列号超过限制，重新取时间戳:lastTimestamp=[{_lastTimestamp}] AND timestamp=[{timestamp}]");
                    }
                }
                // 时间戳改变，毫秒内序列重置
                else
                {
                    _sequence = 0;
                }
                // 上次生成ID的时间截
                _lastTimestamp = timestamp;
                // 移位并通过或运算拼到一起组成64位的ID
                var id = ((timestamp - Twepoch) << TimestampLeftShift) |
                         (WorkerId << WorkerIdShift) |
                         _sequence;

                return id;
            }
        }

        /// <summary>
        /// 阻塞到下一个毫秒，直到获得新的时间戳
        /// </summary>
        /// <param name="lastTimestamp">上次生成ID的时间截</param>
        /// <returns></returns>
        protected virtual long TilNextMillis(long lastTimestamp)
        {
            var timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }

            return timestamp;
        }
        /// <summary>
        /// 返回以毫秒为单位的当前时间
        /// </summary>
        /// <returns></returns>
        protected virtual long TimeGen()
        {
            return (long)(DateTime.UtcNow - new DateTime
                (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 获取本机mac(跨平台的方法)
        /// </summary>
        /// <returns></returns>
        private static string GetLocalMacAddress()
        {
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            if (nics == null || nics.Length < 1)
            {
                return string.Empty;
            }


            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                {
                    continue;
                }

                PhysicalAddress address = adapter.GetPhysicalAddress();
                byte[] bytes = address.GetAddressBytes();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("X2"));

                    if (i != bytes.Length - 1)
                    {
                        sb.Append("-");
                    }
                }

                return sb.ToString();
            }

            return string.Empty;
        }
    }
}
