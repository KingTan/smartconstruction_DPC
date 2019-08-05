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

namespace ProtocolAnalysis.TowerCrane._021303
{
    public class CommandIssued_021303
    {
        public static bool isIPFrist = true;
        public static bool isComFrist = true;

        //用于重发
        public static Action<List<object>> RepeatSendIP_Action = RepeatSendIP_Fun;
        public static Action<List<object>> RepeatSendCommandIssued_Action = RepeatSendCommandIssued_AFun;

     

        #region 供架构命令下发调用的公用方法
        /// <summary>
        /// 向终端设备发送设置IP的数据
        /// </summary>
        public static void SetIPConfig(IList<TcpSocketClient> SocketList)
        {
            try
            {
                //craneNo,ip,port
                DataTable dt = DB_MysqlTowerCrane.GetIPCongfig021303(isIPFrist);
                if (dt.Rows.Count > 0)
                {
                    var tcpid = from tcpsocketclient in SocketList
                                select new { EquipmentID = (tcpsocketclient.External.External as TcpClientBindingExternalClass).EquipmentID, TVersion = (tcpsocketclient.External.External as TcpClientBindingExternalClass).TVersion, client = tcpsocketclient };
                    var result = from t in tcpid
                                 join d in dt.AsEnumerable() on t.EquipmentID equals d.Field<string>("craneNo")
                                 select new { EquipmentID = t.EquipmentID, TVersion = t.TVersion, TcpClient = t.client, Ip = d.Field<string>("i_ip"), Port = d.Field<string>("i_port") };
                    foreach (var resultTemp in result)
                    {
                        List<object> temp = new List<object> { resultTemp.TVersion, resultTemp.EquipmentID, resultTemp.Ip, resultTemp.Port, resultTemp.TcpClient };
                        DB_MysqlTowerCrane.SetIPStatus021303(resultTemp.EquipmentID, "1");
                        //执行发送
                        GprsResolveDataV021303.IPConfigureSendingEp(temp);
                        RepeatSendIP_Action.BeginInvoke(temp,null,null);
                    }
                }
            }
            catch (Exception) { }
        }
        /// <summary>
        /// 命令下发
        /// </summary>
        public static void CommandIssued(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlTowerCrane.GetCommandIssued021303(isComFrist);
                var tcpid = from tcpsocketclient in SocketList
                            select new { EquipmentID = (tcpsocketclient.External.External as TcpClientBindingExternalClass).EquipmentID, TVersion = (tcpsocketclient.External.External as TcpClientBindingExternalClass).TVersion, client = tcpsocketclient };
                var result = from t in tcpid
                             join d in dt.AsEnumerable() on t.EquipmentID equals d.Field<string>("craneNo")
                             select new { EquipmentID = t.EquipmentID, TVersion = t.TVersion, TcpClient = t.client, ct_cmdValue = d.Field<string>("ct_cmdValue"), ct_paramConfig = d.Field<string>("ct_paramConfig"), ct_state = d.Field<string>("ct_state") };
                foreach (var resultTemp in result)
                {
                    List<object> temp = new List<object> { resultTemp.TVersion, resultTemp.EquipmentID, resultTemp.ct_cmdValue, resultTemp.ct_paramConfig, resultTemp.ct_state, resultTemp.TcpClient };
                    if (resultTemp.ct_state == "0")
                        DB_MysqlTowerCrane.SetCommandIssuedStatus021303(resultTemp.EquipmentID, "1");
                    else if (resultTemp.ct_state == "3")
                        DB_MysqlTowerCrane.SetCommandIssuedStatus021303(resultTemp.EquipmentID, "4");
                    //执行发送
                    GprsResolveDataV021303.CommandIssuedSendingEp(temp);
                    RepeatSendCommandIssued_Action.BeginInvoke(temp, null, null);
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region 重发
        //IP重发
        public static void RepeatSendIP_Fun(List<object> obj)
        {
            //List<object> temp = new List<object> { resultTemp.TVersion, resultTemp.EquipmentID, resultTemp.Ip, resultTemp.Port, resultTemp.TcpClient };
            int flag = 0;
            while(true)
            {
                Thread.Sleep(10000);//10秒重发一次
                if (DB_MysqlTowerCrane.GetIPRepeatSend021303(obj[1].ToString()) > 0) GprsResolveDataV021303.IPConfigureSendingEp(obj);
                else break;
                flag++;
                if (flag >= 5) break;
            }
        }
        //控制命令重发
        public static void RepeatSendCommandIssued_AFun(List<object> obj)
        {
            // List<object> temp = new List<object> { resultTemp.TVersion, resultTemp.EquipmentID, resultTemp.ct_cmdValue, resultTemp.ct_paramConfig, resultTemp.ct_state, resultTemp.TcpClient };
            int flag = 0;
            while (true)
            {
                Thread.Sleep(10000);//10秒重发一次
                if (DB_MysqlTowerCrane.GetCommandIssuedRepeatSend021303(obj[1].ToString()) > 0) GprsResolveDataV021303.CommandIssuedSendingEp(obj);
                else break;
                flag++;
                if (flag >= 5) break;
            }
        }
        #endregion

    }
}
