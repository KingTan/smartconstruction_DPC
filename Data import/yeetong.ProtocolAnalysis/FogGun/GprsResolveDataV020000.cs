using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Architecture;
using Newtonsoft.Json;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis.FogGun
{
   public class GprsResolveDataV020000
   {
       static public byte[] Version = new byte[3];
        #region 数据解析入口
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                if (c >= 12)
                {
                    #region 拆结构
                    string dataHexString = ConvertData.ToHexString(b, 0, c);
                    //头
                    string startFlag = dataHexString.Substring(0, 4);
                    //协议版本号
                    Version[0] = b[2]; Version[1] = b[3]; Version[2] = b[4];
                    string version = "V" + dataHexString.Substring(4, 2) + "." + dataHexString.Substring(6, 2) + "." + dataHexString.Substring(8, 2);
                    //命令字
                    byte command = b[5];
                    //数据长度
                    short datalength = Convert.ToInt16(dataHexString.Substring(14, 2) + dataHexString.Substring(12, 2), 16);
                    //CRC
                    short crc = Convert.ToInt16(dataHexString.Substring(dataHexString.Length - 8, 2) + dataHexString.Substring(dataHexString.Length - 6, 2), 16);
                    //结束符
                    string endFlag = dataHexString.Substring(dataHexString.Length - 4);
                    //数据域
                    string dataMagStr = dataHexString.Substring(16, dataHexString.Length - 24);
                    #endregion

                    DBFrame df = new DBFrame();
                    df.contenthex = ConvertData.ToHexString(b, 0, c);
                    df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;
                    TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;

                    byte[] dataMagAry = ConvertData.HexToByte(dataMagStr);
                    switch (command)
                    {
                        //心跳
                        case 0x10:
                            ReceiveDispose_Heartbeat(dataMagAry, client, ref df);   //dataMagAry
                            break;
                        //定时功能相关参数配置  离线时使用
                        case 0x11:
                            ReceiveDispose_TimingConfig(TcpExtendTemp);
                            break;
                        //控制  手动开启和关闭
                        case 0x12:
                            ReceiveDispose_ManualControl(TcpExtendTemp);
                            break;
                        //定时控制  开启后工作一段时间自动关闭
                        case 0x13:
                            if (TcpExtendTemp.EquipmentTag == 0)
                            {
                                ReceiveDispose_TimedControl(TcpExtendTemp);
                            }
                            else if (TcpExtendTemp.EquipmentTag == 3)
                            {
                                ReciveResponseAboutFoggunSettingTime(TcpExtendTemp);
                            }
                            TcpExtendTemp.EquipmentTag = 0;
                            break;
                        //ip配置设备应答
                        case 0x14:
                            
                            ReceiveDispose_IPConfiguration(TcpExtendTemp);
                            break;
                        case 0x15: //时间校准
                            ReceiveDispose_TimeCalibration(dataMagAry, client, ref df);
                            break;
                        case 0x16: //实时数据帧
                            ReceiveDispose_Current(dataMagAry, client, ref df);
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
                        DB_MysqlFogGun.SaveFogGun(df);
                    }
                }
                return "";
            }
            catch (Exception ex)
            { return ""; }
        }
        #endregion

        #region 设备发来的数据的处理
        //心跳
        //7A 7A 01 00 02 00 0E 00 58 58 46 41 31 31 31 31 00 00 00 00 00 00 E3 36 7B 7B
        //7a 7a 01 00 02 00 07 00 00 17 11 01 15 46 55 d3 0f 7b 7b
        static public void ReceiveDispose_Heartbeat(byte[] data, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                DateTime now = System.DateTime.Now;
                Frame_Heartbeat dataFrame = new Frame_Heartbeat();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                for (int i = 8, j = 0; i < 16; i++, j++)
                {
                    sn[j] = data[j];
                }
                dataFrame.DeviceNo = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                try
                {
                    //RTC
                    string tStr = ConvertData.ToHexString(data, 8, 6);
                    dataFrame.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    //string nowStr = now.ToString("yyMMddHHmmss");
                    //dataFrame.RTC = DateTime.ParseExact(nowStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
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

        //定时功能相关参数配置
        static public void ReceiveDispose_TimingConfig(TcpClientBindingExternalClass TcpExtendTemp)
        {
            Frame_TimingConfig dataFrame = new Frame_TimingConfig();
            if (TcpExtendTemp.EquipmentID != null && !TcpExtendTemp.EquipmentID.Equals(""))
            {
                dataFrame.DeviceNo = TcpExtendTemp.EquipmentID;
                DB_MysqlFogGun.SaveTimingConfig(dataFrame, 2);
                ToolAPI.XMLOperation.WriteLogXmlNoTail("定时功能相关参数配置应答：", dataFrame.DeviceNo);
            }
        }
        //手动控制
        static public void ReceiveDispose_ManualControl(TcpClientBindingExternalClass TcpExtendTemp)
        {
            Frame_ManualControl dataFrame = new Frame_ManualControl();
            if (TcpExtendTemp.EquipmentID != null && !TcpExtendTemp.EquipmentID.Equals(""))
            {
                dataFrame.DeviceNo = TcpExtendTemp.EquipmentID;
                //缓存 等待写入数据库
                DB_MysqlFogGun.SaveManualControl(dataFrame, 2);
                ToolAPI.XMLOperation.WriteLogXmlNoTail("手动控制应答", dataFrame.DeviceNo);
            }
        }
        //定时控制
        static public void ReceiveDispose_TimedControl(TcpClientBindingExternalClass TcpExtendTemp)
        {
            Frame_TimedControl dataFrame = new Frame_TimedControl();
            if (TcpExtendTemp.EquipmentID != null && !TcpExtendTemp.EquipmentID.Equals(""))
            {
                dataFrame.DeviceNo = TcpExtendTemp.EquipmentID;
                //缓存 等待写入数据库
                DB_MysqlFogGun.SaveTimedControl(dataFrame, 2);
             
                ToolAPI.XMLOperation.WriteLogXmlNoTail("定时控制应答", dataFrame.DeviceNo);
            }
        }
        /// <summary>
        /// 时间校准
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="df"></param>
        static public void ReceiveDispose_TimeCalibration(byte[] data, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                DateTime now = System.DateTime.Now;
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
                byte[] result = SendJoint_HeartbeatJZ(dataFrame);
                if (result != null)
                    client.SendBuffer(result);

                //包装
                //df.deviceid = dataFrame.DeviceNo;
                // df.datatype = "checkTime";
                //df.contentjson = JsonConvert.SerializeObject(dataFrame);
            }
            catch (Exception)
            { }
        }
        //实时数据
        //7A 7A 02 00 00 16 12 00 31 32 33 34 35 36 37 38 18 09 14 17 31 00 00 00 00 00 5B 3E 7B 7B
        //7A 7A 02 00 00 16 12 00 31 32 33 34 35 36 37 38 18 09 14 17 31 00 00 00 01 6A 0D 3E 7B 7B
        static public void ReceiveDispose_Current(byte[] data, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                DateTime now = System.DateTime.Now;
                Frame_Current dataFrame = new Frame_Current();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(data, 0, sn, 0, 8);
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
                //警报灯状态
                dataFrame.WarningLampStatus = data[14];
                //雾炮喷淋状态
                try
                {
                    dataFrame.FogGunStatus = data[16];
                }
                catch(Exception ex )
                    { }

                //包装
                df.deviceid = dataFrame.DeviceNo;
                df.datatype = "current";
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
                msgTemp.Add(0x02);
                msgTemp.Add(0x00);
                msgTemp.Add(0x00);
                //命令字
                msgTemp.Add(0x10);
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
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, 7));
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("心跳拼接出现异常", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        static public byte[] SendJoint_HeartbeatJZ(Frame_Heartbeat Heartbeat)
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x02);
                msgTemp.Add(0x00);
                msgTemp.Add(0x00);
                //命令字
                msgTemp.Add(0x15);
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
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, 7));
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("心跳拼接出现异常", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        //定时功能相关参数配置
        static public byte[] SendJoint_TimingConfig(Frame_TimingConfig TimingConfig)
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x02);
                msgTemp.Add(0x00);
                msgTemp.Add(0x00);
                //命令字
                msgTemp.Add(0x11);
                //数据长度
                msgTemp.Add(0x0E);
                msgTemp.Add(0x00);
                //设备编号
                byte[] DeviceNo = Encoding.ASCII.GetBytes(TimingConfig.DeviceNo);
                msgTemp.AddRange(DeviceNo);
                //定时功能开关
                msgTemp.Add(byte.Parse(TimingConfig.TimingSwitch));
                //定时启动的时间
                string tm = TimingConfig.Time.Replace(":", "");
                byte[] time = ConvertData.HexToByte(tm);
                msgTemp.AddRange(time);
                //持续时间
                byte[] timeout = ValueTypeToByteArray.GetBytes_LittleEndian((ushort.Parse(TimingConfig.Timeout)));
                msgTemp.AddRange(timeout);
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, 14));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);

                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                ToolAPI.XMLOperation.WriteLogXmlNoTail("定时功能相关参数配置命令包：", TimingConfig.DeviceNo + ";" + ConvertData.ToHexString(byteTemp, 0, byteTemp.Length));
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("定时功能相关参数配置拼接出现异常", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        //手动控制
        static public byte[] SendJoint_ManualControl(Frame_ManualControl ManualControl)
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x02);
                msgTemp.Add(0x00);
                msgTemp.Add(0x00);
                //命令字
                msgTemp.Add(0x12);
                //数据长度
                msgTemp.Add(0x09);
                msgTemp.Add(0x00);
                //设备编号
                byte[] DeviceNo = Encoding.ASCII.GetBytes(ManualControl.DeviceNo);
                msgTemp.AddRange(DeviceNo);
                //设备状态
                msgTemp.Add(byte.Parse(ManualControl.DeviceState));
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, 9));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);

                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                ToolAPI.XMLOperation.WriteLogXmlNoTail("手动控制命令包：", ManualControl.DeviceNo + ";" + ConvertData.ToHexString(byteTemp, 0, byteTemp.Length));
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("手动控制拼接出现异常", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        //IP配置帧设备的应答
        static public void ReceiveDispose_IPConfiguration(TcpClientBindingExternalClass TcpExtendTemp)
        {
            Frame_UpdateIpPort dataFrame = new Frame_UpdateIpPort();
            if (TcpExtendTemp.EquipmentID != null && !TcpExtendTemp.EquipmentID.Equals(""))
            {
                dataFrame.DeviceNo = TcpExtendTemp.EquipmentID;
                dataFrame.State = 2;
                dataFrame.issuccess = true;
                //缓存 等待写入数据库
                DB_MysqlFogGun.SaveUpdateIpPort(dataFrame);
                DB_MysqlFogGun.RecordCommandIssued(dataFrame.DeviceNo, 2);
                ToolAPI.XMLOperation.WriteLogXmlNoTail("IP配置应答", dataFrame.DeviceNo);
            }
           
        }
        //定时控制
        static public byte[] SendJoint_TimedControl(Frame_TimedControl TimedControl)
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x02);
                msgTemp.Add(0x00);
                msgTemp.Add(0x00);
                //命令字
                msgTemp.Add(0x13);
                //数据长度
                msgTemp.Add(0x0A);
                msgTemp.Add(0x00);
                //设备编号
                byte[] DeviceNo = Encoding.ASCII.GetBytes(TimedControl.DeviceNo);
                msgTemp.AddRange(DeviceNo);
                //持续时间

                byte[] timeout = ValueTypeToByteArray.GetBytes_LittleEndian((ushort.Parse(TimedControl.Timeout)));

                msgTemp.AddRange(timeout);
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, 10));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);

                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                ToolAPI.XMLOperation.WriteLogXmlNoTail("定时控制命令包：", TimedControl.DeviceNo + ";" + ConvertData.ToHexString(byteTemp, 0, byteTemp.Length));
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("定时控制拼接出现异常", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        #endregion
        static public byte[] SendJoint_UpdateIpPort(Frame_UpdateIpPort UpdateIpPort)
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(Version[0]);
                msgTemp.Add(Version[1]);
                msgTemp.Add(Version[2]);
                //命令字
                msgTemp.Add(0x14);
                //数据长度
                UInt16 length = (UInt16)(UpdateIpPort.IPLength + UpdateIpPort.PortLength + 2);
                msgTemp.Add((byte)(length % 256));
                msgTemp.Add((byte)(length / 256));
                //ip长度
                msgTemp.Add(UpdateIpPort.IPLength);
                //ip
                msgTemp.AddRange(Encoding.ASCII.GetBytes(UpdateIpPort.IP));
                //端口号长度
                msgTemp.Add(UpdateIpPort.PortLength);
                //端口号
                msgTemp.AddRange(Encoding.ASCII.GetBytes(UpdateIpPort.Port));
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, length));
                msgTemp.Add(crc16[0]);
                msgTemp.Add(crc16[1]);
                //结束符
                msgTemp.Add(0x7B);
                msgTemp.Add(0x7B);

                byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                ToolAPI.XMLOperation.WriteLogXmlNoTail("更改IP命令包：", UpdateIpPort.DeviceNo + ";" + ConvertData.ToHexString(byteTemp, 0, byteTemp.Length));
                return byteTemp;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("定时控制拼接出现异常", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        /// <summary>
        /// 雾泡喷淋 定时喷淋
        /// </summary>
        /// <returns></returns>
        public static byte[] FoggunSettingTimeWork(FoggunSettingTimeWorkModel model)
        {
            try
            {
                List<byte> msgTemp = new List<byte>();
                //头
                msgTemp.Add(0x7A);
                msgTemp.Add(0x7A);
                //协议版本号
                msgTemp.Add(0x02);
                msgTemp.Add(0x00);
                msgTemp.Add(0x00);
                //命令字
                msgTemp.Add(0x13);
                //数据长度
                msgTemp.Add(0x0A);
                msgTemp.Add(0x00);
                //设备编号
                byte[] DeviceNo = Encoding.ASCII.GetBytes(model.equipmentNo);
                msgTemp.AddRange(DeviceNo);
                //喷淋持续时间
                byte[] time = ValueTypeToByteArray.GetBytes_LittleEndian((ushort.Parse(model.workCycle.ToString())));
                msgTemp.AddRange(time);
                //校验和
                byte[] byteTemp = new byte[msgTemp.Count];
                msgTemp.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, 10));
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("FoggunSettingTimeWork 拼接包时出错", ex.Message + "|" + ex.StackTrace.ToString());
                return null;
            }
        }
        /// <summary>
        /// 雾泡下发定时喷淋命令后的回复
        /// </summary>
        /// <param name="TcpExtendTemp"></param>
        static void ReciveResponseAboutFoggunSettingTime(TcpClientBindingExternalClass TcpExtendTemp)
        {
            if (TcpExtendTemp.EquipmentID != null && !TcpExtendTemp.EquipmentID.Equals(""))
            {
                DB_MysqlFogGun.UpdateFoggunSettingTimeWork(TcpExtendTemp.EquipmentID.Trim(), 2, TcpExtendTemp.uuid);
                ToolAPI.XMLOperation.WriteLogXmlNoTail("雾泡喷淋定时工作命令下发 回复", TcpExtendTemp.EquipmentID);
            }
        }
    }
}
