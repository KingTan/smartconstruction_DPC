using Architecture;
using ProtocolAnalysis.RaiseDustNoise;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis
{
    public class Protocolanalysis_XA
    {
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                DBFrame df = new DBFrame();
                df.contenthex = ConvertData.ToHexString(b, 0, c);
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XA", "XA设备数据原包", df.contenthex);
                df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;
                TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;

                //实时数据
                OnResolveCurrent(b, c, client, ref df);

                if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
                {
                    TcpExtendTemp.EquipmentID = df.deviceid;
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        #region 实时数据
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <param name="client"></param>
        /// <param name="df"></param>
        public static void OnResolveCurrent(byte[] b, int bCount, TcpSocketClient client, ref DBFrame df)
        {
            string tStr = ConvertData.ToHexString(b, 0, 1);
            //if (tStr != "04"||tStr!="05")
            //    return;
            xadust_Current current = new xadust_Current();

            //设备编号
            current.sn = "XA_" + tStr;

            current.Rtc = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            UShortValue s = new UShortValue();
            string str = b[3].ToString("X2") + b[4].ToString("X2");
            int result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            int resu = Convert.ToInt32(str, 16);

            float res = ((float)result / (float)100) - 40;
            //温度
            current.Temperature = res.ToString("0.0");

            str = b[5].ToString("X2") + b[6].ToString("X2");
            result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            resu = Convert.ToInt32(str, 16);

             res = ((float)result / (float)100);
            //湿度
            current.Humidity = res.ToString("0.0");

            str = b[9].ToString("X2") + b[10].ToString("X2");
            result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            resu = Convert.ToInt32(str, 16);
            res = ((float)result / (float)100);
            //气压
            current.pressure = res.ToString("0.0");

            str = b[11].ToString("X2") + b[12].ToString("X2");
            resu = Convert.ToInt32(str, 16);

            res = ((float)resu / 100);
            //风速
            current.Wind = res.ToString("0.0");

            s.bValue1 = b[13];
            s.bValue2 = b[14];
            //风向
            current.WindDirection = Wind_Direction(s.sValue.ToString()).ToString();

            str = b[17].ToString("X2") + b[18].ToString("X2");
            result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            resu = Convert.ToInt32(str, 16);
            //PM10
            current.Pm10 = result.ToString("0.0");

            str = b[19].ToString("X2") + b[20].ToString("X2");
            result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            resu = Convert.ToInt32(str, 16);
            //噪音
            res = ((float)result / (float)10);
            current.Noise = res.ToString("0.0");

            str = b[31].ToString("X2") + b[32].ToString("X2");
            result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            resu = Convert.ToInt32(str, 16);

            //PM2.5 
            current.Pm2_5 = result.ToString("0.0");

            current.GprsSignal = "0";
            current.automatic = "0";
            current.Manual = "0";
            current.particulates = "0";
            current.alarm = "0";
            df.deviceid = current.sn;
            df.datatype = "current";
            df.contentjson = JsonConvert.SerializeObject(current);
            FogGun.Linkage_dust linkage_dust = new FogGun.Linkage_dust();
            linkage_dust.Equipment = current.sn;
            linkage_dust.PM25 = float.Parse(current.Pm2_5);
            linkage_dust.PM10 = float.Parse(current.Pm10);
            FogGun.Linkage.Dust_data_Process(linkage_dust);
            if (df.contentjson != null && df.contentjson != "")
            {
                DB_MysqlRaiseDustNoise.SaveRaiseDustNoise(df);
            }
        }
        #endregion


        /// <summary>
        /// 风向转换
        /// </summary>
        /// <param name="wind"></param>
        /// <returns></returns>
        public static int Wind_Direction(string wind)
        {
            int windValue = 0;
            switch (wind)
            {
                //东北偏北
                case "0": windValue = 1; break;
                //东北
                case "1": windValue = 2; break;
                //东北偏东
                case "2": windValue = 3; break;
                //东
                case "3": windValue = 4; break;
                //东南偏东
                case "4": windValue = 5; break;
                //东南
                case "5": windValue = 6; break;
                //东南偏南
                case "6": windValue = 7; break;
                //南
                case "7": windValue = 8; break;
                //西南偏南
                case "8": windValue = 9; break;
                //西南
                case "9": windValue = 10; break;
                //西南偏西
                case "10": windValue = 11; break;
                //西
                case "11": windValue = 12; break;
                //西北偏西
                case "12": windValue = 13; break;
                //西北
                case "13": windValue = 14; break;
                //西北偏北
                case "14": windValue = 15; break;
                //北
                case "15": windValue = 0; break;
                default: break;

            }

            return windValue;
        }

    }
}
