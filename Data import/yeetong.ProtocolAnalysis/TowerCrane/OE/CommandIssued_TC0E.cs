using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Architecture;
using TCPAPI;
using ToolAPI;
namespace ProtocolAnalysis.TowerCrane.OE
{
    public class CommandIssued_TC0E
    {
        //更改ip
        public static  void Crane_SetIPConfig(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlTowerCrane.GetDataCongfig();
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
                                string craneNoServer = dt.Rows[i]["equipmentNo"].ToString();
                                if (craneNo != null && craneNo.Equals(craneNoServer))
                                {
                                    byte[] message = GprsResolveDataV0E.Byte_IP(dt.Rows[i]);
                                    if (message != null)
                                    {
                                        DB_MysqlTowerCrane.UpdateDataCongfig(craneNoServer,1,false);
                                        DB_MysqlTowerCrane.UpdateIPCommandIssued(craneNoServer,1);
                                        SocketList[j].SendBuffer(message);
                                        ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsCrane.Crane_SetIPConfig:info", string.Format("【{0}】更改设备{1}的ip,{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), craneNo, ConvertData.ToHexString(message, 0, message.Length)));
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
                DataTable dt = DB_MysqlTowerCrane.GetControlCongfig();
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
                                string craneNoServer = dt.Rows[i]["equipmentNo"].ToString();
                                if (craneNo != null && craneNo.Equals(craneNoServer))
                                {
                                    byte[] message = GprsResolveDataV0E.Byte_Control(dt.Rows[i]);
                                    if (message != null)
                                    {
                                        SocketList[j].SendBuffer(message);
                                        ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsCrane.Crane_SetControl:info", string.Format("【{0}】控制设备{1}的ip,{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), craneNo, ConvertData.ToHexString(message, 0, message.Length)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
