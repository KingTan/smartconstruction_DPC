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
            string url = "https://111.56.13.177:52001/zhgd_iot_tower/_doc/";
            string content = "{\"id\":\"123456\",\"weight\": 2.16,\"rate\": 2,\"@timestamp\":1564930243001}";
            DPC.Restful.Post(url, content);
        }
    }
}
