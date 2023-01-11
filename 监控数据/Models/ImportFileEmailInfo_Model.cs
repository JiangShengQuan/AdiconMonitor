/****************************************************************************
 * Copyright (c) 2021 All Rights Reserved.
 * CLR版本： 4.0.30319.42000
 *机器名称：LAPTOP-UVIMPB7R
 *公司名称：
 *命名空间：监控数据
 *文件名：  ImportFileEmailInfo_Model
 *版本号：  V1.0.0.0
 *唯一标识：41ac5adf-1361-4b36-b196-787acdca88ac
 *当前的用户域：LAPTOP-UVIMPB7R
 *创建人：  adiministrator
 *电子邮箱：602678264@qq.com
 *创建时间：2021/3/19 13:36:18
 
 *描述：
 *
 *=====================================================================
 *修改标记
 *修改时间：2021/3/19 13:36:18
 *修改人： adiministrator
 *版本号： V1.0.0.0
 *描述：
 *
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 监控数据
{
    public class ImportFileEmailInfo_Model
    {
        public string FID {set;get;}            //编号
        public string Subject {set;get;}        //邮件标题
        public string Mail_from {set;get;}      //发件人
        public string ErrorMessage {set;get;}   //错误信息
        public string FileName {set;get;}       //文件名称
        public string CreateTime {set;get;}     //创建时间
        public string EmailStatus {set;get;}    //解析状态  异常=2
        public string SourceType {set;get;}     
        public string FileContent {set;get;}
    }
}
