using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using 监控数据.Tools;

namespace 监控数据
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //private SqlCeHelper sqlce = new SqlCeHelper();
        SqlCeConnection cc = new SqlCeConnection();
        DataTable dt = new DataTable();

        private void Form1_Load(object sender, EventArgs e)
        {
            //sqlce.ExecNonQuery("insert into mailtophone(mail,phone) values (11,11)");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cc = new SqlCeConnection("Data Source=set.sdf");
            cc.Open();
            SqlCeDataAdapter ada = new SqlCeDataAdapter("select * from mailtophone", cc);
            ada.Fill(dt);
        }
    }
}
