using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using WindowsFormsDemo;
using Email.POP3;
using System.Globalization;
using 监控数据.Models;
using 监控数据.Tools;

namespace 监控数据
{
    public partial class FormMain : Form
    {
        DataTable DT_SqlQuery;
        public FormMain()
        {
            InitializeComponent();
            DT_SqlQuery = sqlitehelper.Query("select id,fname,initialization,query,remark,useflag,WEB_HOOK,secret from SqlQuery");
        }
        
        List<ImportFileEmailInfo_Model> datalist = new List<ImportFileEmailInfo_Model>(); //邮件解析
        List<FileRecieveInfoDetail_Model> hjdatalist = new List<FileRecieveInfoDetail_Model>(); //混检表
        List<FileRecieveInfo_Model> drycdatalist = new List<FileRecieveInfo_Model>(); //LIMS导入异常
        List<FileRecieveInfo_Model> bdwdrlist = new List<FileRecieveInfo_Model>(); //录入组比对未导入

        List<FileRecieveInfoDetail_Model> pdfdatalist = new List<FileRecieveInfoDetail_Model>(); //PDF生成异常

        DingtalkHelper Dingtalk = new DingtalkHelper();

        string WEB_HOOK, weekstr, WEB_HOOK_xg, WEB_HOOK_jc, WEB_HOOK_xgsh, WEB_HOOK_xgds, WEB_HOOK_hpvtct;  //钉钉机器人webhook
        string secret, secret_xg, secret_jc, secret_xgsh, secret_xgds, secret_hpvtct; //钉钉机器人加签密钥
        double setTime; //倒计时
        int min, s;
        string notin; //不提醒的发件人
        string atMobiles,atALL,btx;
        string hjtx; //混检提醒人号码        
        string Mail_from, mailfrom, phone = "0", CreateTime;
        public string serverip, servername, login, password, serveripLISDB, servernameLISDB, loginLISDB, passwordLISDB;
        string cgzh, cgmm, xgzh, xgmm;  //邮件异常回复邮箱账号密码
        string cgzh_jx, cgmm_jx, xgzh_jx, xgmm_jx;  //邮件解析邮箱账号密码        
        string starttime,endtime;
        string sendmsg, sendmsghj;
        string err, sendmail, sendpwd, ErrorMessage, subject;

        SqlCeHelper sqlcehelper = new SqlCeHelper();
        SQLiteHelper sqlitehelper = new SQLiteHelper();

        private void 设置ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormSet form_set = new FormSet();
            form_set.ShowDialog();
        }

