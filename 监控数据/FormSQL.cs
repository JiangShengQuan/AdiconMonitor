using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using 监控数据.Tools;
//using System.Data.SqlServerCe;

namespace 监控数据
{
    public partial class FormSQL : Form
    {
        SqlCeHelper sqlcehelper = new SqlCeHelper();
        SQLiteHelper sqlitehelper = new SQLiteHelper();
        public FormSQL()
        {
            InitializeComponent();
        }

        private void FormSQL_Load(object sender, EventArgs e)
        {
            this.comboBox1.DataSource = this.sqlitehelper.Query("select fname from SqlQuery");  //设置ComboBox的数据源
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
            //this.sqlcehelper.ExecNonQuery("update SqlQuery set pvalue ='" + dateTimePicker2.Value + "' where pcode='Timedpush' ");
            string sql = "update SqlQuery set query =\"" + textBox1.Text.Trim() + "\" where fname=\"" + label1.Text + "\"";
            //string newsql = sql.Replace("'", "\"");
            int i = this.sqlitehelper.ExecNonQuery(sql);  //  .Replace("\n", "").Replace("\t", "").Replace("\r", "")
            if (i > 0)
            {
                MessageBox.Show("保存成功！","提示");
            }
            else
            {
                MessageBox.Show("保存失败！","提示");
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            label1.Text = this.comboBox1.SelectedValue.ToString();
            if (label1.Text != "System.Data.DataRowView")
            {
                textBox1.Text = this.sqlitehelper.Query("select query from SqlQuery where fname='" + label1.Text + "'").Rows[0][0].ToString();
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
            textBox1.Text = this.sqlitehelper.Query("select initialization from SqlQuery where fname='" + label1.Text + "'").Rows[0][0].ToString();
        }
    }
}
