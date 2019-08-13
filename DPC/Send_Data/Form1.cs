using Newtonsoft.Json;
using ProtocolAnalysis.Iot_v1.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Send_Data
{
    public partial class 数据制造器 : Form
    {
        public 数据制造器()
        {
            InitializeComponent();
        }
        #region 塔吊
        /// <summary>
        /// 塔吊报警循环
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "请稍后。。。。。。"; label1.Refresh();
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"tower\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"is_warning\":\"Y\",\"warning_type\":[\"04\",\"05\"],\"timestamp\":1565338446000,\"weight\":231,\"height\":231,\"range\":231,\"rotation\":231,\"moment_forces\":231,\"wind_grade\":2,\"wind_speed\":231,\"dip_x\":000,\"dip_y\":000,\"boom_arm_length\":60,\"blance_arm_length\":1,\"tower_body_height\":55,\"tower_hat_height\":2,\"driver_id_code\":\"411402188754652254\"}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Tower_send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Tower_send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.weight = Class1.NextDouble1(new Random(), 0.3, 5.0);
                tower_Send_Frame.height = Class1.NextDouble1(new Random(), 10, 55);
                tower_Send_Frame.is_warning = "Y";
                tower_Send_Frame.warning_type = new string[] { "04" };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第二帧
                tower_Send_Frame.weight = Class1.NextDouble1(new Random(), 0.3, 5.0);
                tower_Send_Frame.height = Class1.NextDouble1(new Random(), 10, 55);
                tower_Send_Frame.is_warning = "Y";
                tower_Send_Frame.warning_type = new string[] { "05" };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第三帧
                tower_Send_Frame.weight = 0.0d;
                tower_Send_Frame.height = Class1.NextDouble1(new Random(), 10, 55);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }
        /// <summary>
        /// 塔吊不报警循环
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = "请稍后。。。。。。"; label1.Refresh();
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"tower\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"is_warning\":\"Y\",\"warning_type\":[\"04\",\"05\"],\"timestamp\":1565338446000,\"weight\":231,\"height\":231,\"range\":231,\"rotation\":231,\"moment_forces\":231,\"wind_grade\":2,\"wind_speed\":231,\"dip_x\":000,\"dip_y\":000,\"boom_arm_length\":60,\"blance_arm_length\":1,\"tower_body_height\":55,\"tower_hat_height\":2,\"driver_id_code\":\"411402188754652254\"}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Tower_send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Tower_send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.weight = Class1.NextDouble1(new Random(), 0.3, 5.0);
                tower_Send_Frame.height = Class1.NextDouble1(new Random(), 10, 55);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第二帧
                tower_Send_Frame.weight = Class1.NextDouble1(new Random(), 0.3, 5.0);
                tower_Send_Frame.height = Class1.NextDouble1(new Random(), 10, 55);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第三帧
                tower_Send_Frame.weight = 0.0d;
                tower_Send_Frame.height = Class1.NextDouble1(new Random(), 10, 55);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }
        /// <summary>
        /// 塔吊不满足
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            label1.Text = "请稍后。。。。。。"; label1.Refresh();
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"tower\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"is_warning\":\"Y\",\"warning_type\":[\"04\",\"05\"],\"timestamp\":1565338446000,\"weight\":231,\"height\":231,\"range\":231,\"rotation\":231,\"moment_forces\":231,\"wind_grade\":2,\"wind_speed\":231,\"dip_x\":000,\"dip_y\":000,\"boom_arm_length\":60,\"blance_arm_length\":1,\"tower_body_height\":55,\"tower_hat_height\":2,\"driver_id_code\":\"411402188754652254\"}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Tower_send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Tower_send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.weight = 0.1;
                tower_Send_Frame.height = Class1.NextDouble1(new Random(), 10, 55);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第二帧
                tower_Send_Frame.weight = 0;
                tower_Send_Frame.height = Class1.NextDouble1(new Random(), 10, 55);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);

                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }
        #endregion

        #region 升降机
        private void button6_Click(object sender, EventArgs e)
        {
            label1.Text = "请稍后。。。。。。"; label1.Refresh();
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"lift\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"is_warning\":\"Y\",\"warning_type\":[\"04\",\"12\"],\"timestamp\":1565338446000,\"weight\":231,\"height\":231,\"floor\":2,\"peoples\":5,\"speed\":231,\"wind_grade\":2,\"wind_speed\":231,\"dip_x\":000,\"dip_y\":000,\"floor_height\":80,\"door_status\":\"1&0&0\",\"driver_id_code\":\"411402188754652254\"}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Lift_send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Lift_send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.speed = Class1.NextDouble1(new Random(), 0.1, 5.0);
                tower_Send_Frame.is_warning = "Y";
                tower_Send_Frame.warning_type = new string[] { "04" };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第二帧
                tower_Send_Frame.speed = Class1.NextDouble1(new Random(), 0.1, 5.0);
                tower_Send_Frame.is_warning = "Y";
                tower_Send_Frame.warning_type = new string[] { "10" };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第三帧
                tower_Send_Frame.speed = 0;
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }
        private void button5_Click(object sender, EventArgs e)
        {
            label1.Text = "请稍后。。。。。。"; label1.Refresh();
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"lift\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"is_warning\":\"Y\",\"warning_type\":[\"04\",\"12\"],\"timestamp\":1565338446000,\"weight\":231,\"height\":231,\"floor\":2,\"peoples\":5,\"speed\":231,\"wind_grade\":2,\"wind_speed\":231,\"dip_x\":000,\"dip_y\":000,\"floor_height\":80,\"door_status\":\"1&0&0\",\"driver_id_code\":\"411402188754652254\"}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Lift_send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Lift_send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.speed = Class1.NextDouble1(new Random(), 0.1, 5.0);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第二帧
                tower_Send_Frame.speed = Class1.NextDouble1(new Random(), 0.1, 5.0);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第三帧
                tower_Send_Frame.speed = 0;
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }
        private void button4_Click(object sender, EventArgs e)
        {
            label1.Text = "请稍后。。。。。。"; label1.Refresh();
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"lift\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"is_warning\":\"Y\",\"warning_type\":[\"04\",\"12\"],\"timestamp\":1565338446000,\"weight\":231,\"height\":231,\"floor\":2,\"peoples\":5,\"speed\":231,\"wind_grade\":2,\"wind_speed\":231,\"dip_x\":000,\"dip_y\":000,\"floor_height\":80,\"door_status\":\"1&0&0\",\"driver_id_code\":\"411402188754652254\"}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Lift_send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Lift_send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.speed = 0;
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
                label1.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region 卸料 
        private void button9_Click(object sender, EventArgs e)
        {
            label1.Text = "请稍后。。。。。。"; label1.Refresh();
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"discharge\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"is_warning\":\"Y\",\"warning_type\":[\"04\"],\"timestamp\":1565338446000,\"weight\":231,\"dip_x\":000,\"dip_y\":000}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Discharge__send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Discharge__send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.weight = Class1.NextDouble1(new Random(), 0.1, 5.0);
                tower_Send_Frame.is_warning = "Y";
                tower_Send_Frame.warning_type = new string[] { "04" };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第二帧
                tower_Send_Frame.weight = Class1.NextDouble1(new Random(), 0.1, 5.0);
                tower_Send_Frame.is_warning = "Y";
                tower_Send_Frame.warning_type = new string[] { "10" };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第三帧
                tower_Send_Frame.weight = 0;
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }
        private void button8_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"discharge\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"is_warning\":\"Y\",\"warning_type\":[\"04\"],\"timestamp\":1565338446000,\"weight\":231,\"dip_x\":000,\"dip_y\":000}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Discharge__send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Discharge__send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.weight = Class1.NextDouble1(new Random(), 0.1, 5.0);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第二帧
                tower_Send_Frame.weight = Class1.NextDouble1(new Random(), 0.1, 5.0);
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                //第三帧
                tower_Send_Frame.weight = 0;
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                sendstring = JsonConvert.SerializeObject(temp);
                t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            label1.Text = "请稍后。。。。。。"; label1.Refresh();
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"discharge\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"is_warning\":\"Y\",\"warning_type\":[\"04\"],\"timestamp\":1565338446000,\"weight\":231,\"dip_x\":000,\"dip_y\":000}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Discharge__send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Discharge__send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.weight = 0;
                tower_Send_Frame.is_warning = "N";
                tower_Send_Frame.warning_type = new string[] { };
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                Thread.Sleep(1000);
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }

        #endregion

        #region 扬尘噪音
        private void button10_Click(object sender, EventArgs e)
        {
            label1.Text = "请稍后。。。。。。"; label1.Refresh();
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"dust_noise\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"timestamp\":1565338446000,\"pm2_5\":231,\"pm10\":000,\"tsp\":000,\"noise\":1300,\"temperature\":2000,\"humidity\":4500,\"wind_speed\":030,\"wind_grade\":1,\"wind_direction\":000,\"air_pressure\":000,\"rainfall\":000}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Dust_noise__send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Dust_noise__send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.pm2_5 = Class1.NextDouble1(new Random(), 5, 150);
                tower_Send_Frame.pm10 = Class1.NextDouble1(new Random(), 5, 150);
                tower_Send_Frame.noise = Class1.NextDouble1(new Random(), 5, 70);
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }
        private void button11_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"dust_noise\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"sn\":\"12345678\",\"timestamp\":1565338446000,\"pm2_5\":231,\"pm10\":000,\"tsp\":000,\"noise\":1300,\"temperature\":2000,\"humidity\":4500,\"wind_speed\":030,\"wind_grade\":1,\"wind_direction\":000,\"air_pressure\":000,\"rainfall\":000}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Dust_noise__send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Dust_noise__send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.pm2_5 = Class1.NextDouble1(new Random(), 200, 500);
                tower_Send_Frame.pm10 = Class1.NextDouble1(new Random(), 200, 500);
                tower_Send_Frame.noise = Class1.NextDouble1(new Random(), 100, 170);
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }
        #endregion

        #region 人员考勤
        private void button12_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            try
            {
                string datastring = "{\"frame_type\":\"real_time_data\",\"equipment_type\":\"personnel\",\"time_stamp\":\"2019-08-0822:27:00\",\"frame_token\":\"z7d8jfgn39ki987779jh2\",\"short_link\":\"true\",\"data\":{\"project_code\":\"123456\",\"sn\":\"13246\",\"gate_no\":\"145\",\"timestamp\":1565338446000,\"channel_no\":\"1\",\"cert_mode\":\"03\",\"in_or_out\":\"01\",\"personal_id_code\":\"421546325978541124\",\"features_code\":\"\"}}";
                Send_frame temp = JsonConvert.DeserializeObject<Send_frame>(datastring);
                Personnel_send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Personnel_send_frame>(temp.data.ToString());
                //第一帧
                tower_Send_Frame.timestamp = Class1.ConvertDateTimeLong(DateTime.Now);
                temp.data = tower_Send_Frame;
                temp.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("39.104.228.149", 6000);
                string sendstring = JsonConvert.SerializeObject(temp);
                int t = client.Send(Encoding.UTF8.GetBytes(sendstring));
                client.Close();
                MessageBox.Show("完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            label1.Text = "";
        }
        #endregion


    }
}
