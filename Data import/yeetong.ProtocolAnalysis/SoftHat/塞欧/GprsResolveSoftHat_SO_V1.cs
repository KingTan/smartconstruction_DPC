using Architecture;
using ProtocolAnalysis.SoftHat;
using ProtocolAnalysis.SoftHat.Mysql;
using ProtocolAnalysis.TowerCrane;
using ProtocolAnalysis.TowerCrane.OE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis
{
    public class GprsResolveSoftHat_SO_V1
    {
        static Dictionary<string, Frame_CurrentList> IDCard_Id_list = new Dictionary<string, Frame_CurrentList>();
        /// <summary>
        /// 解析、存储、显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            if (b.Length < 3)
                return "";

            DBFrame df = new DBFrame();
            df.contenthex = ConvertData.ToHexString(b, 0, c);
            df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;

            byte typ = b[4];

            if (typ == 0x01)//实时数据
            {
               byte[] rb =  OnResolveRealData(b, c, ref df);
                if (rb != null)
                    client.SendBuffer(rb);

            }
            else if (typ == 0x02)//上传周期设置
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->收到人员定位设置上传周期参数配置应答", "应答内容" + Encoding.ASCII.GetString(b));
                try
                {
                    OnResolvePeriodCurrent(b, c);

                }
                catch (Exception ex)
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->收到人员定位设置上传周期参数配置异常", ex.ToString());
                }
            }
            else if (typ == 0x03)//网络设置
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->收到人员定位设置网络参数配置应答", "应答内容：" + Encoding.ASCII.GetString(b));
                try
                {
                    OnResolveIpCurrent(b, c);
                }
                catch (Exception ex)
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->收到人员定位设置网络参数配置应答异常", ex.ToString());
                }
            }

            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
            {
                TcpExtendTemp.EquipmentID = df.deviceid;
            }
            //存入数据库
            if (df.contentjson != null && df.contentjson != "")
            {
                DB_MysqlHat.SaveSoftHat(df);
            }
            return "";
        }

        /// <summary>
        /// 实时数据
        /// 7A 7A 01 00 01 00 54 54 36 36 36 36 36 36 00 0D 01 B7 00 01 00 00 00 00 00 0B 52 03 01 9E B9 7B 7B 
        /// </summary>
        /// <param name="b"></param>
        public static Byte[] OnResolveRealData(byte[] b, int bCount, ref DBFrame df)
        {
            string aa = ConvertData.ToHexString(b, 0, b.Length); ;
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7A7A")
                return null;

            Frame_CurrentV1 dataCurrent = new Frame_CurrentV1();
            Frame_CurrentList frame_CurrentList = new Frame_CurrentList();


            byte[] t = new byte[8];
            for (int i = 6, j = 0; i < 14; i++, j++)
            {
                t[j] = b[i];
            }
            UShortValue s = new UShortValue();

            //设备编号
            dataCurrent.DeviceNum = Encoding.ASCII.GetString(t);

            frame_CurrentList.DeviceNo = dataCurrent.DeviceNum;

            string str = b[16].ToString("X2");
            //设备类型
            dataCurrent.DeviceType = Convert.ToInt32(str, 16).ToString();

            str = b[17].ToString("X2");
            //GPRS 信号强度
            dataCurrent.GprsSignal = Convert.ToInt32(str, 16).ToString();

            s.bValue1 = b[18];
            s.bValue2 = b[19];
            str = b[18].ToString("X2") + b[19].ToString("X2");
            //人员数量
            dataCurrent.PeopleNum = Convert.ToInt32(str, 16);

            frame_CurrentList.PeopleNumber =dataCurrent.PeopleNum;
            byte[] returnbyte = new byte[b.Length - 24];
            Array.Copy(b, 20, returnbyte, 0, b.Length - 24);
            int count = 0;
            string peroLable = "";
            for (int i = 0; i < returnbyte.Length;)
            {

                byte[] cs = new byte[9];
                string st = "";
                for (int j = 0; j < 9; j++)
                {
                    int index = j + i;
                    if (index >= returnbyte.Length)
                    {
                        continue;
                    }
                    cs[j] = returnbyte[index];

                }
                if (dataCurrent.DeviceType == "1")
                {

                    string PeopleLable = "";
                    for (int k = 4; k < 8; k++)
                    {
                        PeopleLable += cs[k].ToString("X2");
                    }

                    PeopleLable = Hex2Ten(PeopleLable);
                    frame_CurrentList.IDCard_Id.Add(PeopleLable.ToString().PadLeft(8, '0'), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                i = i + 9;
                count++;
            }

            //人员标签号
            dataCurrent.PeopleLable = peroLable;

            //日期
            dataCurrent.Rtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            tStr = ConvertData.ToHexString(b, b.Length - 2, 2);
            if (tStr != "7B7B")
                return null;

            byte[] bytes = new byte[22];
            Array.Copy(b, 0, bytes, 0, 14);
            bytes[14] = 0x00;
            bytes[15] = 0x02;
            bytes[16] = b[18];
            bytes[17] = b[19];
            //校验和
            byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 0, bytes.Length-4));
            bytes[18] = crc16[0];//0x00;//算校验和
            bytes[19] = crc16[1];
            bytes[20] = 0x7B;
            bytes[21] = 0x7B;
            //存数据库
            //看看是否发送短信
            //存入数据库
            df.deviceid = dataCurrent.DeviceNum;
            df.datatype = "current";
            df.contentjson = JsonConvert.SerializeObject(frame_CurrentList);


            return bytes;
        }

        /// <summary>
        /// 设置网络参数
        /// </summary>
        public static void OnResolveIpCurrent(byte[] b, int bCount)
        {
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7A7A")
                return;

            byte[] t = new byte[8];
            for (int i = 6, j = 0; i < 14; i++, j++)
            {
                t[j] = b[i];
            }

            //设备编号
            string DeviceNum = Encoding.ASCII.GetString(t);
            string str = b[16].ToString("X2");

            //判断应答状态
            if (Convert.ToInt32(str, 16) == 1)
            {

                DB_MysqlHat.UpdateDataConfig(DeviceNum, 2, true);

            }
        }
        /// <summary>
        /// 设置上传周期
        /// </summary>
        public static void OnResolvePeriodCurrent(byte[] b, int bCount)
        {

            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7A7A")
                return;

            byte[] t = new byte[8];
            for (int i = 6, j = 0; i < 14; i++, j++)
            {
                t[j] = b[i];
            }

            //设备编号
            string DeviceNum = Encoding.ASCII.GetString(t);
            string str = b[16].ToString("X2");

            //判断应答状态
            if (Convert.ToInt32(str, 16) == 1)
            {

                DB_MysqlHat.UpdatePeriodDataConfig(DeviceNum, 2,true);

            }
        }

        #region 十六进制转十进制
        /// <summary>
        /// 十六进制转换到十进制
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static string Hex2Ten(string hex)
        {
            int ten = 0;
            for (int i = 0, j = hex.Length - 1; i < hex.Length; i++)
            {
                ten += HexChar2Value(hex.Substring(i, 1)) * ((int)Math.Pow(16, j));
                j--;
            }
            return ten.ToString();
        }

        public static int HexChar2Value(string hexChar)
        {
            switch (hexChar)
            {
                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    return Convert.ToInt32(hexChar);
                case "a":
                case "A":
                    return 10;
                case "b":
                case "B":
                    return 11;
                case "c":
                case "C":
                    return 12;
                case "d":
                case "D":
                    return 13;
                case "e":
                case "E":
                    return 14;
                case "f":
                case "F":
                    return 15;
                default:
                    return 0;
            }
        }
        #endregion

    }
}
