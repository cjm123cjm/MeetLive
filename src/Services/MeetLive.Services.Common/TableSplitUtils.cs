using System.Text;

namespace MeetLive.Services.Common
{
    public class TableSplitUtils
    {
        //分表名称
        private static string SPLIT_TABLE_MEETING_CHAT_MESSAGE = "MeetingChatMessages";
        //sql
        private static string CREATE_TABLE_TEMP = "CREATE TABLE IF NOT EXISTS {0} LIKE {1};";
        //分多少张表
        private static int SPLIT_TABLE_COUNT = 32;

        /// <summary>
        /// 生成分表的sql语句
        /// </summary>
        /// <param name="templateTableName"></param>
        /// <param name="tableIndex"></param>
        /// <param name="tableCount"></param>
        /// <returns></returns>
        public static string GetCreateTableSql(string templateTableName, int tableIndex, int tableCount)
        {
            int padLen = tableCount.ToString().Length;

            string tableName = templateTableName + "_" + tableIndex.ToString().PadLeft(padLen, '0');

            return string.Format(CREATE_TABLE_TEMP, tableName, templateTableName);
        }

        /// <summary>
        /// 获取数据在哪张表里
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="tableCount"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetTableName(string prefix, int tableCount, string key)
        {
            byte[] data = Encoding.UTF8.GetBytes(key);
            long tableIndex = (Hash32(data) % tableCount) + 1;

            var length = tableCount.ToString().Length;

            return prefix + "_" + tableIndex.ToString().PadLeft(length, '0');
        }


        /// <summary>
        /// 获取MeetingChatMessage表
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public static string GetMeetingChatMessageTable(string meetingId)
        {
            return GetTableName(SPLIT_TABLE_MEETING_CHAT_MESSAGE, SPLIT_TABLE_COUNT, meetingId);
        }

        /// <summary>
        /// 打印sql分表语句
        /// </summary>
        public static void GetSplitTableSql()
        {
            for (int i = 1; i <= SPLIT_TABLE_COUNT; i++)
            {
                Console.WriteLine(GetCreateTableSql(SPLIT_TABLE_MEETING_CHAT_MESSAGE, i, SPLIT_TABLE_COUNT));
            }
        }

        /// <summary>
        /// MurmurHash2 32位实现（字节数组版本）
        /// </summary>
        public static uint Hash32(byte[] data, uint seed = 0x9747b28c)
        {
            const uint m = 0x5bd1e995;
            const int r = 24;

            int length = data.Length;
            uint h = seed ^ (uint)length;
            int len_4 = length >> 2;

            // 处理每4个字节为一组
            for (int i = 0; i < len_4; i++)
            {
                int i_4 = i << 2;

                // 构建32位整数（注意字节顺序：小端）
                uint k = (uint)((data[i_4 + 0] & 0xff) |
                               ((data[i_4 + 1] & 0xff) << 8) |
                               ((data[i_4 + 2] & 0xff) << 16) |
                               ((data[i_4 + 3] & 0xff) << 24));

                k *= m;
                k ^= k >> r; // 注意：C# 中 >> 是有符号右移，>>> 在 C# 中不存在
                k *= m;
                h *= m;
                h ^= k;
            }

            // 处理剩余的字节
            int len_m = len_4 << 2;
            int left = length - len_m;

            if (left != 0)
            {
                if (left >= 3)
                    h ^= (uint)((data[length - 3] & 0xff) << 16);
                if (left >= 2)
                    h ^= (uint)((data[length - 2] & 0xff) << 8);
                if (left >= 1)
                    h ^= (uint)(data[length - 1] & 0xff);

                h *= m;
            }

            // 最后处理
            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }
}
