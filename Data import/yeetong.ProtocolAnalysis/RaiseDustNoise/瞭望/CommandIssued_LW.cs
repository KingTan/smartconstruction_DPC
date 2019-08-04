using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Architecture;
using TCPAPI;
namespace ProtocolAnalysis
{
    public class CommandIssued_LW
    {
        public static void GetGetRealTimeData(IList<TcpSocketClient> SocketList)
        {
            byte[] SendData = { 0x01, 0x03, 0x05, 0x01, 0x00, 0x10, 0x15, 0x0a };
            int count = 0;
            for (int i = 0; i < SocketList.Count; i++)
            {
                try
                {
                    TcpClientBindingExternalClass TcpExtendTemp = SocketList[i].External.External as TcpClientBindingExternalClass;
                    //if (TcpExtendTemp.TVersion == "" || TcpExtendTemp.TVersion == "lw")
                    //{
                    //    SocketList[i].SendBuffer(SendData);
                    //    count++;
                    //}
                }
                catch (Exception) { }
            }
            if (count > 0)
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\LW", "瞭望命令下发，下发设备数：", SocketList.Count.ToString());
        }
    }
}
