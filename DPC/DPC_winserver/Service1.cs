using Architecture;
using ProtocolAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DPC_winserver
{
    public partial class Service1 : ServiceBase
    {
        MainClass mc = new MainClass();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //解析
            Subject sub = new Subject();
            sub.DataAnalysis += ProtocolAnalysisSE_Main.ProtocolPackageResolver;
            //命令下发
            CommandIssued_Main.CommandIssued_MainInit();
            sub.CommandSending += CommandIssued_Main.CommandIssuedInitEvent;
            mc.App_Open(sub);
        }

        protected override void OnStop()
        {
            mc.App_Close();
        }
    }
}
