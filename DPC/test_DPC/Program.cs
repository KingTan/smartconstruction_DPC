using Architecture;
using DPC;
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


            //RedisCacheHelper.Add("塔吊1", 1565021089001);
            //long value = RedisCacheHelper.Get<long>("塔吊1");
            //RedisCacheHelper.Remove("塔吊1");
            //value = RedisCacheHelper.Get<long>("塔吊1");

            //string key = "equipment:projectid:01_01:" + "123456";
            //string value = RedisCacheHelper.Get<string>(key);
            //key = "equipment:online_time:01_01:"  + "123456";
            //long value1 = RedisCacheHelper.Get<long>(key);
        }
    }
}
