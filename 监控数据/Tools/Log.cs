/****************************************************************************
 * Copyright (c) 2021 All Rights Reserved.
 * CLR版本： 4.0.30319.42000
 *机器名称：LAPTOP-UVIMPB7R
 *公司名称：
 *命名空间：监控数据
 *文件名：  Log
 *版本号：  V1.0.0.0
 *唯一标识：e893ac04-d7d7-4b82-9995-3ac5be5fd92f
 *当前的用户域：LAPTOP-UVIMPB7R
 *创建人：  adiministrator
 *电子邮箱：602678264@qq.com
 *创建时间：2021/3/19 19:59:15
 
 *描述：
 *
 *=====================================================================
 *修改标记
 *修改时间：2021/3/19 19:59:15
 *修改人： adiministrator
 *版本号： V1.0.0.0
 *描述：
 *
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace 监控数据
{
    public class Log
    {
        public static void LogMeg(string name, string msg, string path)
        {
            //当前的日志文件夹的名字
            string foldeName = DateTime.Now.Year + "-" + DateTime.Now.Month;
            //判断有没有当前日志文件夹的名字
            if (!Directory.Exists(path + "/" + foldeName))
            {
                //如果不存在，创建文件夹
                Directory.CreateDirectory(path + "/" + foldeName);
            }
            //当前文件所处的位置
            string logPath = path + "/" + foldeName;

            //文件的名字（日期)           
            string fileName = DateTime.Now.Month + "-" + DateTime.Now.Day + ".txt";

            FileStream stream = null;
            //判断文件是否存在
            if (File.Exists(logPath + "/" + fileName))
            {
                //打开文件进行操作,在文件末尾再添加一个文件
                stream = File.Open(logPath + "/" + fileName, FileMode.Append);
            }
            else
            {
                //如果不存在，创建文件
                stream = File.Create(logPath + "/" + fileName);
            }
            //然后对文件进行写操作
            StreamWriter writer = new StreamWriter(stream);
            using (writer)
            {
                string log = "[" + DateTime.Now.ToLongTimeString() + "] " + name + " " + msg;
                writer.WriteLine(log);
            }
        }
    }
}
