using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CK3E_TEST
{
    public partial class FormCK3E : Form
    {
        private ALL all;

        public FormCK3E()
        {
            InitializeComponent();
            all = ALL.GetInstance();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool success = all.CK3E[0].Open();
            if (success)
            {
                MessageBox.Show("连接成功");
            }
            else
            {
                MessageBox.Show("连接失败");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool success = all.CK3E[0].Close();
            if (success)
            {
                MessageBox.Show("关闭成功");
            }
            else
            {
                MessageBox.Show("关闭失败");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string ans = null;
            if (textBox1.Text != null)
            {
                ans = all.CK3E[0].Send(textBox1.Text);
                MessageBox.Show(ans);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string error = null;
            string path = System.AppDomain.CurrentDomain.BaseDirectory + @"JH.txt";
            string[] program = all.CK3E[0].ReadFile(path);
            //string[] program = {"&3", "A", "M4001=1", "close", "undefine all", "delete all", "delete gather",
            //    "close", "&3", "#1->b", "#2->x", "#3->y", "#4->z", "#5->u", "#6->v", "#7->w", "#8->a",
            //    "&3 define rotary 200", "i42=0", "#1j/", "#2j/", "#3j/", "#4j/", "#5j/", "#6j/", "#7j/",
            //    "#8j/", "open rot", "clear", "close", "b3 r", "open rot", "spline2", "abs", "Dwell 0",
            //    "M4001=2"};

            bool success = all.CK3E[0].Download(program, out error);

            if (success)
            {
                MessageBox.Show("下载成功");
            }
            else
            {
                MessageBox.Show("下载失败");
            }
        }
    }
}
