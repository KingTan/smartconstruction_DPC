using Architecture;
using DPC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis
{
    public class ProtocolAnalysis_V1
    {
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
              
                byte typ = b[5];
                //心跳
                if (typ == 0x00)
                {
                    byte[] rb = OnResolveHeabert(b, c, client);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                //实时数据
                if (typ == 0x01)
                {
                    OnResolveCurrent(b, c, client);
                }
                
                //参数上传
                if (typ == 0x04)
                {
                    byte[] rb = OnResolveParm(b, c, client);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                
                //时间校准
                if (typ == 0x08)
                {
                    byte[] rb = OnResolveTime(b, c);
                    if (rb != null)
                        client.SendBuffer(rb);
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
        /// <param name="df"></param>
        /// <returns></returns>
        public static byte[] OnResolveHeabert(byte[] b, int bCount, TcpSocketClient client)
        {
            if (bCount != 0x1A)
                return null;
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
            bytes[4] = 0x04;
            //命令字
            bytes[5] = 0x00;
            //包装

            //更新在线时间
            Dust_noise_operation.Update_equminet_last_online_time(data.sn,DPC.DPC_Tool.GetTimeStamp(DateTime.Now));
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
        public static void OnResolveCurrent(byte[] b, int bCount, TcpSocketClient client)
        {
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7A7A")
                return;
            Zhgd_iot_dust_noise_current current = new  Zhgd_iot_dust_noise_current();
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            current.sn = Encoding.ASCII.GetString(t);
            tStr = ConvertData.ToHexString(b, 16, 6);
            try
            {
                current.timestamp = DPC_Tool.GetTimeStamp( DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture));

            }
            catch
            {
                current.timestamp = DPC_Tool.GetTimeStamp(DateTime.Now);
            }
            UShortValue s = new UShortValue();

            s.bValue1 = b[22];
            s.bValue2 = b[23];
            if (s.sValue.ToString() == "0")
            {
                current.pm2_5 = 5;
            }
            else
            {
                current.pm2_5 = s.sValue;
            }
            s.bValue1 = b[24];
            s.bValue2 = b[25];
            if (s.sValue.ToString() == "0")
            {
                current.pm10 = 5;
            }
            else
            {
                current.pm10 = s.sValue;
            }
            s.bValue1 = b[26];
            s.bValue2 = b[27];
            current.noise =double.Parse( (float.Parse(s.sValue.ToString()) / 10).ToString("0.0"));
            s.bValue1 = b[28];
            s.bValue2 = b[29];
            current.temperature = double.Parse((float.Parse(s.sValue.ToString()) / 10).ToString("0.0"));
            s.bValue1 = b[30];
            s.bValue2 = b[31];
            current.humidity = double.Parse((float.Parse(s.sValue.ToString()) / 10).ToString("0.0"));
            s.bValue1 = b[32];
            s.bValue2 = b[33];
            current.wind_speed = double.Parse((float.Parse(s.sValue.ToString()) / 10).ToString("0.0"));
            s.bValue1 = b[34];
            s.bValue2 = b[35];
            current.wind_direction = s.sValue;
        //    current.GprsSignal = ((sbyte)b[36]).ToString();
           // current.automatic = b[37].ToString();
            //current.Manual = b[38].ToString();
            s.bValue1 = b[39];
            s.bValue2 = b[40];
            current.air_pressure = double.Parse((float.Parse(s.sValue.ToString()) / 100).ToString("0.00"));
            s.bValue1 = b[41];
            s.bValue2 = b[42];
            current.tsp = s.sValue;
            Dust_noise_operation.Send_dust_noise_Current(current);
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
        public static byte[] OnResolveParm(byte[] b, int bCount, TcpSocketClient client)
        {
            byte[] rb = new byte[12];
            rb[0] = 0x7A;
            rb[1] = 0x7A;
            rb[2] = 0x01;
            rb[3] = 0x00;
            rb[4] = 0x04;
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
            bytes[4] = 0x04;
            //命令字
            bytes[5] = 0x08;
            return bytes;
        }
        #endregion
    }
}
