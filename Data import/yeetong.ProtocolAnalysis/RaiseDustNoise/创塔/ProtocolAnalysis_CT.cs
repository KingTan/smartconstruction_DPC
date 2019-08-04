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
    public class ProtocolAnalysis_CT
    {
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                DBFrame df = new DBFrame();
                df.contenthex = ConvertData.ToHexString(b, 0, c);
                df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;

                uint Command = Convert.ToUInt32(("0x" + ConvertData.ToHexString(b, 4, 4)), 16);
                //传送的各个环境信息
                if (Command == 0x02)
                {
                    byte[] rb = OnResolveRealData(b, c, ref df);
                    if (rb != null)
                        client.SendBuffer(rb);
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
                return "";

            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\CT", "创塔设备数据解析异常", ex.Message);
            }
            //图片的封包信息(拍照和取图片一致的)
            //else if (Command == 0x8004)
            //{
            //    OnResolveBitmap(b, c, client);
            //}
            return "";
        }
        //实时数据
        //0000005E0000000200000000CFCDDE09595329CE000D94900000177000001996000000000050000002A300000D5C000001570000002000000000000000000000000000016697000000000000000000000000000000000000000000000000
        private static byte[] OnResolveRealData(byte[] b, int c, ref DBFrame df)
        {
            //设备一分钟传输一次数据，如果一分钟之内有可能是重复数据
            try
            {
                // int Total_Length = int.Parse(ConvertData.ToHexString(b, 0, 4)); //4	Unsigned Integer消息总长度(含消息头及消息体)
                Current_CT data = new Current_CT();
                short Command_Id = short.Parse(ConvertData.ToHexString(b, 4, 4));//4	Unsigned Short命令或响应类型0x0802; //（监控数据）7        
                data.Sequence_Id = ConvertDataInt32(ConvertData.ToHexString(b, 8, 4));//4	Unsigned Integer消息流水号(保留未启用)
                //int sss = ConvertDataInt32(ConvertData.ToHexString(b, 12, 4));//4Unsigned Integer设备编号
                data.Device_Id = ConvertDataUInt32(ConvertData.ToHexString(b, 12, 4));//4Unsigned Integer设备编号
                data.Time_stamp = ConvertDataInt32(ConvertData.ToHexString(b, 16, 4));//4Unisgned时间戳(unixtime)
                // data.Data_type = short.Parse(ConvertData.ToHexString(b, 18, 4));//4	Unsigned Short数据类型类型

                //data.SPM = ((Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 20, 4))) / (Decimal)10000;//4	Unsigned Integer粉尘数据（需要除10000）
                data.SPM = (((Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 20, 4))) / (Decimal)10000) / 1000;//4	Unsigned Integer粉尘数据（需要除10000） 歌瑞丽单位ug/m3  所以还得除以1000
                data.PM25 = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 24, 4)) / (Decimal)100;//4	Unsigned IntegerPM2.5
                data.PM10 = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 28, 4)) / (Decimal)100;//4	Unsigned IntegerPM10

                data.TYPE = Convert.ToInt16(("0x" + ConvertData.ToHexString(b, 32, 2)), 16);//2	UnsignedShort0：SPM有效
                //1：PM2.5有效
                //2：PM10 有效
                //3：3个都有效
                //data.windDirection = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 34, 4)) / (Decimal)100;//4	Unsigned Integer风向角度0,后台显示正北
                //data.windSpeed = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 38, 4)) / (Decimal)10;	//4	Unsigned Integer风速为0,后台显示0m/S


                //风速和风向先调整一下
                data.windDirection = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 38, 4)) / (Decimal)10;//4	现在是风角度
                data.windSpeed = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 34, 4)) / (Decimal)100;	//4	现在是风速 2016/10/31号更改成除以100
                // data.windSpeed = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 34, 4)) / (Decimal)10;	//4	现在是风速

                data.Temperature = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 42, 4)) / (Decimal)100;//4	Unsigned Integer温度数据（2661 表示 26.61度）
                data.Humidity = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 46, 4)) / (Decimal)10;	//4	Unsigned Integer湿度数据（7661 表示76.61）
                data.Noise = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 50, 4)) / (Decimal)10;//4	Unsigned Integer噪音等效值（763噪音值为76.3）
                data.maxNoise = (Decimal)ConvertDataInt32(ConvertData.ToHexString(b, 54, 4)) / (Decimal)10;//4	Unsigned Integer噪音峰值（927噪音峰值为92.7）

                #region 创塔的协议
                data.GPS_Y = HexToDouble(ConvertData.ToHexString(b, 58, 8));//8	DoubleGPS经度
                data.GPS_X = HexToDouble(ConvertData.ToHexString(b, 66, 8));//8	DoubleGPS纬度
                data.Pressure = HexToFloat(ConvertData.ToHexString(b, 74, 4));//78Float大气压值
                //data.Remark = ConvertData.ToHexString(b, 152, 16);//16UnsignedChar保留字段（16BYTE）
                #endregion
                #region 格瑞利等其它
                data.GPS_Y = (double)ConvertDataInt32(ConvertData.ToHexString(b, 58, 4)) / 1000000d;//8	DoubleGPS经度
                data.GPS_X = (double)ConvertDataInt32(ConvertData.ToHexString(b, 62, 4)) / 1000000d;//8	DoubleGPS纬度
                data.Pressure = (float)ConvertDataInt32(ConvertData.ToHexString(b, 66, 4));//78Float大气压值
                //data.Remark = ConvertData.ToHexString(b, 152, 16);//16UnsignedChar保留字段（16BYTE）
                #endregion


                byte[] replayrb = new byte[12];
                replayrb[0] = 0;
                replayrb[1] = 0;
                replayrb[2] = 0;
                replayrb[3] = 12;
                replayrb[4] = 0x80;
                replayrb[5] = 0x20;
                replayrb[6] = 0;
                replayrb[7] = 0;
                replayrb[8] = 0;
                replayrb[9] = 0;
                //byte[] replayrb = new byte[10];
                //replayrb[0] = b[0];//长度怎么这样赋值呢？?????????
                //replayrb[1] = b[1];
                //replayrb[2] = b[2];
                //replayrb[3] = b[3];
                //replayrb[4] = 0x80;
                //replayrb[5] = 0x20;
                //replayrb[6] = b[6];
                //replayrb[7] = b[7];
                //replayrb[8] = b[8];
                //replayrb[9] = b[9];


                df.deviceid = data.Device_Id.ToString();
                df.datatype = "current";
                df.contentjson = JsonConvert.SerializeObject(data);

                FogGun.Linkage_dust linkage_dust = new FogGun.Linkage_dust();
                linkage_dust.Equipment = data.Device_Id.ToString();
                linkage_dust.PM25 = (float)data.PM25;
                linkage_dust.PM10 = (float)data.PM10;
                FogGun.Linkage.Dust_data_Process(linkage_dust);

                return replayrb;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\CT", "创塔设备实时数据解析异常：", ex.Message);
                return null;
            }
        }
        #region 辅助转换方法
        private static int ConvertDataInt32(string bStr)
        {
            return Convert.ToInt32(("0x" + bStr), 16);
        }
        private static uint ConvertDataUInt32(string bStr)
        {
            return Convert.ToUInt32(("0x" + bStr), 16);
        }
        /// <summary>
        /// 16 转换成float  2字节
        /// 不能有空格或者- 之类的 必须连续例如 6546D43E
        /// </summary>
        /// <param name="p_strRaw"></param>
        /// <returns></returns>
        private static float HexToFloat(string p_strRaw)
        {
            int len = p_strRaw.Length;
            byte[] TempArry = new byte[len / 2];
            for (int i = 0; i < len / 2; i++)
            {
                TempArry[i] = Convert.ToByte(p_strRaw.Substring(i * 2, 2), 16);
            }
            return BitConverter.ToSingle(TempArry, 0);
        }

        /// <summary>
        /// 16 转换成Int16  2字节
        /// </summary>
        /// <param name="p_strRaw"></param>
        /// <returns></returns>
        private static Int16 HexToInt16(string p_strRaw)
        {
            int len = p_strRaw.Length;
            byte[] TempArry = new byte[len / 2];
            for (int i = 0; i < p_strRaw.Length / 2; i++)
            {
                TempArry[i] = Convert.ToByte(p_strRaw.Substring(i * 2, 2), 16);
            }
            return BitConverter.ToInt16(TempArry, 0);
        }
        /// <summary>
        /// 转换成DOUBLE
        /// </summary>
        /// <param name="p_strRaw"></param>
        /// <returns></returns>
        private static double HexToDouble(string p_strRaw)
        {
            int len = p_strRaw.Length;
            byte[] TempArry = new byte[len / 2];
            for (int i = 0; i < p_strRaw.Length / 2; i++)
            {
                TempArry[i] = Convert.ToByte(p_strRaw.Substring(i * 2, 2), 16);
            }
            return BitConverter.ToDouble(TempArry, 0);
        }
        #endregion

    }
}
