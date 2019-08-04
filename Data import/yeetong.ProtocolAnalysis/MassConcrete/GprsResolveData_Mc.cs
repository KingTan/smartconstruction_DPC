using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Threading;
using TCPAPI;
using Architecture;
using ToolAPI;
using Newtonsoft.Json;
using ProtocolAnalysis.MassConcrete;//存放对象的
namespace ProtocolAnalysis
{
    public static class GprsResolveData_Mc
    {
        #region 解析入口
        /// <summary>
        /// 解析、存储、显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            if (b.Length < 5)
                return "";
            DBFrame df = new DBFrame();
            df.contenthex = ConvertData.ToHexString(b, 0, c);
            df.version = "01";
            byte typ = b[4];
            if (typ == 0x68)//心跳
            {
                TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                if (TcpExtendTemp.TVersion == null || TcpExtendTemp.TVersion == "")
                {
                    OnResolveHeabert(b, c, ref df);
                    TcpExtendTemp.TVersion = "0";
                }
                else if (int.Parse(TcpExtendTemp.TVersion) >= 20)
                {
                    OnResolveHeabert(b, c, ref df);
                    TcpExtendTemp.TVersion = "0";
                }
                else
                {
                    int flagP = int.Parse(TcpExtendTemp.TVersion);
                    int result = flagP + 1;
                    TcpExtendTemp.TVersion = result.ToString();
                }
            }
            else //实时数据
            {
                OnResolveRealData(b, c, ref df);
            }
            //存入数据库
            if (df.contentjson != null && df.contentjson != "")
            {
                DB_MysqlMassConcrete.SaveMassConcrete(df);
            }
            return "";
        }
        #endregion

        #region 具体命令解析
        #region 实时数据
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// 发送38 31 39 31 09 30 30 31 09 30 30 30 34 09 32 30 31 34 31 31 30 31 09 31 30 3A 32 33 3A 31 32 09 31 34 09 20 32 36 2E 37 09 20 32 36 2E 37 09 20 32 36 2E 38 09 20 32 36 2E 37 09 20 30 30 2E 30 09 20 32 36 2E 37 09 20 30 30 2E 30 09 20 30 30 2E 30 09 20 30 30 2E 30 09 20 30 30 2E 30 09 20 30 30 2E 30 09 20 30 30 2E 30 09 20 30 30 2E 30 09 20 30 32 2E 37 09 30 36 09 30 30 2E 30 09 30 30 2E 30 09 30 30 2E 30 09 30 30 2E 30 09 30 30 2E 30 09 33 2E 38 09 0D 0A
        public static void OnResolveRealData(byte[] b, int bCount, ref DBFrame df)
        {
            try
            {
                RealTimeData rtd = new RealTimeData();
                string dataHexString = ConvertData.ToHexString_Space(b, 0, bCount);
                string[] stringSeparators = new string[] { " 09 " };
                //判断起始符+版本号进行分割包
                string[] DataHexAry = dataHexString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                rtd.EquipmentID = HexSpaceToAscii(DataHexAry[0]);
                rtd.SubEquipmentCount = HexSpaceToAscii(DataHexAry[1]);
                rtd.SubEquipmentID = HexSpaceToAscii(DataHexAry[2]);
                string date = HexSpaceToAscii(DataHexAry[3]);
                string dateTime = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " + HexSpaceToAscii(DataHexAry[4]);
                rtd.Time = dateTime;
                rtd.PassTemperatureMaxCount = HexSpaceToAscii(DataHexAry[5]);
                string PassTemperatureArray = "";
                for (int i = 0; i < 13;i++ )
                {
                    if (i != 12)
                        PassTemperatureArray += HexSpaceToAscii(DataHexAry[6 + i]) + "&";
                    else
                        PassTemperatureArray += HexSpaceToAscii(DataHexAry[6 + i]);
                }
                rtd.PassTemperatureArray = PassTemperatureArray;
                rtd.SubCellVoltage = HexSpaceToAscii(DataHexAry[19]);
                rtd.PassHumidityMaxCount = HexSpaceToAscii(DataHexAry[20]);
                string PassHumidityArray = "";
                for (int i = 0; i < 5; i++)
                {
                    if (i != 4)
                        PassHumidityArray += HexSpaceToAscii(DataHexAry[21 + i]) + "&";
                    else
                        PassHumidityArray += HexSpaceToAscii(DataHexAry[21 + i]);
                }
                rtd.PassHumidityArray = PassHumidityArray;
                rtd.CellVoltage = HexSpaceToAscii(DataHexAry[26]);
                //存入数据库
                df.deviceid = rtd.EquipmentID;
                df.datatype = "current";
                df.contentjson = JsonConvert.SerializeObject(rtd);
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveData_Mc.OnResolveRealData异常", ex.Message);
            }
        }
        #endregion

        #region 心跳
        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="b"></param>
        /// 发送38 31 39 31 68 0D 0A
        private static void OnResolveHeabert(byte[] b, int bCount, ref DBFrame df)
        {
            try
            {
                byte[] sn = new byte[4];   //设备编号
                Array.Copy(b, 0, sn, 0, 4);
                Heartbeat hb = new Heartbeat();
                hb.EquipmentID = Encoding.ASCII.GetString(sn);
                hb.Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //存入数据库
                df.deviceid = hb.EquipmentID;
                df.datatype = "heartbeat";
                df.contentjson = JsonConvert.SerializeObject(hb);
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveData_Mc.OnResolveHeabert异常", ex.Message);
            }
        }
        #endregion
        #endregion


        static string HexSpaceToAscii(string HexSpaceStr)
        {
            return Encoding.ASCII.GetString(ConvertData.HexToByte(HexSpaceStr.Replace(" ", "")));
        }
    }

}
