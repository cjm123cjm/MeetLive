using System.Security.Cryptography;
using System.Text;

namespace MeetLive.Services.Common
{
    public class MD5Util
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var strResult = BitConverter.ToString(result).Replace("-", "");

                return strResult;
            }
        }
    }
}
