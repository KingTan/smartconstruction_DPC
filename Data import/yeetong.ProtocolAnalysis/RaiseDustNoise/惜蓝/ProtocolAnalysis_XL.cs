using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using TCPAPI;
using System.Reflection;
using ToolAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：惜蓝实时数据解析
    创建时间：2017.6.28
    文件功能描述：惜蓝实时数据解析
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace ProtocolAnalysis
{
    public class ProtocolAnalysis_XL
    {
        public static void OnResolveRecvMessage(string snList,string Content)
        {
            try
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XL", "惜蓝设备数据原包", Content);
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(Content);
                string sss = jo1["datas"].ToString();
                JObject datason = (JObject)JsonConvert.DeserializeObject(sss);
                string[] split = snList.Split(',');
                if (split.Length > 0)
                {
                    for (int i = 0; i < split.Length; i++)
                    {
                        string goup = datason[split[i].ToString()].ToString();
                        JArray jo3 = (JArray)JsonConvert.DeserializeObject(goup);
                        if (jo3.Count > 0)
                        {
                            for (int j = 0; j < jo3.Count; j++)
                            {
                                Current_XL dust = new Current_XL();
                                dust.deNh3 = jo3[j]["deNh3"].ToString();
                                dust.deAqi = jo3[j]["deAqi"].ToString();
                                dust.deCh2o = jo3[j]["deCh2o"].ToString();
                                dust.deCl2 = jo3[j]["deCl2"].ToString();
                                dust.deCo = jo3[j]["deCo"].ToString();
                                dust.deCo2 = jo3[j]["deCo2"].ToString();
                                dust.deCode = jo3[j]["deCode"].ToString();
                                dust.deDir = jo3[j]["deDir"].ToString();
                                dust.deH2s = jo3[j]["deH2s"].ToString();
                                dust.deHcl = jo3[j]["deHcl"].ToString();
                                dust.deHum = jo3[j]["deHum"].ToString();
                                dust.deNh3 = jo3[j]["deNh3"].ToString();
                                dust.deNo2 = jo3[j]["deNo2"].ToString();
                                dust.deNoise = jo3[j]["deNoise"].ToString();
                                dust.deO2 = jo3[j]["deO2"].ToString();
                                dust.deO3 = jo3[j]["deO3"].ToString();
                                dust.dePm10 = jo3[j]["dePm10"].ToString();
                                dust.dePm25 = jo3[j]["dePm25"].ToString();
                                dust.dePre = jo3[j]["dePre"].ToString();
                                dust.deSo2 = jo3[j]["deSo2"].ToString();
                                dust.deSpeed = jo3[j]["deSpeed"].ToString();
                                dust.deTem = jo3[j]["deTem"].ToString();
                                dust.deTime = jo3[j]["deTime"].ToString();
                                dust.deVoc = jo3[j]["deVoc"].ToString();
                                try
                                {
                                    string Json = JsonConvert.SerializeObject(dust);
                                    ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XL", "惜蓝设备实时数据JSON为：", dust.deCode + ";" + Json);
                                }
                                catch (Exception ex)
                                {
                                    ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XL", "惜蓝设备实时数据JSON异常：", dust.deCode + ";" + ex.Message);
                                }
                                DB_XL.SaveCurrentAsyn.BeginInvoke(dust,null,null);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XL", "惜蓝设备实时数据解析异常：", ex.Message);
            }

        }
    }

}
