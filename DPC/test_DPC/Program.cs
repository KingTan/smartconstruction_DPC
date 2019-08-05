using Architecture;
using ProtocolAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace test_DPC
{
    class Program
    {
        static void Main(string[] args)
        {
            //string url = "https://111.56.13.177:52001/zhgd_iot_tower/_doc/";
            //string content = "{\"id\":\"123456\",\"weight\": 2.16,\"rate\": 2,\"@timestamp\":1564930243001}";
            //DPC.Restful.Post(url, content);
            MainClass mc = new MainClass();
            //解析
            Subject sub = new Subject();
            sub.DataAnalysis += ProtocolAnalysisSE_Main.ProtocolPackageResolver;
            //命令下发
            CommandIssued_Main.CommandIssued_MainInit();
            sub.CommandSending += CommandIssued_Main.CommandIssuedInitEvent;
            mc.App_Open(sub);
            Console.ReadLine();
        }
    }
}
