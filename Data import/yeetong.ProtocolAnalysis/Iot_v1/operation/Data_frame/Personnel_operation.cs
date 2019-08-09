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
    /// 人员考勤
    /// </summary>
    public class Personnel_operation
    {
        public static void Data_operation(object datatemp)
        {
            Personnel_send_frame personnel_Send_Frame = JsonConvert.DeserializeObject<Personnel_send_frame>(datatemp.ToString());
            if (personnel_Send_Frame != null)
            {
                Zhgd_iot_personnel_records data = new Zhgd_iot_personnel_records();
                data.project_code = personnel_Send_Frame.project_code;
                data.sn = personnel_Send_Frame.sn;
                data.gate_no = personnel_Send_Frame.gate_no;
                data.@timestamp = personnel_Send_Frame.@timestamp;
                data.channel_no = personnel_Send_Frame.channel_no;
                data.cert_mode = personnel_Send_Frame.cert_mode;
                data.in_or_out = personnel_Send_Frame.in_or_out;
                data.personal_id_code = personnel_Send_Frame.personal_id_code;
                data.features_code = personnel_Send_Frame.features_code;

                //进行数据put 
                DPC.Personnel_operation.Send_personnel_records(data);

            }
        }
    }
}