        private void 监控配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSQL form_sql = new FormSQL();
            form_sql.ShowDialog();
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            int hour = DateTime.Now.Hour;
            int SetHour = Convert.ToInt32(dateTimePicker2.Value.ToString("HH"));
            if (hour < SetHour)
            {
                starttime = DateTime.Now.Date.AddDays(-1).ToString("yyyy/MM/dd") + " " + dateTimePicker2.Value.ToString("HH:mm:ss");
            }
            else
            {
                starttime = DateTime.Now.Date.ToString("yyyy/MM/dd") + " " + dateTimePicker2.Value.ToString("HH:mm:ss");
            }
            SZ_SampleAddress(starttime);

        }

        /// <summary>
        /// 新冠审核情况
        /// </summary>
        /// <param name="dt"></param>
        private void SZ_SampleAddress(string dt)
        {
            if (Convert.ToInt16(DT_SqlQuery.Select("fname='新冠审核情况'")[0][5]) == 1)
            {
                string sqlxg = DT_SqlQuery.Select("fname='新冠审核情况'")[0][3].ToString();
                string webhook = DT_SqlQuery.Select("fname='新冠审核情况'")[0][6].ToString();
                string secret = DT_SqlQuery.Select("fname='新冠审核情况'")[0][7].ToString();

                List<SqlParameter> list_SZ_SampleAddress = new List<SqlParameter>();
                list_SZ_SampleAddress.Add(new SqlParameter("@starttime", starttime));
                //list_SZ_SampleAddress.Add(new SqlParameter("@endtime", endtime));

                StringBuilder sbBarCode = new StringBuilder();
                List<sample_Model> sample = new List<sample_Model>(); //数据存放

                using (SqlDataReader reader = SqlHelper.GetReader(sqlxg, list_SZ_SampleAddress.ToArray()))
                {
                     if (reader.HasRows)
                     {
                         //Log.LogMeg("reader.HasRows", "有查到数据", ".\\log\\");
                         while (reader.Read())
                         {
                             sample.Add(new sample_Model()
                             {
                                 SampleAddress = reader["xg_type"].ToString(),
                                 srsl = reader["sl"].ToString()
                             });
                         }
                     }
                 }


                if (sample.Count > 0)
                {
                    for (int i = 0; i < sample.Count; i++)
                    {
                        sbBarCode.Append(sample[i].SampleAddress + sample[i].srsl);
                        sbBarCode.Append("\n");
                    }

                    Dingtalk.SendDingtalk(sbBarCode.ToString(), "", "0", webhook, secret);
                    sbBarCode.Clear();
                }
                else
                {
                    Log.LogMeg("---新冠标本情况---", starttime + " 至 " + endtime + " 无异常！", ".\\log\\");
                }
            }
        }

        private void sQL语句配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSQL form_sql = new FormSQL();
            form_sql.ShowDialog();
        }

        //核酸上传异常推送
        private void btn_err_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt16(DT_SqlQuery.Select("fname='核酸上传异常'")[0][5]) == 1)
            {
                string sqlxg = DT_SqlQuery.Select("fname='核酸上传异常'")[0][3].ToString();
                string webhook = DT_SqlQuery.Select("fname='核酸上传异常'")[0][6].ToString();
                string secret = DT_SqlQuery.Select("fname='核酸上传异常'")[0][7].ToString();

                string dtstrarttime = DateTime.Now.AddMinutes(-30).ToString();

                List<SqlParameter> errlist = new List<SqlParameter>();
                errlist.Add(new SqlParameter("@starttime", dtstrarttime));

                List<sample_Model> sample_err = new List<sample_Model>();

                using (SqlDataReader reader = SqlHelper.GetReader(sqlxg, errlist.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            sample_err.Add(new sample_Model()
                            {
                                ybid = reader["barcode"].ToString(),
                                brxm = reader["brxm"].ToString(),
                                CertificatesType = reader["CertificatesType"].ToString(),
                                idnumber = reader["idnumber"].ToString(),
                                bgrq = reader["ReportTime"].ToString(),
                                beizhu = reader["Remark"].ToString()
                            });
                        }
                    }
                }

                if (sample_err.Count > 0)
                {
                    StringBuilder sbBarCode_err = new StringBuilder();
                    for (int i = 0; i < sample_err.Count; i++)
                    {
                        string ybid = sample_err[i].ybid;
                        string brxm = sample_err[i].brxm;
                        string CertificatesType = sample_err[i].CertificatesType;
                        string idnumber = sample_err[i].idnumber;
                        string bgrq = sample_err[i].ywy;
                        string beizhu = sample_err[i].ywyphone;
                        sbBarCode_err.Append(sample_err[i].ybid + sample_err[i].brxm + " " + sample_err[i].CertificatesType + sample_err[i].idnumber + " " + sample_err[i].bgrq + " " + sample_err[i].beizhu + "\n");
                    }

                    Dingtalk.SendDingtalk(sbBarCode_err.ToString(), "", "0", WEB_HOOK_xgsh, secret_xgsh);
                    sbBarCode_err.Clear();
                }
                else
                {
                    Log.LogMeg("---核酸上传---", starttime + " 至 " + endtime + " 无异常！", ".\\log\\");
                }
            }

        }

        private void btn_hpvtct_Click(object sender, EventArgs e)
        {
            string sql_hpvtct;
            Log.LogMeg("---HPV、TCT阳性提醒---time", starttime + " | " + endtime, ".\\log\\");

            sql_hpvtct = this.sqlitehelper.Query(@"select query from SqlQuery where fname='HPV、TCT阳性提醒'").Rows[0][0].ToString();

            List<SqlParameter> hpvtctlist = new List<SqlParameter>();
            //hpvtctlist.Add(new SqlParameter("@time", decimal.Negate(num)));  //decimal.Negate(a)  取负数

            StringBuilder sbBarCode = new StringBuilder();
            List<CommonModel> sample = new List<CommonModel>(); 

            using (SqlDataReader reader = SqlHelper.GetReader(sql_hpvtct, hpvtctlist.ToArray()))
            {
                if (reader.HasRows)
                {
                    Log.LogMeg("reader.HasRows", "有查到数据", ".\\log\\");
                    while (reader.Read())
                    {
                        sample.Add(new CommonModel()
                        {
                            t0 = reader["mc"].ToString().Trim(),
                            t1 = reader["phone"].ToString(),
                            t2 = reader["ywy"].ToString(),
                            t3 = reader["ybid"].ToString(),
                            t4 = reader["brxm"].ToString().Trim(),
                            t5 = reader["sex"].ToString(),
                            t6 = reader["age"].ToString(),
                            t7 = reader["SjDoctor"].ToString().Trim(),
                            t8 = reader["bgtime"].ToString(),
                            t9 = reader["xmstring"].ToString().Trim(),
                            t10 = reader["xm"].ToString().Trim()
                        });
                    }
                }
            }
            if (sample.Count > 0)
            {
                string mc1 = sample[0].t0;
                string phone1 = sample[0].t1;
                string ywy1 = sample[0].t2;

                sbBarCode.Append("---HPV、TCT阳性提醒---" + "\n");
                Log.LogMeg("---HPV|TCT阳性提醒---1", "记录触发时间： " + DateTime.Now.ToString("HH:mm:ss"), ".\\log\\");
                for (int i = 0; i < sample.Count; i++)
                {
                    string mc2 = sample[i].t0;
                    string phone2 = sample[i].t1;
                    string ywy2 = sample[i].t2;
                    if (mc1 == mc2)
                    {
                        sbBarCode.Append(sample[i].t3 + " "+ sample[i].t4 + "\n"+ "送检医生：" + sample[i].t7 + "\n" +"结果：" + sample[i].t10 + ((i == sample.Count - 1) ? "" : "\n"));
                    }
                    else
                    {
                        sbBarCode.Append("\n" + "客户名称：" + mc1 + "\n");
                        if (!string.IsNullOrEmpty(ywy1))
                        {
                            sbBarCode.Append("代表：" + ywy1);
                        }

                        Dingtalk.SendDingtalk(sbBarCode.ToString(), phone1, "0", WEB_HOOK_hpvtct, secret_hpvtct);                      

                        Log.LogMeg("---HPV|TCT阳性提醒---2", "记录需要发送的内容： " + "\n" + sbBarCode.ToString(), ".\\log\\");

                        mc1 = mc2;
                        ywy1 = ywy2;
                        phone1 = phone2;
                        sbBarCode.Clear();
                        sbBarCode.Append("---HPV、TCT阳性提醒---" + "\n");
                        //sbBarCode.Append(sample[i].ybid + ((i == sample.Count - 1) ? "" : "、"));
                        sbBarCode.Append(sample[i].t3 + " " + sample[i].t4 + "\n" + "送检医生：" + sample[i].t7 + "\n" + "结果：" + sample[i].t10 + ((i == sample.Count - 1) ? "" : "\n"));

                    }
                    if (i == sample.Count - 1)   //最后一条数据的发送，或者只有一条的时候
                    {
                        sbBarCode.Append("\n" + "客户名称：" + mc1 + "\n");
                        if (!string.IsNullOrEmpty(ywy1))
                        {
                            sbBarCode.Append("代表：" + ywy1);
                        }
                      
                        Dingtalk.SendDingtalk(sbBarCode.ToString(), phone1, "0", WEB_HOOK_hpvtct, secret_hpvtct);

                        Log.LogMeg("---HPV|TCT阳性提醒---3", "记录需要发送的内容： " + "\n" + sbBarCode.ToString(), ".\\log\\");
                        sbBarCode.Clear();
                    }
                }
            }
            else
            {
                Log.LogMeg("---HPV|TCT阳性提醒---", starttime + " 至 " + endtime + " 无异常！", ".\\log\\");
            }
        }

        private void PustData()
        {
            string sql_PustData = "";
            sql_PustData = @"
select 
(select '总管数: ' +CAST( COUNT(*) AS CHAR(10)) AS AA
FROM lisdb.dbo.lis_ybxx a JOIN lisdb.dbo.lis_xmcdz b ON a.yqdh=b.yqdh AND a.ybbh=b.ybbh AND a.cdrq=b.cdrq 
LEFT JOIN  lisdb.dbo.xt_yymc_print c ON  SUBSTRING(a.ybid,1,6) =c.dh
WHERE  a.yqdh='pcr' AND b.xmdh='XG-RNA' AND ybzt IN ('p','s' ) AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND a.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120) ) t0
,
(SELECT  '单检管数: ' +CAST(COUNT(*) AS CHAR(10)) AS BB
FROM lisdb.dbo.lis_ybxx b   JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'   AND ybzt IN ('p','s' )
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120)   AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
and NOT EXISTS (SELECT * FROM  lisdb.dbo.FileRecieveInfoDetail WHERE  Barcode= ybid )) t1
,
(SELECT '混检5管数: ' +CAST(COUNT(distinct barcode) AS CHAR(10))AS DJ
FROM  lisdb.dbo.FileRecieveInfoDetail a JOIN lisdb.dbo.lis_ybxx b ON a.Barcode=b.ybid LEFT JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'  and InspectionProject like'%5N1%'  AND ybzt IN ('p','s') AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120)   AND ISNULL(BRXM,'') <> '' 
) t2
,
(SELECT '混检10管数: ' +CAST(COUNT(distinct barcode) AS CHAR(10))AS DD
FROM  lisdb.dbo.FileRecieveInfoDetail a JOIN lisdb.dbo.lis_ybxx b ON a.Barcode=b.ybid LEFT JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'  and InspectionProject like'%10N1%'  AND ybzt IN ('p','s') AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120)   AND ISNULL(BRXM,'') <> '' 
) t3
,
(SELECT  '混检20管数: ' +CAST(COUNT(distinct barcode)  AS CHAR(10))  AS CC
FROM  lisdb.dbo.FileRecieveInfoDetail a JOIN lisdb.dbo.lis_ybxx b ON a.Barcode=b.ybid LEFT JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'  and InspectionProject like'%20N1%' AND ybzt IN ('p','s')  AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120)  AND ISNULL(BRXM,'') <> '' ) t4
 ,
(SELECT  '单检人数: ' +CAST(COUNT(*) AS CHAR(10)) AS tt
FROM lisdb.dbo.lis_ybxx b   JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'   AND ybzt IN ('p','s' )
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120)   AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
and NOT EXISTS (SELECT * FROM  lisdb.dbo.FileRecieveInfoDetail WHERE  Barcode= ybid)
) t5
,
(SELECT  '混检5人数: ' +CAST(COUNT(barcode)AS CHAR(10)) AS GJ
FROM  lisdb.dbo.FileRecieveInfoDetail a JOIN lisdb.dbo.lis_ybxx b ON a.Barcode=b.ybid LEFT JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'  and InspectionProject like'%5N1%'  AND ybzt IN ('p','s')  AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120 ) AND ISNULL(BRXM,'') <> ''
)t6
, 
(SELECT  '混检10人数: ' +CAST(COUNT(barcode)AS CHAR(10)) AS GG
FROM  lisdb.dbo.FileRecieveInfoDetail a JOIN lisdb.dbo.lis_ybxx b ON a.Barcode=b.ybid LEFT JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'  and InspectionProject like'%10N1%'  AND ybzt IN ('p','s')  AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120 ) AND ISNULL(BRXM,'') <> ''
)t7
, 
(SELECT  '混检20人数: ' +CAST(COUNT(barcode) AS CHAR(10)) AS FF
FROM  lisdb.dbo.FileRecieveInfoDetail a JOIN lisdb.dbo.lis_ybxx b ON a.Barcode=b.ybid LEFT JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'  and InspectionProject like'%20N1%' AND ybzt IN ('p','s')  AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120)   AND ISNULL(BRXM,'') <> '' 
)t8
,
(select '总人数: ' +CAST(SUM(tt)AS CHAR(10)) AS ww FROM ((SELECT   CAST(COUNT(*)  AS INT ) AS tt
FROM lisdb.dbo.lis_ybxx b   JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'   AND ybzt IN ('p','s' )
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120)   AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
and NOT EXISTS (SELECT * FROM  lisdb.dbo.FileRecieveInfoDetail WHERE  Barcode= ybid)) 
UNION ALL 
(SELECT   CAST(COUNT(barcode)  AS INT ) AS tt  FROM  lisdb.dbo.FileRecieveInfoDetail a JOIN lisdb.dbo.lis_ybxx b ON a.Barcode=b.ybid LEFT JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'  and InspectionProject like'%5N1%'  AND ybzt IN ('p','s')  AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120 ) AND ISNULL(BRXM,'') <> '')  
union ALL
(SELECT   CAST(COUNT(barcode)  AS INT ) AS tt  FROM  lisdb.dbo.FileRecieveInfoDetail a JOIN lisdb.dbo.lis_ybxx b ON a.Barcode=b.ybid LEFT JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'  and InspectionProject like'%10N1%'  AND ybzt IN ('p','s')  AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120 ) AND ISNULL(BRXM,'') <> '')  
union ALL
(SELECT   CAST(COUNT(barcode) AS INT )AS tt FROM  lisdb.dbo.FileRecieveInfoDetail a JOIN lisdb.dbo.lis_ybxx b ON a.Barcode=b.ybid LEFT JOIN  lisdb.dbo.lis_xmcdz c ON b.yqdh=c.yqdh AND b.cdrq=c.cdrq AND b.ybbh=c.ybbh
WHERE b.yqdh='pcr' AND c.xmdh='XG-RNA'  and InspectionProject like'%20N1%' AND ybzt IN ('p','s')  AND (SUBSTRING(ybid,1,1) IN ('X','Z') or SUBSTRING(ybid,1,4) IN ('8A14')) 
AND b.CDRQ=CONVERT(CHAR(10),DATEADD(day,-1,GETDATE()),120)   AND ISNULL(BRXM,'') <> '' ))  AS gsg) t9

                ";

            List<SqlParameter> list_PustData = new List<SqlParameter>();
            list_PustData.Add(new SqlParameter("@starttime", starttime));
            //list_SZ_SampleAddress.Add(new SqlParameter("@endtime", endtime));

            StringBuilder sbBarCode = new StringBuilder();
            List<CommonModel> commonModel = new List<CommonModel>(); //数据存放

            using (SqlDataReader reader = SqlHelper.GetReader(sql_PustData, list_PustData.ToArray()))
            {
                if (reader.HasRows)
                {
                    //Log.LogMeg("reader.HasRows", "有查到数据", ".\\log\\");
                    while (reader.Read())
                    {
                        commonModel.Add(new CommonModel()
                        {
                            //cdrq = reader["cdrq"].ToString(),
                            t0 = reader["t0"].ToString(),
                            t1 = reader["t1"].ToString(),
                            t2 = reader["t2"].ToString(),
                            t3 = reader["t3"].ToString(),
                            t4 = reader["t4"].ToString(),
                            t5 = reader["t5"].ToString(),
                            t6 = reader["t6"].ToString(),
                            t7 = reader["t7"].ToString(),
                            t8 = reader["t8"].ToString(),
                            t9 = reader["t9"].ToString(),
                        });
                    }
                }
            }
            //Log.LogMeg("Debug", commonModel.Count.ToString(), ".\\log\\");

            //时间: 2022 - 05 - 24 07:51:28
            //• 新冠上报数据汇总2022 - 05 - 23：
            //总管数: 2548
            //单检管数: 241
            //混检10管数: 2292
            //混检20管数: 0
            //单检人数: 241
            //混检10人数: 22534
            //混检20人数: 0
            //总人数: 22775   DateTime.Now.Date.AddDays(-1).ToString("yyyy/MM/dd") 

            if (commonModel.Count > 0)
            {
                sbBarCode.Append("时间：" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo) + "\n");
                sbBarCode.Append("• 新冠上报数据汇总：" +  DateTime.Now.Date.AddDays(-1).ToString("yyyy/MM/dd", DateTimeFormatInfo.InvariantInfo) + "\n");
                for (int i = 0; i < commonModel.Count; i++)
                {
                    sbBarCode.Append(commonModel[i].t0 + "\n" + commonModel[i].t1 + "\n" + commonModel[i].t2 + "\n" + commonModel[i].t3 + "\n" + commonModel[i].t4 + "\n" + commonModel[i].t5 + "\n" + commonModel[i].t6 + "\n" + commonModel[i].t7 + "\n" + commonModel[i].t8 + "\n" + commonModel[i].t9 + "\n");
                }
                Dingtalk.SendDingtalk(sbBarCode.ToString(), "", "0", WEB_HOOK_xgds, secret_xgds);
                sbBarCode.Clear();
            }
            else
            {
                Log.LogMeg("---新冠标本情况---", starttime + " 至 " + endtime + " 无异常！", ".\\log\\");
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            this.sqlitehelper.ExecNonQuery("update t_sys_parm set pvalue ='" + dateTimePicker2.Value +"' where pcode='Timedpush' ");

            // this.sqlcehelper.Query("select max(createtime) from t_barcode_log where barcode ='" + imagebarcode + "'");
        }

        private void button_select_Click(object sender, EventArgs e)
        {
            string shj;
            decimal num = decimal.Parse(textBox_time.Text);

            #region 邮件解析异常 
            if (Convert.ToInt16(DT_SqlQuery.Select("fname='邮件解析异常'")[0][5]) == 1)
            {
                string webhook = DT_SqlQuery.Select("fname='邮件解析异常'")[0][6].ToString();
                string secret = DT_SqlQuery.Select("fname='邮件解析异常'")[0][7].ToString();                
                string sql = DT_SqlQuery.Select("fname='邮件解析异常'")[0][3].ToString();
                datalist.Clear();
                //string sql = @"SELECT  [FID],[Subject],[Mail_from],[ErrorMessage],[FileName],[CreateTime],[EmailStatus],[SourceType],[FileContent] FROM lis.dbo.ImportFileEmailInfo with(nolock) WHERE CreateTime > DATEADD(MINUTE,@time,GETDATE()) AND (LEN(CAST(ErrorMessage AS VARCHAR(5000))) > 0  OR ErrorMessage LIKE '%附件%') AND Subject NOT LIKE '%【阿里云邮】您的账号异地登录提醒%'  AND Subject NOT LIKE '%自动回复%' ";
                if (notin.Length > 0)
                {
                    sql = sql + " AND Mail_from NOT IN (";
                    foreach (string mail in notin.Split('|'))
                    {
                        sql = sql + "'" + mail + "',";
                    }
                    sql = sql + "'')";
                }
                List<SqlParameter> pslist = new List<SqlParameter>();
                
                pslist.Add(new SqlParameter("@time", decimal.Negate(num)));  //decimal.Negate(a)  取负数
                                                                             //Log.LogMeg("获取多少分钟内的数据", textBox_time.Text.ToString(), ".\\log\\");
                using (SqlDataReader reader = SqlHelper.GetReader(sql, pslist.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            datalist.Add(new ImportFileEmailInfo_Model()
                            {
                                FID = reader["FID"].ToString(),
                                Subject = reader["Subject"].ToString(),
                                Mail_from = reader["Mail_from"].ToString(),
                                ErrorMessage = reader["ErrorMessage"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                CreateTime = reader["CreateTime"].ToString(),
                                EmailStatus = reader["EmailStatus"].ToString(),
                                SourceType = reader["SourceType"].ToString(),
                                FileContent = reader["FileContent"].ToString()
                            });
                        }
                    }
                }

                if (datalist.Count > 0)
                {
                    foreach (ImportFileEmailInfo_Model sd in datalist)
                    {
                        subject = sd.Subject;    //邮件标题                    
                        Mail_from = sd.Mail_from;       //发件人
                        ErrorMessage = sd.ErrorMessage;  //错误信息
                        CreateTime = sd.CreateTime;

                        if (subject.Length > 0 || Mail_from.Length > 0 || ErrorMessage.Length > 0)
                        {
                            Log.LogMeg("监控满足条件的异常", CreateTime + " " + Mail_from + " " + subject + " " + ErrorMessage, ".\\log\\");
                        }

                        #region 不提醒条码重复的情况
                        if (btx == "1")
                        {
                            string err = "";
                            if (ErrorMessage.Contains("保存条码数据失败"))
                            {
                                if (ErrorMessage.Contains("；"))
                                {
                                    foreach (string str in ErrorMessage.Split('；'))
                                    {
                                        if (str.Contains("保存条码数据失败"))
                                        {
                                            err += "";
                                        }
                                        else
                                        {
                                            err += str;
                                        }
                                    }
                                    ErrorMessage = err;
                                }
                                else
                                {
                                    ErrorMessage = "";
                                }
                            }
                        }
                        #endregion

                        if (ErrorMessage.Contains("。"))
                        {
                            ErrorMessage = ErrorMessage.Substring(0, ErrorMessage.IndexOf("。"));
                        }

                        if (ErrorMessage.Length > 0)
                        {
                            if (ErrorMessage.Contains("附件数据为空或不能解析") || ErrorMessage.Contains("登录失败")) //解析程序异常提醒
                            {
                                //发钉钉
                                sendmsg = "发件时间：" + CreateTime + "\n" + "发件地址：" + Mail_from + "\n" + "邮件标题：" + subject + "\n" + "错误信息：" + ErrorMessage;
                                string s = Dingtalk.SendDingtalk(sendmsg, hjtx, atALL, WEB_HOOK, secret);
                                if (s != "0")
                                {
                                    Log.LogMeg("POST返回信息", s, ".\\log\\");
                                    Log.LogMeg("异常信息记录", "\r" + sendmsg + "\r" + "电话：" + hjtx, ".\\log\\");
                                }
                            }
                            else
                            {
                                #region 发钉钉
                                sendmsg = "发件时间：" + CreateTime + "\n" + "发件地址：" + Mail_from + "\n" + "邮件标题：" + subject + "\n" + "错误信息：" + ErrorMessage;

                                //群里@指定人
                                if (atMobiles.Length > 0 && atMobiles.Contains(Mail_from))
                                {
                                    if (atMobiles.Contains("|"))
                                    {
                                        foreach (string mailphone in atMobiles.Split('|'))
                                        {
                                            int i = mailphone.IndexOf('=');
                                            mailfrom = mailphone.Substring(0, i);
                                            if (Mail_from == mailfrom)
                                            {
                                                phone = mailphone.Substring(i + 1);
                                                break;  //获取到指定人手机号后，不再获取，直接跳出当前遍历
                                            }
                                            else
                                            {
                                                phone = "0";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int i = atMobiles.IndexOf('=');
                                        mailfrom = atMobiles.Substring(0, i);
                                        if (Mail_from == mailfrom)
                                        {
                                            phone = atMobiles.Substring(i + 1);
                                        }
                                        else
                                        {
                                            phone = "0";
                                        }
                                    }

                                }

                                //string s = Dingtalk.SendDingtalk(sendmsg, phone, atALL, WEB_HOOK, secret);
                                string s = Dingtalk.SendDingtalk(sendmsg, phone, atALL, webhook, secret);
                                if (s != "0")
                                {
                                    Log.LogMeg("POST返回信息", s, ".\\log\\");
                                    Log.LogMeg("异常信息记录", "\r" + sendmsg + "\r" + "电话：" + phone, ".\\log\\");
                                }
                                phone = "0"; //@人后初始化，免得下次@错人
                                #endregion

                                #region 发邮件
                                sendmsg = "发件时间：" + CreateTime + "\r" + "发件地址：" + Mail_from + "\r" + "邮件标题：" + subject + "\r" + "错误信息：" + ErrorMessage;
                                if (subject.Contains("混检对接(临时标记)"))
                                {
                                    sendmail = xgzh;    //新冠邮箱账号
                                    sendpwd = xgmm;     //新冠邮箱密码
                                }
                                else
                                {
                                    sendmail = cgzh;    //常规邮箱账号
                                    sendpwd = cgmm;     //常规邮箱密码
                                }

                                if (sendmail.Length > 0 && sendpwd.Length > 0)
                                {
                                    MailHelper mh = new MailHelper(sendmail, sendpwd);
                                    if (mh.Send(sendmail, Mail_from, "", "对接邮件解析异常", sendmsg.Replace("\r", "<br>"), out err, null))
                                    {
                                        Log.LogMeg("邮件发送成功", "\r" + "发件人：" + sendmail + "\r" + "收件人：" + Mail_from + "\r" + "异常：" + err, ".\\log\\");
                                    }
                                    else
                                    {
                                        Log.LogMeg("邮件发送失败", "\r" + "发件人：" + sendmail + "\r" + "收件人：" + Mail_from + "\r" + "异常：" + err, ".\\log\\");
                                    }
                                }
                                #endregion

                            }
                        }
                    }
                }
            
            }
            #endregion

            #region 混检表查询
            if (Convert.ToInt16(DT_SqlQuery.Select("fname='混检表查询'")[0][5]) == 1)                
            {
                string webhook = DT_SqlQuery.Select("fname='混检表查询'")[0][6].ToString();
                string secret = DT_SqlQuery.Select("fname='混检表查询'")[0][7].ToString();
                string sqlhj = DT_SqlQuery.Select("fname='混检表查询'")[0][3].ToString();
                //           string sqlhj = @"SELECT DISTINCT LEFT(Barcode,6) khdh,mc AS khmc,MIN(CreateDate) AS kssj,COUNT(Barcode) AS sl
                //From lisdb.dbo.FileRecieveInfoDetail a WITH(NOLOCK) INNER JOIN lisdb.dbo.xt_yymc_print b WITH(NOLOCK)
                //ON LEFT(a.Barcode,6) = b.dh
                //where createdate > DATEADD(MINUTE,@time,GETDATE()) and inspectionproject not in ('新型冠状病毒核酸检测（10N1）','新型冠状病毒核酸检测（5N1）','新型冠状病毒核酸检测(10N1)','新型冠状病毒核酸检测(5N1)','新型冠状病毒核酸检测(20N1)','新型冠状病毒核酸检测（20N1）') 
                //GROUP BY LEFT(Barcode,6),mc ";
                List<SqlParameter> hjlist = new List<SqlParameter>();
                hjlist.Add(new SqlParameter("@time", decimal.Negate(time)));  //decimal.Negate(a)  取负数
                using (SqlDataReader reader = SqlHelper.GetReaderLISDB(sqlhj, hjlist.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            hjdatalist.Add(new FileRecieveInfoDetail_Model()
                            {
                                khdh = reader["khdh"].ToString(),
                                khmc = reader["khmc"].ToString(),
                                kssj = reader["kssj"].ToString(),
                                sl = reader["sl"].ToString()
                            });
                        }
                    }
                }

                
                if (hjdatalist.Count > 0)
                {
                    foreach (FileRecieveInfoDetail_Model hj in hjdatalist)
                    {
                        string khdh = hj.khdh;
                        string khmc = hj.khmc;
                        string kssj = hj.kssj;
                        string sl = hj.sl;
                        sendmsghj = "---混检表数据异常---" + "\n" + "混检数据项目名称必须是“新型冠状病毒核酸检测（5N1）”或“新型冠状病毒核酸检测（10N1）”或“新型冠状病毒核酸检测（20N1）”" + "\n" + "解析时间：" + kssj + "\n" + "客户代号：" + khdh + "\n" + "客户名称：" + khmc + "\n" + "条码数：" + sl;
                    }
                    if (hjtx.Length == 11)
                    {
                        //shj = Dingtalk.SendDingtalk(sendmsghj, hjtx, atALL, WEB_HOOK, secret);
                        shj = Dingtalk.SendDingtalk(sendmsghj, hjtx, atALL, webhook, secret);
                    }
                    else
                    {
                        //shj = Dingtalk.SendDingtalk(sendmsghj, "0", atALL, WEB_HOOK, secret);
                        shj = Dingtalk.SendDingtalk(sendmsghj, "0", atALL, webhook, secret);
                    }

                    if (shj != "0")
                    {
                        Log.LogMeg("混检表异常", "\r" + sendmsghj, ".\\log\\");
                        hjdatalist.Clear();
                        sendmsghj = "";
                    }
                }
            }

            #endregion

            #region PDF报告单生成异常
            if (Convert.ToInt16(DT_SqlQuery.Select("fname='PDF报告单生成异常'")[0][5]) == 1)
            {
                string webhook = DT_SqlQuery.Select("fname='PDF报告单生成异常'")[0][6].ToString();
                string secret = DT_SqlQuery.Select("fname='PDF报告单生成异常'")[0][7].ToString();
                string sqlpdf = DT_SqlQuery.Select("fname='PDF报告单生成异常'")[0][3].ToString();
                //string sqlpdf = @"
                //select ybid from blsjk.dbo.t_bljbxxb WITH(NOLOCK) where issh = '1' and autopdf = '' AND Bgtime >= GETDATE()-3 AND DATEAD(HOUR,4,bgtime)< GETDATE() AND LEFT(ybid,6) NOT IN ('179007','999999')
                //UNION
                //select ybid from blsjk.dbo.t_tctbgd WITH(NOLOCK) where issh = '1' and autopdf = '' AND Bgtime >= GETDATE()-3 AND DATEAD(HOUR,4,bgtime)< GETDATE() AND LEFT(ybid,6) NOT IN ('179007','999999')
                //UNION
                //select ybid from lisdb.dbo.t_bb_ts_lis_ybxx WITH(NOLOCK) where ybzt IN ('s','p') and flag_pdf='' AND bgrq >= GETDATE()-3 AND DATEAD(HOUR,4,bgrq)< GETDATE() AND LEFT(ybid,6) NOT IN ('179007','999999')
                //";
                List<SqlParameter> pdflist = new List<SqlParameter>();
                pdflist.Add(new SqlParameter("@time", decimal.Negate(time)));  //decimal.Negate(a)  区负数
                DataTable dt = SqlHelper.GetList(sqlpdf);
                pdfdatalist.Clear(); //先清空，再插入数据
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        pdfdatalist.Add(new FileRecieveInfoDetail_Model()
                        {
                            khdh = dt.Rows[i]["ybid"].ToString()
                        });
                    }
                }

                int bjsl;
                //if (string.IsNullOrEmpty(pdfsl))
                //{
                bjsl = 80;
                //}
                //else
                //{
                //    bjsl = Convert.ToInt16(pdfsl);
                //}
                if (pdfdatalist.Count > bjsl)  //大于100条数据再提醒
                {
                    StringBuilder sbBarCodepdf = new StringBuilder(); //StringBuilder对于大量数据时的效果比string好
                    sbBarCodepdf.Append("---PDF报告单生成异常---" + "\n"); //Append追加
                    sbBarCodepdf.Append("样本审核超过4小时仍未生成PDF的报告单数量：" + pdfdatalist.Count.ToString() + "\n");
                    //sbBarCodepdf.Append("条码：\n");
                    //for (int i = 0; i < pdfdatalist.Count; i++)
                    //{
                    //    sbBarCodepdf.Append(pdfdatalist[i].khdh + ((i == pdfdatalist.Count - 1) ? "" : ","));
                    //}
                    sbBarCodepdf.Append("\n请检查PDF报告单生成工具是否异常!");

                    sendmsghj = sbBarCodepdf.ToString(); //也可以直接用sbBarCode.ToString()发送

                    if (hjtx.Length == 11)  //异常提醒人电话号码11位
                    {
                        //shj = Dingtalk.SendDingtalk(sendmsghj, hjtx, atALL, WEB_HOOK, secret); 
                        shj = Dingtalk.SendDingtalk(sendmsghj, hjtx, atALL, webhook, secret);
                    }
                    else
                    {
                        //shj = Dingtalk.SendDingtalk(sendmsghj, hjtx, atALL, WEB_HOOK, secret); 
                        shj = Dingtalk.SendDingtalk(sendmsghj, hjtx, atALL, webhook, secret);
                    }

                    if (shj != "0")
                    {
                        Log.LogMeg("PDF报告生成异常推送", "\r" + sendmsghj, ".\\log\\");
                        pdfdatalist.Clear();
                        sendmsghj = "";
                    }
                }
         
            }
            
            #endregion

            #region 邮件解析程序异常，导致信息表里面没有数据（条码项目自动比对自动导入异常）       
            if (serverip == serveripLISDB && Convert.ToInt16(DT_SqlQuery.Select("fname='LIMS自动导入异常'")[0][5]) == 1)
            {
                string webhook = DT_SqlQuery.Select("fname='LIMS自动导入异常'")[0][6].ToString();
                string secret = DT_SqlQuery.Select("fname='LIMS自动导入异常'")[0][7].ToString();
                string sqldryc = DT_SqlQuery.Select("fname='LIMS自动导入异常'")[0][3].ToString();
                //string sqldryc = @"SELECT CreateDate,Barcode FROM lis.dbo.FileRecieveInfo WITH(NOLOCK)
                //            WHERE CreateDate >= DATEADD(MINUTE,@time,GETDATE())  and ErrorStatus = '2' and 
                //            NOT EXISTS
                //             (
	               //             SELECT Barcode FROM LisSampleDB.dbo.SampleInfo WITH(NOLOCK)
	               //             WHERE CreateDate >= DATEADD(MINUTE,@time,GETDATE())  
	               //             AND LisSampleDB.dbo.SampleInfo.Barcode = lis.dbo.FileRecieveInfo.Barcode
                //            ) ";
                List<SqlParameter> dryclist = new List<SqlParameter>();
                dryclist.Add(new SqlParameter("@time", decimal.Negate(time)));  //decimal.Negate(a)  取负数
                using (SqlDataReader reader = SqlHelper.GetReader(sqldryc, dryclist.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            drycdatalist.Add(new FileRecieveInfo_Model()
                            {
                                CreateDate = reader["CreateDate"].ToString(),
                                Barcode = reader["Barcode"].ToString()
                            });
                        }
                    }
                }

                string sdr;
                if (drycdatalist.Count > 0)
                {
                    foreach (FileRecieveInfo_Model dr in drycdatalist)
                    {
                        string CreateDate = dr.CreateDate;
                        string Barcode = dr.Barcode;
                        sendmsghj = "---LIMS自动导入异常---" + "\n" + "创建时间：" + CreateDate + "\n" + "条码号：" + Barcode;
                    }
                    if (hjtx.Length == 11)
                    {
                        //sdr = Dingtalk.SendDingtalk(sendmsghj, hjtx, atALL, WEB_HOOK, secret);
                        sdr = Dingtalk.SendDingtalk(sendmsghj, hjtx, atALL, webhook, secret);
                    }
                    else
                    {
                        //sdr = Dingtalk.SendDingtalk(sendmsghj, "0", atALL, WEB_HOOK, secret);
                        sdr = Dingtalk.SendDingtalk(sendmsghj, "0", atALL, webhook, secret);
                    }

                    if (sdr != "0")
                    {
                        Log.LogMeg("自动导入异常", "\r" + sendmsghj, ".\\log\\");
                        drycdatalist.Clear();
                        //this.dataGridView2.DataSource = null;
                        sendmsghj = "";
                    }
                }
            }
            #endregion

            #region 录入组比对项目，未导入提醒
