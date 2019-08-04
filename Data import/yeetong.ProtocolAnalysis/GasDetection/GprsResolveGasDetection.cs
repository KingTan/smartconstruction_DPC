using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
using Architecture;
using ToolAPI;
using Newtonsoft.Json;
using ProtocolAnalysis;
using ProtocolAnalysis.GasDetection;
using ProtocolAnalysis.GasDetection.Mysql;



public static class GprsResolveGasDetection
{
    #region 接收加解析
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
        df.version = "01";
        if (c == 8)//心跳
        {
            OnResolve_HearBeat(b, c,df);
        }
        else if (c == 63)//实时数据
        {
            if (crc16_modbus(b, c))
            {
                OnResolve_Current(b, c, df, client);
            }
        }
        else
        {
            return "";
        }
        TcpClientBindingExternalClass TcpExtendTemp1 = client.External.External as TcpClientBindingExternalClass;
        if (TcpExtendTemp1.EquipmentID == null || TcpExtendTemp1.EquipmentID.Equals(""))
        {
            TcpExtendTemp1.EquipmentID = df.deviceid;
        }
       
        //存入数据库
        if (df.contentjson != null && df.contentjson != "")
        {
            DB_MysqlGasDetection.SaveGasDetection(df);
        }
        return "";
    }

    /// <summary>
    /// CO实时数据解析
    /// </summary>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="client"></param>
    public static void OnResolveRecvMessageAboutCO(byte[] b, int c, TcpSocketClient client)
    {
        DBFrame df = new DBFrame();
        df.contenthex = ConvertData.ToHexString(b, 0, c);
        df.version = ConvertData.ToHexString(b, 2, 1); 
        //CO的实时数据
        OnResolve_CurrentAboutCO(b, c, df, client);

        TcpClientBindingExternalClass TcpExtendTemp1 = client.External.External as TcpClientBindingExternalClass;
        if (TcpExtendTemp1.EquipmentID == null || TcpExtendTemp1.EquipmentID.Equals(""))
        {
            TcpExtendTemp1.EquipmentID = df.deviceid;
        }

        //存入数据库
        if (df.contentjson != null && df.contentjson != "")
        {
            DB_MysqlGasDetection.SaveGasDetection(df);
        }

    }

    /// <summary>
    /// CO实时数据处理
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    /// <param name="df"></param>
    /// <param name="client"></param>
    private static void OnResolve_CurrentAboutCO(byte[] b, int bCount, DBFrame df, TcpSocketClient client)
    {
        Frame_Current current = new Frame_Current();

        current.DeviceNo = ConvertData.ToHexString(b,3,6);//设备号

        byte[] temp = new byte[] { b[16], b[17], b[18], b[19] };
        current.GasStrength = (float)ToolAPI.ByteArrayToValueType.GetSingle_BigEndian(temp, 0);//co的值

        current.ReceiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        df.datatype = "current";
        df.deviceid = current.DeviceNo;
        df.contentjson = JsonConvert.SerializeObject(current);
    }
    /// <summary>
    /// 实时数据
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    ///
    /// 接受：06 03 3A 00 64 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 CC CD 41 A0 00 00 42 48 00 00 00 00 00 00 00 00 00 00 00 00 00 00 B4 C2 00 20 00 00 00 00 00 00 70 1C

    private static void OnResolve_Current(byte[] b, int bCount, DBFrame df, TcpSocketClient client)
    {
        Frame_Current current = new Frame_Current();
        TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
        if (!string.IsNullOrEmpty(TcpExtendTemp.EquipmentID))
        {
            current.DeviceNo = TcpExtendTemp.EquipmentID;//dtu地址
            current.Addr = b[0];//主机地址
            //量程
            current.Range = ToolAPI.ByteArrayToValueType.GetUInt16_BigEndian(b, 3);
            //低报警点
            byte[] temp = new byte[] { b[26],b[25],b[28],b[27] };
            current.LowAlarm = ToolAPI.ByteArrayToValueType.GetSingle_LittleEndian(temp, 0);
            //A1报警点
            temp = new byte[] { b[30], b[29], b[32], b[31] };
            current.A1Alarm = ToolAPI.ByteArrayToValueType.GetSingle_LittleEndian(temp, 0);
            //A2报警点
            temp = new byte[] { b[34], b[33], b[36], b[35] };
            current.A2Alarm = ToolAPI.ByteArrayToValueType.GetSingle_LittleEndian(temp, 0);
            //实时AD值
            current.GasStrengthAD = ToolAPI.ByteArrayToValueType.GetUInt32_BigEndian(b, 51);
            //气体值
            temp = new byte[] { b[56], b[55], b[58], b[57] };
            current.GasStrength = ToolAPI.ByteArrayToValueType.GetSingle_LittleEndian(temp, 0);
            //报警状态
            current.AlarmStatus = ToolAPI.ByteArrayToValueType.GetUInt16_BigEndian(b, 59);
            //二级报警点>一级报警点
            if ((b[60] & 0x10) != 0)
                current.AlarmStatusFlag = 2;
            else if ((b[60] & 0x20) != 0)
                current.AlarmStatusFlag = 1;
            else
                current.AlarmStatusFlag = 0;
            current.ReceiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            df.datatype = "current";
            df.deviceid = current.DeviceNo;
            df.contentjson = JsonConvert.SerializeObject(current);
        }
    }
    /// <summary>
    /// 心跳
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bCount"></param>
    /// <param name="df"></param>
    /// <returns></returns>
    /// 48 57 30 30 30 30 30 36
    /// 设备编号拼接规则：5位+3位（10进制）
    private static void OnResolve_HearBeat(byte[] b, int bCount, DBFrame df)
    {
        Frame_Current current = new Frame_Current();
        current.DeviceNo = Encoding.ASCII.GetString(b, 0, 8);
        current.ReceiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        df.datatype = "heartbeat";
        df.deviceid = current.DeviceNo;
        df.contentjson = JsonConvert.SerializeObject(current);
    }
    #endregion

    #region 拼接下发包
    /// <summary>
    ///  发送：06 03 01 34 00 1D C4 46  
    /// </summary>
    /// <param name="TcpExtendTemp"></param>
    /// <returns></returns>
    public  static byte[] SplitJointCommand(TcpClientBindingExternalClass TcpExtendTemp)
    {
        try
        {
            List<byte> bytel = new List<byte>();
            bytel.AddRange(new byte[] { byte.Parse(TcpExtendTemp.EquipmentID.Substring(5)), 0x03, 0x01, 0x34, 0x00, 0x1D });
            bytel.AddRange(Get_crc16_modbus(bytel.ToArray()));
            return bytel.ToArray();
        }
        catch (Exception) { return null; }
    }
    #endregion

    #region CRC modbus
    public static bool crc16_modbus(byte[] modbusdata, int Length)//Length为modbusdata的长度
    {
        if (Length >= 2)
        {
            uint i, j;
            uint crc16 = 0xFFFF;
            for (i = 0; i < Length - 2; i++)
            {
                crc16 ^= modbusdata[i]; // CRC = BYTE xor CRC
                for (j = 0; j < 8; j++)
                {
                    if ((crc16 & 0x01) == 1) //如果CRC最后一位为1�右移一位后carry=1�则将CRC右移一位后�再与POLY16=0xA001进行xor运算
                        crc16 = (crc16 >> 1) ^ 0xA001;
                    else //如果CRC最后一位为0�则只将CRC右移一位
                        crc16 = crc16 >> 1;
                }
            }
            byte[] crcAry = ToolAPI.ValueTypeToByteArray.GetBytes_LittleEndian((UInt16)crc16);
            if (crcAry[0] == modbusdata[Length - 2] && crcAry[1] == modbusdata[Length - 1])
                return true;
            else
                return false;
        }
        else
            return false;

    }

    public static byte[] Get_crc16_modbus(byte[] modbusdata)//Length为modbusdata的长度
    {
        uint i, j;
        uint crc16 = 0xFFFF, Length = (uint)modbusdata.Length;
        for (i = 0; i < Length; i++)
        {
            crc16 ^= modbusdata[i]; // CRC = BYTE xor CRC
            for (j = 0; j < 8; j++)
            {
                if ((crc16 & 0x01) == 1) //如果CRC最后一位为1�右移一位后carry=1�则将CRC右移一位后�再与POLY16=0xA001进行xor运算
                    crc16 = (crc16 >> 1) ^ 0xA001;
                else //如果CRC最后一位为0�则只将CRC右移一位
                    crc16 = crc16 >> 1;
            }
        }
        byte[] crcAry = ToolAPI.ValueTypeToByteArray.GetBytes_LittleEndian((UInt16)crc16);
        return crcAry;
    }
    #endregion
}

