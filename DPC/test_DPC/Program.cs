using Architecture;
using DPC;
using Newtonsoft.Json;
using ProtocolAnalysis;
using ProtocolAnalysis.Iot_v1.model;
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


            //MainClass mc = new MainClass();
            ////解析
            //Subject sub = new Subject();
            //sub.DataAnalysis += ProtocolAnalysisSE_Main.ProtocolPackageResolver;
            ////命令下发
            //CommandIssued_Main.CommandIssued_MainInit();
            //sub.CommandSending += CommandIssued_Main.CommandIssuedInitEvent;
            //mc.App_Open(sub);
            //Console.ReadLine();


            //RedisCacheHelper.Add("塔吊1", 1565021089001);
            //long value = RedisCacheHelper.Get<long>("塔吊1");
            //RedisCacheHelper.Remove("塔吊1");
            //value = RedisCacheHelper.Get<long>("塔吊1");

            //string key = "equipment:projectid:01_01:" + "123456";
            //string value = RedisCacheHelper.Get<string>(key);
            //key = "equipment:online_time:01_01:"  + "123456";
            //long value1 = RedisCacheHelper.Get<long>(key);

            //人员测试
            //Zhgd_iot_personnel_records zhgd_Iot_Personnel_Records = new Zhgd_iot_personnel_records();
            //zhgd_Iot_Personnel_Records.timestamp = DPC_Tool.GetTimeStamp(DateTime.Now);
            //zhgd_Iot_Personnel_Records.project_code = "123456";
            //zhgd_Iot_Personnel_Records.sn = "1";
            //zhgd_Iot_Personnel_Records.gate_no = "0";
            //zhgd_Iot_Personnel_Records.channel_no = "进";
            //zhgd_Iot_Personnel_Records.cert_mode = Cert_mode.IC卡;
            //zhgd_Iot_Personnel_Records.in_or_out = In_or_out.进;
            //zhgd_Iot_Personnel_Records.personal_id_code = "1234567894524852";
            //Personnel_operation.Send_personnel_records(zhgd_Iot_Personnel_Records);


            //string sd = "{\"frame_type\": \"register\",\"equipment_type\": \"tower\",\"time_stamp\": \"2019-08-08 22:27:00\",\"frame_token\": \"\",\"data\": {\"vendor_code\": \"z7d8jfgn39ki987779jh2\"}}";

            //Send_frame  sf = JsonConvert.DeserializeObject<Send_frame>(sd);
            //////Register_send_frame register_Send_Framae = sf.data as Register_send_frame;
            //Register_send_frame register_Send_Frame = JsonConvert.DeserializeObject<Register_send_frame>(sf.data.ToString());


            //RedisCacheHelper.Add("asd","");

            //  API_request_data.Unload_KY.App_Open();
            //string s = "{\"msg\":\"成功\",\"code\":\"200\",\"data\":[{\"B03-Avg\":\"52.70\",\"W02-Avg\":\"0.10\",\"T01-Avg\":\"28.50\",\"latitude\":\"32.5111570000\",\"siteName\":\"姜堰桃李府项目\",\"W01-Avg\":\"110.30\",\"PM10-Avg\":\"7.30\",\"PM25-Avg\":\"3.00\",\"siteId\":1081,\"state\":1,\"time\":20190814195703,\"longitude\":\"120.1677020000\",\"H01-Avg\":\"73.00\"},{\"B03-Avg\":\"52.70\",\"W02-Avg\":\"0.10\",\"T01-Avg\":\"28.50\",\"latitude\":\"32.5111570000\",\"siteName\":\"姜堰桃李府项目\",\"W01-Avg\":\"110.30\",\"PM10-Avg\":\"7.30\",\"PM25-Avg\":\"3.00\",\"siteId\":1081,\"state\":1,\"time\":20190814195703,\"longitude\":\"120.1677020000\",\"H01-Avg\":\"73.00\"}]}";
            //API_request_data.科宇.扬尘噪音.Get_Real_list_result _Result = JsonConvert.DeserializeObject<API_request_data.科宇.扬尘噪音.Get_Real_list_result>(s);
            //API_request_data.科宇.扬尘噪音.Dust_noise_real dust_Noise_Real = API_request_data.科宇.扬尘噪音.Dust_noise_real.Get_Dust_noise_real(_Result.data[0].ToString());

            API_request_data.Dust_noise_KY.App_Open();
        }
    }
}
