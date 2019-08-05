using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Architecture;
using ProtocolAnalysis.DisCharge;
using ProtocolAnalysis.Lift;
using ProtocolAnalysis.TowerCrane._021303;
using ProtocolAnalysis.TowerCrane.OE;
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
                    CommandIssuedInitEvent += CommandIssued_TC0E.Crane_SetIPConfig;
                    CommandIssuedInitEvent += CommandIssued_TC0E.Crane_SetControl;
                    CommandIssuedInitEvent += CommandIssued_021303.SetIPConfig;
                    CommandIssuedInitEvent += CommandIssued_021303.CommandIssued;
                    break;
                //升降机
                case 1:
                    CommandIssuedInitEvent += CommandIssued_Lift.Crane_SetIPConfig;
                    CommandIssuedInitEvent += CommandIssued_Lift.Crane_SetControl;
                    CommandIssuedInitEvent += CommandIssued_Lift.Lift_featureIssued;
                    break;
                //卸料
                case 2: CommandIssuedInitEvent += CommandIssued_DisCharge.Crane_SetIPConfig; break;
                //扬尘
                case 4:
                    CommandIssuedInitEvent += CommandIssued_LW.GetGetRealTimeData;
                    CommandIssuedInitEvent += CommandIssued_dust.Dust_SetIPConfig;
                    break;
                case 13://临边防护
                    break;
                default: break;
            }
        }
        public static Action<IList<TcpSocketClient>> CommandIssuedInitEvent;

    }
}
