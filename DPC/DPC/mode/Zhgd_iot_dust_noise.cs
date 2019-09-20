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
        /// <summary>
        /// AQI
        /// </summary>
        public double aqi { get; set; }

         public Zhgd_iot_dust_noise_current()
        {
            pm2_5 = 0;
            pm10 = 0;
            tsp = 0;
            noise = 0;
            temperature =0;
            humidity = 0;
            wind_speed = 0;
            wind_grade = 0;
            wind_direction = 0;
            air_pressure = 0;
            rainfall = 0;
        }
    }
}
