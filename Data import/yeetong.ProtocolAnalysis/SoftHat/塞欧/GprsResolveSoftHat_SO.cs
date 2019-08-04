using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TCPAPI;
using Architecture;
using ToolAPI;
using Newtonsoft.Json;
using ProtocolAnalysis;
using ProtocolAnalysis.SoftHat.Mysql;
using ProtocolAnalysis.SoftHat;
using System.Timers;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Net.Sockets;
using System.Net;

public static class GprsResolveSoftHat_SO
{
    static Dictionary<string, Frame_CurrentList> IDCard_Id_list = new Dictionary<string, Frame_CurrentList>();
    static int time = int.Parse(ToolAPI.INIOperate.IniReadValue("goyo", "interval", MainStatic.Path));
    static System.Timers.Timer timer = new System.Timers.Timer();
    //需要推送的设备
    static System.Timers.Timer timerpop = new System.Timers.Timer();
    static Dictionary<string, string> timerpopList = new Dictionary<string, string>();

    static string delayTime = "";
    static GprsResolveSoftHat_SO()
    {
        timer.Enabled = true;
        timer.Interval = time * 60 * 1000;//time*60*1000; //执行间隔时间,单位为毫秒;
        timer.Elapsed += new ElapsedEventHandler((s, e) => OnTimedEvent(s, e));
        timer.Start();
        delayTime = ToolAPI.INIOperate.IniReadValue("DelayTimeConfig", "time", MainStatic.Path);

        timerpop.Enabled = true;
        timerpop.Interval = time * 47 * 1000;//time*60*1000; //执行间隔时间,单位为毫秒;
        timerpop.Elapsed += new ElapsedEventHandler((s, e) => OnTimedEventPop(s, e));
        timerpop.Start();
    }
    static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
    {
        timingTask();
    }
    static void OnTimedEventPop(object source, System.Timers.ElapsedEventArgs e)
    {
        UpdatePopListTask();
    }
    /// <summary>
    /// 心跳及实时数据解析
    /// </summary>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
    {
        if (c >= 19)
        {
            byte[] bytes = OnResolve(b, c);
            if (bytes != null)
                client.SendBuffer(bytes);
        }
        return "";
    }
    /// <summary>
    /// 实时数据
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    /// 实时数据53 30 30 37 34 35 32 30 30 30 35 38 46 31 38 30 30 0D 0A
    /// 心跳    53 30 30 37 34 35 32 30 30 30 30 30 30 30 30 30 30 0D 0A
    private static byte[] OnResolve(byte[] b, int bCount)
    {
        //延时时间
        int delay = 0;
        try
        {
            delay = Convert.ToInt32(delayTime);
        }
        catch
        {
            delay = 60;
        }
        //数据解析
        string DeviceNo = Encoding.ASCII.GetString(b, 1, 6);
        string IDCard_Id = Encoding.ASCII.GetString(b, 7, 8);
        //初始化
        if (IDCard_Id_list.ContainsKey(DeviceNo))
        {
            if (IDCard_Id_list[DeviceNo].IDCard_Id.ContainsKey(IDCard_Id))
            {
                IDCard_Id_list[DeviceNo].IDCard_Id[IDCard_Id] = DateTime.Now.AddSeconds(delay).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                IDCard_Id_list[DeviceNo].IDCard_Id.Add(IDCard_Id, DateTime.Now.AddSeconds(delay).ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
        else
        {
            Frame_CurrentList frame_CurrentList = new Frame_CurrentList();
            frame_CurrentList.DeviceNo = DeviceNo;
            frame_CurrentList.IDCard_Id.Add(IDCard_Id, DateTime.Now.AddSeconds(delay).ToString("yyyy-MM-dd HH:mm:ss"));
            IDCard_Id_list.Add(DeviceNo, frame_CurrentList);
        }
        //应答
        byte[] by = new byte[1];
        by[0] = 48;
        return by;
    }
    /// <summary>
    /// 定时任务
    /// </summary>
    public static void timingTask()
    {
        List<string> devL = new List<string>();
        DBFrame df = new DBFrame();
        df.contenthex = "";
        df.version = "01";
        df.datatype = "current";
        //遍历基站字典
        foreach (var item in IDCard_Id_list)
        {
            if (item.Value.IDCard_Id.Count > 0)
            {
                //赋值插入
                df.deviceid = item.Key;
                df.contentjson = JsonConvert.SerializeObject(item.Value);
                //清楚个数
                item.Value.IDCard_Id.Clear();
                //存储本地
                DB_MysqlHat.SaveSoftHat(df);
                //进行推送
                if (timerpopList.ContainsKey(df.deviceid))
                {
                    try
                    {
                        string[] ip_port = timerpopList[df.deviceid].Split(':');
                        Socket mClientSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        IPAddress ipaddress = null;
                        ipaddress = IPAddress.Parse(ip_port[0]);
                        IPEndPoint vEndPoint = new IPEndPoint(ipaddress, int.Parse(ip_port[1]));
                        byte[] vByte = Encoding.UTF8.GetBytes(df.contentjson);
                        int result = mClientSend.SendTo(vByte, vEndPoint);
                        mClientSend.Shutdown(SocketShutdown.Both);
                        mClientSend.Close();
                    }
                    catch (Exception ex) { ToolAPI.XMLOperation.WriteLogXmlNoTail("UDp异常", ex.Message); }
                }
            }
            else
            {
                devL.Add(item.Key);
            }
        }
        //清除2波都不存在的基站
        foreach(string item in  devL)
        {
            if (IDCard_Id_list.ContainsKey(item))
                IDCard_Id_list.Remove(item);
        }
    }

    //定时更新推送列表
    public static void UpdatePopListTask()
    {
        try
        {
            string sql = string.Format("select devid,ip,port from safety_hat where isenable=1");
            DataTable result = SIXH.DBUtility.DBoperateClass.DBoperateObj.ExecuteDataTable(sql, null, CommandType.Text);
            timerpopList.Clear();
            if (result!=null&& result.Rows.Count>0)
            {
                foreach(DataRow dataRow in result.Rows)
                {
                    string devid = dataRow["devid"].ToString();
                    string ip_port = dataRow["ip"].ToString()+":"+ dataRow["port"].ToString();
                    timerpopList.Add(devid, ip_port);
                }
            }
        }
        catch (Exception ex)
        {
            ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdatePopListTask异常", ex.Message);
        }
    }
}

