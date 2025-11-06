using System.Security.Cryptography;
using System.Text;

namespace MeetLive.Services.Common
{
    /// <summary>
    /// 加密/解密工具类
    /// </summary>
    public static class DesEncryptUtil
    {
        /// <summary>
        /// Aes加密
        /// </summary>
        /// <param name="plainText">要加密的明文</param>
        /// <param name="key">Aes生成的公开Key</param>
        /// <param name="iv">Aes生成的IV向量</param>
        /// <returns>从内存流返回加密文。</returns>
        public static string AesEncrypt(string plainText, string key, string iv)
        {
            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = Convert.FromBase64String(iv);

                // 创建一个加密器来执行流转换。 
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // 创建用于加密的流。
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // 将所有数据写入流。
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // 从内存流返回加密的字节。
            return Convert.ToBase64String(encrypted);
        }


        /// <summary>
        /// 密文解密成明文
        /// </summary>
        /// <param name="cipherText">要解密的密文</param>
        /// <param name="key">AES生成的解密KEY（和加密KEY是配套的,或者说是一样的）</param>
        /// <param name="iv">AES生成的解密IV（和加密IV是配套的，或者说是一样的）</param>
        /// <returns></returns>
        public static string AesDecrypt(string cipherText, string key, string iv)
        {
            string plaintext = null;

            // 用指定的键和IV创建一个Aes对象。 
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = Convert.FromBase64String(iv);

                // 创建一个解密器来执行流转换。 
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // 创建用于解密的流。
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // 从解密流中读取解密的字节，并将它们放入字符串中。 
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }


        /// <summary>
        /// RSA公钥加密数据
        /// </summary>
        /// <param name="plaintextData">密文</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static string RsaEncrypt(string plaintextData, string publicKey)
        {
            using var rsa = new RSACryptoServiceProvider();
            // 导入公钥
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
            var maxBlockSize = rsa.KeySize / 8 - 11; //加密块最大长度限制
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintextData);
            // 如果加密文长度小于最大限制，则可以直接返回
            if (plaintextBytes.Length <= maxBlockSize)
            {
                var encrypt = rsa.Encrypt(plaintextBytes, false);
                return Convert.ToBase64String(encrypt);
            }

            using MemoryStream plaintextStream = new MemoryStream(plaintextBytes);
            using MemoryStream cryptStream = new();
            var buffer = new byte[maxBlockSize];
            // readSize 表示实际读取的长度
            var blockSize = plaintextStream.Read(buffer, 0, maxBlockSize);
            while (blockSize > 0)
            {
                var toEncrypt = new byte[blockSize];
                Array.Copy(buffer, 0, toEncrypt, 0, blockSize);
                // 分段加密
                var encrypt = rsa.Encrypt(toEncrypt, false);

                // 将加密后的密文写入流中进行保存
                cryptStream.Write(encrypt, 0, encrypt.Length);

                blockSize = plaintextStream.Read(buffer, 0, maxBlockSize);
            }

            // 最终的密文
            return Convert.ToBase64String(cryptStream.ToArray());
        }


        /// <summary>
        /// RSA私钥解密数据
        /// </summary>
        /// <param name="ciphertext">需要解密的密文</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        public static string RsaDecrypt(string ciphertext, string privateKey)
        {
            var cipherData = Convert.FromBase64String(ciphertext);

            using var rsa = new RSACryptoServiceProvider();
            // 导入私钥密钥
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

            using MemoryStream cipherStream = new(cipherData); // 存放所有密文的流
            using MemoryStream plaintextStream = new(); // 用于存放明文的流

            // 解密块最大长度限制
            var maxBlockSize = rsa.KeySize / 8;
            var buffer = new byte[maxBlockSize];
            var blockSize = cipherStream.Read(buffer, 0, maxBlockSize);
            while (blockSize > 0)
            {
                var plaintextBuffer = new byte[blockSize];
                Array.Copy(buffer, 0, plaintextBuffer, 0, blockSize);

                // 分段解密
                var decrypt = rsa.Decrypt(plaintextBuffer, false);
                plaintextStream.Write(decrypt, 0, decrypt.Length);

                blockSize = cipherStream.Read(buffer, 0, maxBlockSize);
            }

            // 最终的明文
            return Encoding.UTF8.GetString(plaintextStream.ToArray());
        }


        /// <summary>
        /// 32位MD5大写加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Md5Encrypt(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
                                   // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
                pwd = pwd + s[i].ToString("x2");
            }
            return pwd.ToUpper();
        }
    }
}
