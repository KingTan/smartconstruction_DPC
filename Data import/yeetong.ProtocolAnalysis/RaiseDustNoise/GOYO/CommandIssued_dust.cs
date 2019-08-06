using Architecture;
using ProtocolAnalysis.RaiseDustNoise;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis
{
    public class CommandIssued_dust
    {
        //更改ip
        public static void Dust_SetIPConfig(IList<TcpSocketClient> SocketList)
        {
            string craneNo = "";
            try
            {
                DataTable dt = DB_MysqlRaiseDustNoise.GetIpCongfig();
                if (dt != null)
                {
                    int iRows = dt.Rows.Count;
                    if (iRows > 0)
                    {
                        for (int i = 0; i < iRows; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                craneNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                if (craneNo == dt.Rows[i]["equipmentNo"].ToString())
                                {
                                    string ipaddress = dt.Rows[i]["ip_dn"].ToString();
                                    string port = dt.Rows[i]["port"].ToString();
                                    string Protocol = (SocketList[j].External.External as TcpClientBindingExternalClass).TVersion;

                                    byte[] message = null;
                                    if ((SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag == 1)
                                    {
                                        message = MsgIP(Protocol, craneNo, ipaddress, port, 1);
                                    }
                                    else if ((SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag == 2)
                                    {
                                        message = MsgIP(Protocol, craneNo, ipaddress, port, 2);
                                    }
                                    else if ((SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag == 4)
                                    {
                                        message = MsgIP(Protocol, craneNo, ipaddress, port, 4);
                                    }
                                    if (message != null)
                                    {
                                        SocketList[j].SendBuffer(message);
                                        if (DB_MysqlRaiseDustNoise.UpdateDataCongfig(craneNo, "1", false) > 0)
                                        {
                                            DB_MysqlRaiseDustNoise.RecordIPCommandIssued(craneNo, 1);
                                            ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_SetIPConfig 扬尘->更改ip命令下发成功,设备编号:" + craneNo, "下发消息内容：" + Encoding.ASCII.GetString(message));
                                        }
                                        else
                                        {
                                            ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_SetIPConfig 扬尘->更改ip命令下发失败,设备编号:" + craneNo, "下发消息内容：" + Encoding.ASCII.GetString(message));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                DataTable d = DB_MysqlRaiseDustNoise.GetParam();
                if (d != null)
                {
                    int iRows = d.Rows.Count;
                    if (iRows > 0)
                    {
                        for (int i = 0; i < iRows; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                if ((SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag > 0 && (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag != 3)
                                {
                                    craneNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                    if (craneNo == d.Rows[i]["equipmentNo"].ToString())
                                    {
                                        string p25 = d.Rows[i]["pm2_5_Alarm"].ToString();
                                        string pm10 = d.Rows[i]["pm10_Alarm"].ToString();
                                        string pn = d.Rows[i]["noise_pattern"].ToString();
                                        string clc = d.Rows[i]["noise_cycle"].ToString();
                                        string oc = d.Rows[i]["noise_oc"].ToString();

                                        string tsp_factor = "";
                                        try
                                        {
                                            tsp_factor = d.Rows[i]["tsp_factor"].ToString();
                                            tsp_factor = (Convert.ToDouble(tsp_factor) * 10).ToString();
                                        }
                                        catch { tsp_factor = ""; }
                                        string pm2_5_factor = "";
                                        try
                                        {
                                            pm2_5_factor = d.Rows[i]["pm2_5_factor"].ToString();
                                            pm2_5_factor = (Convert.ToDouble(pm2_5_factor) * 10).ToString();
                                        }
                                        catch { pm2_5_factor = ""; }
                                        string pm_10_factor = "";
                                        try
                                        {
                                            pm_10_factor = d.Rows[i]["pm_10_factor"].ToString();
                                            pm_10_factor = (Convert.ToDouble(pm_10_factor) * 10).ToString();
                                        }
                                        catch
                                        {
                                            pm_10_factor = "";
                                        }
                                        string Protocol = (SocketList[j].External.External as TcpClientBindingExternalClass).TVersion;
                                        byte[] message = null;

                                        // 1是老设备 2 是新的设备 3是1.0.4版本协议
                                        if ((SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag == 1)
                                        {
                                            message = MsgParam(Protocol, craneNo, p25, pm10, pn, clc, oc, "", "", "", 1);
                                        }
                                        else if ((SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag == 2)
                                        {
                                            message = MsgParam(Protocol, craneNo, p25, pm10, pn, clc, oc, pm2_5_factor, pm_10_factor, tsp_factor, 2);
                                        }
                                        else if ((SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag == 4)
                                        {
                                            message = MsgParam(Protocol, craneNo, p25, pm10, pn, clc, oc, pm2_5_factor, pm_10_factor, tsp_factor, 4);
                                        }
                                        ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_SetIPConfig 扬尘->更改参数配置命令下发,设备编号:" + craneNo, "下发消息内容：" + ConvertData.ToHexString(message, 0, message.Count()) + "  设备的类型" + (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag);
                                        if (message != null)
                                        {
                                            SocketList[j].SendBuffer(message);
                                            string data = ConvertData.ToHexString(message, 0, message.Count());
                                            ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_SetIPConfig:", "下发消息内容：" + data);
                                            if (DB_MysqlRaiseDustNoise.UpdateDataParam(craneNo, "1") > 0)
                                            {
                                                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_SetIPConfig 扬尘->更改参数配置命令下发成功,设备编号:" + craneNo, "下发消息内容：" + Encoding.ASCII.GetString(message));
                                            }
                                            else
                                            {
                                                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_SetIPConfig 扬尘->更改参数配置命令下发失败,设备编号:" + craneNo, "下发消息内容：" + Encoding.ASCII.GetString(message));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                DB_MysqlRaiseDustNoise.UpdateDataParam(craneNo, "3");
            }
        }

        static byte[] MsgIP(string protocols, string CraneNo, string ip, string port, int EquipmentTag)
        {
            try
            {
                List<byte> resultB = new List<byte>();
                resultB.Add(0x7A); resultB.Add(0x7A);
                if (protocols == null || protocols == "") return null;
                //byte[] framesByte = ConvertData.HexToByte(protocols);
                //resultB.AddRange(framesByte);
                resultB.Add(0x01);
                resultB.Add(0x00);
                if (EquipmentTag == 1)
                {
                    resultB.Add(0x00);
                }
                else if (EquipmentTag == 2)
                {
                    resultB.Add(0x03);
                }
                else if (EquipmentTag == 4)
                {
                    resultB.Add(0x04);
                }
                resultB.Add(0x05);
                int count = ip.Length + port.Length + 2;
                resultB.Add((byte)count); resultB.Add(0x00);//数据长度
                //ip长度
                resultB.Add((byte)ip.Length);
                //ip
                byte[] ipary = Encoding.ASCII.GetBytes(ip);
                resultB.AddRange(ipary);
                //端口
                resultB.Add((byte)port.Length);
                //端口号
                byte[] portA = Encoding.ASCII.GetBytes(port);
                resultB.AddRange(portA);
                //CRC
                byte[] byteTemp = new byte[resultB.Count];
                resultB.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, resultB.Count - 8));
                resultB.Add(crc16[0]);
                resultB.Add(crc16[1]);
                //结束符
                resultB.Add(0x7B);
                resultB.Add(0x7B);
                byteTemp = new byte[resultB.Count];
                resultB.CopyTo(byteTemp);
                return byteTemp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 参数
        /// </summary>
        /// <param name="protocols"></param>
        /// <param name="CraneNo">设备号</param>
        /// <param name="pm25">pm25</param>
        /// <param name="pm10">pm10</param>
        /// <param name="ms"></param>
        /// <param name="clcly"></param>
        /// <param name="ocTime"></param>
        /// <returns></returns>
        static byte[] MsgParam(string protocols, string CraneNo, string pm25, string pm10, string ms, string clcly, string ocTime, string pm2_5_factor, string pm_10_factor, string tsp_factor, int isNewEquipment)
        {
            try
            {
                List<byte> resultB = new List<byte>();
                resultB.Add(0x7A); resultB.Add(0x7A);
                if (protocols == null || protocols == "") return null;
                resultB.Add(0x01);
                resultB.Add(0x00);
                if (isNewEquipment == 1)
                {
                    resultB.Add(0x00);
                    resultB.Add(0x03);
                    resultB.Add(0x17);
                }
                else if (isNewEquipment == 2)
                {
                    resultB.Add(0x03);
                    resultB.Add(0x03);
                    resultB.Add(0x1a);
                }
                else if (isNewEquipment == 4)
                {
                    resultB.Add(0x04);
                    resultB.Add(0x03);
                    resultB.Add(0x1b);
                }
                resultB.Add(0x00);//数据长度
                byte[] ipary = Encoding.ASCII.GetBytes(CraneNo);
                resultB.AddRange(ipary);
                //byte[] rtc = Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyMMddHHmmss"));
                //resultB.AddRange(rtc);
                DateTime now = System.DateTime.Now;
                resultB.Add(byte.Parse(now.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber));
                resultB.Add(byte.Parse(now.Month.ToString(), System.Globalization.NumberStyles.HexNumber));
                resultB.Add(byte.Parse(now.Day.ToString(), System.Globalization.NumberStyles.HexNumber));
                resultB.Add(byte.Parse(now.Hour.ToString(), System.Globalization.NumberStyles.HexNumber));
                resultB.Add(byte.Parse(now.Minute.ToString(), System.Globalization.NumberStyles.HexNumber));
                resultB.Add(byte.Parse(now.Second.ToString(), System.Globalization.NumberStyles.HexNumber));
                byte[] p25 = ToolAPI.ValueTypeToByteArray.GetBytes_LittleEndian(UInt16.Parse(pm25));
                resultB.AddRange(p25);
                byte[] p10 = ToolAPI.ValueTypeToByteArray.GetBytes_LittleEndian(UInt16.Parse(pm10));
                resultB.AddRange(p10);

                if (ms.Equals("0"))
                {
                    resultB.Add(0x01);
                    resultB.Add(0x00);
                }
                else
                {
                    resultB.Add(0x00);
                    resultB.Add(0x01);
                }
                resultB.Add(byte.Parse(clcly));
                if (isNewEquipment == 4)
                {
                    resultB.Add(0x00);
                }
                byte[] oct = ToolAPI.ValueTypeToByteArray.GetBytes_LittleEndian(UInt16.Parse(ocTime));
                resultB.AddRange(oct);

                if (isNewEquipment == 2 || isNewEquipment == 4)//新设备添加的参数
                {
                    resultB.Add(byte.Parse(pm2_5_factor));
                    resultB.Add(byte.Parse(pm_10_factor));
                    resultB.Add(byte.Parse(tsp_factor));
                }

                //crc
                byte[] byteTemp = new byte[resultB.Count];
                resultB.CopyTo(byteTemp); //BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 1));
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, resultB.Count - 8));
                resultB.Add(crc16[0]);
                resultB.Add(crc16[1]);
                //结束符
                resultB.Add(0x7B);
                resultB.Add(0x7B);
                byteTemp = new byte[resultB.Count];
                resultB.CopyTo(byteTemp);
                string tStr = ConvertData.ToHexString(byteTemp, 0, byteTemp.Length);
                return byteTemp;
                
            }
            catch (Exception EX)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("MsgParam-->异常", "异常信息：" + EX.Message);
                return null;
            }

        }
    }
}
