using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Architecture;
using ProtocolAnalysis.DisCharge;
using ProtocolAnalysis.FogGun;
using ProtocolAnalysis.GasDetection;
using ProtocolAnalysis.Lift;
using ProtocolAnalysis.TowerCrane._021303;
using ProtocolAnalysis.TowerCrane.OE;
using TCPAPI;
using ProtocolAnalysis.IdentityVerification;
using ProtocolAnalysis.TowerMcLrk.OE;
using ProtocolAnalysis.SoftHat;

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
                //雾炮
                case 3: CommandIssuedInitEvent += CommandIssued_FogGun.FogGun_SetCommond; break;
                //扬尘
                case 4:
                    CommandIssuedInitEvent += CommandIssued_LW.GetGetRealTimeData;
                    CommandIssuedInitEvent += CommandIssued_dust.Dust_SetIPConfig;
                    break;
                //大体积混凝土
                case 5:
                    CommandIssuedInitEvent += CommandIssued_MCLRK.MCLRK_SetControl;
                    break;
                //人员定位
                case 6:
                    CommandIssuedInitEvent += CommandIssued_SoftHat.Crane_SetIPConfig;
                    CommandIssuedInitEvent += CommandIssued_SoftHat.Crane_SetPeriodConfig;
                    break;
                //气体侦测
                case 8:
                    CommandIssuedInitEvent += CommandIssued_GasDetection.GetGetRealTimeData;
                    break;
                case 10:
                    CommandIssuedInitEvent += CommandIssued_IdentityVerification.IdentityVerification_SetIPConfig;//更改ip
                    CommandIssuedInitEvent += CommandIssued_IdentityVerification.IdentityVerification_SetIris;//下发特征库
                    CommandIssuedInitEvent += CommandIssued_IdentityVerification.IdentityVerification_SetIrisdelete;//删除虹膜
                    CommandIssuedInitEvent += CommandIssued_IdentityVerification.IdentifyVerification_Current; //检查设备60秒之内没有发送平台的数据
                    break;
                case 12: //电表
                    CommandIssuedInitEvent += CommandIssued_elec.GetSmartMeter_SN;//
                    break;
                case 13://临边防护
                    break;
                default: break;
            }
        }
        public static Action<IList<TcpSocketClient>> CommandIssuedInitEvent;

    }
}
