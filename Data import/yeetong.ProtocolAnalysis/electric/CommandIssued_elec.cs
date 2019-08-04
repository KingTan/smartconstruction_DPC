using Architecture;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis
{
   public class CommandIssued_elec
    {
        #region 供架构命令下发调用的公用方法
        /// <summary>
        /// 向终端设备获取正向电能
        /// </summary>
        public static void GetSmartMeter_SN(IList<TcpSocketClient> SocketList)
        {
          //  IGprsCraneDataComm gd = new GprsDataComm().CreateInstance();
            try
            {
                TcpSocketClient[] SocketListCopy = new TcpSocketClient[SocketList.Count];
                SocketList.CopyTo(SocketListCopy, 0);
                foreach (TcpSocketClient resultTemp in SocketListCopy)
                {
                    ///FEFEFE  引导码（固定）
                    ///68  针头（固定）
                    ///44 45 44 00 00 00 逆序表号（自己拼（BCD码））==== 用变
                    ///68 针头（固定）
                    ///01 控制码（01 读取，04 写入）
                    ///02 数据长度
                    ///43c3 读取电表，5bf3 读闸状态
                    TcpClientBindingExternalClass TcpExtendTemp = resultTemp.External.External as TcpClientBindingExternalClass; ;  //网关号
                    string gateway_sn = TcpExtendTemp.EquipmentID;
                    if (!string.IsNullOrEmpty(gateway_sn))
                    {
                        DataTable dt = DB_MysqlElectric.GetsndataToSn(gateway_sn);//gd.GetsndataToSn(gateway_sn);
                        //byte[] dataTemp1 = new byte[] { 0xFE, 0xFE, 0xFE, 0x68, 0x49, 0x69, 0x52, 0x00, 0x00, 0x00, 0x68, 0x01, 0x02, 0x43, 0xC3, 0xA6, 0x16 };//读电能
                        //resultTemp.SendBuffer(dataTemp1);
                        foreach (DataRow dr in dt.Rows)  //读电能
                        {
                            string value = dr["equipmentNo"].ToString();
                            byte[] bt = StrToHexByte(value);
                            byte[] dataTemp = new byte[] { 0xFE, 0xFE, 0xFE, 0x68, bt[5], bt[4], bt[3], bt[2], bt[1], bt[0], 0x68, 0x01, 0x02, 0x43, 0xC3, 0xA6, 0x16 };//读电能

                            //做检验
                            byte[] arytemp = new byte[12];
                            Array.Copy(dataTemp,3, arytemp, 0, 12);
                            dataTemp[15] = Check_Sum(arytemp);

                            resultTemp.SendBuffer(dataTemp);
                            ToolAPI.XMLOperation.WriteLogXmlNoTail("电能", ConvertData.ToHexString(dataTemp, 0, dataTemp.Length));
                            Thread.Sleep(1000);
                        }
                        foreach (DataRow dr in dt.Rows)  //读闸状态
                        {
                            string value = dr["equipmentNo"].ToString();
                            byte[] bt = StrToHexByte(value);
                            byte[] dataTemp = new byte[] { 0xFE, 0xFE, 0xFE, 0x68, bt[5], bt[4], bt[3], bt[2], bt[1], bt[0], 0x68, 0x01, 0x02, 0x5B, 0xF3, 0xEE, 0x16 };//读闸状态

                            //做检验
                            byte[] arytemp = new byte[12];
                            Array.Copy(dataTemp, 3, arytemp, 0, 12);
                            dataTemp[15] = Check_Sum(arytemp);

                            resultTemp.SendBuffer(dataTemp);
                            ToolAPI.XMLOperation.WriteLogXmlNoTail("闸状态", ConvertData.ToHexString(dataTemp, 0, dataTemp.Length));
                            Thread.Sleep(1000);
                        }
                        DataTable oc = DB_MysqlElectric.GetOpenOrClose(gateway_sn); //该网关下的要关闭或者要开启的，电表控制表
                        foreach (DataRow dr in oc.Rows)
                        {
                            byte[] dataTemp;
                            string sn = dr["sn"].ToString();
                            byte[] bt = StrToHexByte(sn);
                            if (dr["openstate"].ToString().Equals("0"))
                                dataTemp = new byte[] { 0xFE, 0xFE, 0xFE, 0x68, bt[5], bt[4], bt[3], bt[2], bt[1], bt[0], 0x68, 0x04, 0x08, 0x5B, 0xF3, 0x33, 0x33, 0x33, 0x33, 0x99, 0xCC, 0x28, 0x16 };//合闸
                            else
                                dataTemp = new byte[] { 0xFE, 0xFE, 0xFE, 0x68, bt[5], bt[4], bt[3], bt[2], bt[1], bt[0], 0x68, 0x04, 0x08, 0x5B, 0xF3, 0x33, 0x33, 0x33, 0x33, 0x88, 0x66, 0xB1, 0x16 };//拉闸

                            //做检验
                            byte[] arytemp = new byte[18];
                            Array.Copy(dataTemp, 3, arytemp, 0, 18);
                            dataTemp[21] = Check_Sum(arytemp);

                            resultTemp.SendBuffer(dataTemp);
                            ToolAPI.XMLOperation.WriteLogXmlNoTail("闸控制", ConvertData.ToHexString(dataTemp, 0, dataTemp.Length));
                            Thread.Sleep(1000);
                        }
                        //byte[] dataTemp = new byte[] { 0xFE, 0xFE, 0xFE, 0x68, 0x44, 0x45, 0x44, 0x00, 0x00, 0x00, 0x68, 0x01, 0x02, 0x43, 0xC3, 0xA6, 0x16 };//读电能
                        //byte[] dataTemp = new byte[] { 0xFE, 0xFE, 0xFE, 0x68, 0x44, 0x45, 0x44, 0x00, 0x00, 0x00, 0x68, 0x01, 0x02, 0x5B, 0xF3, 0xEE, 0x16 };//读闸状态
                        //byte[] dataTemp = new byte[] { 0xFE, 0xFE, 0xFE, 0x68, 0x44, 0x45, 0x44, 0x00, 0x00, 0x00, 0x68, 0x04, 0x08, 0x5B, 0xF3, 0x33, 0x33, 0x33, 0x33, 0x88, 0x66, 0xB1, 0x16 };//拉闸
                        // byte[] dataTemp = new byte[] { 0xFE, 0xFE, 0xFE, 0x68, 0x44, 0x45, 0x44, 0x00, 0x00, 0x00, 0x68, 0x04, 0x08, 0x5B, 0xF3, 0x33, 0x33, 0x33, 0x33, 0x99, 0xCC, 0x28, 0x16 };//合闸
                    }
                }
            }
            catch (Exception ex) { }
        }
        #endregion
        /// <summary>
        /// 字符串转byte
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public static  Byte Check_Sum(byte[] memorySpage)
        {
            int num = 0;
            for (int i = 0; i < memorySpage.Length; i++)
            {
                num = (num + memorySpage[i]) % 0xffff;
            }
            //实际上num 这里已经是结果了，如果只是取int 可以直接返回了
            memorySpage = BitConverter.GetBytes(num);
            //返回累加校验和
            return memorySpage[0] ;
        }
    }
}
