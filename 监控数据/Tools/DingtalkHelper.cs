using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace 监控数据
{
    public class DingtalkHelper
    {
        #region 调用钉钉接口

        /// <summary>
        /// 调用钉钉接口
        /// </summary>
        /// <param name="msg">要发送的消息</param>
        /// <param name="phone">手机号</param>
        /// <param name="atALL">是否@全员，1是，0否</param>
        /// <param name="WEB_HOOK">钉钉机器人WEB_HOOK</param>
        /// <param name="secret">钉钉机器人加签秘钥</param>
        /// <returns></returns>
        public string SendDingtalk(string msg, string phone,string atALL,string WEB_HOOK,string secret)
        {
            string textMsg;
            try
            {
                if (phone.Length == 11)
                {
                    textMsg = "{ \"msgtype\": \"text\", \"text\": {\"content\": \"" + msg + "\"},\"at\": {\"atMobiles\": [\"" + phone + "\"],\"isAtAll\": false}}";

                }
                else if (atALL == "1")
                {
                    textMsg = "{ \"msgtype\": \"text\", \"text\": {\"content\": \"" + msg + "\"},\"at\": {\"isAtAll\": true}}";
                }
                else
                {
                    textMsg = "{ \"msgtype\": \"text\", \"text\": {\"content\": \"" + msg + "\"}}";
                }
                //MessageBox.Show(textMsg);

                //msg = "研发管理-编译管理：编译服务器编译海客项目";
                //String textMsg = "{ \"msgtype\": \"link\", \"link\": {\"text\": \"" + msg + "\",\"title\": \" 8001-自动线综合管控系统\", \"picUrl\":\"\", \"messageUrl\": \"http://www.iotgate.com.cn/biz/task-view-430.html\"}}";

                /* msg = "#### 杭州天气 @齐阿荣(齐阿荣)\n" +
                  "> 9度，西北风1级，空气良89，相对温度73%\n\n" +
                  "> ![screenshot](https://gw.alipayobjects.com/zos/skylark-tools/public/files/84111bbeba74743d2771ed4f062d1f25.png)\n" +
                  "> ###### 10点20分发布 [天气](https://www.seniverse.com/) \n";
                String textMsg = "{ \"msgtype\": \"markdown\", \"markdown\": {\"title\": \"杭州天气\",\"text\":\"" + msg + "\"},\"at\": {\"atMobiles\": [\"18042697096\"],\"isAtAll\": false}}";
                 */

                //获取毫秒时间戳
                TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                long shijianchuo = ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000);


                //获取签名值
                string sign = addSign(shijianchuo,secret);
                string url = "&timestamp=" + shijianchuo + "&sign=" + sign;
                string s = Post(WEB_HOOK + url, textMsg, null);
                Log.LogMeg("SendDingtalk--调用返回：", s, ".\\log\\");
                return s;
            }
            catch (Exception ex)
            {
                //将错误信息写入系统日志
                string info = "调用钉钉接口失败：" + ex.Message;
                SqlHelper.WriteLog(info);
                //throw new Exception(info);
                return "";
            }

        }
        #endregion

        #region Post方法
        /// <summary>
        /// 以Post方式提交命令
        /// </summary>
        /// <param name="apiurl">请求的URL</param>
        /// <param name="jsonString">请求的json参数</param>
        /// <param name="headers">请求头的key-value字典</param>
        public static String Post(string apiurl, string jsonString, Dictionary<String, String> headers)
        {
            WebRequest request = WebRequest.Create(@apiurl);//初始化新的WebRequest
            request.Method = "POST";//设置要在此请求中使用的协议方法
            request.ContentType = "application/json";//设置所发送数据的内容类型
            if (headers != null)
            {
                foreach (var keyValue in headers)
                {
                    if (keyValue.Key == "Content-Type")
                    {
                        request.ContentType = keyValue.Value;
                        continue;
                    }
                    request.Headers.Add(keyValue.Key, keyValue.Value);//设置与请求关联的报文名称/值对的集合
                }
            }

            if (String.IsNullOrEmpty(jsonString))
            {
                request.ContentLength = 0;
            }
            else
            {
                byte[] bs = Encoding.UTF8.GetBytes(jsonString);
                request.ContentLength = bs.Length;//ContentLength设置所发送的请求数据的内容长度
                Stream newStream = request.GetRequestStream();//GetRequestStream:返回用于将数据写入Internet资源的Stream
                newStream.Write(bs, 0, bs.Length);
                newStream.Close();
            }


            WebResponse response = request.GetResponse();//返回对Internet请求的响应
            Stream stream = response.GetResponseStream();//从Internet资源返回数据流
            Encoding encode = Encoding.UTF8;
            StreamReader reader = new StreamReader(stream, encode);
            string resultJson = reader.ReadToEnd();
            return resultJson;
        }
        #endregion

        /// <summary>
        /// 加签
        /// </summary>
        /// <param name="zTime">当前时间戳</param>
        /// <returns></returns>
        private string addSign(long zTime,string secret)
        {
            //string secret = "SEC2cd71daa7dbd4b101a67e9713b88cab83fc7c158213101302ba015870439e382"; //加签密钥
            string stringToSign;
            byte[] keyByte;
            var encoding = new System.Text.ASCIIEncoding();
         
            stringToSign = zTime + "\n" + secret;
            keyByte = encoding.GetBytes(secret);      

            byte[] messageBytes = encoding.GetBytes(stringToSign);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return HttpUtility.UrlEncode(Convert.ToBase64String(hashmessage), Encoding.UTF8);
            }
        }
    }
}
