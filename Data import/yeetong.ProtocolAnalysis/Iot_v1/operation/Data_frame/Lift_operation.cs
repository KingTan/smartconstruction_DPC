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
    /// 升降机
    /// </summary>
    public class Lift_operation
    {
        public static void Data_operation(object datatemp)
        {
            Lift_send_frame lift_Send_Frame = JsonConvert.DeserializeObject<Lift_send_frame>(datatemp.ToString());
            if (lift_Send_Frame != null)
            {
                Zhgd_iot_lift_current data = new Zhgd_iot_lift_current();
                data.sn = lift_Send_Frame.sn;
                data.is_warning = lift_Send_Frame.is_warning;
                data.warning_type = lift_Send_Frame.warning_type;
                data.@timestamp = lift_Send_Frame.@timestamp;
                data.weight = lift_Send_Frame.weight;
                data.height = lift_Send_Frame.height;
                data.floor = lift_Send_Frame.floor;
                data.peoples = lift_Send_Frame.peoples;
                data.speed = lift_Send_Frame.speed;
                data.wind_grade = lift_Send_Frame.wind_grade;
                data.wind_speed = lift_Send_Frame.wind_speed;
                data.dip_x = lift_Send_Frame.dip_x;
                data.dip_y = lift_Send_Frame.dip_y;
                data.floor_height = lift_Send_Frame.floor_height;
                data.door_status = lift_Send_Frame.door_status;
                data.driver_id_code = lift_Send_Frame.driver_id_code;

                //进行数据put 
                DPC.Lift_operation.Send_Lift_Current(data);

            }
        }
    }
}
