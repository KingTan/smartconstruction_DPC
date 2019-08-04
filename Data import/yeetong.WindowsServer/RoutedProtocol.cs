using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using GOYO_Architecture;
using GOYO_ProtocolAnalysis;


namespace GOYO_SpecialEquipmentServer
{
    partial class RoutedProtocol : ServiceBase
    {
        MainClass mc = new MainClass();
       
        /// <summary>
        /// 
        /// </summary>
        public RoutedProtocol()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            //这些事件都可以根据配置文件来确定
            //UDP
            //UdpSever udps = new UdpSever();
            //udps.eventTrigger += ProtocolAnalysisSE_MainUdp.ProtocolPackageUdpResolver;
            //udps.Listener(int.Parse(MainStatic.Port));
            //TCP
            Subject sub = new Subject();
            sub.DataAnalysis += ProtocolAnalysisSE_Main.ProtocolPackageResolver;
            CommandIssued_Main.CommandIssued_MainInit();
            sub.CommandSending += CommandIssued_Main.CommandIssuedInitEvent;
            mc.App_Open(sub);
        }
       
        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            mc.App_Close();
        }
    }
}
