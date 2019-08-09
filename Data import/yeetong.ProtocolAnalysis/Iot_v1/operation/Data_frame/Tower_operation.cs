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
    /// 塔机
    /// </summary>
    public class Tower_operation
    {
        public static void Data_operation(object datatemp)
        {
            Tower_send_frame tower_Send_Frame = JsonConvert.DeserializeObject<Tower_send_frame>(datatemp.ToString());
            if(tower_Send_Frame!=null)
            {
                Zhgd_iot_tower_current data = new Zhgd_iot_tower_current();
                data.sn = tower_Send_Frame.sn;
                data.is_warning = tower_Send_Frame.is_warning;
                data.warning_type = tower_Send_Frame.warning_type;
                data.@timestamp = tower_Send_Frame.@timestamp;
                data.weight = tower_Send_Frame.weight;
                data.height = tower_Send_Frame.height;
                data.range = tower_Send_Frame.range;
                data.rotation = tower_Send_Frame.rotation;
                data.moment_forces = tower_Send_Frame.moment_forces;
                data.wind_grade = tower_Send_Frame.wind_grade;
                data.wind_speed = tower_Send_Frame.wind_speed;
                data.dip_x = tower_Send_Frame.dip_x;
                data.dip_y = tower_Send_Frame.dip_y;
                data.boom_arm_length = tower_Send_Frame.boom_arm_length;
                data.balance_arm_length = tower_Send_Frame.balance_arm_length;
                data.tower_body_height = tower_Send_Frame.tower_body_height;
                data.tower_hat_height = tower_Send_Frame.tower_hat_height;
                data.driver_id_code = tower_Send_Frame.driver_id_code;

                //进行数据put 
                DPC.Tower_operation.Send_tower_Current(data);
              
            }
        }
    }
}
