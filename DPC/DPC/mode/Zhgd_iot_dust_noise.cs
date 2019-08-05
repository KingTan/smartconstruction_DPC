using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPC
{
    /// <summary>
    /// 实时数据
    /// </summary>
    public class Zhgd_iot_dust_noise_current : Zhgd_iot
    {
        /// <summary>
        /// pm2.5
        /// </summary>
        public double pm2_5 { get; set; }
        /// <summary>
        /// pm10
        /// </summary>
        public int pm10 { get; set; }
        /// <summary>
        /// tsp
        /// </summary>
        public int tsp { get; set; }
        /// <summary>
        /// 噪音 
        /// </summary>
        public string noise { get; set; }
        /// <summary>
        /// 温度  
        /// </summary>
        public string temperature { get; set; }
        /// <summary>
        /// 湿度  
        /// </summary>
        public string humidity { get; set; }
        /// <summary>
        /// 风速  
        /// </summary>
        public string wind_speed { get; set; }
        /// <summary>
        /// 风级 
        /// </summary>
        public string wind_grade { get; set; }
        /// <summary>
        /// 气压 
        /// </summary>
        public string air_pressure { get; set; }
        /// <summary>
        /// 降雨量 
        /// </summary>
        public string rainfall { get; set; }
    }
}
