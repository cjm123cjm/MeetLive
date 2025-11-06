using System.Drawing;
using System.Drawing.Imaging;

namespace MeetLive.Services.Common.Captcha
{
    /// <summary>
    /// 生成验证码
    /// </summary>
    public static class CreateCaptcha
    {
        /// <summary>
        /// 获取数字验证码
        /// </summary>
        /// <param name="n">验证码数</param>
        /// <returns></returns>
        public static string CreateNumCode(int n)
        {
            char[] numChar = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            string charCode = string.Empty;
            Random random = new Random();
            for (int i = 0; i < n; i++)
            {
                charCode += numChar[random.Next(numChar.Length)];
            }
            return charCode;
        }

        /// <summary>
        /// 获取字符验证码（数字+字母验证码）
        /// </summary>
        /// <param name="n">验证码数</param>
        /// <returns></returns>
        public static string CreateCharCode(int n)
        {
            char[] strChar = { 'a', 'b','c','d','e','f','g','h','i','j','k','l','m',
                'n','o','p','q','r','s','t','u','v','w','x','y','z','0','1','2','3',
                '4','5','6','7','8','9','A','B','C','D','E','F','G','H','I','J','K',
                'L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};

            string charCode = string.Empty;
            Random random = new Random();
            for (int i = 0; i < n; i++)
            {
                charCode += strChar[random.Next(strChar.Length)];
            }
            return charCode;
        }

        /// <summary>
        /// 获取运算符验证码
        /// </summary>
        /// <returns></returns>
        public static string CreateArithCode(out string resultCode)
        {
            string checkCode = "";
            Random random = new Random();
            int intFirst = random.Next(1, 20);//生成第一个数字
            int intSec = random.Next(1, 20);//生成第二个数字
            int intTemp = 0;
            switch (random.Next(1, 3).ToString())
            {
                case "2":
                    if (intFirst < intSec)
                    {
                        intTemp = intFirst;
                        intFirst = intSec;
                        intSec = intTemp;
                    }
                    checkCode = intFirst + "-" + intSec + "=";
                    resultCode = (intFirst - intSec).ToString();
                    break;
                default:
                    checkCode = intFirst + "+" + intSec + "=";
                    resultCode = (intFirst + intSec).ToString();
                    break;
            }
            return checkCode;
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="n">验证码数</param>
        /// <param name="type">类型 0：数字 1：字符</param>
        /// <returns></returns>
        public static VerifyCode CreateVerifyCode(int n, VerifyCodeType type)
        {
            int codeW = 170;//宽度
            int codeH = 50;//高度
            int fontSize = 32;//字体大小
            //初始化验证码
            string charCode = string.Empty;
            string resultCode = "";
            switch (type.ToString())
            {
                case "NUM":
                    charCode = CreateNumCode(n);
                    break;
                case "ARITH":
                    charCode = CreateArithCode(out resultCode);
                    n = charCode.Length;
                    break;
                default:
                    charCode = CreateCharCode(n);
                    break;
            }
            //颜色列表
            Color[] colors = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.DarkBlue };
            //字体列表
            string[] fonts = { "Times New Roman", "Verdana", "Arial", "Gungsuh" };
            //创建画布
            Bitmap bitmap = new Bitmap(codeW, codeH);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            Random random = new Random();
            //画躁线
            for (int i = 0; i < n; i++)
            {
                int x1 = random.Next(codeW);
                int y1 = random.Next(codeH);
                int x2 = random.Next(codeW);
                int y2 = random.Next(codeH);
                Color color = colors[random.Next(colors.Length)];
                Pen pen = new Pen(color);
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }
            //画噪点
            for (int i = 0; i < 100; i++)
            {
                int x = random.Next(codeW);
                int y = random.Next(codeH);
                Color color = colors[random.Next(colors.Length)];
                bitmap.SetPixel(x, y, color);
            }
            //画验证码
            for (int i = 0; i < n; i++)
            {
                string fontStr = fonts[random.Next(fonts.Length)];
                Font font = new Font(fontStr, fontSize);
                Color color = colors[random.Next(colors.Length)];
                graphics.DrawString(charCode[i].ToString(), font, new SolidBrush(color), (float)i * 30 + 5, (float)0);
            }
            //写入内存流
            try
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Jpeg);
                VerifyCode verifyCode = new VerifyCode()
                {
                    Code = type.ToString() == "ARITH" ? resultCode : charCode,
                    Image = stream.ToArray()
                };
                return verifyCode;
            }
            //释放资源
            finally
            {
                graphics.Dispose();
                bitmap.Dispose();
            }
        }
    }
}
