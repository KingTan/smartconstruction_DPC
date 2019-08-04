using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Architecture;
using ProtocolAnalysis.SoftHat.Mysql;
using SIXH.DBUtility;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis.SoftHat
{
    public class CommandIssued_SoftHat
    {
        // static DateTime now = DateTime.Now;
        static Dictionary<string, DateTime> dic = new Dictionary<string, DateTime>();
        //更改ip
        public static void Crane_SetIPConfig(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlHat.GetIpCongfig();
                if (dt != null)
                {
                    int iRows = dt.Rows.Count;
                    if (iRows > 0)
                    {
                        for (int i = 0; i < iRows; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                string craneNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                if (craneNo == dt.Rows[i]["equipmentNo"].ToString())
                                {
                                    string ip = "";
                                    string port = "";
                                    string Protocol = "";
                                    int y = 0;
                                    while (y < 10) //如果收不到应答连续下发10次，如果10次设备都没有应答及可视为下发设备失败
                                    {

                                        if (y == 0)
                                        {
                                            ip = dt.Rows[i]["ip_dn"].ToString();
                                            port = dt.Rows[i]["port"].ToString();
                                            Protocol = (SocketList[j].External.External as TcpClientBindingExternalClass).TVersion;
                                        }
                                        byte[] message = MsgIP(Protocol, craneNo, ip, port);
                                        string tStr = ConvertData.ToHexString(message, 0, message.Length);
                                        if (message != null)
                                        {
                                            SocketList[j].SendBuffer(message);
                                        }
                                        Thread.Sleep(2000);//歇700毫秒，等待设备应答
                                        int status = DB_MysqlHat.GetIssueStatus(craneNo, "IP");

                                        if (status == 2) //如果等于1，代表该包已经下发到了设备
                                            break;

                                        y++;
                                    }
                                    if (y >= 9)
                                    {
                                        int statusagin = DB_MysqlHat.GetIssueStatus(craneNo, "IP");//如果执行完了10次，最后一次看一下该包的状态
                                        if (statusagin == 3 || statusagin == 0) //如果该包执行完了10还是有问题，平台就不发了，并且更新执行下发的表。
                                        {
                                            DB_MysqlHat.UpdateDataConfig(craneNo, 3, false);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }



        /// <summary>
        /// 设置上传周期
        /// </summary>
        /// <param name="SocketList"></param>
        public static void Crane_SetPeriodConfig(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlHat.GetUploadPeriodCongfig();
                if (dt != null)
                {
                    int iRows = dt.Rows.Count;
                    if (iRows > 0)
                    {
                        for (int i = 0; i < iRows; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                string craneNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                if (craneNo == dt.Rows[i]["equipmentNo"].ToString())
                                {
                                    string Period = "";
                                    string Protocol = "";
                                    int y = 0;
                                    while (y < 10) //如果收不到应答连续下发10次，如果10次设备都没有应答及可视为下发设备失败
                                    {
                                        if (y == 0)
                                        {
                                            Period = dt.Rows[i]["period"].ToString();
                                            Protocol = (SocketList[j].External.External as TcpClientBindingExternalClass).TVersion;
                                        }
                                        byte[] message = MsgPeriod(Protocol, craneNo, Period);
                                        string tStr = ConvertData.ToHexString(message, 0, message.Length);
                                        if (message != null)
                                        {
                                            SocketList[j].SendBuffer(message);
                                        }
                                        Thread.Sleep(2000);//歇700毫秒，等待设备应答
                                        int status = DB_MysqlHat.GetIssueStatus(craneNo, "period");

                                        if (status == 2) //如果等于2，代表该包已经下发成功到了设备
                                            break;

                                        y++;
                                    }
                                    if (y >= 9)
                                    {
                                        int statusagin = DB_MysqlHat.GetIssueStatus(craneNo, "period");//如果执行完了10次，最后一次看一下该包的状态
                                        if (statusagin == 3 || statusagin == 0) //如果该包执行完了10还是有问题，平台就不发了，并且更新执行下发的表。
                                        {
                                            DB_MysqlHat.UpdatePeriodDataConfig(craneNo, 3, false);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 修改ip地址
        /// </summary>
        /// <param name="protocols"></param>
        /// <param name="CraneNo"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        static byte[] MsgIP(string protocols, string CraneNo, string ip, string port)
        {
            try
            {
                List<byte> resultB = new List<byte>();
                resultB.Add(0x7A); resultB.Add(0x7A);
                if (protocols == null || protocols == "") return null;
                byte[] framesByte = ConvertData.HexToByte(protocols);
                resultB.AddRange(framesByte);
                resultB.Add(0x03);
                resultB.Add(0x00);

                //设备编号
                framesByte = Encoding.ASCII.GetBytes(CraneNo);
                resultB.AddRange(framesByte);

                int count = ip.Length + port.Length;
                resultB.Add(0x00); resultB.Add(0x14);//数据长度

                //端口号
                byte[] portA = Encoding.ASCII.GetBytes(port);
                if (port.Length < 5 && port.Length != 5)
                {
                    resultB.AddRange(portA);
                    for (int j = 0; j < (5 - port.Length); j++)
                    {
                        resultB.Add(0x00);
                    }
                }
                else if (port.Length == 5)
                {
                    resultB.AddRange(portA);
                }

                //ip
                byte[] ipary = Encoding.ASCII.GetBytes(ip);
                if (ip.Length < 15 && ip.Length != 15)
                {
                    resultB.AddRange(ipary);
                    for (int i = 0; i < (15 - ip.Length); i++)
                    {
                        resultB.Add(0x00);
                    }
                }
                else if (ip.Length == 15)
                {
                    resultB.AddRange(ipary);
                }



                //CRC
                byte[] byteTemp = new byte[resultB.Count];
                resultB.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 0, resultB.Count));
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
        /// 上传周期
        /// </summary>
        /// <param name="Period"></param>
        /// <returns></returns>
        static byte[] MsgPeriod(string protocols, string CraneNo, string Period)
        {
            try
            {
                List<byte> resultB = new List<byte>();
                resultB.Add(0x7A); resultB.Add(0x7A);
                if (protocols == null || protocols == "") return null;
                byte[] framesByte = ConvertData.HexToByte(protocols);
                resultB.AddRange(framesByte);
                resultB.Add(0x02);
                resultB.Add(0x00);

                //设备编号
                framesByte = Encoding.ASCII.GetBytes(CraneNo);
                resultB.AddRange(framesByte);

                //长度
                resultB.Add(0x00);
                resultB.Add(0x02);

                //周期
                string a = String.Format("{0:X}", int.Parse(Period));
                framesByte = ConvertData.HexToByte(a);
                resultB.Add(0x00);

                resultB.AddRange(framesByte);


                //CRC
                byte[] byteTemp = new byte[resultB.Count];
                resultB.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 0, resultB.Count));
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


        static DateTime GetDateTime(string craneNo)
        {
            foreach (var item in dic)
                if (item.Key.Equals(craneNo))
                    return item.Value;
            return DateTime.Now.AddMinutes(-1);
        }

    }
}
