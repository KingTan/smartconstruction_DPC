using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.model
{
    /// <summary>
    /// 扬尘噪音数据接受类
    /// </summary>
    public class Dust_noise__send_frame
    {
        /// <summary>
        /// 设备序列码
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// 检索时间 一般为采集时间
        /// </summary>
        public long @timestamp { get; set; }
        /// <summary>
        /// pm2.5
        /// </summary>
        public double pm2_5 { get; set; }
        /// <summary>
        /// pm10
        /// </summary>
        public double pm10 { get; set; }
        /// <summary>
        /// tsp
        /// </summary>
        public double tsp { get; set; }
        /// <summary>
        /// 噪音 
        /// </summary>
        public double noise { get; set; }
        /// <summary>
        /// 温度  
        /// </summary>
        public double temperature { get; set; }
        /// <summary>
        /// 湿度  
        /// </summary>
        public double humidity { get; set; }
        /// <summary>
        /// 风速  
        /// </summary>
        public double wind_speed { get; set; }
        /// <summary>
        /// 风级 
        /// </summary>
        public int wind_grade { get; set; }
        /// <summary>
        /// 风向
        /// </summary>
        public double wind_direction { get; set; }
        /// <summary>
        /// 气压 
        /// </summary>
        public double air_pressure { get; set; }
        /// <summary>
        /// 降雨量 
        /// </summary>
        public double rainfall { get; set; }
    }
}
