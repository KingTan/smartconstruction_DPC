using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
using Architecture;
using ToolAPI;
using Newtonsoft.Json;
using ProtocolAnalysis;
using ProtocolAnalysis.InfraredContrast.Mysql;
using ProtocolAnalysis.InfraredContrast;



public static class GprsResolveInfraredContrast
{
    /// <summary>
    /// 心跳及实时数据解析
    /// </summary>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
    {
        DBFrame df = new DBFrame();
        df.contenthex = ConvertData.ToHexString(b, 0, c);
        df.version = "01";//版本好默认
        if (c==8&&b[0]!=0xA7)//心跳
        {
            OnResolve_HearBeat(b,c,df);
        }
        else if (c == 21 && b[0] == 0xA7)//实时数据
        {
            OnResolve_Current(b, c, df);
        }
        else
        {
            return "";
        }
        TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
        if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
        {
            TcpExtendTemp.EquipmentID = df.deviceid;
        }
        //存入数据库
        if (df.contentjson != null && df.contentjson != "")
        {
            DB_MysqlInfraredContrast.SaveInfraredContrast(df);
        }
        return "";
    }
    /// <summary>
    /// 实时数据
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    /// A7 A7 A0 01 00 01 00 10 00 48 57 30 30 30 30 30 38 01 00 B7 B7

    private static void OnResolve_Current(byte[] b, int bCount, DBFrame df)
    {
        Frame_Current current = new Frame_Current();
        current.DeviceNo = Encoding.ASCII.GetString(b,9,8);
        current.Alarmstatus = Convert.ToString(b[17], 2).PadLeft(8, '0'); ;
        current.DismantleStatus = Convert.ToString(b[18], 2).PadLeft(8, '0'); ;
        current.ReceiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        df.datatype = "current";
        df.deviceid = current.DeviceNo;
        df.contentjson = JsonConvert.SerializeObject(current);
    }
    /// <summary>
    /// 心跳
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    /// <param name="df"></param>
    /// <returns></returns>
    /// 48 57 30 30 30 30 30 38
    private static void OnResolve_HearBeat(byte[] b, int bCount, DBFrame df)
    {
        Frame_Current current = new Frame_Current();
        current.DeviceNo = Encoding.ASCII.GetString(b,0,8);
        current.ReceiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        df.datatype = "heartbeat";
        df.deviceid = current.DeviceNo;
        df.contentjson = JsonConvert.SerializeObject(current);
    }
}

