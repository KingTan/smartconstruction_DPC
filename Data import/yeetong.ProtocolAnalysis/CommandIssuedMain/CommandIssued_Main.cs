using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Architecture;
using TCPAPI;

namespace ProtocolAnalysis
{
    public class CommandIssued_Main
    {
        public static void CommandIssued_MainInit()
        {
            switch (MainStatic.DeviceType)
            {
                //塔吊
                case 0:
           
                    break;
                //升降机
                case 1:
                    break;
                //卸料
                //case 2: CommandIssuedInitEvent += CommandIssued_DisCharge.Crane_SetIPConfig; break;
                //扬尘
                case 4:
                   
                    break;
                case 13://临边防护
                    break;
                default: break;
            }
        }
        public static Action<IList<TcpSocketClient>> CommandIssuedInitEvent;

    }
}
