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

namespace ProtocolAnalysis.DisCharge
{
    public class CommandIssued_DisCharge
    {
        //更改ip
        public static void Crane_SetIPConfig(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlDisCharge.GetIPConfiguration();
                if (dt != null)
                {
                    int iRows = dt.Rows.Count;
                    if (iRows > 0)
                    {
                        for (int i = 0; i < iRows; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                string DeviceNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                string DeviceNoServer = dt.Rows[i]["equipmentNo"].ToString();
                                if (DeviceNo != null && DeviceNo.Equals(DeviceNoServer))
                                {
                                    Frame_IPConfiguration IPConfiguration = new Frame_IPConfiguration();
                                    IPConfiguration.DeviceNo = DeviceNoServer;
                                    IPConfiguration.IP = dt.Rows[i]["ip_dn"].ToString();
                                    IPConfiguration.Port = dt.Rows[i]["port"].ToString();
                                    byte[] message = GprsResolveDataV102.SendJoint_IPConfiguration(IPConfiguration);
                                    if (message != null)
                                    {
                                        SocketList[j].SendBuffer(message);
                                        DB_MysqlDisCharge.UpdateIPConfiguration(IPConfiguration, 1,false);
                                        DB_MysqlDisCharge.RecordCommandIssued(DeviceNo, 1);
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
