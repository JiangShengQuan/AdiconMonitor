using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 监控数据.Models
{
    public class dingRotBotDto
    {
        /// <summary>
        /// 目前只支持text
        /// </summary>
        public string msgtype { get; set; }
        /// <summary>
        /// 消息文本
        /// </summary>
        public dingText text { get; set; }
        /// <summary>
        /// 加密的消息ID
        /// </summary>
        public string msgId { get; set; }
        /// <summary>
        /// 消息的时间戳，单位ms
        /// </summary>
        public string createAt { get; set; }
        /// <summary>
        /// 1-单聊、2-群聊
        /// </summary>
        public string conversationType { get; set; }
        /// <summary>
        /// 加密的会话ID
        /// </summary>
        public string conversationId { get; set; }
        /// <summary>
        /// 会话标题(群聊时才有)
        /// </summary>
        public string conversationTitle { get; set; }
        /// <summary>
        /// 加密的发送者ID
        /// </summary>
        public string senderId { get; set; }
        /// <summary>
        /// 发送者昵称
        /// </summary>
        public string senderNick { get; set; }
        /// <summary>
        /// 发送者当前群的企业corpId(企业内部群有)
        /// </summary>
        public string senderCorpId { get; set; }
        /// <summary>
        /// 发送者在企业内的userid(企业内部群有)
        /// </summary>
        public string senderStaffId { get; set; }
        /// <summary>
        /// 加密的机器人ID
        /// </summary>
        public string chatbotUserId { get; set; }
        /// <summary>
        /// 被@人的信息dingtalkId: 加密的发送者IDstaffId: 发送者在企业内的userid(企业内部群有)
        /// </summary>
        public Array atUsers { get; set; }
  
        public class dingText
        {
            public string content { get; set; }  
        }

        public class dingUser
        {
            public string dingtalkId { get; set; }
            public string staffId { get; set; }
        }
    }
}
