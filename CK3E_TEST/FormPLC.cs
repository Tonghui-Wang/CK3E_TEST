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
    public partial class FormPLC : Form
    {
        private ALL all;

        public FormPLC()
        {
            InitializeComponent();
            all = ALL.GetInstance();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            all.PLC.Open();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            all.PLC.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12;
            n1 = (all.PLC.Status[0] & 0x01) == 0x01 ? "急停报警" : "无急停";
            n2 = (all.PLC.Status[0] & 0x02) == 0x02 ? "自动模式" : "手动模式";
            n3 = (all.PLC.Status[0] & 0x04) == 0x04 ? "正压异常" : "正压正常";
            n4 = (all.PLC.Status[0] & 0x08) == 0x08 ? "市电异常" : "市电正常";
            n5 = (all.PLC.Status[0] & 0x10) == 0x10 ? "机器人1控制器异常" : "机器人1控制器正常";
            n6 = (all.PLC.Status[0] & 0x20) == 0x20 ? "机器人2控制器异常" : "机器人2控制器正常";
            n7 = (all.PLC.Status[0] & 0x40) == 0x40 ? "机器人1驱动器上电" : "机器人1驱动器下电";
            n8 = (all.PLC.Status[0] & 0x80) == 0x80 ? "机器人2驱动器上电" : "机器人1驱动器下电";
            n9 = (all.PLC.Status[1] & 0x01) == 0x01 ? "机器人1液位a过低" : "机器人1液位a正常";
            n10 = (all.PLC.Status[1] & 0x02) == 0x02 ? "机器人1液位b过低" : "机器人1液位b正常";
            n11 = (all.PLC.Status[1] & 0x04) == 0x04 ? "机器人2液位a过低" : "机器人2液位a过低";
            n12 = (all.PLC.Status[1] & 0x08) == 0x08 ? "机器人2液位b过低" : "机器人1液位b过低";

            textBox1.Text = n1 + "\r\n" + n2 + "\r\n" + n3 + "\r\n" + n4 + "\r\n"
                + n5+ "\r\n" + n6 + "\r\n" + n7 + "\r\n" + n8 + "\r\n"
                + n9 + "\r\n" + n10 + "\r\n" + n11 + "\r\n" + n12 + "\r\n";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int num1 = int.Parse(textBox2.Text);
            bool num2 = bool.Parse(textBox3.Text);
            all.PLC.Operate(num1, num2);
        }
    }
}
