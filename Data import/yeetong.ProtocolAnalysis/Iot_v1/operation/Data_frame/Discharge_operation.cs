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
    /// 卸料平台
    /// </summary>
    public class Discharge_operation
    {
        public static void Data_operation(object datatemp)
        {
            Discharge__send_frame discharge_Send_Frame = JsonConvert.DeserializeObject<Discharge__send_frame>(datatemp.ToString());
            if (discharge_Send_Frame != null)
            {
                Zhgd_iot_discharge_current data = new Zhgd_iot_discharge_current();
                data.sn = discharge_Send_Frame.sn;
                data.is_warning = discharge_Send_Frame.is_warning;
                data.warning_type = discharge_Send_Frame.warning_type;
                data.@timestamp = discharge_Send_Frame.@timestamp;
                data.weight = discharge_Send_Frame.weight;
                data.dip_x = discharge_Send_Frame.dip_x;
                data.dip_y = discharge_Send_Frame.dip_y;

                //进行数据put 
                DPC.Discharge_operation.Send_discharge_Current(data);

            }
        }
    }
}
