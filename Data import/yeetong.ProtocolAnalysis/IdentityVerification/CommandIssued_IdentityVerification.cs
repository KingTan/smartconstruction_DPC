using Architecture;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis.IdentityVerification
{
    public class CommandIssued_IdentityVerification
    {
        static public Dictionary<string, DataRow> IrisissuedDic = new Dictionary<string, DataRow>();
        //更改ip
        public static void IdentityVerification_SetIPConfig(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlIdentityVerification.GetOrder();
                if (dt != null)
                {
                    int iRows = dt.Rows.Count;
                    if (iRows > 0)
                    {
                        for (int i = 0; i < iRows; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                string equipmentNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                string equipmentNoServer = dt.Rows[i]["equipment"].ToString();
                                if (equipmentNo != null && equipmentNo.Equals(equipmentNoServer))
                                {
                                    byte[] message = GprsResolve_IdentityVerification.Byte_IP(dt.Rows[i]);//得到拼接包
                                    if (message != null)
                                    {
                                        SocketList[j].SendBuffer(message);
                                        DB_MysqlIdentityVerification.UpdateOrder(equipmentNo, "1");//更新数据库的状态
                                        ToolAPI.XMLOperation.WriteLogXmlNoTail("IdentityVerification_SetIPConfig:info", string.Format("【{0}】更改设备{1}的ip,{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), equipmentNo, ConvertData.ToHexString(message, 0, message.Length)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        //下发特征库
        public static void IdentityVerification_SetIris(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlIdentityVerification.GetIris();
                if (dt != null)
                {
                    int iRows = dt.Rows.Count;
                    if (iRows > 0)
                    {
                        for (int i = 0; i < iRows; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                string equipmentNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                string equipmentNoServer = dt.Rows[i]["equipment"].ToString();
                                string identity_card = dt.Rows[i]["identity_card"].ToString();
                                if (equipmentNo != null && equipmentNo.Equals(equipmentNoServer))
                                {
                                    if (!IrisissuedDic.ContainsKey(equipmentNo))
                                        IrisissuedDic.Add(equipmentNo, dt.Rows[i]);
                                    else
                                    {
                                        IrisissuedDic[equipmentNo] = dt.Rows[i];
                                    }
                                    byte[] message = GprsResolve_IdentityVerification.Byte_Iris("1", dt.Rows[i]);//得到拼接包
                                    if (message != null)
                                    {
                                        SocketList[j].SendBuffer(message);
                                        DB_MysqlIdentityVerification.UpdateIris(equipmentNo, identity_card, "1");//更新数据库的状态
                                        ToolAPI.XMLOperation.WriteLogXmlNoTail("IdentityVerification_SetIPConfig:info", string.Format("【{0}】更改设备{1}的ip,{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), equipmentNo, ConvertData.ToHexString(message, 0, message.Length)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        //删除某一个虹膜
        public static void IdentityVerification_SetIrisdelete(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlIdentityVerification.GetIrisdelete();
                if (dt != null)
                {
                    int iRows = dt.Rows.Count;
                    if (iRows > 0)
                    {
                        for (int i = 0; i < iRows; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                string equipmentNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                string equipmentNoServer = dt.Rows[i]["equipment"].ToString();
                                string identity_card = dt.Rows[i]["identity_card"].ToString();
                                if (equipmentNo != null && equipmentNo.Equals(equipmentNoServer))
                                {
                                    byte[] message = GprsResolve_IdentityVerification.Byte_Irisdelete(dt.Rows[i]);//得到拼接包
                                    if (message != null)
                                    {
                                        SocketList[j].SendBuffer(message);
                                        DB_MysqlIdentityVerification.UpdateIrisdelete(equipmentNo, identity_card, "1");//更新数据库的状态
                                        ToolAPI.XMLOperation.WriteLogXmlNoTail("IdentityVerification_SetIPConfig:info", string.Format("【{0}】更改设备{1}的ip,{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), equipmentNo, ConvertData.ToHexString(message, 0, message.Length)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        /// <summary>
        /// 检查设备60秒之内是否接到了实时数据
        /// </summary>
        /// <param name="SocketList"></param>
        public static void IdentifyVerification_Current(IList<TcpSocketClient> SocketList)
        {
            try
            {
                for (int j = 0; j < SocketList.Count; j++)
                {
                    string equipmentNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                    string rtc = DB_MysqlIdentityVerification.GetCurrentToSn(equipmentNo); //获取实时数据的时间
                    if (!string.IsNullOrEmpty(rtc)) //如果时间存在，说明设备已经60秒没有向平台发数据了，立即应答设备
                    {
                        byte[] message = GprsResolve_IdentityVerification.Byte_Current(equipmentNo, rtc);
                        if (message != null)
                        {
                            SocketList[j].SendBuffer(message);
                            DB_MysqlIdentityVerification.UpdateIsSendAnswer(equipmentNo);
                        }
                    }
                }
            }
            catch (Exception) { 
            }
        }
    }
}
