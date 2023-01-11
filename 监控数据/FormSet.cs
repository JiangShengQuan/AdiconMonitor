using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace 监控数据
{
    public partial class FormSet : Form
    {
        public FormSet()
        {
            InitializeComponent();           
        }


        SqlCeHelper sqlcehelper = new SqlCeHelper();

        #region 保存
        private void button1_Click(object sender, EventArgs e)
        {
            GetSetINI.SetiniProfile("系统设置", "钉钉机器人Webhook", textBox1.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "钉钉机器人加签密钥", textBox18.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "新冠钉钉机器人Webhook", textBox20.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "新冠钉钉机器人加签密钥", textBox19.Text.Trim(), ".\\set.ini"); 

            GetSetINI.SetiniProfile("系统设置", "获取多少分钟内的数据", textBox2.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "不提醒的发件人", textBox7.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "混检表异常提醒人号码", textBox8.Text.Trim(), ".\\set.ini");

            string all,autostart,btx;
            if(checkBox1.Checked) { all = "1"; } else { all = "0"; }
            if (checkBox2.Checked) { autostart = "1"; } else { autostart = "0"; }
            if (checkBox3.Checked) { btx = "1"; } else { btx = "0"; }

            GetSetINI.SetiniProfile("系统设置", "atALL", all, ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "atMobiles", textBox13.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "自动开始", autostart, ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "不提醒保存条码数据失败错误", btx, ".\\set.ini");

            GetSetINI.SetiniProfile("系统设置", "常规对接邮箱账号", textBox9.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "常规对接邮箱密码", CryptoDES.EncryptDES(textBox10.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "新冠对接邮箱账号", textBox11.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "新冠对接邮箱密码", CryptoDES.EncryptDES(textBox12.Text.Trim(), "fzadicon"), ".\\set.ini");

            GetSetINI.SetiniProfile("系统设置", "常规解析邮箱账号", textBox24.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "常规解析邮箱密码", CryptoDES.EncryptDES(textBox23.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "新冠解析邮箱账号", textBox22.Text.Trim(), ".\\set.ini");
            GetSetINI.SetiniProfile("系统设置", "新冠解析邮箱密码", CryptoDES.EncryptDES(textBox21.Text.Trim(), "fzadicon"), ".\\set.ini");

            GetSetINI.SetiniProfile("LIS库连接设置", "服务器名称", CryptoDES.EncryptDES(textBox6.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LIS库连接设置", "数据库名称", CryptoDES.EncryptDES(textBox5.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LIS库连接设置", "登录名", CryptoDES.EncryptDES(textBox3.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LIS库连接设置", "密码", CryptoDES.EncryptDES(textBox4.Text.Trim(), "fzadicon"), ".\\set.ini");

            GetSetINI.SetiniProfile("LISDB库连接设置", "服务器名称", CryptoDES.EncryptDES(textBox14.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LISDB库连接设置", "数据库名称", CryptoDES.EncryptDES(textBox16.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LISDB库连接设置", "登录名", CryptoDES.EncryptDES(textBox17.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LISDB库连接设置", "密码", CryptoDES.EncryptDES(textBox15.Text.Trim(), "fzadicon"), ".\\set.ini");

            MessageBox.Show("设置成功，请重新打开程序！", "提示");
            System.Environment.Exit(0);
        }
        #endregion

        #region 窗口加载
        private void Form_Set_Load(object sender, EventArgs e)
        {
            textBox1.Text = GetSetINI.GetiniProfile("系统设置", "钉钉机器人Webhook", "", 128, ".\\set.ini");
            textBox18.Text = GetSetINI.GetiniProfile("系统设置", "钉钉机器人加签密钥", "", 128, ".\\set.ini");
            textBox20.Text = GetSetINI.GetiniProfile("系统设置", "新冠钉钉机器人Webhook", "", 128, ".\\set.ini");
            textBox19.Text = GetSetINI.GetiniProfile("系统设置", "新冠钉钉机器人加签密钥", "", 128, ".\\set.ini");

            textBox2.Text = GetSetINI.GetiniProfile("系统设置", "获取多少分钟内的数据", "10", 128, ".\\set.ini");
            textBox7.Text = GetSetINI.GetiniProfile("系统设置", "不提醒的发件人", "", 1024, ".\\set.ini");
            textBox8.Text = GetSetINI.GetiniProfile("系统设置", "混检表异常提醒人号码", "", 128, ".\\set.ini");
            textBox13.Text = GetSetINI.GetiniProfile("系统设置", "atMobiles", "", 5000, ".\\set.ini");

            textBox9.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "常规对接邮箱账号", "", 128, ".\\set.ini"), "fzadicon");
            textBox10.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "常规对接邮箱密码", "", 128, ".\\set.ini"), "fzadicon");
            textBox11.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "新冠对接邮箱账号", "", 128, ".\\set.ini"), "fzadicon");
            textBox12.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "新冠对接邮箱密码", "", 128, ".\\set.ini"), "fzadicon");

            textBox24.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "常规解析邮箱账号", "", 128, ".\\set.ini"), "fzadicon");
            textBox23.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "常规解析邮箱密码", "", 128, ".\\set.ini"), "fzadicon");
            textBox22.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "新冠解析邮箱账号", "", 128, ".\\set.ini"), "fzadicon");
            textBox21.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("系统设置", "新冠解析邮箱密码", "", 128, ".\\set.ini"), "fzadicon");             

            textBox3.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "登录名", "lisuser", 128, ".\\set.ini"), "fzadicon");
            textBox4.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "密码", "lisuser", 128, ".\\set.ini"), "fzadicon");
            textBox6.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "服务器名称", "10.5.0.1", 128, ".\\set.ini"), "fzadicon");
            textBox5.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LIS库连接设置", "数据库名称", "lis", 128, ".\\set.ini"), "fzadicon");

            textBox17.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "登录名", "lisuser", 128, ".\\set.ini"), "fzadicon");
            textBox15.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "密码", "lisuser", 128, ".\\set.ini"), "fzadicon");
            textBox14.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "服务器名称", "10.5.0.1", 128, ".\\set.ini"), "fzadicon");
            textBox16.Text = CryptoDES.DecryptDES(GetSetINI.GetiniProfile("LISDB库连接设置", "数据库名称", "lisdb", 128, ".\\set.ini"), "fzadicon");

            if (GetSetINI.GetiniProfile("系统设置", "atALL", "", 128, ".\\set.ini") == "1")
            {
                checkBox1.Checked = true;
                textBox13.Enabled = false;
            }
            else
            {
                checkBox1.Checked = false;
                textBox13.Enabled = true;
            }

            if (GetSetINI.GetiniProfile("系统设置", "自动开始", "", 128, ".\\set.ini") == "1")
            {
                checkBox2.Checked = true;
            }
            else
            {
                checkBox2.Checked = false;
            }

            if (GetSetINI.GetiniProfile("系统设置", "不提醒保存条码数据失败错误", "", 128, ".\\set.ini") == "1")
            {
                checkBox3.Checked = true;
            }
            else
            {
                checkBox3.Checked = false;
            }

        }
        #endregion

        #region LIS库连接测试
        private void button3_Click(object sender, EventArgs e)
        {
            SqlHelper.connStr = String.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}", textBox6.Text, textBox5.Text, textBox3.Text, textBox4.Text);
            try
            {
                SqlConnection mySqlConnection = new SqlConnection(SqlHelper.connStr);//创建SqlConnection对象并连接到数据库
                SqlCommand mySqlCommand = mySqlConnection.CreateCommand();//创建SqlCommand对象
                mySqlConnection.Open();//用Connection对象的Open()方法打开数据库
                MessageBox.Show("连接成功!");
            }
            catch
            {
                MessageBox.Show("连接有误,请修改!");
                return;
            }

            GetSetINI.SetiniProfile("LIS库连接设置", "服务器名称", CryptoDES.EncryptDES(textBox6.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LIS库连接设置", "数据库名称", CryptoDES.EncryptDES(textBox5.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LIS库连接设置", "登录名", CryptoDES.EncryptDES(textBox3.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LIS库连接设置", "密码", CryptoDES.EncryptDES(textBox4.Text.Trim(), "fzadicon"), ".\\set.ini");
        }
        #endregion

        #region LISDB库连接测试
        private void button4_Click(object sender, EventArgs e)
        {
            SqlHelper.connStrLISDB = String.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}", textBox14.Text, textBox16.Text, textBox17.Text, textBox15.Text);
            try
            {
                SqlConnection mySqlConnection = new SqlConnection(SqlHelper.connStrLISDB);//创建SqlConnection对象并连接到数据库
                SqlCommand mySqlCommand = mySqlConnection.CreateCommand();//创建SqlCommand对象
                mySqlConnection.Open();//用Connection对象的Open()方法打开数据库
                MessageBox.Show("连接成功!");
            }
            catch
            {
                MessageBox.Show("连接有误,请修改!");
                return;
            }

            GetSetINI.SetiniProfile("LISDB库连接设置", "服务器名称", CryptoDES.EncryptDES(textBox14.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LISDB库连接设置", "数据库名称", CryptoDES.EncryptDES(textBox16.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LISDB库连接设置", "登录名", CryptoDES.EncryptDES(textBox17.Text.Trim(), "fzadicon"), ".\\set.ini");
            GetSetINI.SetiniProfile("LISDB库连接设置", "密码", CryptoDES.EncryptDES(textBox15.Text.Trim(), "fzadicon"), ".\\set.ini");
        }
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox13.Enabled = false;
            }
            else
            {
                textBox13.Enabled = true;
            }
        }               

    }
}
