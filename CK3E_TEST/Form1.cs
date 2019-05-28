using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CK3E_TEST
{
    public partial class Form1 : Form
    {
        private int num = 2;
        private ContactCk3e[] pmac;

        public ContactCk3e[] PMAC
        {
            get { return pmac; }
        }

        public Form1()
        {
            InitializeComponent();

            pmac = new ContactCk3e[num];
            for (int i = 0; i < num; i++)
            {
                pmac[i] = new ContactCk3e(i);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool success = PMAC[0].Open();
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
            bool success = PMAC[0].Close();
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
                ans = PMAC[0].Send(textBox1.Text);
                MessageBox.Show(ans);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string error = null;
            string[] program = { };
            
            bool success = PMAC[0].Download(program, out error);

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
