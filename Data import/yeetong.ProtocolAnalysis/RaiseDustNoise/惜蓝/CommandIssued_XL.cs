using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TCPAPI;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：惜蓝命令下发
    创建时间：2017.6.28
    文件功能描述：惜蓝命令下发
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace ProtocolAnalysis
{
    public class CommandIssued_XL
    {
        static string  Proid = "";
        static string URL = "";
        static CommandIssued_XL()
        {
            string path = Application.StartupPath + "\\Config.ini";
            Proid = ToolAPI.INIOperate.IniReadValue("XL", "Proid", path);
            URL = ToolAPI.INIOperate.IniReadValue("XL", "URL", path);
        }
        public static Action<IList<TcpSocketClient>> SendGetRealTimeDataEvent = GetGetRealTimeData;

        static void GetGetRealTimeData(IList<TcpSocketClient> SocketList)
        {
            try
            {
                string sn = DB_XL.GetDustSn(Proid); //获取该工地所有的设备编号
                if (!string.IsNullOrEmpty(sn))
                {
                    string Content = GetJosnNewString(sn);
                    ProtocolAnalysis_XL.OnResolveRecvMessage(sn, Content);
                }
                else
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XL", "惜蓝命令下发", "数据库获取sn为空");
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XL", "惜蓝命令下发异常：", ex.Message);
            }
        }


        // 解析质量安全接口
        static string GetJosnNewString(string sn)
        {
            System.Collections.Specialized.NameValueCollection para = new System.Collections.Specialized.NameValueCollection();
            //string url = "http://hj.ech-med.com/appweb/getEnvData";
            System.Net.WebClient WebClientObj = new System.Net.WebClient();
            para.Add("token", "990df564cfab4d4096434736ae451027");
            para.Add("deviceIds", sn);
            para.Add("startTime", "");
            para.Add("endTime", "");
            byte[] byRemoteInfo = WebClientObj.UploadValues(URL, "POST", para);//请求地址,传参方式,参数集合
            string rtContent = System.Text.Encoding.UTF8.GetString(byRemoteInfo);//获取返回值 
            return rtContent;
        }
    }
}
