using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using TCPAPI;
using System.Reflection;
using ToolAPI;
using Newtonsoft.Json;
using Architecture;
using ProtocolAnalysis.RaiseDustNoise;
using System.Text.RegularExpressions;
namespace ProtocolAnalysis
{
    public class ProtocolAnalysis_WWYC
    {
        /// <summary>
        /// 解析、存储、显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                DBFrame df = new DBFrame();
                df.contenthex = ConvertData.ToHexString(b, 0, c);
                df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;

                //把字节流转换为字符串
                string dataTemp = Encoding.ASCII.GetString(b);
                if (dataTemp.Contains("CN=2011"))//固定数据
                {
                    OnResolveRealData(dataTemp, c, ref df);
                }

                TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
                {
                    TcpExtendTemp.EquipmentID = df.deviceid;
                }
                //存入数据库
                if (df.contentjson != null && df.contentjson != "")
                {
                    DB_MysqlRaiseDustNoise.SaveRaiseDustNoise(df);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\WWYC", "万维盈创设备数据解析异常", ex.Message);
            }
            return "";
        }
        #region 实时数据
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param
        /// ##0265ST=22;CN=2011;PW=123456;MN=WWYC0010000001;CP=&&DataTime=20160719000100;925-Rtd=32.9,925-Flag=N;107-Rtd=56.2,107-Flag=N;126-Rtd=28.1,126-Flag=N;128-Rtd=81.0,128-Flag=N;129-Rtd=0.1,129-Flag=N;130-Rtd=315.0,130-Flag=N;127-Rtd=99.803,127-Flag=N;DI1-Rtd=1.0,DI1-Flag=N;103-Rtd=2.80,103-Flag=N;B03-Rtd=65.0,B03-Flag=N;926-Rtd=299.803,926-Flag=N;927-Rtd=199.803,927-Flag=N&&BC81
        private static void OnResolveRealData(string dataTemp, int c, ref DBFrame df)
        {
            try
            {
                Current_WWYC Current = new Current_WWYC();
                Current.MN = GetAttributeValue(dataTemp, "MN");

                Current.Time = GetAttributeValue(dataTemp, "DataTime");
                Current.Time = Current.Time.Equals("") ? DateTime.Now.ToString("MMddHHmmss") : Current.Time;

                Current.PM2_5 = GetAttributeValueComma(dataTemp, "925-Rtd");
                Current.PM2_5 = Current.PM2_5.Equals("") ? "0" : Current.PM2_5;

                Current.PM10 = GetAttributeValueComma(dataTemp, "107-Rtd");
                Current.PM10 = Current.PM10.Equals("") ? "0" : Current.PM10;

                Current.Temperature = GetAttributeValueComma(dataTemp, "126-Rtd");
                Current.Temperature = Current.Temperature.Equals("") ? "0" : Current.Temperature;

                Current.Humidity = GetAttributeValueComma(dataTemp, "128-Rtd");
                Current.Humidity = Current.Humidity.Equals("") ? "0" : Current.Humidity;

                Current.WindSpeed = GetAttributeValueComma(dataTemp, "129-Rtd");
                Current.WindSpeed = Current.WindSpeed.Equals("") ? "0" : Current.WindSpeed;

                Current.WindDirection = GetAttributeValueComma(dataTemp, "130-Rtd");
                Current.WindDirection = Current.WindDirection.Equals("") ? "0" : Current.WindDirection;

                Current.Pressure = GetAttributeValueComma(dataTemp, "127-Rtd");
                Current.Pressure = Current.Pressure.Equals("") ? "0" : Current.Pressure;

                Current.TSP = GetAttributeValueComma(dataTemp, "103-Rtd");
                Current.TSP = Current.TSP.Equals("") ? "0" : Current.TSP;

                Current.Noise = GetAttributeValueComma(dataTemp, "B03-Rtd");
                Current.Noise = Current.Noise.Equals("") ? "0" : Current.Noise;

                Current.Longitude = GetAttributeValueComma(dataTemp, "926-Rtd");
                Current.Longitude = Current.Longitude.Equals("") ? "0" : Current.Longitude;

                Current.Latitude = GetAttributeValueComma(dataTemp, "927-Rtd");
                Current.Latitude = Current.Latitude.Equals("") ? "0" : Current.Latitude;

                df.deviceid = Current.MN;
                df.datatype = "current";
                df.contentjson = JsonConvert.SerializeObject(Current);
                FogGun.Linkage_dust linkage_dust = new FogGun.Linkage_dust();
                linkage_dust.Equipment = Current.MN;
                linkage_dust.PM25 = float.Parse(Current.PM2_5);
                linkage_dust.PM10 = float.Parse(Current.PM10);
                FogGun.Linkage.Dust_data_Process(linkage_dust);
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\WWYC", "万维盈创设备实时数据解析异常：", ex.Message);
            }
        }
        #endregion

        #region 获取对应的字段的值
        /// <summary>
        /// 获取对应属性的值 =和;
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="Attribute"></param>
        /// <returns></returns>
        static private string GetAttributeValue(string sourceString, string Attribute)
        {
            Attribute += "=";
            return GetValue(sourceString, Attribute, ";");
        }
        /// <summary>
        /// 获取对应属性的值 =和,
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="Attribute"></param>
        /// <returns></returns>
        static private string GetAttributeValueComma(string sourceString, string Attribute)
        {
            Attribute += "=";
            return GetValue(sourceString, Attribute, ",");
        }
        /// <summary>
        /// 街区一段字符中对应的开始和结束之间的字符
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <param name="start">开始字符串</param>
        /// <param name="end">结束字符串</param>
        /// <returns></returns>
        static private string GetValue(string sourceString, string start, string end)
        {
            Regex rg = new Regex("(?<=(" + start + "))[.\\s\\S]*?(?=(" + end + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(sourceString).Value;
        }
        #endregion
    }

}
