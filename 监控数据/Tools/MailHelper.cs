using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsDemo
{
    //public static class MHelper
    //{
    //    /// <summary>
    //    /// 发送邮件
    //    /// </summary>
    //    /// <param name="ToMail">收件人邮箱，邮箱后面加,(支持多个邮箱)</param>
    //    /// <param name="Content">邮件内容</param>
    //    /// <param name="Port">阿里云：默认值25</param>
    //    /// <param name="FromName">发件人：信息中心（默认值）</param>
    //    /// <param name="Title">邮件标题：您有新留言！（默认值）</param>
    //    /// <param name="EmailCode">发件人：（默认值）</param>
    //    /// <param name="EmailPassword">发件人邮箱密码：********（默认值）</param>
    //    /// <param name="Server">服务器，阿里云企业邮箱smtp.mxhichina.com(默认值)；@163.com：smtp.163.com;网易企业邮箱：smtp.ym.163.com（默认值）</param>
    //    /// <param name="Attachment">附件</param>
    //    /// <returns>发送成功True,发送失败false</returns>
    //    public static bool SendMail(string ToMail, string Content, int Port, string FromName, string Title, string EmailCode, string EmailPassword, string Server, string Attachment)
    //    {
    //        try
    //        {
    //            //string strMailSmtp = System.Configuration.ConfigurationSettings.AppSettings["mailSmtp"].ToString();
    //            //string strMailAccount = System.Configuration.ConfigurationSettings.AppSettings["mailAccount"].ToString();
    //            //string strMailPwd = System.Configuration.ConfigurationSettings.AppSettings["mailPwd"].ToString();
    //            string strMailSmtp = "smtp.mxhichina.com";
    //            string strMailAccount = "shengquan.jiang@adicon.com.cn";
    //            string strMailPwd = "Jsq?921015.";

    //            FromName = FromName == "" ? "信息中心" : FromName;
    //            Title = Title == "" ? "提醒：您有新留言！" : Title;
    //            EmailCode = EmailCode == "" ? strMailAccount : EmailCode;
    //            EmailPassword = EmailPassword == "" ? strMailPwd : EmailPassword;
    //            Server = Server == "" ? strMailSmtp : Server;
    //            Port = Port == 0 ? 25 : Port;
    //            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
    //            for (int i = 0; i < ToMail.Split(',').Length - 1; i++)
    //            {
    //                msg.To.Add(ToMail.Split(',')[i].ToString());
    //            }
    //            msg.From = new System.Net.Mail.MailAddress(EmailCode, FromName, System.Text.Encoding.UTF8);
    //            /* 上面3个参数分别是发件人地址（可以随便写），发件人姓名，编码*/
    //            msg.Subject = Title;//邮件标题
    //            msg.SubjectEncoding = System.Text.Encoding.UTF8;//邮件标题编码
    //            msg.Body = Content;//邮件内容
    //            msg.BodyEncoding = System.Text.Encoding.UTF8;//邮件内容编码
    //            if (Attachment.Length > 0) { msg.Attachments.Add(new System.Net.Mail.Attachment(Attachment)); }//添加附件
    //            msg.IsBodyHtml = true;//是否是HTML邮件
    //            msg.Priority = System.Net.Mail.MailPriority.Normal;//邮件优先级
    //            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
    //            client.Credentials = new System.Net.NetworkCredential(EmailCode, EmailPassword);
    //            //填写你的GMail邮箱和密码
    //            client.Port = Port;//163企业邮箱的端口25
    //            client.Host = Server;//smtp.ym.163.com
    //            client.EnableSsl = false;//Ssl加密
    //            object userState = msg;
    //            client.SendAsync(msg, userState);
    //            return true;
    //        }
    //        catch { return false; }
    //    }

    //}

    public class MailHelper
    {
        private string[] seperator = { ",", ";" };

        //创建一个邮件服务器类
        SmtpClient myclient = new SmtpClient();
        public MailHelper(string user, string pwd, string host = "smtp.adicon.com.cn", int port = 25)
        {
            myclient.Host = host;
            //SMTP服务端口
            myclient.Port = port;
            //验证登录
            //myclient.Credentials = new NetworkCredential(user, CryptoDES.Decrypt(pwd));//"@"输入有效的邮件名, "*"输入有效的密码
            myclient.Credentials = new NetworkCredential(user, pwd);//"@"输入有效的邮件名, "*"输入有效的密码
                                                                    //myclient.UseDefaultCredentials = true;
        }
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="address">自己的邮箱</param>
        /// <param name="to">收件人邮箱</param>
        /// <param name="cc">抄送人邮箱</param>
        /// <param name="title">邮件主题</param>
        /// <param name="content">邮件内容</param>
        /// <param name="err">错误信息</param>
        /// <param name="files">附件</param>
        /// <returns></returns>
        public bool Send(string address, string to, string cc, string title, string content, out string err, List<MailInfo> files = null)
        {
            err = null;
            try
            {
                //if (string.IsNullOrWhiteSpace(to)) throw new Exception(Words.WORDS_UNKNOWN_EMAIL_RECEIVER);
                //声明一个Mail对象
                MailMessage mymail = new MailMessage();
                //发件人地址
                //如是自己，在此输入自己的邮箱
                mymail.From = new MailAddress(address, "对接邮件解析异常", Encoding.UTF8);
                //收件人地址
                foreach (var toItem in to.Split(this.seperator, StringSplitOptions.RemoveEmptyEntries))
                {
                    mymail.To.Add(new MailAddress(toItem));
                }
                //邮件主题
                mymail.Subject = title;
                //邮件标题编码
                mymail.SubjectEncoding = Encoding.UTF8;
                //发送邮件的内容
                mymail.Body = content;
                //邮件内容编码
                mymail.BodyEncoding = Encoding.UTF8;
                if (files != null)
                    //添加附件
                    foreach (var file in files)
                    {

                        Attachment attachment = new Attachment(ReturnStream(file.File), file.MailName);
                        mymail.Attachments.Add(attachment);
                    }
                if (!string.IsNullOrWhiteSpace(cc))
                {
                    //抄送到其他邮箱
                    foreach (var str in cc.Split(this.seperator, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mymail.CC.Add(new MailAddress(str));
                    }
                }

                //是否是HTML邮件
                mymail.IsBodyHtml = true;
                //邮件优先级
                mymail.Priority = MailPriority.High;

                try
                {
                    myclient.Send(mymail);
                    err = null;
                    return true;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }

        //把byte[]转成stream
        public System.IO.MemoryStream ReturnStream(byte[] streamByte)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(streamByte);
            //System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            return ms;
        }
        //public bool SendMail(MailSender sender, out string errorMsg)
        //{
        //    //声明一个Mail对象
        //    MailMessage mymail = new MailMessage();
        //    //发件人地址
        //    //如是自己，在此输入自己的邮箱
        //    mymail.From = new MailAddress(sender.SendAddress, sender.SendNickName, Encoding.UTF8);
        //    //收件人地址

        //    mymail.To.Add(new MailAddress(sender.ReceiveAddress));
        //    //邮件主题
        //    mymail.Subject = sender.Title;
        //    //邮件标题编码
        //    mymail.SubjectEncoding = Encoding.UTF8;
        //    //发送邮件的内容
        //    mymail.Body = sender.Content;
        //    //邮件内容编码
        //    mymail.BodyEncoding = Encoding.UTF8;
        //    //添加附件
        //    foreach (var attachment in sender.Attachments)
        //    {
        //        mymail.Attachments.Add(attachment);
        //    }

        //    //抄送到其他邮箱
        //    foreach (var str in sender.Cc)
        //    {
        //        mymail.CC.Add(new MailAddress(str));
        //    }

        //    //是否是HTML邮件
        //    mymail.IsBodyHtml = sender.IsBodyHtml;
        //    //邮件优先级
        //    mymail.Priority = sender.MailPriority;
        //    //创建一个邮件服务器类
        //    SmtpClient myclient = new SmtpClient();
        //    myclient.Host = sender.Host;
        //    //SMTP服务端口
        //    myclient.Port = sender.Port;
        //    //验证登录
        //    myclient.Credentials = new NetworkCredential(sender.Username, sender.Password);//"@"输入有效的邮件名, "*"输入有效的密码
        //    try
        //    {
        //        myclient.Send(mymail);
        //        errorMsg = "";
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMsg = ex.Message;
        //        return false;
        //    }

        //}
    }
    public class MailInfo
    {
        /// <summary>
        /// 附件名
        /// </summary>
        public string MailName { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public byte[] File { get; set; }
    }
    public class MailSender
    {
        /// <summary>
        /// 发件人地址
        /// </summary>
        public string SendAddress { set; get; }
        /// <summary>
        /// 发件人别名
        /// </summary>
        public string SendNickName { set; get; }
        /// <summary>
        /// 收件人地址
        /// </summary>
        public List<string> ReceiveAddress { set; get; }
        /// <summary>
        /// 邮件服务器
        /// </summary>
        public string Host { set; get; }
        /// <summary>
        /// 邮件端口
        /// </summary>
        public int Port = 25;
        /// <summary>
        /// 发件人用户名
        /// </summary>
        public string Username { set; get; }
        /// <summary>
        /// 发件人密码
        /// </summary>
        public string Password { set; get; }
        /// <summary>
        /// 邮件标题
        /// </summary>
        public string Title { set; get; }
        /// <summary>
        /// 邮件正文
        /// </summary>
        public string Content { set; get; }
        /// <summary>
        /// 是否是Html格式
        /// </summary>
        public bool IsBodyHtml { set; get; } = false;
        /// <summary>
        /// 抄送人列表
        /// </summary>
        public List<string> Cc { set; get; }
        /// <summary>
        /// 附件列表
        /// </summary>
        public List<Attachment> Attachments { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public MailPriority MailPriority { set; get; } = MailPriority.Normal;
    }
}
