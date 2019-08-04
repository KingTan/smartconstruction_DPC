using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
using Architecture;
using ToolAPI;
using Newtonsoft.Json;
using ProtocolAnalysis;
using ProtocolAnalysis.StrongEMonitor;



public static class GprsResolveStrongE
{
    public static string OnResolveRecvMessage(byte[] b, UdpState client)
    {
        DBFrame df = new DBFrame();
        df.contenthex = ConvertData.ToHexString(b, 0, b.Length);
        df.version = "01";
        ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\QD", "QD数据原包", df.contenthex);
        switch (b[3])
        {
            case 0x02: //心跳
                OnResolve_HeartBeat(b, ref df);
                break;
            case 0x24:
                OnResolve_Current(b, ref df);
                break;
            default:
                break;
        }
        return "";
    }
    /// <summary>
    /// 心跳
    /// </summary>
    /// <param name="b"></param>
    /// <param name="df"></param>
    private static void OnResolve_HeartBeat(byte[] b, ref DBFrame df)
    {
        try
        {
            if (b[0] != 0xFF || b[1] != 0xFF)
                return;
            string dataStr = ConvertData.ToHexString(b, 0, b.Length);
            dataStr = dataStr.Replace("FF00", "FF");
            b = ConvertData.HexToByte(dataStr);
            StrongEHeatber data = new StrongEHeatber();
            data.EquipmentNo = ConvertData.ToHexString(b, 6, 12);
            data.Rtc = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            df.deviceid = data.EquipmentNo;
            df.datatype = "heartbeat";
            df.contentjson = JsonConvert.SerializeObject(data);
            if (!string.IsNullOrEmpty(df.contentjson))
            {
                DB_MysqlStrong.SaveStrong(df);
            }
        }
        catch (Exception ex) { XMLOperation.WriteLogXmlNoTail("强电监测心跳数据错误信息", ex.Message); }
    }
    /// <summary>
    /// 实时数据
    /// </summary>
    /// <param name="b"></param>
    /// <param name="df"></param>
    private static void OnResolve_Current(byte[] b, ref DBFrame df)
    {
        try
        {
            if (b[0] != 0xFF || b[1] != 0xFF)
                return;

            string dataStr = ConvertData.ToHexString(b, 0, b.Length);
            dataStr = dataStr.Replace("FF00", "FF");
            b = ConvertData.HexToByte(dataStr);

            StrongECurrent data = new StrongECurrent();
            UShortValue s = new UShortValue();
            IntValue iv = new IntValue();
            data.Uniqueid = "LN6M-L1T4A3V3";
            data.EquipmentNo = ConvertData.ToHexString(b, 6, 12);
            string str = ConvertData.ToHexString(b, 20, 2);
            data.Eleakage = HexString2BinString(str);
            data.Etemperature = HexString2BinString(ConvertData.ToHexString(b, 22, 2));
            #region 故障值
            s.bValue1 = b[25];
            s.bValue2 = b[24];
            data.FCurrentLeakagefaultA = s.sValue.ToString(); //实时故障漏电值
            s.bValue1 = b[27];
            s.bValue2 = b[26];
            data.FCurrentemfaultA = s.sValue.ToString(); //实时故障温度A
            s.bValue1 = b[29];
            s.bValue2 = b[28];
            data.FCurrentemfaultB = s.sValue.ToString(); //实时故障温度B
            s.bValue1 = b[31];
            s.bValue2 = b[30];
            data.FCurrentemfaultC = s.sValue.ToString(); //实时故障温度C
            s.bValue1 = b[33];
            s.bValue2 = b[32];
            data.FCurrentemfaultN = s.sValue.ToString(); //实时故障温度D
            s.bValue1 = b[35];
            s.bValue2 = b[34];
            data.FWaterA = SpiltString(s.sValue.ToString(), 10); //实时故障电流路数A
            s.bValue1 = b[37];
            s.bValue2 = b[36];
            data.FWaterB = SpiltString(s.sValue.ToString(), 10); //实时故障电流路数B
            s.bValue1 = b[39];
            s.bValue2 = b[38];
            data.FWaterC = SpiltString(s.sValue.ToString(), 10); //实时故障电流路数C
            s.bValue1 = b[41];
            s.bValue2 = b[40];
            data.FVoltageA = SpiltString(s.sValue.ToString(), 10); //实时故障电压路数A
            s.bValue1 = b[43];
            s.bValue2 = b[42];
            data.FVoltageB = SpiltString(s.sValue.ToString(), 10); //实时故障电压路数B
            s.bValue1 = b[45];
            s.bValue2 = b[44];
            data.FVoltageC = SpiltString(s.sValue.ToString(), 10); //实时故障电压路数C
            #endregion
            #region 实时值
            s.bValue1 = b[47];
            s.bValue2 = b[46];
            data.CurrentLeakageA = s.sValue.ToString();  //实时值漏电路数
            s.bValue1 = b[49];
            s.bValue2 = b[48];
            data.CurrentTemA = s.sValue.ToString();     //实时值温度路数A
            s.bValue1 = b[51];
            s.bValue2 = b[50];
            data.CurrentTemB = s.sValue.ToString();    //实时值温度路数B
            s.bValue1 = b[53];
            s.bValue2 = b[52];
            data.CurrentTemC = s.sValue.ToString();    //实时值温度路数C
            s.bValue1 = b[55];
            s.bValue2 = b[54];
            data.CurrentTemN = s.sValue.ToString();    //实时值温度路数N
            s.bValue1 = b[57];
            s.bValue2 = b[56];
            data.CWaterA = SpiltString(s.sValue.ToString(), 10); //实时值电流路数A
            s.bValue1 = b[59];
            s.bValue2 = b[58];
            data.CWaterB = SpiltString(s.sValue.ToString(), 10); //实时值电流路数B
            s.bValue1 = b[61];
            s.bValue2 = b[60];
            data.CWaterC = SpiltString(s.sValue.ToString(), 10);  //实时值电流路数C
            s.bValue1 = b[63];
            s.bValue2 = b[62];
            data.CVoltageA = SpiltString(s.sValue.ToString(), 10); //实时值电压路数A
            s.bValue1 = b[65];
            s.bValue2 = b[64];
            data.CVoltageB = SpiltString(s.sValue.ToString(), 10);  //实时值电压路数B
            s.bValue1 = b[67];
            s.bValue2 = b[66];
            data.CVoltageC = SpiltString(s.sValue.ToString(), 10); //实时值电压路数C
            #endregion
            #region 电量
            iv.bValue4 = b[71];
            iv.bValue3 = b[70];
            iv.bValue2 = b[69];
            iv.bValue1 = b[68];
            data.EamountA = SpiltString(iv.iValue.ToString(), 10);  //A相电量
            iv.bValue1 = b[72];
            iv.bValue2 = b[73];
            iv.bValue3 = b[74];
            iv.bValue4 = b[75];
            data.EamountB = SpiltString(iv.iValue.ToString(), 10);  //B相电量
            iv.bValue1 = b[76];
            iv.bValue2 = b[77];
            iv.bValue3 = b[78];
            iv.bValue4 = b[79];
            data.EamountC = SpiltString(iv.iValue.ToString(), 10);  //C相电量
            iv.bValue1 = b[80];
            iv.bValue2 = b[81];
            iv.bValue3 = b[82];
            iv.bValue4 = b[83];
            data.VloadA = SpiltString(iv.iValue.ToString(), 10);   //A相视载功率
            iv.bValue1 = b[84];
            iv.bValue2 = b[85];
            iv.bValue3 = b[86];
            iv.bValue4 = b[87];
            data.VloadB = SpiltString(iv.iValue.ToString(), 10);  //B相视载功率
            iv.bValue1 = b[88];
            iv.bValue2 = b[89];
            iv.bValue3 = b[90];
            iv.bValue4 = b[91];
            data.VloadC = SpiltString(iv.iValue.ToString(), 10);  //C相视载功率
            s.bValue1 = b[93];
            s.bValue2 = b[92];
            data.VfactorA = SpiltString(s.sValue.ToString(), 1000);  //A相功率因数
            s.bValue1 = b[95];
            s.bValue2 = b[94];
            data.VfactorB = SpiltString(s.sValue.ToString(), 1000);  //B相功率因数
            s.bValue1 = b[97];
            s.bValue2 = b[96];
            data.VfactorC = SpiltString(s.sValue.ToString(), 1000);  //C相功率因数
            s.bValue1 = b[99];
            s.bValue2 = b[98];
            data.VfrequencyA = s.sValue.ToString();    //A相频率
            s.bValue1 = b[101];
            s.bValue2 = b[100];
            data.VfrequencyB = s.sValue.ToString();   //B相频率
            s.bValue1 = b[103];
            s.bValue2 = b[102];
            data.VfrequencyC = s.sValue.ToString();   //C相频率
            #endregion
            #region 电流互感器穿心方向
            data.EMutualdirection = "00000000";//ConvertData.ToHexString(b, 105, 2).PadLeft(8, '0'); //电流互感穿心方向
            //if (data.EMutualdirection.Contains("32"))
            //     data.EMutualdirection.Replace("32", "00");
            //if (data.EMutualdirection.Contains("80"))
            //     data.EMutualdirection.Replace("80", "00");
            //if (data.EMutualdirection.Contains())
            #endregion
            #region 故障电弧量报警值
            s.bValue1 = b[107];
            s.bValue2 = b[106];
            data.EarcAlarmA = s.sValue.ToString();     //A故障电弧量报警值
            s.bValue1 = b[109];
            s.bValue2 = b[108];
            data.EarcAlarmB = s.sValue.ToString();     //B故障电弧量报警值
            s.bValue1 = b[111];
            s.bValue2 = b[110];
            data.EarcAlarmC = s.sValue.ToString();     //C故障电弧量报警值
            s.bValue1 = b[113];
            s.bValue2 = b[112];
            data.EarcrealA = s.sValue.ToString();      //A相故障电弧量实时值
            s.bValue1 = b[115];
            s.bValue2 = b[114];
            data.EarcrealB = s.sValue.ToString();      //B相故障电弧量实时值
            s.bValue1 = b[117];
            s.bValue2 = b[116];
            data.EarcrealC = s.sValue.ToString();      //C相故障电弧量实时值
            s.bValue1 = b[119];
            s.bValue2 = b[118];
            data.EVoltagebalanceAlarm = SpiltString(s.sValue.ToString(), 10);  //3相电压平衡度报警值
            s.bValue1 = b[121];
            s.bValue2 = b[120];
            data.EVoltagebalanceReal = SpiltString(s.sValue.ToString(), 10);   //3相电压平衡度实时值
            s.bValue1 = b[123];
            s.bValue2 = b[122];
            data.EWaterbalanceAlarm = SpiltString(s.sValue.ToString(), 10); //3相电流平衡度报警值
            s.bValue1 = b[125];
            s.bValue2 = b[124];
            data.EWaterbalanceReal = SpiltString(s.sValue.ToString(), 10); //3相电流平衡度实时值
            s.bValue1 = b[127];
            s.bValue2 = b[126];
            data.Gpsignal = s.sValue.ToString(); //信号强度实时值
            s.bValue1 = b[129];
            s.bValue2 = b[128];
            data.VaVoltageangle = SpiltString(s.sValue.ToString(), 10);  //Va电压相位角
            s.bValue1 = b[131];
            s.bValue2 = b[130];
            data.VbVoltageangle = SpiltString(s.sValue.ToString(), 10);  //Vb电压相位角
            s.bValue1 = b[133];
            s.bValue2 = b[132];
            data.VcVoltageangle = SpiltString(s.sValue.ToString(), 10);  //Vc电压相位角
            s.bValue1 = b[135];
            s.bValue2 = b[134];
            data.IaWaterangle = SpiltString(s.sValue.ToString(), 10);  //Ia电流相位角
            s.bValue1 = b[137];
            s.bValue2 = b[136];
            data.IbWaterangle = SpiltString(s.sValue.ToString(), 10);  //Ib电流相位角
            s.bValue1 = b[139];
            s.bValue2 = b[138];
            data.IcWaterangle = SpiltString(s.sValue.ToString(), 10);  //Ic电流相位角
            //s.bValue2 = b[141];
            df.deviceid = data.EquipmentNo;
            df.datatype = "current";
            df.contentjson = JsonConvert.SerializeObject(data);
            if (!string.IsNullOrEmpty(df.contentjson))
            {
                DB_MysqlStrong.SaveStrong(df);
            }
            #endregion
        }
        catch (Exception ex) { XMLOperation.WriteLogXmlNoTail("强电监测实时数据错误信息", ex.Message); }
    }
    /// <summary>
    /// 十六进制转二进制
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    static string HexString2BinString(string hexString)
    {
        string result = string.Empty;
        foreach (char c in hexString)
        {
            int v = Convert.ToInt32(c.ToString(), 16);
            int v2 = int.Parse(Convert.ToString(v, 2));
            // 去掉格式串中的空格，即可去掉每个4位二进制数之间的空格，
            result += string.Format("{0:d4}", v2);
        }
        return result;
    }
    /// <summary>
    /// 在字符串后一位保留一个小数点
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    static string SpiltString(string str, int i)
    {
        try
        {
            return (float.Parse(str) / 10).ToString("0.0");
        }
        catch
        {
            return "0";
        }
    }

}

