using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace 监控数据.Tools
{
    public class GetEmail
    {
        private string mstrHost = null;  //主机名称或IP地址
        private int mintPort = 110;  //主机的端口号（默认为110）
        private TcpClient mtcpClient = null;  //客户端
        private NetworkStream mnetStream = null;  //网络基础数据流
        private StreamReader m_stmReader = null;  //读取字节流
        private string mstrStatMessage = null;  //执行STAT命令后得到的消息（从中得到邮件数）

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <remarks>一个邮件接收对象</remarks>
        public GetEmail()
        {
            MailTable.Columns.Add("Type", typeof(string));
            MailTable.Columns.Add("Text", typeof(object));
            MailTable.Columns.Add("Name", typeof(string));
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">主机名称或IP地址</param>
        public GetEmail(string host)
        {
            mstrHost = host;
            MailTable.Columns.Add("Type", typeof(string));
            MailTable.Columns.Add("Text", typeof(object));
            MailTable.Columns.Add("Name", typeof(string));
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">主机名称或IP地址</param>
        /// <param name="port">主机的端口号</param>
        /// <remarks>一个邮件接收对象</remarks>
        public GetEmail(string host, int port)
        {
            mstrHost = host;
            mintPort = port;
        }

        #region 属性

        /// <summary>
        /// 主机名称或IP地址
        /// </summary>
        /// <remarks>主机名称或IP地址</remarks>
        public string HostName
        {
            get { return mstrHost; }
            set { mstrHost = value; }
        }

        /// <summary>
        /// 主机的端口号
        /// </summary>
        /// <remarks>主机的端口号</remarks>
        public int Port
        {
            get { return mintPort; }
            set { mintPort = value; }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 向网络访问的基础数据流中写数据（发送命令码）
        /// </summary>
        /// <param name="netStream">可以用于网络访问的基础数据流</param>
        /// <param name="command">命令行</param>
        /// <remarks>向网络访问的基础数据流中写数据（发送命令码）</remarks>
        private void WriteToNetStream(ref NetworkStream netStream, String command)
        {
            string strToSend = command + "\r\n";
            byte[] arrayToSend = System.Text.Encoding.ASCII.GetBytes(strToSend.ToCharArray());
            netStream.Write(arrayToSend, 0, arrayToSend.Length);
        }

        /// <summary>
        /// 检查命令行结果是否正确
        /// </summary>
        /// <param name="message">命令行的执行结果</param>
        /// <param name="check">正确标志</param>
        /// <returns>
        /// 类型：布尔
        /// 内容：true表示没有错误，false为有错误
        /// </returns>
        /// <remarks>检查命令行结果是否有错误</remarks>
        private bool CheckCorrect(string message, string check)
        {
            if (message.IndexOf(check) == -1)
                return false;
            else
                return true;
        }

        /// <summary>
        /// 邮箱中的未读邮件数
        /// </summary>
        /// <param name="message">执行完LIST命令后的结果</param>
        /// <returns>
        /// 类型：整型
        /// 内容：邮箱中的未读邮件数
        /// </returns>
        /// <remarks>邮箱中的未读邮件数</remarks>
        private int GetMailNumber(string message)
        {
            string[] strMessage = message.Split(' ');
            return Int32.Parse(strMessage[1]);
        }


        #endregion

        /// <summary>
        /// 与主机建立连接
        /// </summary>
        /// <returns>
        /// 类型：布尔
        /// 内容：连接结果（true为连接成功，false为连接失败）
        /// </returns>
        /// <remarks>与主机建立连接</remarks>
        public bool Connect()
        {
            if (mstrHost == null)
                throw new Exception("请提供SMTP主机名称或IP地址！");
            if (mintPort == 0)
                throw new Exception("请提供SMTP主机的端口号");
            try
            {
                mtcpClient = new TcpClient(mstrHost, mintPort);
                mnetStream = mtcpClient.GetStream();
                m_stmReader = new StreamReader(mtcpClient.GetStream());

                string strMessage = m_stmReader.ReadLine();
                if (CheckCorrect(strMessage, "+OK") == true)
                    return true;
                else
                    return false;
            }
            catch (SocketException exc)
            {
                MessageBox.Show(exc.ToString());
                return false;
            }
            catch (NullReferenceException exc)
            {

                MessageBox.Show(exc.ToString());
                return false;
            }
        }

        #region Pop3命令

        /// <summary>
        /// 执行Pop3命令，并检查执行的结果
        /// </summary>
        /// <param name="command">Pop3命令行</param>
        /// <returns>
        /// 类型：字符串
        /// 内容：Pop3命令的执行结果
        /// </returns>
        private string ExecuteCommand(string command)
        {
            string strMessage = null;  //执行Pop3命令后返回的消息

            try
            {
                //发送命令
                WriteToNetStream(ref mnetStream, command);

                //读取多行
                if (command.Substring(0, 4).Equals("LIST") || command.Substring(0, 4).Equals("RETR") || command.Substring(0, 4).Equals("UIDL")) //记录STAT后的消息（其中包含邮件数）
                {
                    strMessage = ReadMultiLine();

                    if (command.Equals("LIST")) //记录LIST后的消息（其中包含邮件数）
                        mstrStatMessage = strMessage;
                }
                //读取单行
                else
                    strMessage = m_stmReader.ReadLine();

                //判断执行结果是否正确
                if (CheckCorrect(strMessage, "+OK"))
                    return strMessage;
                else
                    return "Error";
            }
            catch (IOException exc)
            {
                MessageBox.Show(exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// 在Pop3命令中，LIST、RETR和UIDL命令的结果要返回多行，以点号（.）结尾，
        /// 所以如果想得到正确的结果，必须读取多行
        /// </summary>
        /// <returns>
        /// 类型：字符串
        /// 内容：执行Pop3命令后的结果
        /// </returns>
        private string ReadMultiLine()
        {
            string strMessage = m_stmReader.ReadLine();
            string strTemp = null;
            while (strMessage != ".")
            {
                strTemp = strTemp + "\r\n" + strMessage;
                strMessage = m_stmReader.ReadLine();
            }
            return strTemp;
        }

        //USER命令
        private string USER(string user)
        {
            return ExecuteCommand("USER " + user) + "\r\n";
        }

        //PASS命令
        private string PASS(string password)
        {
            return ExecuteCommand("PASS " + password) + "\r\n";
        }

        //LIST命令
        private string LIST()
        {
            return ExecuteCommand("LIST") + "\r\n";
        }

        //UIDL命令
        private string UIDL()
        {
            return ExecuteCommand("UIDL") + "\r\n";
        }

        //NOOP命令
        private string NOOP()
        {
            return ExecuteCommand("NOOP") + "\r\n";
        }

        //STAT命令
        private string STAT()
        {
            return ExecuteCommand("STAT") + "\r\n";
        }

        //RETR命令
        private string RETR(int number)
        {
            return ExecuteCommand("RETR " + number.ToString()) + "\r\n";
        }

        //DELE命令
        private string DELE(int number)
        {
            return ExecuteCommand("DELE " + number.ToString()) + "\r\n";
        }

        //QUIT命令
        private void Quit()
        {
            WriteToNetStream(ref mnetStream, "QUIT");
        }

        /// <summary>
        /// 收取邮件
        /// </summary>
        /// <param name="user">用户名</param>
        /// <param name="password">口令</param>
        /// <returns>
        /// 类型：字符串数组
        /// 内容：解码前的邮件内容
        /// </returns>
        private DataTable ReceiveMail(string user, string password)
        {
            int iMailNumber = 0;  //邮件数

            if (USER(user).Equals("Error"))
                MessageBox.Show("用户名不正确！");
            if (PASS(password).Equals("Error"))
                MessageBox.Show("用户口令不正确！");
            if (STAT().Equals("Error"))
                MessageBox.Show("准备接收邮件时发生错误！");
            if (LIST().Equals("Error"))
                MessageBox.Show("得到邮件列表时发生错误！");

            try
            {
                iMailNumber = GetMailNumber(mstrStatMessage);

                //没有新邮件
                if (iMailNumber == 0)
                    return null;
                else
                {
                    string[] strMailContent = new string[iMailNumber];

                    for (int i = 1; i <= iMailNumber; i++)
                    {
                        //读取邮件内容

                        GetMailText(RETR(i));

                    }
                    for (int i = 1; i <= iMailNumber; i++)
                    {
                        //读取邮件内容
                        DeleteMail(i);
                    }
                    return MailTable;

                }
            }
            catch (Exception exc)
            {

                MessageBox.Show(exc.ToString());
                return null;
            }
        }

        #endregion

        /// <summary> 
        /// 获取文字主体 
        /// </summary> 
        /// <param name="p_Mail"></param> 
        /// <returns></returns> 

        /// <summary> 
        /// 转换文字里的字符集 
        /// </summary> 
        /// <param name="p_Text"></param> 
        /// <returns></returns> 
        public string GetReadText(string p_Text)
        {
            System.Text.RegularExpressions.Regex _Regex = new System.Text.RegularExpressions.Regex(@"(?<=\=\?).*?(\?\=)+");
            System.Text.RegularExpressions.MatchCollection _Collection = _Regex.Matches(p_Text);
            string _Text = p_Text;
            foreach (System.Text.RegularExpressions.Match _Match in _Collection)
            {
                string _Value = "=?" + _Match.Value;
                if (_Value[0] == '=')
                {
                    string[] _BaseData = _Value.Split('?');
                    if (_BaseData.Length == 5)
                    {
                        System.Text.Encoding _Coding = System.Text.Encoding.GetEncoding(_BaseData[1]);
                        _Text = _Text.Replace(_Value, _Coding.GetString(Convert.FromBase64String(_BaseData[3])));
                    }
                }
                else
                {
                }
            }
            return _Text;
        }

        DataTable MailTable = new DataTable();

        public void GetMailText(string p_Mail)
        {
            string _ConvertType = GetTextType(p_Mail, "\r\nContent-Type: ", ";");
            if (_ConvertType.Length == 0)
            {
                _ConvertType = GetTextType(p_Mail, "\r\nContent-Type: ", "\r");
            }
            int _StarIndex = -1;
            int _EndIndex = -1;
            string _ReturnText = "";
            string _Transfer = "";
            string _Boundary = "";
            string _EncodingName = GetTextType(p_Mail, "charset=", "\r\n").Replace("\r\n", "").Replace("\"", "");

            System.Text.Encoding _Encoding = System.Text.Encoding.Default;
            if (_EncodingName != "") _Encoding = System.Text.Encoding.GetEncoding(_EncodingName);
            switch (_ConvertType)
            {
                case "text/html;":
                    _Transfer = GetTextType(p_Mail, "\r\nContent-Transfer-Encoding: ", "\r\n").Trim();
                    _StarIndex = p_Mail.IndexOf("\r\n\r\n");
                    if (_StarIndex != -1) _ReturnText = p_Mail.Substring(_StarIndex, p_Mail.Length - _StarIndex);
                    switch (_Transfer)
                    {
                        case "8bit":

                            break;
                        case "quoted-printable":
                            _ReturnText = DecodeQuotedPrintable(_ReturnText, _Encoding);
                            break;
                        case "base64":
                            _ReturnText = DecodeBase64(_ReturnText, _Encoding);
                            break;
                    }
                    MailTable.Rows.Add(new object[] { "text/html", _ReturnText });
                    break;
                case "text/plain;":
                    _Transfer = GetTextType(p_Mail, "\r\nContent-Transfer-Encoding: ", "\r\n").Trim();
                    _StarIndex = p_Mail.IndexOf("\r\n\r\n");
                    if (_StarIndex != -1) _ReturnText = p_Mail.Substring(_StarIndex, p_Mail.Length - _StarIndex);
                    switch (_Transfer)
                    {
                        case "8bit":

                            break;
                        case "quoted-printable":
                            _ReturnText = DecodeQuotedPrintable(_ReturnText, _Encoding);
                            break;
                        case "base64":
                            _ReturnText = DecodeBase64(_ReturnText, _Encoding);
                            break;
                    }
                    MailTable.Rows.Add(new object[] { "text/plain", _ReturnText });
                    break;
                case "multipart/alternative;":
                    _Boundary = GetTextType(p_Mail, "boundary=\"", "\"").Replace("\"", "");
                    _StarIndex = p_Mail.IndexOf("--" + _Boundary + "\r\n");
                    if (_StarIndex == -1) return;
                    while (true)
                    {
                        _EndIndex = p_Mail.IndexOf("--" + _Boundary, _StarIndex + _Boundary.Length);
                        if (_EndIndex == -1) break;
                        GetMailText(p_Mail.Substring(_StarIndex, _EndIndex - _StarIndex));
                        _StarIndex = _EndIndex;
                    }
                    break;
                case "multipart/mixed;":
                    _Boundary = GetTextType(p_Mail, "boundary=\"", "\"").Replace("\"", "");
                    _StarIndex = p_Mail.IndexOf("--" + _Boundary + "\r\n");
                    if (_StarIndex == -1) return;
                    while (true)
                    {
                        _EndIndex = p_Mail.IndexOf("--" + _Boundary, _StarIndex + _Boundary.Length);
                        if (_EndIndex == -1) break;
                        GetMailText(p_Mail.Substring(_StarIndex, _EndIndex - _StarIndex));
                        _StarIndex = _EndIndex;
                    }
                    break;
                default:
                    if (_ConvertType.IndexOf("application/") == 0)
                    {
                        _StarIndex = p_Mail.IndexOf("\r\n\r\n");
                        if (_StarIndex != -1) _ReturnText = p_Mail.Substring(_StarIndex, p_Mail.Length - _StarIndex);
                        _Transfer = GetTextType(p_Mail, "\r\nContent-Transfer-Encoding: ", "\r\n").Trim();
                        string _Name = GetTextType(p_Mail, "filename=\"", "\"").Replace("\"", "");
                        _Name = GetReadText(_Name);
                        byte[] _FileBytes = new byte[0];
                        switch (_Transfer)
                        {
                            case "base64":
                                _FileBytes = Convert.FromBase64String(_ReturnText);
                                break;
                        }
                        MailTable.Rows.Add(new object[] { "application/octet-stream", _FileBytes, _Name });

                    }
                    break;
            }
        }


        /// <summary> 
        /// 获取类型（正则） 
        /// </summary> 
        /// <param name="p_Mail">原始文字</param> 
        /// <param name="p_TypeText">前文字</param> 
        /// <param name="p_End">结束文字</param> 
        /// <returns>符合的记录</returns> 
        public string GetTextType(string p_Mail, string p_TypeText, string p_End)
        {
            System.Text.RegularExpressions.Regex _Regex = new System.Text.RegularExpressions.Regex(@"(?<=" + p_TypeText + ").*?(" + p_End + ")+");
            System.Text.RegularExpressions.MatchCollection _Collection = _Regex.Matches(p_Mail);
            if (_Collection.Count == 0) return "";
            return _Collection[0].Value;
        }

        /// <summary> 
        /// QuotedPrintable编码接码 
        /// </summary> 
        /// <param name="p_Text">原始文字</param> 
        /// <param name="p_Encoding">编码方式</param> 
        /// <returns>接码后信息</returns> 
        public string DecodeQuotedPrintable(string p_Text, System.Text.Encoding p_Encoding)
        {
            System.IO.MemoryStream _Stream = new System.IO.MemoryStream();
            char[] _CharValue = p_Text.ToCharArray();
            for (int i = 0; i != _CharValue.Length; i++)
            {
                switch (_CharValue[i])
                {
                    case '=':
                        if (_CharValue[i + 1] == '\r' || _CharValue[i + 1] == '\n')
                        {
                            i += 2;
                        }
                        else
                        {
                            try
                            {
                                _Stream.WriteByte(Convert.ToByte(_CharValue[i + 1].ToString() + _CharValue[i + 2].ToString(), 16));
                                i += 2;
                            }
                            catch
                            {
                                _Stream.WriteByte(Convert.ToByte(_CharValue[i]));
                            }
                        }
                        break;
                    default:
                        _Stream.WriteByte(Convert.ToByte(_CharValue[i]));
                        break;
                }
            }
            return p_Encoding.GetString(_Stream.ToArray());
        }

        /// <summary> 
        /// 解码BASE64 
        /// </summary> 
        /// <param name="p_Text"></param> 
        /// <param name="p_Encoding"></param> 
        /// <returns></returns> 
        public string DecodeBase64(string p_Text, System.Text.Encoding p_Encoding)
        {
            if (p_Text.Trim().Length == 0) return "";
            byte[] _ValueBytes = Convert.FromBase64String(p_Text);
            return p_Encoding.GetString(_ValueBytes);
        }




        /// <summary>
        /// 收取邮件    
        /// </summary>
        /// <param name="user">用户名</param>
        /// <param name="password">口令</param>
        /// <returns>
        /// 类型：字符串数组
        /// 内容：解码前的邮件内容
        /// </returns>
        ///<remarks>收取邮箱中的未读邮件</remarks>
        public DataTable Receive(string user, string password)
        {
            try
            {
                return ReceiveMail(user, password);
            }
            catch (Exception exc)
            {

                MessageBox.Show(exc.ToString());
                return null;
            }
        }

        /// <summary>
        /// 断开所有与服务器的会话
        /// </summary>
        /// <remarks>断开所有与服务器的会话</remarks>
        public void DisConnect()
        {
            try
            {
                Quit();
                if (m_stmReader != null)
                    m_stmReader.Close();
                if (mnetStream != null)
                    mnetStream.Close();
                if (mtcpClient != null)
                    mtcpClient.Close();
            }
            catch (SocketException exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

        /// <summary>
        /// 删除邮件
        /// </summary>
        /// <param name="number">邮件号</param>
        public void DeleteMail(int number)
        {
            //删除邮件
            int iMailNumber = number + 1;
            if (DELE(iMailNumber).Equals("Error"))
                MessageBox.Show("删除第" + iMailNumber.ToString() + "时出现错误！");
        }
    }
}
