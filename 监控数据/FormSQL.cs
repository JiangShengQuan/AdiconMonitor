using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using 监控数据.Models;
using 监控数据.Tools;
//using System.Data.SqlServerCe;

namespace 监控数据
{
    public partial class FormSQL : Form
    {
        SqlCeHelper sqlcehelper = new SqlCeHelper();
        SQLiteHelper sqlitehelper = new SQLiteHelper();
        List<SqlQueryModel> sqlquerylist = new List<SqlQueryModel>(); //邮件解析
        DataTable DT_SqlQuery;
        public FormSQL()
        {
            InitializeComponent();
            Get_SqlQuery();
        }

        private void Get_SqlQuery() {
            DT_SqlQuery = sqlitehelper.Query("select id,fname,initialization,query,remark,useflag,WEB_HOOK,secret from SqlQuery");
        }

        private void FormSQL_Load(object sender, EventArgs e)
        {
            //this.comboBox1.DataSource = this.sqlitehelper.Query("select fname from SqlQuery");  //设置ComboBox的数据源
            this.comboBox1.DataSource = DT_SqlQuery;
            this.comboBox1.DisplayMember = "fname";    //让ComboBox显示fname列
            this.comboBox1.ValueMember = "fname";  //让ComboBox实际的值为fname列 
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string sql = "update SqlQuery set query =\"" + textBox1.Text.Trim() + "\"";
            if (checkBox1.Checked)
            {
                sql = sql + ",useflag = 1 ";
            }
            else
            {
                sql = sql + ",useflag = 0 ";
            }       
            sql = sql + " ,WEB_HOOK=\"" + textBox_webhook.Text.Trim() + "\" ,secret=\"" + textBox_secret.Text.Trim() + "\" where fname =\"" + label1.Text.Trim() + "\"";
            int i = this.sqlitehelper.ExecNonQuery(sql); 
            if (i > 0)
            {
                MessageBox.Show("保存成功！","提示");
            }
            else
            {
                MessageBox.Show("保存失败！","提示");
            }
            Get_SqlQuery();
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            label1.Text = this.comboBox1.SelectedValue.ToString();
            if (label1.Text != "System.Data.DataRowView")
            {
                //textBox1.Text = this.sqlitehelper.Query("select query from SqlQuery where fname='" + label1.Text + "'").Rows[0][0].ToString();

                //DataRow[] dr = dt.Select("fname = '" + label1.Text + "' ");
                //textBox1.Text = dr[0][2].ToString();

                textBox1.Text = DT_SqlQuery.Select("fname = '" + label1.Text + "' ")[0][3].ToString();

                int useflag = Convert.ToInt16(DT_SqlQuery.Select("fname = '" + label1.Text + "' ")[0][5]);
                if (useflag == 1)
                {
                    checkBox1.Checked = true;
                }
                else
                {
                    checkBox1.Checked = false;
                }

                //textBox_webhook.Text = this.sqlitehelper.Query("select WEB_HOOK from SqlQuery where fname='" + label1.Text + "'").Rows[0][0].ToString();
                //textBox_secret.Text = this.sqlitehelper.Query("select secret from SqlQuery where fname='" + label1.Text + "'").Rows[0][0].ToString();

                textBox_webhook.Text = DT_SqlQuery.Select("fname = '" + label1.Text + "' ")[0][6].ToString();
                textBox_secret.Text = DT_SqlQuery.Select("fname = '" + label1.Text + "' ")[0][7].ToString();
            }            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //label1.Text = this.comboBox1.SelectedValue.ToString();
            //if (label1.Text != "System.Data.DataRowView")  
            //{
            //    //MessageBox.Show(label1.Text);
            //    textBox1.Text = this.sqlitehelper.Query("select query from SqlQuery where fname='" + label1.Text + "'").Rows[0][0].ToString();
            //}            
        }

        /// <summary>
        /// 获取默认语句
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //textBox1.Text = this.sqlitehelper.Query("select initialization from SqlQuery where fname='" + label1.Text + "'").Rows[0][0].ToString();
            textBox1.Text = DT_SqlQuery.Select("fname = '" + label1.Text + "' ")[0][2].ToString();
        }

        private void btn_sql_copy_Click(object sender, EventArgs e)
        {
            string msg = textBox1.Text;
            Clipboard.SetDataObject(msg);
            //MessageBox.Show(msg, "SQL语句");
        }

        private void btn_sql_clear_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }
    }
}
