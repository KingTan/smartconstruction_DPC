using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using TCPAPI;
using ToolAPI;
using Newtonsoft.Json;
using Architecture;
using ProtocolAnalysis.RaiseDustNoise;

namespace ProtocolAnalysis
{
    public class ProtocolAnalysis_LF
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
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\LF", "蓝丰设备数据解析异常", ex.Message);
            }
            return "";
        }
        #region 实时数据
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param
        /// 23233032373653543D32323B434E3D323031313B50573D3132333435363B4D4E3D484A4248313630323031303135303B43503D26264461746154696D653D32303137303632313037303030303B504D31302D5274643D37362E30302C504D31302D466C61673D4E3B504D325F352D5274643D36362E30302C504D325F352D466C61673D4E3B4E4F4953452D5274643D35392E362C4E4F4953452D466C61673D4E3B4C415449542D5274643D34352E3830333636353B4C4F4E47542D5274643D3132362E3731313334393B48554D49445F5274643D38382E343B54454D505F5274643D31372E383B57494E445F53504545445F5274643D302E393B57494E445F4449524543545F5274643DE58D97203135372E363B2626384430310D0A
        private static void OnResolveRealData(string dataTemp, int c, ref DBFrame df)
        {
            try
            {
                Current_LF Current = new Current_LF();
                Current.MN = GetAttributeValue(dataTemp, "MN");
                Current.Time = GetAttributeValue(dataTemp, "DataTime");
                Current.Time = Current.Time.Equals("") ? DateTime.Now.ToString("MMddHHmmss") : Current.Time;

                Current.WIND_DIRECT_Rtd = GetAttributeValue(dataTemp, "WIND_DIRECT_Rtd").Replace("??? ", "").Replace("???", "");
                Current.WIND_DIRECT_Rtd = Current.WIND_DIRECT_Rtd.Equals("") ? "0" : Current.WIND_DIRECT_Rtd;

                Current.WIND_SPEED_Rtd = GetAttributeValue(dataTemp, "WIND_SPEED_Rtd");
                Current.WIND_SPEED_Rtd = Current.WIND_SPEED_Rtd.Equals("") ? "0" : Current.WIND_SPEED_Rtd;

                Current.TEMP_Rtd = GetAttributeValue(dataTemp, "TEMP_Rtd");
                Current.TEMP_Rtd = Current.TEMP_Rtd.Equals("") ? "0" : Current.TEMP_Rtd;

                Current.HUMID_Rtd = GetAttributeValue(dataTemp, "HUMID_Rtd");
                Current.HUMID_Rtd = Current.HUMID_Rtd.Equals("") ? "0" : Current.HUMID_Rtd;

                Current.NOISE_Rtd = GetAttributeValueComma(dataTemp, "NOISE-Rtd");
                Current.NOISE_Rtd = Current.NOISE_Rtd.Equals("") ? "0" : Current.NOISE_Rtd;

                Current.NOISE_PEAK_Rtd = GetAttributeValue(dataTemp, "NOISE_PEAK_Rtd");
                Current.NOISE_PEAK_Rtd = Current.NOISE_PEAK_Rtd.Equals("") ? "0" : Current.NOISE_PEAK_Rtd;

                Current.LATIT_Rtd = GetAttributeValue(dataTemp, "LATIT-Rtd");
                Current.LATIT_Rtd = Current.LATIT_Rtd.Equals("") ? "0" : Current.LATIT_Rtd;

                Current.LONGT_Rtd = GetAttributeValue(dataTemp, "LONGT-Rtd");
                Current.LONGT_Rtd = Current.LONGT_Rtd.Equals("") ? "0" : Current.LONGT_Rtd;

                Current.PM10_Rtd = GetAttributeValueComma(dataTemp, "PM10-Rtd");
                Current.PM10_Rtd = Current.PM10_Rtd.Equals("") ? "0" : Current.PM10_Rtd;

                Current.PM2_5_Rtd = GetAttributeValueComma(dataTemp, "PM2_5-Rtd");
                Current.PM2_5_Rtd = Current.PM2_5_Rtd.Equals("") ? "0" : Current.PM2_5_Rtd;

                Current.TSP_Rtd = GetAttributeValueComma(dataTemp, "TSP-Rtd");
                Current.TSP_Rtd = Current.TSP_Rtd.Equals("") ? "0" : Current.TSP_Rtd;

                df.deviceid = Current.MN;
                df.datatype = "current";
                df.contentjson = JsonConvert.SerializeObject(Current);
                FogGun.Linkage_dust linkage_dust = new FogGun.Linkage_dust();
                linkage_dust.Equipment = Current.MN;
                linkage_dust.PM25 = float.Parse(Current.PM2_5_Rtd);
                linkage_dust.PM10 = float.Parse(Current.PM10_Rtd);
                FogGun.Linkage.Dust_data_Process(linkage_dust);
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\LF", "蓝丰设备实时数据解析异常：", ex.Message);
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
