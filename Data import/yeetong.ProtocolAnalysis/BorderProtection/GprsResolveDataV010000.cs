using Architecture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis.BorderProtection
{
   public class GprsResolveDataV010000
    {
        static public byte[] Version = new byte[3];
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            //协议版本号
            Version[0] = b[2]; Version[1] = b[3]; Version[2] = b[4];
            DBFrame df = new DBFrame();
            df.contenthex = ConvertData.ToHexString(b, 0, c);
            df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;

            string str = "";
            //命令字
            byte command = b[5];
            switch (b[5])
            {
                case 0x00://注册帧
                   byte []data= OnResolve_Register(b,ref df);
                    str = ConvertData.ToHexString(b, 0, b.Length);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("临边防护接收设备上传的数据 注册",str);
                    if (data != null)
                    {
                        client.SendBuffer(data);
                    }
                    str = ConvertData.ToHexString(data, 0, data.Length);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("临边防护应答的数据 注册", str);

                    break;
                case 0x01:
                    str = ConvertData.ToHexString(b, 0, b.Length);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("临边防护接收设备上传的数据 平安", str);
                    byte[]data1=OnResolve_SafeData(b, ref df);
                    if (data1 != null)
                    {
                        client.SendBuffer(data1);
                    }
                    str = ConvertData.ToHexString(data1, 0, data1.Length);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("临边防护应答的数据 平安", str);
                    break;
                case 0x02:
                    str = ConvertData.ToHexString(b, 0, b.Length);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("临边防护接收设备上传的数据 警报", str);
                    byte[]data2= OnResolve_AlarmData(b,ref df);
                    if (data2 != null)
                    {
                        client.SendBuffer(data2);
                    }
                    str = ConvertData.ToHexString(data2, 0, data2.Length);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("临边防护应答的数据 警报", str);
                    break;
                case 0x03:
                    str = ConvertData.ToHexString(b, 0, b.Length);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("临边防护接收设备上传的数据 时间校准", str);
                    byte[]data4= OnResolve_TimeCalibrate(b, ref df);
                    if (data4 != null)
                    {
                        client.SendBuffer(data4);
                    }
                    str = ConvertData.ToHexString(data4, 0, data4.Length);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("临边防护应答的数据 时间校准", str);
                    break;
                default:
                    break;
            }
            if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
            {
                TcpExtendTemp.EquipmentID = df.deviceid;
            }
            return "";
        }
        /// <summary>
        /// 注册帧
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <param name="df"></param>
        private static byte[] OnResolve_Register(byte[] b, ref DBFrame df)
        {
            try
            {
                string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
                if (tStr != "7A7A")
                    return null;
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                for (int i = 8, j = 0; i < 16; i++, j++)
                {
                    sn[j] = b[i];
                }
                BorderProtection_Heartbeat data = new BorderProtection_Heartbeat();
                CultureInfo provider = CultureInfo.InvariantCulture;
                provider = new CultureInfo("fr-FR");
                data.sn = Encoding.ASCII.GetString(sn);
                tStr = ConvertData.ToHexString(b, 16, 6);//获取时间 2000/01/01 00:00:00
                string year = tStr.Substring(0, 2);
                string month = tStr.Substring(2, 2);
                string day = tStr.Substring(4, 2);
                string hour = tStr.Substring(6, 2);
                string minute = tStr.Substring(8, 2);
                string second = tStr.Substring(10, 2);
                if (month == "00")
                {
                    month = "01";
                }
                if (day == "00")
                {
                    day = "01";
                }
                string time = string.Format("20{0}/{1}/{2} {3}:{4}:{5}",year ,month ,day,hour,minute,second);
                DateTime getdate = DateTime.ParseExact(time, "yyyy/MM/dd HH:mm:ss", provider);//dd/MM/yyyy HH:mm:ss   yyyy/MM/dd HH:mm:ss
                DateTime now = System.DateTime.Now;
                double compare = (now - getdate).TotalMinutes;
                byte[] bytes = new byte[19];
                if (compare > 1 || compare < 0)  //需要
                {
                    bytes = new byte[19];
                    //数据长度
                    bytes[6] = 0x07;
                    bytes[7] = 0x00;

                    //需要校准时间
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
                else//不需要
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
                    DBMysqlBorderProtection.SaveBorderProtection(df);
                }

                return bytes;
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolve_Register 临边防护异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 安全数据帧
        /// </summary>
        /// <param name="b"></param>
        /// <param name="df"></param>
        /// <returns></returns>
        private static byte[] OnResolve_SafeData(byte[]b,ref DBFrame df)
        {
            try
            {
                string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
                if (tStr != "7A7A")
                    return null;
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                for (int i = 8, j = 0; i < 16; i++, j++)
                {
                    sn[j] = b[i];
                }
                BorderProtection_Current current = new BorderProtection_Current();
                current.DeviceNo = Encoding.ASCII.GetString(sn);
                tStr = ConvertData.ToHexString(b, 16, 6);
                try
                {
                    current.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch
                {
                    current.RTC = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                try
                {
                    current.GPS = Convert.ToDouble(b[22]).ToString();
                }
                catch
                {
                    current.GPS = "0";
                }
                UShortValue s = new UShortValue();
                s.bValue1 = b[23];
                s.bValue2 = b[24];
                current.BatteryLevel = s.sValue.ToString();
                current.RemainEletricPercent = b[25].ToString();
                s.bValue1 = b[26];
                s.bValue2 = b[27];
                current.LinkFailCount = s.sValue.ToString();
                df.deviceid = current.DeviceNo;
                df.datatype = "current";
                df.contentjson = JsonConvert.SerializeObject(current);
                if (df.contentjson != null && df.contentjson != "")
                {
                    DBMysqlBorderProtection.SaveBorderProtection(df);
                }

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

                    //需要校准时间
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
                else//不需要
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
                bytes[5] = 0x01;
                return bytes;
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolve_SafeData 临边防护异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        ///警报信息帧
        /// </summary>
        /// <param name="b"></param>
        /// <param name="df"></param>
        /// <returns></returns>
        private static byte[] OnResolve_AlarmData(byte[] b,ref DBFrame df)
        {
            try
            {
                string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
                if (tStr != "7A7A")
                    return null;
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                for (int i = 8, j = 0; i < 16; i++, j++)
                {
                    sn[j] = b[i];
                }
                BorderProtection_Alarm data = new BorderProtection_Alarm();
                data.DeviceNo = Encoding.ASCII.GetString(sn);
                tStr = ConvertData.ToHexString(b, 16, 6);
                try
                {
                    data.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch
                {
                    data.RTC = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                data.EquipmentAccident = b[22].ToString();
                data.EletricAlarm = b[23].ToString();
                df.deviceid = data.DeviceNo;
                df.datatype = "alarm";
                df.contentjson = JsonConvert.SerializeObject(data);
                if (df.contentjson != null && df.contentjson != "")
                {
                    DBMysqlBorderProtection.SaveBorderProtection(df);
                }
                byte[] rb = new byte[12];
                rb[0] = 0x7A;
                rb[1] = 0x7A;
                rb[2] = 0x01;
                rb[3] = 0x00;
                rb[4] = 0x00;
                rb[5] = 0x02;
                rb[6] = 0x00;
                rb[7] = 0x00;
                rb[8] = 0x00;
                rb[9] = 0x00;
                rb[10] = 0x7B;
                rb[11] = 0x7B;
                return rb;
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolve_AlarmData 临边防护异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 时间校准帧
        /// </summary>
        /// <param name="b"></param>
        /// <param name="df"></param>
        /// <returns></returns>
        private static byte[] OnResolve_TimeCalibrate(byte[] b, ref DBFrame df)
        {
            string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
            if (tStr != "7A7A")
                return null;
            CultureInfo provider = CultureInfo.InvariantCulture;
            provider = new CultureInfo("fr-FR");
            string year = tStr.Substring(0, 2);
            string month = tStr.Substring(2, 2);
            string day = tStr.Substring(4, 2);
            string hour = tStr.Substring(6, 2);
            string minute = tStr.Substring(8, 2);
            string second = tStr.Substring(10, 2);
            if (month == "00")
            {
                month = "01";
            }
            if (day == "00")
            {
                day = "01";
            }
            string time = string.Format("20{0}/{1}/{2} {3}:{4}:{5}", year, month, day, hour, minute, second);
            tStr = ConvertData.ToHexString(b, 16, 6);
            DateTime getdate = DateTime.ParseExact(tStr, "yyyy/MM/dd HH:mm:ss",provider);
            DateTime now = System.DateTime.Now;
            double compare = (now - getdate).TotalMinutes;
            byte[] bytes = new byte[19];
            if (compare > 1 || compare < 0)  //需要
            {
                bytes = new byte[19];
                //数据长度
                bytes[6] = 0x07;
                bytes[7] = 0x00;

                //需要校准时间
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
            else//不需要
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
            bytes[5] = 0x03;
            return bytes;
        }
    }
}
