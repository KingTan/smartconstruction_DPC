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
    public class ProtocolAnalysis_GOYO
    {
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                //string str = System.Text.Encoding.UTF8.GetString(b);
               // string dataTemp = Encoding.ASCII.GetString(b);
               
                //if (b.Length < 26)
                //    return "";

                DBFrame df = new DBFrame();
                df.contenthex = ConvertData.ToHexString(b, 0, c);
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\GOYO", "GOYO设备数据原包", df.contenthex);
                df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;
                TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                byte typ = b[5];
                //心跳
                if (typ == 0x00)
                {
                    byte[] rb = OnResolveHeabert(b, c, client, ref df);
                    string data = ConvertData.ToHexString(b, 0, b.Length);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                //实时数据
                if (typ == 0x01)
                {
                    OnResolveCurrent(b, c, client, ref df);
                }
                //参数配置命令下发后的回复
                if (typ == 0x03)
                {
                    try
                    {
                        ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->收到参数配置应答","应答内容：" + df.contenthex);
                        DB_MysqlRaiseDustNoise.UpdateDataParam(TcpExtendTemp.EquipmentID,"2");
                    }
                    catch (Exception ex)
                    {
                        ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV010206IP写入数据库标识异常", ex.Message);
                    }
                    #region 应答
                    ////离线数据应答
                    //byte[] rb = new byte[12];
                    ////针头
                    //rb[0] = 0x7A;
                    //rb[1] = 0x7A;
                    ////协议版本号
                    //rb[2] = 0x01;
                    //rb[3] = 0x00;
                    //rb[4] = 0x00;
                    ////命令字
                    //rb[5] = 0x03;
                    ////数据长度
                    //rb[6] = 0x00;
                    //rb[7] = 0x00;
                    ////数据区
                    //rb[8] = 0x00;
                    ////校验和
                    //rb[9] = 0x00;
                    //rb[10] = 0x00;
                    ////结束
                    //rb[11] = 0x7B;
                    //rb[12] = 0x7B;
                    //client.SendBuffer(rb);
                    #endregion
                }
                //参数上传
                if (typ == 0x04)
                {
                    byte[] rb = OnResolveParm(b, c, client, ref df);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                if (typ == 0x05) //IP地址配置
                {
                    try
                    {
                        DB_MysqlRaiseDustNoise.UpdateDataCongfig(TcpExtendTemp.EquipmentID,"2",true);
                        DB_MysqlRaiseDustNoise.RecordIPCommandIssued(TcpExtendTemp.EquipmentID, 2);
                    }
                    catch (Exception ex)
                    {
                        ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV010206IP写入数据库标识异常", ex.Message);
                    }
                }
                //时间校准
                if (typ == 0x08)
                {
                    byte[] rb = OnResolveTime(b, c);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
              //  TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
                {
                    TcpExtendTemp.EquipmentID = df.deviceid;
                    TcpExtendTemp.EquipmentTag = 1;
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        #region 心跳
        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <param name="client"></param>
        /// <param name="df"></param>7A7A010000000E005943313830303230000101000000F0FA7B7B
        /// <returns></returns>
        public static byte[] OnResolveHeabert(byte[] b, int bCount, TcpSocketClient client, ref DBFrame df)
        {
            if (bCount != 0x1A)
                return null;
            //if (BitConverter.ToUInt16(b, 22) != ConvertData.CRC16(b, 8, 14))//检验和
            //    return null;
            gdust_Heartbeat data = new gdust_Heartbeat();
            string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
            if (tStr != "7A7A")  
                return null;
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            data.sn = Encoding.ASCII.GetString(t);
            tStr = ConvertData.ToHexString(b, 16, 6);//获取时间
            DateTime getdate = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime now = System.DateTime.Now;
            double compare = (now - getdate).TotalMinutes;
            byte[] bytes = new byte[19];
            if (compare > 1 || compare < 0)  //需要
            {
                bytes = new byte[19];
                //数据长度
                bytes[6] = 0x07;
                bytes[7] = 0x00;
                //////////时间校准标示////
                bytes[8] = 0x01;
                //时间
                bytes[9] = byte.Parse(now.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                bytes[10] = byte.Parse(now.Month.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[11] = byte.Parse(now.Day.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[12] = byte.Parse(now.Hour.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[13] = byte.Parse(now.Minute.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[14] = byte.Parse(now.Second.ToString(), System.Globalization.NumberStyles.HexNumber);
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 7));
                bytes[15] = crc16[0];//0x00;//算校验和
                bytes[16] = crc16[1];
                //结束
                bytes[17] = 0x7B;
                bytes[18] = 0x7B;
                data.Rtc = now.ToString("yyyy-MM-dd HH:mm:ss");


            }
            else   //不需要
            {
                bytes = new byte[13];

                //长度 //7A7A0100000001000000007B7B
                bytes[6] = 0x01;
                bytes[7] = 0x00;
                //数据区
                bytes[8] = 0x00;
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 1));
                bytes[9] = crc16[0];//0x00;//算校验和
                bytes[10] = crc16[1];
                //结束
                bytes[11] = 0x7B;
                bytes[12] = 0x7B;
                data.Rtc = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
            }

            bytes[0] = 0x7A;
            bytes[1] = 0x7A;
            //协议版本
            bytes[2] = 0x01;
            bytes[3] = 0x00;
            bytes[4] = 0x00;
            //命令字
            bytes[5] = 0x00;
            //包装
            df.deviceid = data.sn;
            df.datatype = "heartbeat";
            df.contentjson = JsonConvert.SerializeObject(data);
            if (df.contentjson != null && df.contentjson != "")
            {
                DB_MysqlRaiseDustNoise.SaveRaiseDustNoise(df);
            }
            return bytes;
        }
        #endregion
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
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7A7A")
                return;
            gdust_Current current = new gdust_Current();
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            current.sn = Encoding.ASCII.GetString(t);
            tStr = ConvertData.ToHexString(b, 16, 6);
            try
            {
                current.Rtc = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");

            }
            catch
            {
                current.Rtc = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            UShortValue s = new UShortValue();
           
            s.bValue1 = b[22];
            s.bValue2 = b[23];
            if (s.sValue.ToString() == "0")
            {
                current.Pm2_5 = "5";
            }
            else
            {
                current.Pm2_5 = s.sValue.ToString();
            }
            s.bValue1 = b[24];
            s.bValue2 = b[25];
            if (s.sValue.ToString() == "0")
            {
                current.Pm10 = "5";
            }
            else
            {
                current.Pm10 = s.sValue.ToString();
            }
            s.bValue1 = b[26];
            s.bValue2 = b[27];
            current.Noise = (float.Parse(s.sValue.ToString()) / 10).ToString("0.0");
            s.bValue1 = b[28];
            s.bValue2 = b[29];
            current.Temperature = (float.Parse(s.sValue.ToString()) / 10).ToString("0.0");
            s.bValue1 = b[30];
            s.bValue2 = b[31];
            current.Humidity = (float.Parse(s.sValue.ToString()) / 10).ToString("0.0");
            s.bValue1 = b[32];
            s.bValue2 = b[33];
            current.Wind = (float.Parse(s.sValue.ToString()) / 10).ToString("0.0");
            s.bValue1 = b[34];
            s.bValue2 = b[35];
            current.WindDirection = s.sValue.ToString();
            current.GprsSignal = ((sbyte)b[36]).ToString();
            current.automatic = b[37].ToString();
            current.Manual = b[38].ToString();
            s.bValue1 = b[39];
            s.bValue2 = b[40];
            current.pressure = (float.Parse(s.sValue.ToString()) / 100).ToString("0.00");
            s.bValue1 = b[41];
            s.bValue2 = b[42];
            current.particulates = s.sValue.ToString();
            current.alarm = Convert.ToString(b[43], 2).PadLeft(8, '0');
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
        #region  参数上传
        /// <summary>
        /// 参数上传
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <param name="client"></param>
        /// <param name="df"></param>
        public static byte[] OnResolveParm(byte[] b, int bCount, TcpSocketClient client, ref DBFrame df)
        {
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7A7A")
                return null;
            gdust_para para = new gdust_para();
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            para.sn = Encoding.ASCII.GetString(t);
            tStr = ConvertData.ToHexString(b, 16, 6);
            try
            {
                para.updateRtc = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");

            }
            catch
            {
                para.updateRtc = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            UShortValue s = new UShortValue();
            s.bValue1 = b[22];
            s.bValue2 = b[23];
            para.Pm25alarm = s.sValue.ToString();
            s.bValue1 = b[24];
            s.bValue2 = b[25];
            para.Pm10alarm = s.sValue.ToString();
            para.automatic = b[26].ToString();
            para.Manual = b[27].ToString();
            para.cycle = b[28].ToString();
            s.bValue1 = b[29];
            s.bValue2 = b[30];
            para.linkage = s.sValue.ToString();
            string version = "V " + b[31].ToString() + "." + b[32].ToString() + "." + b[33].ToString();
            para.bootversion = version;
            para.softversion = System.Text.Encoding.ASCII.GetString(b, 34, bCount - 38);
            df.deviceid = para.sn;
            df.datatype = "parameter";
            df.contentjson = JsonConvert.SerializeObject(para);
            if (df.contentjson != null && df.contentjson != "")
            {
                DB_MysqlRaiseDustNoise.SaveRaiseDustNoise(df);
            }
            byte[] rb = new byte[12];
            rb[0] = 0x7A;
            rb[1] = 0x7A;
            rb[2] = 0x01;
            rb[3] = 0x00;
            rb[4] = 0x00;
            rb[5] = 0x04;
            rb[6] = 0x00;
            rb[7] = 0x00;
            rb[8] = 0x00;
            rb[9] = 0x00;
            rb[10] = 0x7B;
            rb[11] = 0x7B;
            return rb;
        }
        #endregion
        #region 时间校准
        /// <summary>
        /// 时间校准
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <returns></returns>
        private static byte[] OnResolveTime(byte[] b, int bCount)
        {
            if (BitConverter.ToUInt16(b, 22) != ConvertData.CRC16(b, 8, 14))//检验和
                return null;
            string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
            if (tStr != "7A7A")
                return null;
            tStr = ConvertData.ToHexString(b, 16, 6);//获取时间
            DateTime getdate = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime now = System.DateTime.Now;
            double compare = (now - getdate).TotalMinutes;
            byte[] bytes = new byte[19];
            if (compare > 1 || compare < 0)  //需要
            {
                bytes = new byte[19];
                //数据长度
                bytes[6] = 0x07;
                bytes[7] = 0x00;
                //////////时间校准标示////
                bytes[8] = 0x01;
                //时间
                bytes[9] = byte.Parse(now.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                bytes[10] = byte.Parse(now.Month.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[11] = byte.Parse(now.Day.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[12] = byte.Parse(now.Hour.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[13] = byte.Parse(now.Minute.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[14] = byte.Parse(now.Second.ToString(), System.Globalization.NumberStyles.HexNumber);
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 7));
                bytes[15] = crc16[0];//0x00;//算校验和
                bytes[16] = crc16[1];
                //结束
                bytes[17] = 0x7B;
                bytes[18] = 0x7B;
            }
            else   //不需要
            {
                bytes = new byte[13];

                //长度
                bytes[6] = 0x01;
                bytes[7] = 0x00;
                //数据区
                bytes[8] = 0x00;
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 1));
                bytes[9] = crc16[0];//0x00;//算校验和
                bytes[10] = crc16[1];
                //结束
                bytes[11] = 0x7B;
                bytes[12] = 0x7B;
            }

            bytes[0] = 0x7A;
            bytes[1] = 0x7A;
            //协议版本
            bytes[2] = 0x01;
            bytes[3] = 0x00;
            bytes[4] = 0x00;
            //命令字
            bytes[5] = 0x08;
            return bytes;
        }
        #endregion
    }
}
