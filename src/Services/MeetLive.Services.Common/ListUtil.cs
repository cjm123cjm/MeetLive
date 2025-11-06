namespace MeetLive.Services.Common
{
    /// <summary>
    /// 集合工具类
    /// </summary>
    public static class ListUtil
    {
        /// <summary>
        /// 将一个比较大的集合分成若干个小集合
        /// </summary>
        /// <param name="list">即将需要切分的大集合</param>
        /// <param name="size">每个小集合存放的数量</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<List<T>> Partition<T>(List<T> list, int size = 1000)
        {
            List<List<T>> partitions = new();
            int loop = Convert.ToInt32(Math.Ceiling(list.Count / 1.0 / size));
            for (int i = 0; i < loop; i++)
            {
                partitions.Add(list.Skip(i * size).Take(size).ToList());
            }

            return partitions;
        }
    }
}