//            if (serverip == serveripLISDB)
//            {
//                string sql_bdwdr = @" SELECT CASE ErrorStatus WHEN 0 THEN '未比对,请及时比对导入' WHEN 1 THEN '已比对,请及时导入' END AS status,
// Barcode,CreateDate,AuditUser,b.UserName,b.Mobile,b.Phone
// FROM lis.dbo.FileRecieveItem a WITH(NOLOCK)
// INNER JOIN LisSampleDB.dbo.Users b WITH(NOLOCK) ON a.AuditUser = b.UserID
// WHERE ErrorStatus = 1 AND a.CreateDate >= GETDATE() - 1  -- ErrorStatus 0 未比对   1 已比对   2已导入
//";
//                List<SqlParameter> list_bdwdr = new List<SqlParameter>();
//                list_bdwdr.Add(new SqlParameter("@time", decimal.Negate(time)));  //decimal.Negate(a)  取负数
//                using (SqlDataReader reader = SqlHelper.GetReader(sql_bdwdr, list_bdwdr.ToArray()))
//                {
//                    if (reader.HasRows)
//                    {
//                        while (reader.Read())
//                        {
//                            bdwdrlist.Add(new FileRecieveInfo_Model()
//                            {
//                                CreateDate = reader["CreateDate"].ToString(),
//                                Barcode = reader["Barcode"].ToString(),
//                                UserName = reader["UserName"].ToString()
//                            });
//                        }
//                    }
//                }

