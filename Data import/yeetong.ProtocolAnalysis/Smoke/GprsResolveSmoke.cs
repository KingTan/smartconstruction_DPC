using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
using Architecture;
using ToolAPI;
using Newtonsoft.Json;
using ProtocolAnalysis;
using ProtocolAnalysis.Smoke;

public static class GprsResolveSmoke
{
    public static string OnResolveRecvMessage(byte[] b, UdpState client)
    {

        DBFrame df = new DBFrame();
        df.contenthex = ConvertData.ToHexString(b, 0, b.Length);
        df.version = "01";
        ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\Smoke", "Smoke数据原包", df.contenthex);
        switch (b[0])
        {
            case 0x01: //注册
                OnResolve_Register(b,ref df);
                break;
            case 0x02: //心跳
                OnResolve_HeartBeat(b,ref df);
                break;
            case 0x6A: //实时数据
                OnResolve_Current(b,ref df);
                break;
            default:
                break;
        }
        return "";
    }
    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    /// <param name="df"></param>
    private static void OnResolve_Register(byte[] b, ref DBFrame df)
    {
        try
        {
            Frame_HeartRegister register = new Frame_HeartRegister();
            register.DeviceNo = ConvertData.ToHexString(b, 1, 8); //设备号
            register.RecTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //时间
            df.contentjson = JsonConvert.SerializeObject(register);
            df.datatype = "register";
            df.deviceid = register.DeviceNo;
            if (!string.IsNullOrEmpty(df.contentjson))
                DB_MysqlSmoke.SaveSmoke(df);
        }
        catch (Exception ex) { XMLOperation.WriteLogXmlNoTail("烟感注册数据错误信息", ex.Message); }
    }
    /// <summary>
    /// 心跳
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    /// <param name="df"></param>
    private static void OnResolve_HeartBeat(byte[] b, ref DBFrame df)
    {
        try
        {
            Frame_HeartRegister heartbeat = new Frame_HeartRegister();
            heartbeat.DeviceNo = ConvertData.ToHexString(b, 1, 8); //设备号
            heartbeat.RecTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //时间
            df.contentjson = JsonConvert.SerializeObject(heartbeat);
            df.datatype = "heartbeat";
            df.deviceid = heartbeat.DeviceNo;
            if (!string.IsNullOrEmpty(df.contentjson))
                DB_MysqlSmoke.SaveSmoke(df);
        }
        catch (Exception ex) { XMLOperation.WriteLogXmlNoTail("烟感心跳数据错误信息", ex.Message); }
    }
    /// <summary>
    /// 实时数据
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    /// <param name="df"></param>
    private static void OnResolve_Current(byte[] b, ref DBFrame df)
    {
        try
        {
            if (b[9] != 0x00 && b[10] != 0x04)
                return;
            string str = ConvertData.ToHexString(b, 0, b.Length);
            XMLOperation.WriteLogXmlNoTail("烟感实时数据", str);
            Frame_SmokeCurrent current=new Frame_SmokeCurrent();
            current.DeviceNo = ConvertData.ToHexString(b, 11, 8);//设备号
            uint Uint = ToolAPI.ByteArrayToValueType.GetUInt32_BigEndian(b, 43);
            current.AlarmNum = Convert.ToString(Uint, 2).PadLeft(32, '0');  //报警码
            current.BatteryVage = (ToolAPI.ByteArrayToValueType.GetUInt16_BigEndian(b, 47) / 100).ToString("0.00"); //电池电压
            current.NBsignal = ConvertData.ToHexString(b, 49, 1);  //NB信号值
            current.Temperature = (ToolAPI.ByteArrayToValueType.GetUInt16_BigEndian(b, 50) / 10).ToString("0.00");  //温度
            current.Rtc = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            df.contentjson = JsonConvert.SerializeObject(current);
            df.datatype = "current";
            df.deviceid = current.DeviceNo;
            if (!string.IsNullOrEmpty(df.contentjson))
                DB_MysqlSmoke.SaveSmoke(df);
        }
        catch (Exception ex){ XMLOperation.WriteLogXmlNoTail("烟感实时数据错误信息", ex.Message); }
    }
}
#region Model
public class Frame_HeartRegister
{
    /// <summary>
    /// 设备号
    /// </summary>
    public string DeviceNo
    {
        get;
        set;
    }
    /// <summary>
    /// 接收时间
    /// </summary>
    public string RecTime
    {
        get;
        set;
    }
}
public class Frame_SmokeCurrent
{
    /// <summary>
    /// 设备编号
    /// </summary>
    public string DeviceNo
    {
        get;
        set;
    }
    /// <summary>
    /// 报警码
    /// </summary>
    public string AlarmNum
    {
        get;
        set;
    }
    /// <summary>
    /// 电池电压
    /// </summary>
    public string BatteryVage
    {
        get;
        set;
    }
    /// <summary>
    /// 温度
    /// </summary>
    public string Temperature
    {
        get;
        set;
    }
    /// <summary>
    /// 时间
    /// </summary>
    public string Rtc
    {
        get;
        set;
    }
    /// <summary>
    /// NB信号值
    /// </summary>
    public string NBsignal
    {
        get;
        set;
    }
}
#endregion
