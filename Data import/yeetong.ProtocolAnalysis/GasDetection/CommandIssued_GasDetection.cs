using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Architecture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TCPAPI;

namespace ProtocolAnalysis.GasDetection
{
    public class CommandIssued_GasDetection
    {
        public static Action<IList<TcpSocketClient>> SendGetRealTimeDataEvent = GetGetRealTimeData;

        public static void GetGetRealTimeData(IList<TcpSocketClient> SocketList)
        {
            try
            {

               foreach(var client in SocketList)
               {
                   TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                   if (TcpExtendTemp != null && !string.IsNullOrEmpty(TcpExtendTemp.EquipmentID) )
                   {
                       byte[] sendAry = GprsResolveGasDetection.SplitJointCommand(TcpExtendTemp);
                       if(sendAry!=null)
                       {
                           client.SendBuffer(sendAry);
                       }
                   }
               }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XL", "惜蓝命令下发异常：", ex.Message);
            }
        }
    }
}
