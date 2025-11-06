using System.Net.Mail;

namespace MeetLive.Services.Common
{
    /// <summary>
    /// 邮件发送工具类
    /// </summary>
    public static class EmailUtil
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="strMailText">邮件内容</param>
        /// <param name="strTitle">标题</param>
        /// <param name="to">收件人(多人用;隔开)</param>
        /// <param name="cc">抄送(多人用;隔开)</param>
        /// <param name="bcc">密抄(多人用;隔开)</param>
        /// <returns></returns>
        public static string NetSendEmail(string strMailText, string strTitle, string to, string cc = "", string bcc = "")
        {
            try
            {
                List<string> toList = to.Split(';').ToList();
                List<string> ccList = cc.Split(';').ToList();
                List<string> bccList = bcc.Split(';').ToList();
                return NetSendEmail(strMailText, strTitle, toList, ccList, bccList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="strMailText"></param>
        /// <param name="strTitle"></param>
        /// <param name="to">收件人</param>
        /// <param name="cc">抄送</param>
        /// <param name="bcc">密抄</param>
        /// <returns></returns>
        public static string NetSendEmail(string strMailText, string strTitle, List<string> to, List<string> cc, List<string> bcc = null, string path = "")
        {
            try
            {
                System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
                mailMessage.Subject = strTitle;
                mailMessage.Body = strMailText;
                mailMessage.From = new System.Net.Mail.MailAddress("2671433141@qq.com"); // 填写你自己需要设置的发件人邮箱
                if (to == null || to.Count == 0)
                {
                    return "请填写收件人";
                }
                foreach (string semail in to)
                {
                    if (!string.IsNullOrEmpty(semail))
                    {
                        mailMessage.To.Add(semail);
                    }

                }
                foreach (string semail in cc)
                {
                    if (!string.IsNullOrEmpty(semail))
                    {
                        mailMessage.CC.Add(semail);
                    }
                }
                if (bcc != null)
                {
                    foreach (string semail in bcc)
                    {
                        if (!string.IsNullOrEmpty(semail))
                        {
                            mailMessage.Bcc.Add(semail);
                        }
                    }
                }
                mailMessage.Priority = System.Net.Mail.MailPriority.Normal;
                mailMessage.IsBodyHtml = true;
                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient();
                smtpClient.Host = "smtp.qq.com";
                smtpClient.Port = 587; // 或者使用 465
                smtpClient.EnableSsl = true; // 必须启用SSL
                smtpClient.UseDefaultCredentials = false;

                // 填写你自己需要设置的发件人邮箱, 以及授权码
                smtpClient.Credentials = new System.Net.NetworkCredential("2671433141@qq.com", "ilycffjqjlxldhia");

                if (!string.IsNullOrEmpty(path))
                {
                    mailMessage.Attachments.Add(new Attachment(path));
                }

                smtpClient.Send(mailMessage);
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
