using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis
{
    [Serializable]
    public class xadust_Current
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// RTC
        /// </summary>
        public string Rtc { get; set; }
        /// <summary>
        /// pm25
        /// </summary>
        public string Pm2_5 { set; get; }
        /// <summary>
        /// pm10
        /// </summary>
        public string Pm10 { get; set; }
        /// <summary>
        /// 噪声
        /// </summary>
        public string Noise { set; get; }
        /// <summary>
        /// 温度
        /// </summary>
        public string Temperature { set; get; }
        /// <summary>
        /// 湿度
        /// </summary>
        public string Humidity { set; get; }
        /// <summary>
        /// 风速
        /// </summary>
        public string Wind { set; get; }
        /// <summary>
        /// 风向
        /// </summary>
        public string WindDirection { set; get; }
        /// <summary>
        /// GPRS信号强度
        /// </summary>
        public string GprsSignal { get; set; }
        /// <summary>
        /// 自动模式
        /// </summary>
        public string automatic { get; set; }
        /// <summary>
        /// 手动模式
        /// </summary>
        public string Manual { get; set; }
        /// <summary>
        /// 气压
        /// </summary>
        public string pressure { get; set; }
        /// <summary>
        /// 颗粒物
        /// </summary>
        public string particulates { get; set; }
        /// <summary>
        /// 警报
        /// </summary>
        public string alarm { get; set; }
    }
}

