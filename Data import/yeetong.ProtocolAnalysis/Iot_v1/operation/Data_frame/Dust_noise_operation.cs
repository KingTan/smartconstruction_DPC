using DPC;
using Newtonsoft.Json;
using ProtocolAnalysis.Iot_v1.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.operation
{
    /// <summary>
    /// 扬尘噪音
    /// </summary>
    public class Dust_noise_operation
    {
        public static void Data_operation(object datatemp)
        {
            Dust_noise__send_frame dust_noise_Send_Frame = JsonConvert.DeserializeObject<Dust_noise__send_frame>(datatemp.ToString());
            if (dust_noise_Send_Frame != null)
            {
                Zhgd_iot_dust_noise_current data = new Zhgd_iot_dust_noise_current();
                data.sn = dust_noise_Send_Frame.sn;
                data.@timestamp = dust_noise_Send_Frame.@timestamp;
                data.pm2_5 = dust_noise_Send_Frame.pm2_5;
                data.pm10 = dust_noise_Send_Frame.pm10;
                data.tsp = dust_noise_Send_Frame.tsp;
                data.noise = dust_noise_Send_Frame.noise;
                data.temperature = dust_noise_Send_Frame.temperature;
                data.humidity = dust_noise_Send_Frame.humidity;
                data.wind_speed = dust_noise_Send_Frame.wind_speed;
                data.wind_grade = dust_noise_Send_Frame.wind_grade;
                data.wind_direction = dust_noise_Send_Frame.wind_direction;
                data.air_pressure = dust_noise_Send_Frame.air_pressure;
                data.rainfall = dust_noise_Send_Frame.rainfall;

                //进行数据put 
                DPC.Dust_noise_operation.Send_dust_noise_Current(data);

            }
        }
    }
}
