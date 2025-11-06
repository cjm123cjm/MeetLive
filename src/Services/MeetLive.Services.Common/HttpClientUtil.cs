using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MeetLive.Services.Common
{
    /// <summary>
    /// Http 请求接口工具类
    /// </summary>
    public static class HttpClientUtil
    {
        /// <summary>
        /// HttpPost请求
        /// </summary>
        /// <param name="url">请求接口的完整地址</param>
        /// <param name="postData">请求参数</param>
        /// <returns></returns>
        public static string Post(string url, object? postData = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = true;
            request.Method = "POST";
            request.ContentType = "application/json";

            if (postData != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postData));
                request.ContentLength = data.Length;
                using Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Flush();
                stream.Close();
            }

            using HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
            using Stream st = resp.GetResponseStream();
            using StreamReader sr = new StreamReader(st, Encoding.UTF8);
            var result = sr.ReadToEnd();

            return result;
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url">请求接口的完整地址</param>
        /// <returns></returns>
        public static string Get(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
            using Stream st = resp.GetResponseStream();
            using StreamReader sr = new StreamReader(st, Encoding.UTF8);
            string result = sr.ReadToEnd();

            return result;
        }
    }
}
