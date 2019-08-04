using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：惜蓝实时数据对象
    创建时间：2017.6.28
    文件功能描述：惜蓝实时数据对象
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace ProtocolAnalysis
{
    [Serializable]
    public class Current_XL
    {
         /// <summary>
        /// NH3
       /// </summary>
       public string deNh3 { get; set; }
       /// <summary>
       /// H2S
       /// </summary>
       public string deH2s { get; set; }
       /// <summary>
       /// SO2
       /// </summary>
       public string deSo2 { get; set; }
       /// <summary>
       /// 温度
       /// </summary>
       public string deTem { get; set; }
       /// <summary>
       /// 湿度
       /// </summary>
       public string deHum { get; set; }
       /// <summary>
       /// HCL
       /// </summary>
       public string deHcl { get; set; }
       /// <summary>
       /// aqi 指数
       /// </summary>
       public string deAqi { get; set; }
       /// <summary>
       /// CO2
       /// </summary>
       public string deCo2 { get; set; }
       /// <summary>
       /// VOC
       /// </summary>
       public string deVoc { get; set; }
       /// <summary>
       /// CH2O
       /// </summary>
       public string deCh2o { get; set; }
       /// <summary>
       /// 噪声
       /// </summary>
       public string deNoise { get; set; }
       /// <summary>
       /// O2
       /// </summary>
       public string deO2 { get; set; }
       /// <summary>
       /// CO
       /// </summary>
       public string deCo { get; set; }
       /// <summary>
       /// O3
       /// </summary>
       public string deO3 { get; set; }
       /// <summary>
       /// CL2
       /// </summary>
       public string deCl2 { get; set; }
       /// <summary>
       /// 风向
       /// </summary>
       public string deDir { get; set; }
       /// <summary>
       /// 时间
       /// </summary>
       public string deTime { get; set; }
       /// <summary>
       /// PM10
       /// </summary>
       public string dePm10 { get; set; }
       /// <summary>
       /// 大气压
       /// </summary>
       public string dePre { get; set; }
       /// <summary>
       /// No2
       /// </summary>
       public string deNo2 { get; set; }
       /// <summary>
       /// PM2.5
       /// </summary>
       public string dePm25 { get; set; }
       /// <summary>
       /// 风速
       /// </summary>
       public string deSpeed { get; set; }
       /// <summary>
       /// 设备编号
       /// </summary>
       public string deCode { get; set; }

       public Current_XL Clone()
        {
            return (Current_XL)this.MemberwiseClone();
        }
    }
}
