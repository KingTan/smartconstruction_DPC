using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：蓝丰实时数据对象
    创建时间：2017.6.28
    文件功能描述：蓝丰实时数据对象
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace ProtocolAnalysis
{
    [Serializable]
    public class Current_LF
    {
        
        /// <summary>
        /// 数据上传时间 时间戳
        /// </summary>
        public string Time { set; get; }
        /// <summary>
        /// ST 212协议的系统编号，“22”代表 空气质量监测；
        /// </summary>
        public string ST { set; get; }
        /// <summary>
        /// 212协议的命令编号，“2011”代表 取污染物实时数据（分钟数据）
        /// </summary>
        public string CN { set; get; }
        /// <summary>
        /// 现无实际用途，可固定为“123456”；
        /// </summary>
        public string PW { set; get; }
        /// <summary>
        /// MN 用作设备识别，共14位，格式规范详见附表1“堆场监测设备MN编号命名规则”。
        /// </summary>
        public string MN { set; get; }
        /// <summary>
        /// WIND_DIRECT_Rtd  参数为风向
        /// </summary>
        public string WIND_DIRECT_Rtd { set; get; }
        /// <summary>
        /// WIND_SPEED_Rtd 参数为风速
        /// </summary>
        public string WIND_SPEED_Rtd { set; get; }
        /// <summary>
        /// TEMP_Rtd  参数为温度
        /// </summary>
        public string TEMP_Rtd { set; get; }
        /// <summary>
        /// HUMID_Rtd  参数为湿度
        /// </summary>
        public string HUMID_Rtd { set; get; }
        /// <summary>
        /// NOISE_Rtd  参数为噪声
        /// </summary>
        public string NOISE_Rtd { set; get; }
        /// <summary>
        /// NOISE_PEAK_Rtd 参数为噪声峰值
        /// </summary>
        public string NOISE_PEAK_Rtd { set; get; }
        /// <summary>
        /// LATIT_Rtd 参数为纬度
        /// </summary>
        public string LATIT_Rtd { set; get; }
        /// <summary>
        /// LONGT_Rtd 参数为经度
        /// </summary>
        public string LONGT_Rtd { set; get; }
        /// <summary>
        /// PM10_Rtd 参数为经度
        /// </summary>
        public string PM10_Rtd { set; get; }
        /// <summary>
        /// PM2_5-Rtd 参数为PM2.5
        /// </summary>
        public string PM2_5_Rtd { set; get; }
        /// <summary>
        /// TSP-Rtd 参数为粉尘的
        /// </summary>
        public string TSP_Rtd { set; get; }

        public Current_LF Clone()
        {
            return (Current_LF)this.MemberwiseClone();
        }
    }
}
