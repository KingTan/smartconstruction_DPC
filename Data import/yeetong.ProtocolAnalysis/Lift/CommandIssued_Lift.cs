using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Architecture;
using SIXH.DBUtility;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis.Lift
{
    public class CommandIssued_Lift
    {
        // static DateTime now = DateTime.Now;
        static Dictionary<string, DateTime> dic = new Dictionary<string, DateTime>();
        //更改ip
        public static void Crane_SetIPConfig(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlLift.GetIpCongfig();
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
                                    //craneNo,ip,port
                                    string ip = dt.Rows[i]["ip_dn"].ToString();
                                    string port = dt.Rows[i]["port"].ToString();
                                    string Protocol = (SocketList[j].External.External as TcpClientBindingExternalClass).TVersion;
                                    byte[] message = MsgIP(Protocol, craneNo, ip, port);
                                    if (message != null)
                                    {
                                        SocketList[j].SendBuffer(message);
                                          //下发后更改状态
                                        DB_MysqlLift.UpdateDataConfig(craneNo, 1, false);
                                        DB_MysqlLift.UpdateIPCommandIssued(craneNo, 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        //更改限位控制信息
        public static void Crane_SetControl(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlLift.GetcontrolCongfig();
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
                                    string flag = dt.Rows[i]["kztype"].ToString();
                                    string Protocol = (SocketList[j].External.External as TcpClientBindingExternalClass).TVersion;
                                    byte[] message = MsgControl(Protocol, craneNo, flag);
                                    if (message != null)
                                        SocketList[j].SendBuffer(message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        /// <summary>
        /// 下发人脸特征码
        /// </summary>
        /// <param name="SocketList"></param>
        public static void Lift_featureIssued(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlLift.GetIssuedfeature();
                if (dt != null)
                {
                    int iRow = dt.Rows.Count;
                    if (iRow > 0)
                    {
                        for (int i = 0; i < iRow; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                string craneNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                if (craneNo == dt.Rows[i]["equipmentNo"].ToString())
                                {
                                    if (DateTime.Now < GetDateTime(craneNo)) //时间对比
                                        return;
                                    string userid = dt.Rows[i]["userid"].ToString();
                                    IList<LiftFeatureIssued> list = DB_MysqlLift.GetIListIssued(craneNo, userid);
                                    foreach (var item in list)
                                    {
                                        int y = 0;
                                        while (y < 10) //如果收不到应答连续下发10次，如果10次设备都没有应答及可视为下发设备失败
                                        {
                                            byte[] message = IssuedFace(item.SumFeaturePack);
                                            if (message != null)
                                                SocketList[j].SendBuffer(message);
                                            Thread.Sleep(2000);//歇700毫秒，等待设备应答
                                            int status = DB_MysqlLift.GetIssueStatus(craneNo, userid, item.CurrentPack);
                                            if (status == 1) //如果等于1，代表该包已经下发到了设备，可以继续下发下一个
                                                break;
                                            if (status == 0) //下发失败，退出所有循环，重新开始
                                            {
                                                DB_MysqlLift.UpdateFaceSendStatus(craneNo, userid, 0);
                                                BindDic(craneNo);//now = DateTime.Now.AddMinutes(2);
                                                return;
                                            }
                                            y++;
                                        }
                                        if (y >= 9)
                                        {
                                            int statusagin = DB_MysqlLift.GetIssueStatus(craneNo, userid, item.CurrentPack); //如果执行完了10次，最后一次看一下该包的状态
                                            if (statusagin == 3 || statusagin == 0) //如果该包执行完了10还是有问题，平台就不发了，并且更新执行下发的表。
                                            {
                                                DB_MysqlLift.UpdateFaceSendStatus(craneNo, userid, 3); //执行下发失败
                                                BindDic(craneNo);
                                                return;
                                            }
                                        }
                                    }
                                    if (DB_MysqlLift.GetAllPackIsError(craneNo, userid) == 0)
                                    {
                                        DB_MysqlLift.UpdateFaceSendStatus(craneNo, userid, 2);
                                        BindDic(craneNo); //now = DateTime.Now.AddMinutes(2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }


       

        static void BindDic(string craneNo)
        {
            if (!dic.ContainsKey(craneNo))
                dic.Add(craneNo, DateTime.Now.AddMinutes(2));
            else
            {
                dic.Remove(craneNo);
                dic.Add(craneNo, DateTime.Now.AddMinutes(2));
            }

        }

        static DateTime GetDateTime(string craneNo)
        {
            foreach (var item in dic)
                if (item.Key.Equals(craneNo))
                    return item.Value;
            return DateTime.Now.AddMinutes(-1);
        }
        static byte[] IssuedFace(string bytes)
        {
            byte[] bt = ConvertData.HexToByte(bytes);
            bt[5] = 0x0A;
            return bt;
        }
        static byte[] MsgControl(string protocols, string CraneNo, string flag)
        {
            try
            {
                List<byte> resultB = new List<byte>();
                resultB.Add(0x7A); resultB.Add(0x7A);
                if (protocols == null || protocols == "") return null;
                byte[] framesByte = ConvertData.HexToByte(protocols);
                resultB.AddRange(framesByte);
                resultB.Add(0x06);
                resultB.Add(0x09); resultB.Add(0x00);//数据长度
                if (CraneNo == null || CraneNo == "" || CraneNo.Length != 8) return null;
                byte[] craneBy = Encoding.ASCII.GetBytes(CraneNo);
                resultB.AddRange(craneBy);
                if (flag == null || flag == "" || flag.Length != 1) return null;
                resultB.Add(flag == "0" ? (byte)1 : (byte)0);
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

        static byte[] MsgIP(string protocols, string CraneNo, string ip, string port)
        {
            try
            {
                List<byte> resultB = new List<byte>();
                resultB.Add(0x7A); resultB.Add(0x7A);
                if (protocols == null || protocols == "") return null;
                byte[] framesByte = ConvertData.HexToByte(protocols);
                resultB.AddRange(framesByte);
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
                resultB.Add(0x02);
                resultB.Add(0x00);

                //周期
                framesByte = ConvertData.HexToByte(Period);
                resultB.AddRange(framesByte);

                //CRC
                byte[] byteTemp = new byte[resultB.Count];
                resultB.CopyTo(byteTemp);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 0, resultB.Count - 4));
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

    }
}
