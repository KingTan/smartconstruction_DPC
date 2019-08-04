using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GOYO_Architecture;
using GOYO_ProtocolAnalysis;
using SIXH.DBUtility;
using ToolAPI;

namespace WinFromStart
{
public partial class Form1 : Form
    {
        MainClass mc = new MainClass();
        public Form1()
        {
            //byte[] temp = new byte[] {0x06,0x03,0x04,0x00,0x00,0x00,0x00,0x8C,0xF3 };
            //GprsResolveGasDetection.crc16_modbus(temp,temp.Length);
            //byte[] fa = ToolAPI.ValueTypeToByteArray.GetBytes_BigEndian(2.1f);



            //byte[] temp = new byte[] { 0xCC,0xCD,0x41,0xA0 };
            //byte[] temp1 = new byte[] { 0xCD, 0xCC, 0xA0, 0x41 };
            //float a1 = ToolAPI.ByteArrayToValueType.GetSingle_LittleEndian(temp, 0);
            //float a2 = ToolAPI.ByteArrayToValueType.GetSingle_BigEndian(temp, 0);
            //float a3 = ToolAPI.ByteArrayToValueType.GetSingle_LittleEndian(temp1, 0);
            //float a4 = ToolAPI.ByteArrayToValueType.GetSingle_BigEndian(temp1, 0);

            InitializeComponent();
            pictureBox1.Image.Tag = "close";
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                mc.App_Close();
                Application.Exit();
            }
            catch (Exception)
            { }
        }

        

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image.Tag.ToString()=="open")
            {
                pictureBox1.Image = WinFromStart.Properties.Resources.close;
                pictureBox1.Image.Tag = "close";
                mc.App_Close();
            }
            else
            {
                pictureBox1.Image = WinFromStart.Properties.Resources.open;
                pictureBox1.Image.Tag = "open";

                UdpSever udps = new UdpSever();
                udps.eventTrigger += ProtocolAnalysisSE_MainUdp.ProtocolPackageUdpResolver;
                udps.Listener(int.Parse(MainStatic.Port));

                //解析
                Subject sub = new Subject();
                sub.DataAnalysis += ProtocolAnalysisSE_Main.ProtocolPackageResolver;
                //命令下发
                CommandIssued_Main.CommandIssued_MainInit();
                sub.CommandSending += CommandIssued_Main.CommandIssuedInitEvent;
                mc.App_Open(sub);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        #region 窗体移动
        private bool isMouseDown = false;
        private Point FormLocation;     //form的location
        private Point mouseOffset;      //鼠标的按下位置
        private void FormMove_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = true;
                FormLocation = this.Location;
                mouseOffset = Control.MousePosition;
            }
        }
        private void FormMove_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }

        private void FormMove_MouseMove(object sender, MouseEventArgs e)
        {
            int _x = 0;
            int _y = 0;
            if (isMouseDown)
            {
                Point pt = Control.MousePosition;
                _x = mouseOffset.X - pt.X;
                _y = mouseOffset.Y - pt.Y;

                this.Location = new Point(FormLocation.X - _x, FormLocation.Y - _y);
            }

        }
        #endregion
    }
}
