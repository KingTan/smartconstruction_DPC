using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolAPI;
using Newtonsoft.Json;
using ProtocolAnalysis;
using ProtocolAnalysis.Smoke;
using TCPAPI;


namespace ProtocolAnalysis.Smoke
{
    public  class ResolveSmoke
    {
        public static string OnResolveRecvMessage(byte[] b, UdpState client)
        {
            DBFrame df = new DBFrame();
            df.contenthex = ConvertData.ToHexString(b, 0, b.Length);
            df.version = "0100";
            ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\Smoke", "Smoke数据原包", df.contenthex);

            switch (b[27])
            {
                case 3: //心跳
                    OnResolve_HeartBeat(b, ref df);
                    break;

                case 2:
                    //实时火警数据
                    if (b[37] == 2)
                    {
                        OnResolve_Current(b, ref df);
                    }
                    //实时设备故障数据
                    else if (b[37] == 4)
                    {
                        OnResolve_HeartBeat(b, ref df);
                    }  
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
        /// <param name="bCount"></param>
        /// <param name="df"></param>
        private static void OnResolve_HeartBeat(byte[] b, ref DBFrame df)
        {
            try
            {
                if (b[27] == 3)
                {
                    Frame_HeartRegister heartbeat = new Frame_HeartRegister();
                    heartbeat.DeviceNo = ConvertData.ToHexString(b, 32, 4) + ConvertData.ToHexString(b, 12, 6); //设备号
                    heartbeat.RecTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //时间
                    df.contentjson = JsonConvert.SerializeObject(heartbeat);
                    df.datatype = "heartbeat";
                    df.deviceid = heartbeat.DeviceNo;
                    if (!string.IsNullOrEmpty(df.contentjson))
                        DB_MysqlSmoke.SaveSmoke(df);
                }
                else
                {
                    XMLOperation.WriteLogXmlNoTail("烟感设备故障", ConvertData.ToHexString(b, 0, b.Length));
                }
               
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
                string str = ConvertData.ToHexString(b, 0, b.Length);
                XMLOperation.WriteLogXmlNoTail("烟感实时数据", str);
                Frame_SmokeCurrent current = new Frame_SmokeCurrent();
                current.DeviceNo = ConvertData.ToHexString(b, 32, 4) + ConvertData.ToHexString(b, 12, 6);//设备号
                uint Uint = ToolAPI.ByteArrayToValueType.GetUInt32_BigEndian(b, 43);
                current.AlarmNum = "00000000000000000000000001000000";  //报警码
                current.BatteryVage = "0"; //电池电压
                current.NBsignal = "0";  //NB信号值
                current.Temperature = "0";  //温度
                current.Rtc = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                df.contentjson = JsonConvert.SerializeObject(current);
                df.datatype = "current";
                df.deviceid = current.DeviceNo;
                if (!string.IsNullOrEmpty(df.contentjson))
                    DB_MysqlSmoke.SaveSmoke(df);
            }
            catch (Exception ex) { XMLOperation.WriteLogXmlNoTail("烟感实时数据错误信息", ex.Message); }
        }
    }
}

