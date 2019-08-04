﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ProtocolAnalysis
{
    [Serializable]
    public class Current_LW
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID { set; get; }
        /// <summary>
        /// 噪声
        /// </summary>
        public float Noise { set; get; }
        /// <summary>
        /// PM10
        /// </summary>
        public float Pm10 { set; get; }
        /// <summary>
        /// 风速
        /// </summary>
        public float Wind { set; get; }
        /// <summary>
        /// 风向
        /// </summary>
        public float WindDirection { set; get; }
        /// <summary>
        /// 温度
        /// </summary>
        public float Temperature { set; get; }
        /// <summary>
        /// 湿度
        /// </summary>
        public float Humidity { set; get; }
        /// <summary>
        /// 气压
        /// </summary>
        public float Pressure { set; get; }
        /// <summary>
        /// 电压
        /// </summary>
        public float Voltage { set; get; }
        /// <summary>
        /// PM2.5
        /// </summary>
        public float Pm2_5 { set; get; }
        /// <summary>
        /// 经度
        /// </summary>
        public float Longitude { set; get; }
        /// <summary>
        /// 纬度
        /// </summary>
        public float Latitude { set; get; }
        /// <summary>
        /// 板载温度
        /// </summary>
        public float BanTemperature { set; get; }
        /// <summary>
        /// 板载湿度
        /// </summary>
        public float BanHumidity { set; get; }
        /// <summary>
        /// TSP
        /// </summary>
        public float TSP { set; get; }

        public Current_LW()
        {
            ID = "";
        }

        public Current_LW Clone()
        {
            return (Current_LW)this.MemberwiseClone();
        }
    }
}
