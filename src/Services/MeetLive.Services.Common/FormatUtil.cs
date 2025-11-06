using System.Text;
using System.Text.RegularExpressions;

namespace MeetLive.Services.Common
{
    /// <summary>
    /// 正则格式工具类
    /// </summary>
    public class FormatUtil
    {
        /// <summary>
        /// 控制全国隐藏证件号（保留前后4位）
        /// </summary>
        /// <param name="outIdNumber"></param>
        /// <returns></returns>
        public static string FormatOutIdNumber(string outIdNumber)
        {
            if (!string.IsNullOrWhiteSpace(outIdNumber) && outIdNumber.Length > 8)
            {
                int count = outIdNumber.Length - 8;
                string left = outIdNumber.Substring(0, 4);
                string right = outIdNumber.Substring(outIdNumber.Length - 4, 4);
                outIdNumber = left;
                for (int i = 0; i < count; i++)
                {
                    outIdNumber += "*";
                }
                outIdNumber += right;
            }
            return outIdNumber;
        }

        /// <summary>
        /// 隐藏手机号（隐藏中间4位）
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return "";
            }
            var contact = phoneNumber.Replace(" ", "");
            //
            var phone = System.Text.RegularExpressions.Regex.Replace(contact, @"[^\d]*", "");
            #region 固话

            //先过滤特殊字符（#*-括号等）
            //string contactTmp = contact.Replace("#", "").Replace("*", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("（", "").Replace("）", "");
            //判断正则表达式
            var IsTel = System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\(?0(\d{2,3}\)?-?)?\d{7,8}$");
            if (IsTel && phone.Length >= 10 && phone.Length <= 12)
            {
                return TxtReplace(phoneNumber, 4, '*');
            }
            #endregion
            #region 手机
            var isPhone = System.Text.RegularExpressions.Regex.IsMatch(phone, @"^1([3456789][0-9]|4[579]|66|7[0135678]|9[89])[0-9]{8}$");
            if (isPhone)
            {
                return TxtReplace(phoneNumber, 4, '*');
            }
            #endregion

            return phoneNumber;
        }


        /// <summary>
        /// 文本替换
        /// </summary>
        /// <param name="txt">数据文本</param>
        /// <param name="len">脱敏长度</param>
        /// <param name="newChar">脱敏字符</param>
        /// <returns></returns>
        public static string TxtReplace(string txt, int len, char newChar)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return string.Empty;
            }
            int count = txt.Length > len ? len : txt.Length;
            var encodeTxt = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                encodeTxt.Append(newChar);
            }
            if (len >= txt.Length)
            {
                return encodeTxt.ToString();
            }
            int leftLength = (txt.Length - len) / 2;
            var result = new StringBuilder(txt.Substring(0, leftLength));
            result.Append(encodeTxt);
            result.Append(txt.Substring(leftLength + len));
            return result.ToString();
        }

        /// <summary>
        /// 文本替换
        /// </summary>
        /// <param name="txt">数据文本</param>
        /// <param name="len">脱敏长度</param>
        /// <param name="newChar">脱敏字符</param>
        /// <returns></returns>
        public static string QueryTxtReplaceStr(string txt, int len, char newChar)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return string.Empty;
            }
            int count = txt.Length > len ? len : txt.Length;
            var encodeTxt = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                encodeTxt.Append(newChar);
            }
            if (len >= txt.Length)
            {
                return encodeTxt.ToString();
            }
            int leftLength = (txt.Length - len) / 2;
            var result = new StringBuilder(txt.Substring(leftLength, len));
            //result.Append(encodeTxt);
            //result.Append(txt.Substring(leftLength + len));
            return result.ToString();
        }


        /// <summary>
        /// 文本替换
        /// </summary>
        /// <param name="txt">数据文本</param>
        /// <param name="startIndex">脱敏数据起始索引（含当前字符）</param>
        /// <param name="len">脱敏长度</param>
        /// <param name="newChar">脱敏字符</param>
        /// <returns></returns>
        public static string TxtReplace(string txt, int startIndex, int len, char newChar)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return string.Empty;
            }
            int txtLength = txt.Length;
            if (txtLength < startIndex + 1)
            {
                return txt;
            }
            StringBuilder result = new StringBuilder();
            result.Append(txt.Substring(0, startIndex));
            if (txtLength <= startIndex + len)
            {
                int replaceCount = txtLength - startIndex;
                for (var i = 1; i <= replaceCount; i++)
                {
                    result.Append(newChar);
                }
                return result.ToString();
            }
            for (var i = 1; i <= len; i++)
            {
                result.Append(newChar);
            }
            result.Append(txt.Substring(startIndex + len));
            return result.ToString();
        }



        /// <summary>
        /// 校验固话
        /// </summary>
        /// <param name="tel">固话</param>
        /// <returns></returns>
        public static bool IsTel(string tel)
        {
            return RegexTel(tel);
        }
        /// <summary>
        /// 校验手机号码
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <returns></returns>
        public static bool IsPhone(string phone)
        {
            //连续11位数字，若符合手机正则（第一位为1，第二位为3~9，后九位为0~9），则取所有数字为手机号码；
            if (phone.Length == 11)
            {
                return RegexPhone(phone);
            }
            //连续12位数字，若第一位为0，后11位符合手机正则，则取后11位数字为手机号码；
            else if (phone.Length == 12 && phone.StartsWith("0"))
            {
                return RegexPhone(phone.Substring(1));
            }
            //连续13位数字，若前两位为86，后11位符合手机正则，则取后11位数字为手机号码；
            else if (phone.Length == 13 && phone.StartsWith("86"))
            {
                return RegexPhone(phone.Substring(2));
            }
            return false;
        }

        /// <summary>
        /// 验证Email地址
        /// </summary>
        /// <param name="emailText">Email地址</param>
        /// <returns></returns>
        public static bool IsEmail(string emailText)
        {
            return Regex.IsMatch(emailText, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }



        /// <summary>
        /// 正则匹配手机号码
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        private static bool RegexPhone(string phone)
        {
            return Regex.IsMatch(phone, "1[3456789]\\d{9}");
        }
        /// <summary>
        /// 正则匹配电话号码
        /// </summary>
        /// <param name="tel"></param>
        /// <returns></returns>
        private static bool RegexTel(string tel)
        {
            string telContentStr = tel.Replace("-", "").Replace(" ", "");
            if (telContentStr.Length >= 7 && telContentStr.Length <= 9 && telContentStr.StartsWith("0"))
            {
                return false;
            }
            if ((telContentStr.StartsWith("400") || telContentStr.StartsWith("800")) && telContentStr.Length == 10)
            {
                return false;
            }
            var isTel = System.Text.RegularExpressions.Regex.IsMatch(tel, "(0[1-9][0-9]{1,2})?([2-9][0-9]{6,7})");
            if (isTel)
            {
                return true;
            }

            //if (telContentStr.Length >= 7 && telContentStr.Length <= 12 && !RegexPhone(telContentStr))
            //{
            //    return true;
            //}
            return false;
        }
    }
}
