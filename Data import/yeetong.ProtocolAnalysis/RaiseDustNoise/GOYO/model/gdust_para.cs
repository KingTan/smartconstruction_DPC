using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis
{
    [Serializable]
    public class gdust_para
    {
        /// <summary>
        /// 设备号
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// 参数修改时间
        /// </summary>
        public string updateRtc { get; set; }
        /// <summary>
        /// pm25的报警值
        /// </summary>
        public string Pm25alarm { get; set; }
        /// <summary>
        /// pm10的报警值
        /// </summary>
        public string Pm10alarm { get; set; }
        /// <summary>
        /// 自动模式
        /// </summary>
        public string automatic { get; set; }
        /// <summary>
        /// 手动模式
        /// </summary>
        public string Manual { get; set; }
        /// <summary>
        /// 上报周期
        /// </summary>
        public string cycle { get; set; }
        /// <summary>
        /// 联动继电器开合时间
        /// </summary>
        public string linkage { get; set; }
        /// <summary>
        /// boot版本号
        /// </summary>
        public string bootversion { get; set; }
        /// <summary>
        /// 软件版本号
        /// </summary>
        public string softversion { get; set; }
    }
    [Serializable]
    public class gdust_para_New
    {
        /// <summary>
        /// 设备号
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// 参数修改时间
        /// </summary>
        public string updateRtc { get; set; }
        /// <summary>
        /// pm25的报警值
        /// </summary>
        public string Pm25alarm { get; set; }
        /// <summary>
        /// pm10的报警值
        /// </summary>
        public string Pm10alarm { get; set; }
        /// <summary>
        /// 自动模式
        /// </summary>
        public string automatic { get; set; }
        /// <summary>
        /// 手动模式
        /// </summary>
        public string Manual { get; set; }
        /// <summary>
        /// 上报周期
        /// </summary>
        public string cycle { get; set; }
        /// <summary>
        /// 联动继电器开合时间
        /// </summary>
        public string linkage { get; set; }
        /// <summary>
        /// boot版本号
        /// </summary>
        public string bootversion { get; set; }
        /// <summary>
        /// 软件版本号
        /// </summary>
        public string softversion { get; set; }

        /// <summary>
        /// pm2.5校准系数
        /// </summary>
        public string PM2_5_Factor { get; set; }

        /// <summary>
        /// pm10校准系数
        /// </summary>
        public string PM10_Factor { get; set; }

        /// <summary>
        /// tsp校准系数
        /// </summary>
        public string TSP_Factor { get; set; }
    }
}
