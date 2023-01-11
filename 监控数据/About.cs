using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 监控数据
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://note.youdao.com/noteshare?id=8b68917bb44c6f93d00001cd61fa8f99&sub=8204D83BB7E84903BAFA86B1FF5D5DA2");
        }
    }
}
