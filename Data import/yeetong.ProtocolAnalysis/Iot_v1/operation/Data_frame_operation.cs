using ProtocolAnalysis.Iot_v1.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.operation
{
    /// <summary>
    /// 数据帧
    /// </summary>
    public class Data_frame_operation
    {
        public static void Data_operation(string equipment_type,object data)
        {
            if (equipment_type == Equipment_type.塔机)
            {
                Tower_operation.Data_operation(data);
            }
            else if (equipment_type == Equipment_type.升降机)
            {
                Lift_operation.Data_operation(data);
            }
            else if (equipment_type == Equipment_type.卸料平台)
            {
                Discharge_operation.Data_operation(data);
            }
            else if (equipment_type == Equipment_type.扬尘噪音)
            {
                Dust_noise_operation.Data_operation(data);
            }
            else if (equipment_type == Equipment_type.人员管理)
            {
                Personnel_operation.Data_operation(data);
            }
            else if (equipment_type == Equipment_type.雾泡喷淋)
            {

            }
        }
    }
}