//                string sdr;
//                if (bdwdrlist.Count > 0)
//                {
//                    foreach (FileRecieveInfo_Model dr in bdwdrlist)
//                    {
//                        string CreateDate = dr.CreateDate;
//                        string Barcode = dr.Barcode;
//                        string userName = dr.UserName;  
//                        sendmsghj = "---LIMS比对未导入---" + "\n" + "创建时间：" + CreateDate + "\n" + "条码号：" + Barcode + "\n" + "比对人：" + userName;
//                    }
//                    if (hjtx.Length == 11)
//                    {
//                        sdr = Dingtalk.SendDingtalk(sendmsghj, hjtx, atALL, WEB_HOOK, secret);
//                    }
//                    else
//                    {
//                        sdr = Dingtalk.SendDingtalk(sendmsghj, "0", atALL, WEB_HOOK, secret);
//                    }

//                    if (sdr != "0")
//                    {
//                        Log.LogMeg("LIMS比对未导入", "\r" + sendmsghj, ".\\log\\");
//                        drycdatalist.Clear();
//                        //this.dataGridView2.DataSource = null;
//                        sendmsghj = "";
//                    }
//                }
//            }
            #endregion

            #region 区域代码维护提醒    福州专用
            if (serverip == "10.5.0.1")
            {
                starttime = DateTime.Now.AddHours(-24).ToString();
                string sql_qydm = @"
                SELECT DISTINCT LEFT(a.Barcode,6) barcode,B.Hospital FROM LisSampleDB.dbo.SampleItem  a  WITH(nolock) 
                INNER JOIN LisSampleDB.DBO.SampleInfo B WITH(nolock)
                ON a.Barcode = b.Barcode
                WHERE LISItem ='XG-RNA' AND NOT EXISTS (
	                SELECT dh FROM lisdb.dbo.xt_yymc_print with(nolock) WHERE  qymc LIKE '%[0-9]%'
	                and LEFT(a.Barcode,6) = dh
                ) AND a.CreateDate >=GETDATE()-3 AND LEFT(a.Barcode,2) IN ('31','3Z','8A','30')
                AND EXISTS (
	                select top 1 * from lisdb.dbo.InfoSyncLog C WITH(NOLOCK) WHERE A.Barcode = C.BarCode 
                )
                ";

                List<SqlParameter> list_qydm = new List<SqlParameter>();
                list_qydm.Add(new SqlParameter("@starttime", starttime));  //decimal.Negate(a)  区负数   DateTime.Now.AddHours(-5).ToString()

                using (SqlDataReader reader = SqlHelper.GetReader(sql_qydm, list_qydm.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        string msg = "== 区域代码维护提醒 ==";
                        while (reader.Read())
                        {
                            msg += "\n" + reader["barcode"].ToString();
                        }
                        msg.Replace("\t", "");
                        Dingtalk.SendDingtalk(msg, hjtx, atALL, WEB_HOOK, secret);
                    }
                }
            }
            #endregion

            #region 福州市重大疫情平台核收下载信息异常提醒     福州专用
            if (serverip == "10.5.0.1")
            {
                string sql_fzszdyq = @"
                SELECT DISTINCT Remark,HospitalBarCode,BarCode from lisdb.dbo.InfoSyncLog
                where  SyncStatus = 2
                AND CreateTime > DATEADD(MINUTE, @time, GETDATE())
                ";
                List<SqlParameter> list_fzszdyq = new List<SqlParameter>();
                list_fzszdyq.Add(new SqlParameter("@time", decimal.Negate(time)));  //decimal.Negate(a)  区负数   DateTime.Now.AddHours(-5).ToString()

                using (SqlDataReader reader = SqlHelper.GetReader(sql_fzszdyq, list_fzszdyq.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        string msg = "== 福州市重大疫情平台核收下载信息异常提醒 ==";
                        while (reader.Read())
                        {
                            msg += "\n" + reader["BarCode"].ToString() + "【" + reader["HospitalBarCode"].ToString() + "】" + reader["Remark"].ToString();
                        }
                        msg.Replace("\t", "");
                        Dingtalk.SendDingtalk(msg, hjtx, atALL, WEB_HOOK, secret);
                    }
                }
            }
            #endregion

            #region 新冠标本采集时间超过24小时提醒    福州专用
            if (serverip == "10.5.0.1")
            {
                string sql_c24 = @"  
                SELECT a.Barcode,b.HospitalSpecial,B.Hospital,B.GatherTime FROM LisSampleDB.dbo.SampleItem  a  WITH(nolock) 
                INNER JOIN LisSampleDB.DBO.SampleInfo B WITH(nolock)
                ON a.Barcode = b.Barcode
                WHERE LISItem ='XG-RNA' AND B.CreateDate > DATEADD(MINUTE, @time, GETDATE())
                AND DATEDIFF(HOUR,b.GatherTime,a.CreateDate) > 24
                AND LEFT(a.Barcode,6) NOT IN ('999999','311192') AND LEN(ISNULL(b.PatientName,'')) > 0
                ";

                List<SqlParameter> list_c24 = new List<SqlParameter>();
                list_c24.Add(new SqlParameter("@time", decimal.Negate(time)));  //decimal.Negate(a)  区负数   DateTime.Now.AddHours(-5).ToString()

                using (SqlDataReader reader = SqlHelper.GetReader(sql_c24, list_c24.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        string msg_c24 = "== 新冠标本采集时间超过24小时提醒 ==";
                        msg_c24 += "\n" + "当前时间：" + DateTime.Now.ToString();
                        while (reader.Read())
                        {
                            msg_c24 += "\n" + reader["Barcode"].ToString() + " " + reader["Hospital"].ToString() + " " + reader["HospitalSpecial"].ToString() + "  采集时间：" + reader["GatherTime"].ToString();
                        }
                        msg_c24.Replace("\t", "");
                        Dingtalk.SendDingtalk(msg_c24, hjtx, atALL, WEB_HOOK, secret);
                    }
                }
            }
            #endregion

            #region 邮件解析异常监控
            POP3 xgPOP3 = new POP3("pop.qiye.aliyun.com", xgzh_jx, xgmm_jx);
            if (xgPOP3.TotalMailCount > 3)
            {
                Dingtalk.SendDingtalk("--- 新冠-邮件解析程序异常 ---", hjtx, atALL, WEB_HOOK_xg, secret_xg);
            }

            POP3 cgPOP3 = new POP3("pop.qiye.aliyun.com", cgzh_jx, cgmm_jx);
            if (cgPOP3.TotalMailCount > 3)
            {
                Dingtalk.SendDingtalk("--- 常规-邮件解析程序异常 ---", hjtx, atALL, WEB_HOOK_xg, secret_xg);
            }
            #endregion

            #region repno 生成异常监控
            if (Convert.ToInt16(DT_SqlQuery.Select("fname='repno生成异常'")[0][5]) == 1)
            {
                string webhook = DT_SqlQuery.Select("fname='repno生成异常'")[0][6].ToString();
                string secret = DT_SqlQuery.Select("fname='repno生成异常'")[0][7].ToString();
                string sqlrepno = DT_SqlQuery.Select("fname='repno生成异常'")[0][3].ToString();
                List<SqlParameter> list_repno = new List<SqlParameter>();
                //list_repno.Add(new SqlParameter("@starttime", starttime));

                StringBuilder sbBarCode = new StringBuilder();
                List<sample_Model> sample_repno = new List<sample_Model>(); //数据存放

                //using (SqlDataReader reader = SqlHelper.GetReaderByProcedure("lisdb.dbo.Procedure_repno", list_repno.ToArray()))
                using (SqlDataReader reader = SqlHelper.GetReader(sqlrepno, list_repno.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            sample_repno.Add(new sample_Model()
                            {
                                ybid = reader["ybid"].ToString()
                            });
                        }
                    }
                }


                if (sample_repno.Count > 0)
                {
                    sbBarCode.Append("==repno生成异常==\n");
                    for (int i = 0; i < sample_repno.Count; i++)
                    {
                        sbBarCode.Append(sample_repno[i].ybid + ((i == sample_repno.Count - 1) ? "" : "、" ));
                    }
                    //Dingtalk.SendDingtalk(sbBarCode.ToString(), "", "0", WEB_HOOK, secret);
                    Dingtalk.SendDingtalk(sbBarCode.ToString(), "", "0", webhook, secret);
                    sbBarCode.Clear();
                }
                else
                {
                    Log.LogMeg("---repno生成异常监控---", endtime + " 无异常！", ".\\log\\");
                }
            }

            #endregion

            #region 核酸数据导入采样点异常
            if (serverip == serveripLISDB && Convert.ToInt16(DT_SqlQuery.Select("fname='核酸信息导入'")[0][5]) == 1)
            {
                string webhook = DT_SqlQuery.Select("fname='核酸信息导入'")[0][6].ToString();
                string secret = DT_SqlQuery.Select("fname='核酸信息导入'")[0][7].ToString();
                string sql_cyd = DT_SqlQuery.Select("fname='核酸信息导入'")[0][3].ToString();

                List<SqlParameter> list_cyd = new List<SqlParameter>();
                StringBuilder sbBarCode = new StringBuilder();
                List<sample_Model> sample_cyd = new List<sample_Model>(); //数据存放

                list_cyd.Add(new SqlParameter("@time", decimal.Negate(num)));  //decimal.Negate(a)  取负数
    
                using (SqlDataReader reader = SqlHelper.GetReader(sql_cyd, list_cyd.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            sample_cyd.Add(new sample_Model()
                            {
                                cjsj = reader["createtime"].ToString(),
                                ybid = reader["filename"].ToString(),
                                beizhu = reader["content"].ToString()
                            });
                        }
                    }
                }


                if (sample_cyd.Count > 0)
                {
                    sbBarCode.Append("==核酸数据导入采样点匹配==\n");
                    for (int i = 0; i < sample_cyd.Count; i++)
                    {
                        sbBarCode.Append(sample_cyd[i].cjsj + "\n" + sample_cyd[i].ybid + "\n" + sample_cyd[i].beizhu + "\n");
                    }
                    Dingtalk.SendDingtalk(sbBarCode.ToString(), hjtx, atALL, WEB_HOOK, secret);
                    sbBarCode.Clear();
                }
                else
                {
                    Log.LogMeg("---核酸数据导入采样点匹配---", endtime + " 无异常！", ".\\log\\");
                }
            }

            #endregion

            #region 核酸采样监控
            if ( Convert.ToInt16(DT_SqlQuery.Select("fname='核酸采样监控'")[0][5]) == 1)
            {
                string webhook = DT_SqlQuery.Select("fname='核酸采样监控'")[0][6].ToString();
                string secret = DT_SqlQuery.Select("fname='核酸采样监控'")[0][7].ToString();
                string sql_hscj = DT_SqlQuery.Select("fname='核酸采样监控'")[0][3].ToString();

                List<SqlParameter> list_hscj = new List<SqlParameter>();
                StringBuilder sbBarCode = new StringBuilder();
                List<sample_Model> sample_hscj = new List<sample_Model>(); //数据存放

                list_hscj.Add(new SqlParameter("@time", decimal.Negate(num)));  //decimal.Negate(a)  取负数

                using (SqlDataReader reader = SqlHelper.GetReader(sql_hscj, list_hscj.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            sample_hscj.Add(new sample_Model()
                            {
                                beizhu = reader["msg"].ToString()
                            });
                        }
                    }
                }


                if (sample_hscj.Count > 0)
                {
                    sbBarCode.Append("==核酸采样监控==\n");
                    for (int i = 0; i < sample_hscj.Count; i++)
                    {
                        sbBarCode.Append(sample_hscj[i].beizhu + "\n");
                    }
                    Dingtalk.SendDingtalk(sbBarCode.ToString(), hjtx, atALL, webhook, secret);
                    sbBarCode.Clear();
                }
                else
                {
                    Log.LogMeg("---核酸采样监控---", endtime + " 无异常！", ".\\log\\");
                }
            }

            #endregion
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) //每十分钟调用这个后台进程
        {
            button_select_Click(null, null);    //监控
            btn_hpvtct_Click(null, null); //HPV TCT阳性提醒
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
        }

        #region 有信息无标本数据明细
        public void GetXinguang_yxxwbb(string khdh, int fw)
        {
            string sql_yxxwbb;
            if (string.IsNullOrEmpty(khdh))
            {
                sql_yxxwbb = @"
                SELECT Barcode,PatientName,CreateDate FROM LisSampleDB.dbo.SampleInfo
                WHERE LEFT(Barcode,6) <> '999999'
                AND DATEDIFF(HOUR,CreateDate,GETDATE()) >= @fw
                AND CreateDate >= GETDATE() - 1
                AND NOT EXISTS (
	                SELECT ybid FROM lisdb.dbo.lis_ybxx WHERE yqdh = 'pcr' AND cyrq >= GETDATE() - 2
	                AND LEFT(ybid,6) <> '999999'
	                AND Barcode = ybid
                )
                ORDER BY CreateDate
                ";
            }
            else
            {
                sql_yxxwbb = this.sqlitehelper.Query(@"select query from SqlQuery where fname='新冠有信息无标本-机场'").Rows[0][0].ToString();
            }

            List<SqlParameter> yxxwbblist = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(khdh))
            {
                yxxwbblist.Add(new SqlParameter("@khdh", khdh));
            }
            yxxwbblist.Add(new SqlParameter("@fw", fw));

            StringBuilder sbBarCode = new StringBuilder();
            List<sample_Model> sample = new List<sample_Model>(); //有标本无信息数据存放

            using (SqlDataReader reader = SqlHelper.GetReader(sql_yxxwbb, yxxwbblist.ToArray()))
            {
                if (reader.HasRows)
                {
                    Log.LogMeg("reader.HasRows", "有查到数据", ".\\log\\");
                    while (reader.Read())
                    {
                        sample.Add(new sample_Model()
                        {
                            ybid = reader["Barcode"].ToString(),
                            yytm = reader["HospitalSpecial"].ToString(),
                            brxm = reader["PatientName"].ToString(),
                            cjsj = reader["CreateDate"].ToString()
                        });
                    }
                }
            }

            if (sample.Count > 0)
            {
                sbBarCode.Append("---新冠有信息无标本---" + "\n");
                Log.LogMeg("---新冠有信息无标本---1", "记录触发时间： " + DateTime.Now.ToString("HH:mm:ss"), ".\\log\\");
                for (int i = 0; i < sample.Count; i++)
                {
                    sbBarCode.Append("条码：" + sample[i].ybid + "[" + sample[i].yytm + "] 姓名：" + sample[i].brxm + " 采集时间：" + sample[i].cjsj + ((i == sample.Count - 1) ? "" : "\n"));                    
                }
                Dingtalk.SendDingtalk(sbBarCode.ToString(), "", "0", WEB_HOOK_jc, secret_jc);
                sbBarCode.Clear();
            }
            else
            {
                Log.LogMeg("---新冠有信息无标本---", starttime + " 至 " + endtime + " 无异常！", ".\\log\\");
            }
        }
        #endregion

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
           
        }        

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            button4_Click(null, null); //新冠标本审核情况
        }

        //去除字符串中的换行符 "\r\n"
        private string cleanString(string newStr)
        {
            string tempStr = newStr.Replace("\n", "");
            return tempStr = tempStr.Replace("\r", "");
        }    

        #region 有标本无信息数据明细 
        private void btn_ybbwxx_Click(object sender, EventArgs e)
        {
            starttime = DateTime.Now.AddHours(-2).ToString();
            endtime = DateTime.Now.AddHours(-1).ToString();
            GetXinGuang_ybbwxx(starttime, endtime, null);          
        }

        /// <summary>
        /// 新冠有标本无信息（具体条码情况）
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <param name="khdh"></param>
        private void GetXinGuang_ybbwxx(string starttime,string endtime,string khdh) 
        {
            string sql_ybbwxx;
            Log.LogMeg("---新冠有标本无信息---time", starttime + " | " + endtime, ".\\log\\");

            sql_ybbwxx = this.sqlitehelper.Query(@"select query from SqlQuery where fname='新冠有标本无信息'").Rows[0][0].ToString();
            
            List<SqlParameter> ybbwxxlist = new List<SqlParameter>();
            ybbwxxlist.Add(new SqlParameter("@starttime", starttime));
            ybbwxxlist.Add(new SqlParameter("@endtime", endtime));

            StringBuilder sbBarCode = new StringBuilder();
            List<sample_Model> sample = new List<sample_Model>(); //有标本无信息数据存放

            using (SqlDataReader reader = SqlHelper.GetReader(sql_ybbwxx, ybbwxxlist.ToArray()))
            {
                if (reader.HasRows)
                {
                    Log.LogMeg("reader.HasRows", "有查到数据", ".\\log\\");
                    while (reader.Read())
                    {
                        sample.Add(new sample_Model()
                        {
                            ybid = reader["ybid"].ToString(),
                            yytm = reader["yytm"].ToString(),
                            khmc = reader["mc"].ToString(),
                            ywy = reader["username"].ToString(),
                            ywyphone = reader["linkmosiphone"].ToString(),
                            ybbh = reader["ybbh"].ToString()
                        });
                    }
                }
            }
            if (sample.Count > 0)
            {
                string khmc1 = sample[0].khmc;
                string ywy1 = sample[0].ywy;
                string ywyphone1 = sample[0].ywyphone;

                sbBarCode.Append("---新冠有标本无信息---" + "\n");
                sbBarCode.Append("以下条码刷入系统时间段：" + starttime + " 至 " + endtime + "\n" + "为了使报告能及时审核，请及时【联系客户提交送检信息】 或 【发送标本信息邮件】" + "\n");
                Log.LogMeg("---新冠有标本无信息---1", "记录触发时间： " + DateTime.Now.ToString("HH:mm:ss"), ".\\log\\");
                for (int i = 0; i < sample.Count; i++)
                {
                    string khmc2 = sample[i].khmc;
                    string ywy2 = sample[i].ywy;
                    string ywyphone2 = sample[i].ywyphone;
                    if (khmc1 == khmc2)
                    {
                        if (sample[i].yytm.Length > 5)
                        {
                            sbBarCode.Append(sample[i].ybid+ "【" + sample[i].yytm + "】" + "【" + sample[i].ybbh + "】" + ((i == sample.Count - 1) ? "" : "、"));
                        }
                        else
                        {
                            sbBarCode.Append(sample[i].ybid + ((i == sample.Count - 1) ? "" : "、"));
                        }                        
                    }
                    else
                    {
                        sbBarCode.Append("\n" + "客户名称：" + khmc1 + "\n");
                        if (!string.IsNullOrEmpty(ywy1))
                        {
                            sbBarCode.Append("代表：" + ywy1);
                        }

                        Dingtalk.SendDingtalk(sbBarCode.ToString(), ywyphone1, "0", WEB_HOOK_xg, secret_xg);                        

                        Log.LogMeg("---新冠有标本无信息---2", "记录需要发送的内容： " + "\n" + sbBarCode.ToString(), ".\\log\\");


                        khmc1 = khmc2;
                        ywy1 = ywy2;
                        ywyphone1 = ywyphone2;
                        sbBarCode.Clear();
                        sbBarCode.Append("---新冠有标本无信息---" + "\n");
                        sbBarCode.Append("以下条码刷入系统时间段：" + starttime + " 至 " + endtime + "\n" + "为了使报告能及时审核，请及时【联系客户提交送检信息】 或 【发送标本信息邮件】" + "\n");
                        if (sample[i].yytm.Length > 5)
                        {
                            sbBarCode.Append(sample[i].ybid + "【" + sample[i].yytm + "】" + "【" + sample[i].ybbh + "】" + ((i == sample.Count - 1) ? "" : "、"));
                        }
                        else
                        {
                            sbBarCode.Append(sample[i].ybid + ((i == sample.Count - 1) ? "" : "、"));
                        }
                    }
                    if (i == sample.Count - 1)   //最后一条数据的发送，或者只有一条的时候
                    {
                        sbBarCode.Append("\n" + "客户名称：" + khmc1 + "\n");
                        if (!string.IsNullOrEmpty(ywy1))
                        {
                            sbBarCode.Append("代表：" + ywy1);
                        }

                        if (khdh == "400") //东软有信息无标本异常提醒到  邮件解析异常群提醒
                        {
                            Dingtalk.SendDingtalk(sbBarCode.ToString(), hjtx, "0", WEB_HOOK, secret);
                            //Dingtalk.SendDingtalk(sendmsg, hjtx, atALL, WEB_HOOK, secret);
                        }
                        else
                        {
                            Dingtalk.SendDingtalk(sbBarCode.ToString(), ywyphone1, "0", WEB_HOOK_xg, secret_xg);
                        }
                        

                        Log.LogMeg("---新冠有标本无信息---3", "记录需要发送的内容： " + "\n" + sbBarCode.ToString(), ".\\log\\");
                        sbBarCode.Clear();
                    }
                }
            }
            else
            {
                Log.LogMeg("---新冠有标本无信息---", starttime + " 至 " + endtime + " 无异常！", ".\\log\\");
            }
        }
        #endregion        

        #region 新冠每日统计
        private void btn_xg_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt16(DT_SqlQuery.Select("fname='新冠每日统计'")[0][5]) == 1)
            {
                starttime = DateTime.Now.AddHours(-24).ToString();
                string sqlxg = DT_SqlQuery.Select("fname='新冠每日统计'")[0][3].ToString();

                List<SqlParameter> xglist = new List<SqlParameter>();
                xglist.Add(new SqlParameter("@starttime", starttime));  //decimal.Negate(a)  区负数   DateTime.Now.AddHours(-5).ToString()

                using (SqlDataReader reader = SqlHelper.GetReader(sqlxg, xglist.ToArray()))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string msg = "== 新冠每日统计 ==" + "\n" + "统计时间段：" + starttime + " 至 " + DateTime.Now.ToString() + "\n" +
                                "刷入LIS数量：" + reader["刷入LIS数量"].ToString() + "\n" + "已审核数量：" + reader["已审核数量"].ToString() + "\n" + "有标本无信息：" + reader["有标本无信息"].ToString();
                            string shj = Dingtalk.SendDingtalk(msg, "新冠", atALL, WEB_HOOK_xg, secret_xg);
                            if (shj != "0")
                            {
                                Log.LogMeg("新冠每日统计", shj, ".\\log\\");
                            }
                        }
                    }
                }

                starttime = DateTime.Now.AddHours(-24).ToString();
                endtime = DateTime.Now.ToString();
                GetXinGuang_ybbwxx(starttime, endtime, null);
            }
        }
        #endregion  

        #region 开始
        private void button_start_Click(object sender, EventArgs e)
        {
            timer1.Start();
            button_select.Enabled = false;
            button_start.Enabled = false;
            button_end.Enabled = true;           
        }
        #endregion
              
        #region 窗口打开加载
        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Text = "监控程序Ver："+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " | " + System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location).ToString("yyyy-MM-dd HH:mm:ss") + "";

            serverip = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "服务器名称", "", 128, ".\\set.ini"), "fzadicon");
            servername = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "数据库名称", "", 128, ".\\set.ini"), "fzadicon");
            login = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "登录名", "", 128, ".\\set.ini"), "fzadicon");
            password = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "密码", "", 128, ".\\set.ini"), "fzadicon");

            serveripLISDB = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "服务器名称", "", 128, ".\\set.ini"), "fzadicon");
            servernameLISDB = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "数据库名称", "", 128, ".\\set.ini"), "fzadicon");
            loginLISDB = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "登录名", "", 128, ".\\set.ini"), "fzadicon");
            passwordLISDB = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "密码", "", 128, ".\\set.ini"), "fzadicon");

            if (serverip.Length < 1 || servername.Length < 1 || login.Length < 1 || password.Length < 1 || serveripLISDB.Length < 1 || servernameLISDB.Length < 1 || loginLISDB.Length < 1 || passwordLISDB.Length < 1)
            {
                FormSet form_set = new FormSet();
                form_set.ShowDialog();
            }

            button_end.Enabled = false;
            string time = GetSetINI.GetiniProfile("系统设置", "获取多少分钟内的数据", "5", 128, ".\\set.ini").Trim();
            textBox_time.Text = time;

            WEB_HOOK = GetSetINI.GetiniProfile("系统设置", "钉钉机器人Webhook", "", 256, ".\\set.ini").Trim();
            secret = GetSetINI.GetiniProfile("系统设置", "钉钉机器人加签密钥", "", 256, ".\\set.ini").Trim();

            WEB_HOOK_xg = GetSetINI.GetiniProfile("系统设置", "新冠钉钉机器人Webhook", "", 256, ".\\set.ini").Trim();
            secret_xg = GetSetINI.GetiniProfile("系统设置", "新冠钉钉机器人加签密钥", "", 256, ".\\set.ini").Trim();

            WEB_HOOK_jc = GetSetINI.GetiniProfile("系统设置", "airportwebhook", "", 256, ".\\set.ini").Trim();
            secret_jc = GetSetINI.GetiniProfile("系统设置", "airportkey", "", 256, ".\\set.ini").Trim();

            WEB_HOOK_xgsh = GetSetINI.GetiniProfile("系统设置", "新冠审核情况Webhook", "", 256, ".\\set.ini").Trim();
            secret_xgsh = GetSetINI.GetiniProfile("系统设置", "新冠审核情况加签密钥", "", 256, ".\\set.ini").Trim();

            WEB_HOOK_xgds = GetSetINI.GetiniProfile("系统设置", "大筛审核情况Webhook", "", 256, ".\\set.ini").Trim();
            secret_xgds = GetSetINI.GetiniProfile("系统设置", "大筛审核情况加签密钥", "", 256, ".\\set.ini").Trim();

            WEB_HOOK_hpvtct = GetSetINI.GetiniProfile("系统设置", "HPVTCTWebhook", "", 256, ".\\set.ini").Trim();
            secret_hpvtct = GetSetINI.GetiniProfile("系统设置", "HPVTCT加签密钥", "", 256, ".\\set.ini").Trim();

            notin = GetSetINI.GetiniProfile("系统设置", "不提醒的发件人", "", 1024, ".\\set.ini").Trim();
            btx = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "不提醒保存条码数据失败错误", "", 128, ".\\set.ini"), "fzadicon");

            atALL = GetSetINI.GetiniProfile("系统设置", "atALL", "", 128, ".\\set.ini");
            if (atALL != "1")
            {
                atMobiles = GetSetINI.GetiniProfile("系统设置", "atMobiles", "", 5000, ".\\set.ini").Trim();
            }
            else
            {
                atMobiles = "";
            }                                  
            hjtx = GetSetINI.GetiniProfile("系统设置", "混检表异常提醒人号码", "", 128, ".\\set.ini").Trim();

            cgzh = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "常规对接邮箱账号", "", 128, ".\\set.ini"), "fzadicon");
            cgmm = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "常规对接邮箱密码", "", 128, ".\\set.ini"), "fzadicon");
            xgzh = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "新冠对接邮箱账号", "", 128, ".\\set.ini"), "fzadicon");
            xgmm = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "新冠对接邮箱密码", "", 128, ".\\set.ini"), "fzadicon");
                        
            cgzh_jx = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "常规解析邮箱账号", "", 128, ".\\set.ini"), "fzadicon");
            cgmm_jx = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "常规解析邮箱密码", "", 128, ".\\set.ini"), "fzadicon");
            xgzh_jx = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "新冠解析邮箱账号", "", 128, ".\\set.ini"), "fzadicon");
            xgmm_jx = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "新冠解析邮箱密码", "", 128, ".\\set.ini"), "fzadicon");

            timer2.Start();         
           //把得到的星期转换成中文
            switch (DateTime.Now.DayOfWeek.ToString())
            {
                 case "Monday": weekstr = "星期一"; break;
                 case "Tuesday": weekstr = "星期二"; break;
                 case "Wednesday": weekstr = "星期三"; break;
                 case "Thursday": weekstr = "星期四"; break;
                 case "Friday": weekstr = "星期五"; break;
                 case "Saturday": weekstr = "星期六"; break;
                 case "Sunday": weekstr = "星期日"; break;
            }

            string zdks = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "自动开始", "", 128, ".\\set.ini"), "fzadicon");

            if (zdks == "1")
            {
                button_start_Click(null, null);
            }

            timer4.Start();

            dateTimePicker2.Value = Convert.ToDateTime(this.sqlitehelper.Query("select pvalue from t_sys_parm where pcode='Timedpush'").Rows[0][0].ToString()); //获取配置时间


        }
        #endregion

        #region 结束
        private void button_end_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            button_select.Enabled = true;
            button_start.Enabled = true;
            button_end.Enabled = false;
        }
        #endregion

        #region Timer计时器

        int time;
        //定时运行
        private void timer1_Tick(object sender, EventArgs e)
        {
            time = Convert.ToInt16(textBox_time.Text.Trim());
            timer1.Interval = 1000 * 60 * time;  //设置运行时间间隔   1000毫秒=1秒

            setTime = Convert.ToDouble(textBox_time.Text.Trim()) * 60;//设置倒计时（分钟）;
            min = (int)setTime / 60;
            s = (int)setTime % 60;

            backgroundWorker1.RunWorkerAsync(); // BackgroundWorker 类允许您在单独的专用线程上运行操作。 耗时的操作（如下载和数据库事务）在长时间运行时可能会导致用户界面 (UI) 似乎处于停止响应状态。 如果您需要能进行响应的用户界面，而且面临与这类操作相关的长时间延迟，则可以使用 BackgroundWorker 类方便地解决问题。
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        //当前时间
        private void timer2_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = DateTime.Now.ToString() + " " + weekstr;
            string hh = dateTimePicker2.Text;
            int hour = DateTime.Now.Hour;
            if (hour < 8)
            {
                dateTimePicker1.Text = DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd");
            }
            else
            {
                dateTimePicker1.Text = DateTime.Now.Date.ToString("yyyy-MM-dd");
            }
        }

        private void timer4_Tick(object sender, EventArgs e)    //每个时间点触发 只能两个任务（如果三个的话，有时候触发不成功）
        {
            if (DateTime.Now.ToString("HH:mm:ss") == dateTimePicker2.Value.ToString("HH:mm:ss"))
            {
                btn_xg_Click(null, null);   //新冠每日统计 
                Log.LogMeg("新冠每日统计-自动", "记录触发时间： " + DateTime.Now.ToString("HH:mm:ss"), ".\\log\\");
                //if (serverip == "10.9.0.5") //长沙
                //{
                //    button5_Click(null, null);
                //}
            }
            
            if (Regex.Match(DateTime.Now.ToString("HH:mm:ss"), @"^(([01][\d])|([2][0-4])):00:00$").Success)  //每整点
            {
                btn_ybbwxx_Click(null, null);   //新冠有标本无信息                     
                backgroundWorker2.RunWorkerAsync();
                //if (serverip != "10.9.0.5")
                //{
                //    backgroundWorker3.RunWorkerAsync();
                //}
                btn_err_Click(null, null); //核酸上传异常
                Log.LogMeg("整点触发", "记录触发时间： " + DateTime.Now.ToString("HH:mm:ss"), ".\\log\\");
            }

            if (Regex.Match(DateTime.Now.ToString("HH:mm:ss"), @"^(([01][\d])|([2][0-4])):30:00$").Success)  //每半点 
            {
                if (serverip == "10.21.1.12") //10.21.1.12 深圳
                {
                    backgroundWorker2.RunWorkerAsync();
                    //button4_Click(null, null); //深圳采样点审核
                }
                //if (serverip != "10.9.0.5")
                //{
                //    backgroundWorker3.RunWorkerAsync();
                //}
                btn_err_Click(null, null); //核酸上传异常
                Log.LogMeg("半点触发", "记录触发时间： " + DateTime.Now.ToString("HH:mm:ss"), ".\\log\\");
            }

            //if (Regex.Match(DateTime.Now.ToString("HH:mm:ss"), @"^(([01][\d])|([2][0-4])):45:00$").Success)  //45分 
            //{    
            //    //backgroundWorker3.RunWorkerAsync();           
            //    //Log.LogMeg("45分触发", "记录触发时间： " + DateTime.Now.ToString("HH:mm:ss"), ".\\log\\");
            //}

            //if (DateTime.Now.ToString("HH:mm:ss") == "09:00:00")     //每天9点
            //{
            //    button2_Click(null, null);   //机场新冠每日统计 
            //    Log.LogMeg("机场新冠每日统计-自动", "记录触发时间： " + DateTime.Now.ToString("HH:mm:ss"), ".\\log\\");
            //}
        }
        #endregion

        #region 托盘
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
            this.Focus();
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)    //最小化到系统托盘
            {
                notifyIcon1.Visible = true;    //显示托盘图标
                this.Hide();    //隐藏窗口
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //注意判断关闭事件Reason来源于窗体按钮，否则用菜单退出时无法退出!
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;    //取消"关闭窗口"事件
                this.WindowState = FormWindowState.Minimized;    //使关闭时窗口向右下角缩小的效果
                notifyIcon1.Visible = true;
                this.Hide();
                return;
            }
        }

        #region 私有方法　处理窗体的　显示　隐藏　关闭(退出)
        private void ExitMainForm()
        {
            if (MessageBox.Show("您确定要退出程序吗？", "确认退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                this.notifyIcon1.Visible = false;
                this.Close();
                this.Dispose();
                Application.Exit();
            }
        }

        private void HideMainForm()
        {
            this.Hide();
        }

        private void ShowMainForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }
        #endregion
        
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitMainForm();
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMainForm();
        }

        private void 隐藏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HideMainForm();
        }

        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSet form_set = new FormSet();
            form_set.ShowDialog();
        }

        #endregion

    }
}
