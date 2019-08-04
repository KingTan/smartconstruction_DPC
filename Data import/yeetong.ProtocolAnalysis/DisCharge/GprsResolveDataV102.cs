using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Architecture;
using ProtocolAnalysis.DisCharge;
using Newtonsoft.Json;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis
{
    public class GprsResolveDataV102
    {
        #region 数据解析入口
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                if (c > 12)
                {
                    #region 拆结构
                    string dataHexString = ConvertData.ToHexString(b, 0, c);
                    //头
                    string startFlag = dataHexString.Substring(0, 4);
                    //协议版本号
                    string version = "V" + dataHexString.Substring(4, 1).Replace("0", "") + dataHexString.Substring(5, 1) + "." + dataHexString.Substring(6, 1).Replace("0", "") + dataHexString.Substring(7, 1) + "." + dataHexString.Substring(8, 1).Replace("0", "") + dataHexString.Substring(9, 1);
                    //命令字
                    byte command = b[5];
                    //数据长度
                    short datalength = Convert.ToInt16(dataHexString.Substring(14, 2) + dataHexString.Substring(12, 2), 16);
                    //CRC
                    short crc = Convert.ToInt16(dataHexString.Substring(dataHexString.Length - 8, 2) + dataHexString.Substring(dataHexString.Length - 6, 2), 16);
                    //结束符
                    string endFlag = dataHexString.Substring(dataHexString.Length - 4);
                    //数据域
                    string dataMagStr = dataHexString.Substring(16, datalength * 2);
                    #endregion

                    DBFrame df = new DBFrame();
                    df.contenthex = ConvertData.ToHexString(b, 0, c);
                    df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;
                    TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;

                    byte[] dataMagAry = ConvertData.HexToByte(dataMagStr);
                    switch (command)
                    {
                        //心跳
                        case 0x00:
                            ReceiveDispose_Heartbeat(dataMagAry, client, ref df);
                            break;
                        //实时数据
                        case 0x01:
                            ReceiveDispose_Current(dataMagAry, client, true, ref df);
                            break;
                        //保留
                        case 0x02:
                            break;
                        //离线数据
                        case 0x03:
                            ReceiveDispose_Current(dataMagAry, client, false, ref df);
                            break;
                        //参数信息
                        case 0x04:
                            ReceiveDispose_ParameterConfig(dataMagAry, client, ref df);
                            break;
                        //IP地址配置
                        case 0x05:
                            ReceiveDispose_IPConfiguration(dataMagAry, TcpExtendTemp);
                            break;
                        //保留
                        case 0x06:
                            break;
                        //设备运行时间
                        case 0x07:
                            ReceiveDispose_RunTime(dataMagAry, client, ref df);
                            break;
                        //时间校准
                        case 0x08:
                            ReceiveDispose_TimeCalibration(dataMagAry, client, ref df);
                            break;
                        default: break;
                    }


                    if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
                    {
                        TcpExtendTemp.EquipmentID = df.deviceid;
                    }
                    //存入数据库
                    if (df.contentjson != null && df.contentjson != "")
                    {
                        DB_MysqlDisCharge.SaveDisCharge(df);
                    }
                }
                return "";
            }
            catch(Exception ex)
            {
                return "";
            }
        }
        #endregion
        #region 设备发来的数据的处理
        //心跳
        //7A 7A 01 00 02 00 0E 00 31 32 33 34 35 36 37 38 17 10 31 17 34 00 C9 6A 7B 7B
        //7a 7a 01 00 02 00 07 00 01 17 10 31 17 38 29 27 cb 7b 7b 
        static public void ReceiveDispose_Heartbeat(byte[] data, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                Frame_Heartbeat dataFrame = new Frame_Heartbeat();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                for (int i = 0; i < 8; i++)
                {
                    sn[i] = data[i];
                }
                dataFrame.DeviceNo = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //RTC
                string tStr = ConvertData.ToHexString(data, 8, 6);
                try
                {
                    dataFrame.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    dataFrame.RTC = null;
                }
                //拼接应答
                byte[] result = SendJoint_Heartbeat(dataFrame);
                if (result != null)
                    client.SendBuffer(result);

                //包装
                df.deviceid = dataFrame.DeviceNo;
                df.datatype = "heartbeat";
                df.contentjson = JsonConvert.SerializeObject(dataFrame);
            }
            catch (Exception)
            { }
        }
        //实时数据
        //7A 7A 01 00 02 01 15 00 31 32 33 34 35 36 37 38 17 10 31 17 34 00 01 00 02 00 03 00 01 15 03 7B 7B
        //有报警7A 7A 01 00 02 01 15 00 31 32 33 34 35 36 37 38 17 10 31 17 34 00 01 00 02 00 03 00 0f DB E2 7B 7B
        static public void ReceiveDispose_Current(byte[] data, TcpSocketClient client, bool isCurrent, ref DBFrame df)
        {
            try
            {
                Frame_Current dataFrame = new Frame_Current();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                for (int i = 0; i < 8; i++)
                {
                    sn[i] = data[i];
                }
                dataFrame.DeviceNo = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //RTC
                string tStr = ConvertData.ToHexString(data, 8, 6);
                try
                {
                    dataFrame.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    dataFrame.RTC = DateTime.Now;
                }
                //当前载重
                UShortValue us = new UShortValue();
                us.bValue1 = data[14];
                us.bValue2 = data[15];
                dataFrame.Load = us.sValue;
                //倾角X
                ShortValue s = new ShortValue();
                s.bValue1 = data[16];
                s.bValue2 = data[17];
                dataFrame.AngleX = (double)((double)(s.sValue) / 100d);
                //倾角Y
                s.bValue1 = data[18];
                s.bValue2 = data[19];
                dataFrame.AngleY = (double)((double)(s.sValue) / 100d);
                //报警状态
                string state = Convert.ToString(data[20], 2).PadLeft(8, '0');
                dataFrame.AlarmType = state;
                dataFrame.WeightWarning = (byte)(state[7] - 48);
                dataFrame.WeightAlarm = (byte)(state[6] - 48);
                dataFrame.AngleWarning = (byte)(state[5] - 48);
                dataFrame.AngleAlarm = (byte)(state[4] - 48);
                //离线数据应答
                if (!isCurrent)
                {
                    byte[] result = SendJoint_OffLine();
                    if (result != null)
                        client.SendBuffer(result);
                }

                //包装
                df.deviceid = dataFrame.DeviceNo;
                df.datatype = "current";
                df.contentjson = JsonConvert.SerializeObject(dataFrame);
            }
            catch (Exception)
            { }
        }
        //参数配置
        //7A 7A 01 00 02 04 32 00 31 32 33 34 35 36 37 38 17 10 31 17 34 00 17 10 31 01 00 01 01 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 09 00 0a 00 0b 00 0c 00 0d 00 31 32 33 D4 18 7b 7b
        //7a 7a 01 00 02 04 00 00 00 00 7b 7b 
        static public void ReceiveDispose_ParameterConfig(byte[] data, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                Frame_ParameterConfig dataFrame = new Frame_ParameterConfig();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                for (int i = 0; i < 8; i++)
                {
                    sn[i] = data[i];
                }
                dataFrame.DeviceNo = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //参数修改时间
                string tStr = ConvertData.ToHexString(data, 8, 6);
                try
                {
                    dataFrame.ParameterUpdateTime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    dataFrame.ParameterUpdateTime = DateTime.Now;
                }
                //安装时间
                tStr = ConvertData.ToHexString(data, 14, 3);
                try
                {
                    dataFrame.InstallDate = DateTime.ParseExact(tStr, "yyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    dataFrame.InstallDate = DateTime.Now;
                }
                //额定载荷
                UShortValue us = new UShortValue();
                us.bValue1 = data[17];
                us.bValue2 = data[18];
                dataFrame.LoadRating = us.sValue;
                //预警系数
                dataFrame.EarlyAlarmCoefficient = data[19];
                //报警系数
                dataFrame.AlarmCoefficient = data[20];
                //空载AD1
                ShortValue s = new ShortValue();
                s.bValue1 = data[21];
                s.bValue2 = data[22];
                dataFrame.EmptyAD1 = s.sValue;
                //空载AD2
                s.bValue1 = data[23];
                s.bValue2 = data[24];
                dataFrame.EmptyAD2 = s.sValue;
                //空载AD3
                s.bValue1 = data[25];
                s.bValue2 = data[26];
                dataFrame.EmptyAD3 = s.sValue;
                //空载AD4
                s.bValue1 = data[27];
                s.bValue2 = data[28];
                dataFrame.EmptyAD4 = s.sValue;
                //空载AD
                s.bValue1 = data[29];
                s.bValue2 = data[30];
                dataFrame.EmptyAD = s.sValue;
                //标准重物AD1
                s.bValue1 = data[31];
                s.bValue2 = data[32];
                dataFrame.StandardLoadAD1 = s.sValue;
                //标准重物AD2
                s.bValue1 = data[33];
                s.bValue2 = data[34];
                dataFrame.StandardLoadAD2 = s.sValue;
                //标准重物AD3
                s.bValue1 = data[35];
                s.bValue2 = data[36];
                dataFrame.StandardLoadAD3 = s.sValue;
                //标准重物AD4
                s.bValue1 = data[37];
                s.bValue2 = data[38];
                dataFrame.StandardLoadAD1 = s.sValue;
                //标准重物AD
                s.bValue1 = data[39];
                s.bValue2 = data[40];
                dataFrame.StandardLoadAD = s.sValue;
                //标准重物
                s.bValue1 = data[41];
                s.bValue2 = data[42];
                dataFrame.StandardLoad = s.sValue;
                //倾角预警值
                s.bValue1 = data[43];
                s.bValue2 = data[44];
                dataFrame.AngleEarlyAlarm = s.sValue;
                //倾角报警值
                s.bValue1 = data[45];
                s.bValue2 = data[46];
                dataFrame.AngleAlarm = s.sValue;
                //软件版本号 ASCII
                byte[] SoftVersion = new byte[data.Length - 47];
                for (int i = 47, j = 0; i < data.Length; i++, j++)
                {
                    SoftVersion[j] = data[i];
                }
                dataFrame.SoftVersion = Encoding.ASCII.GetString(SoftVersion);
                //拼接应答
                byte[] result = SendJoint_ParameterConfig(dataFrame);
                if (result != null)
                    client.SendBuffer(result);

                //包装
                df.deviceid = dataFrame.DeviceNo;
                df.datatype = "parameterUpload";
                df.contentjson = JsonConvert.SerializeObject(dataFrame);
            }
            catch (Exception)
            { }
        }
        //IP配置帧设备的应答
        static public void ReceiveDispose_IPConfiguration(byte[] data, TcpClientBindingExternalClass TcpExtendTemp)
        {
            try
            {
                Frame_IPConfiguration dataFrame = new Frame_IPConfiguration();
                dataFrame.DeviceNo = TcpExtendTemp.EquipmentID;
                //缓存 等待写入数据库
                DB_MysqlDisCharge.UpdateIPConfiguration(dataFrame, 2,true);
                DB_MysqlDisCharge.RecordCommandIssued(dataFrame.DeviceNo, 2);

            }
            catch (Exception)
            { }

        }
        //运行时间帧
        //7A 7A 01 00 02 07 1c 00 31 32 33 34 35 36 37 38 01 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 2F D2 7b 7b
        //7a 7a 01 00 02 07 00 00 00 00 7b 7b
        static public void ReceiveDispose_RunTime(byte[] data, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                Frame_RunTime dataFrame = new Frame_RunTime();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                for (int i = 0; i < 8; i++)
                {
                    sn[i] = data[i];
                }
                dataFrame.DeviceNo = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII

                //总运行时间
                UIntValue ui = new UIntValue();
                ui.bValue1 = data[8];
                ui.bValue2 = data[9];
                ui.bValue1 = data[10];
                ui.bValue2 = data[11];
                dataFrame.TotalRunTime = ui.iValue / 3600;
                //开机运行时间
                ui.bValue1 = data[12];
                ui.bValue2 = data[13];
                ui.bValue1 = data[14];
                ui.bValue2 = data[15];
                dataFrame.PowerOnTime = ui.iValue / 3600;
                //应答
                byte[] result = SendJoint_RunTime();
                if (result != null)
                    client.SendBuffer(result);

                //包装
                df.deviceid = dataFrame.DeviceNo;
                df.datatype = "runtimeEp";
                df.contentjson = JsonConvert.SerializeObject(dataFrame);
            }
            catch (Exception)
            { }
        }
        //时间校准
        //7A 7A 01 00 02 08 0e 00 31 32 33 34 35 36 37 38 17 11 01 09 58 00 54 FA 7b 7b
        //7a 7a 01 00 02 08 07 00 01 17 11 01 10 09 02 a2 6b 7b 7b
        static public void ReceiveDispose_TimeCalibration(byte[] data, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                Frame_TimeCalibration dataFrame = new Frame_TimeCalibration();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                for (int i = 0; i < 8; i++)
                {
                    sn[i] = data[i];
                }
                dataFrame.DeviceNo = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //RTC
                string tStr = ConvertData.ToHexString(data, 8, 6);
                try
                {
                    dataFrame.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    dataFrame.RTC = null;
                }
                //拼接应答
                byte[] result = SendJoint_TimeCalibration(dataFrame);
                if (result != null)
                    client.SendBuffer(result);

                //包装
                df.deviceid = dataFrame.DeviceNo;
                df.datatype = "checkTime";
                df.contentjson = JsonConvert.SerializeObject(dataFrame);
            }
            catch (Exception)
            { }
        }
        #endregion

        #region 服务器发到设备的拼接
        //心跳
        static public byte[] SendJoint_Heartbeat(Frame_Heartbeat Heartbeat)
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x01);
                msgTemp.Add(0x00);
                msgTemp.Add(0x02);
                //命令字
                msgTemp.Add(0x00);
                //数据长度
                msgTemp.Add(0x07);
                msgTemp.Add(0x00);
                if (Heartbeat.RTC == null)
                {
                    //时间标识
                    msgTemp.Add(0x01);
                }
                else
                {
                    double compare = Math.Abs((DateTime.Now - (DateTime)Heartbeat.RTC).TotalMinutes);
                    if (compare > 1)//需要校验
                    {
                        //时间标识
                        msgTemp.Add(0x01);
                    }
                    else//不需要校验
                    {
                        //时间标识
                        msgTemp.Add(0x00);
                    }
                }
                //RTC
                DateTime dt = DateTime.Now;
                msgTemp.Add(byte.Parse(dt.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Month.ToString(), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Day.ToString(), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Hour.ToString(), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Minute.ToString(), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Second.ToString(), System.Globalization.NumberStyles.HexNumber));
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, byteTemp.Length - 8));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);

                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV100.SendJoint_Heartbeat:error",ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        //参数配置
        static public byte[] SendJoint_ParameterConfig(Frame_ParameterConfig ParameterConfig)
        {
             try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x01);
                msgTemp.Add(0x00);
                msgTemp.Add(0x02);
                //命令字
                msgTemp.Add(0x04);
                //数据长度
                msgTemp.Add(0x00);
                msgTemp.Add(0x00);
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, byteTemp.Length - 8));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);

                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV100.SendJoint_ParameterConfig:error", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        //离线数据的应答
        static public byte[] SendJoint_OffLine()
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x01);
                msgTemp.Add(0x00);
                msgTemp.Add(0x02);
                //命令字
                msgTemp.Add(0x03);
                //数据长度
                msgTemp.Add(0x00);
                msgTemp.Add(0x00);
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, byteTemp.Length - 8));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);
                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV100.SendJoint_OffLine:error", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        //IP 配置帧
        static public byte[] SendJoint_IPConfiguration(Frame_IPConfiguration IPConfiguration)
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x01);
                msgTemp.Add(0x00);
                msgTemp.Add(0x02);
                //命令字
                msgTemp.Add(0x05);

                byte ipLength = (byte)IPConfiguration.IP.Length;
                byte portLength = (byte)IPConfiguration.Port.Length;
                //数据长度
                byte length = (byte)(ipLength + portLength + 2);
                msgTemp.Add(length);
                msgTemp.Add(0x00);
                //ip或域名的长度
                msgTemp.Add(ipLength);
                //ip或域名
                byte[] ip = Encoding.ASCII.GetBytes(IPConfiguration.IP);
                msgTemp.AddRange(ip);
                //端口的长度
                msgTemp.Add(portLength);
                //端口
                byte[] port = Encoding.ASCII.GetBytes(IPConfiguration.Port);
                msgTemp.AddRange(port);
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, byteTemp.Length-8));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);

                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV100.SendJoint_IPConfiguration:error", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        //运行时间的应答
        static public byte[] SendJoint_RunTime()
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x01);
                msgTemp.Add(0x00);
                msgTemp.Add(0x02);
                //命令字
                msgTemp.Add(0x07);
                //数据长度
                msgTemp.Add(0x00);
                msgTemp.Add(0x00);
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, byteTemp.Length - 8));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);
                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV100.SendJoint_RunTime:error", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        //时间校准
        static public byte[] SendJoint_TimeCalibration(Frame_TimeCalibration TimeCalibration)
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x01);
                msgTemp.Add(0x00);
                msgTemp.Add(0x02);
                //命令字
                msgTemp.Add(0x08);
                //数据长度
                msgTemp.Add(0x07);
                msgTemp.Add(0x00);
                if (TimeCalibration.RTC == null)
                {
                    //时间标识
                    msgTemp.Add(0x01);
                }
                else
                {
                    double compare = Math.Abs((DateTime.Now - (DateTime)TimeCalibration.RTC).TotalMinutes);
                    if (compare > 1)//需要校验
                    {
                        //时间标识
                        msgTemp.Add(0x01);
                    }
                    else//不需要校验
                    {
                        //时间标识
                        msgTemp.Add(0x00);
                    }
                }
                //RTC
                DateTime dt = DateTime.Now;
                msgTemp.Add(byte.Parse(dt.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Month.ToString(), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Day.ToString(), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Hour.ToString(), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Minute.ToString(), System.Globalization.NumberStyles.HexNumber));
                msgTemp.Add(byte.Parse(dt.Second.ToString(), System.Globalization.NumberStyles.HexNumber));
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, byteTemp.Length - 8));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);

                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV100.SendJoint_TimeCalibration:error", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        #endregion


    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public class UIntValue
    {
        [FieldOffset(0)]
        public byte bValue1;
        [FieldOffset(1)]
        public byte bValue2;
        [FieldOffset(2)]
        public byte bValue3;
        [FieldOffset(3)]
        public byte bValue4;
        [FieldOffset(0)]
        public uint iValue;
    }
}
