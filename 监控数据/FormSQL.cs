using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 监控数据
{
    public partial class FormSQL : Form
    {
        public FormSQL(string textValue)
        {
            InitializeComponent();
            textBox1.Text = textValue;
        }

        private void FormSQL_Load(object sender, EventArgs e)
        {
             
        }
    }
}
