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
    public class Zhgd_iot_lift_current : Zhgd_iot
    {
        /// <summary>
        /// 重量
        /// </summary>
        public double weight { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public double height { get; set; }
        /// <summary>
        /// 楼层
        /// </summary>
        public int floor { get; set; }
        /// <summary>
        /// 人数 
        /// </summary>
        public int peoples { get; set; }
        /// <summary>
        /// 速度  
        /// </summary>
        public double speed { get; set; }
        /// <summary>
        /// 风级
        /// </summary>
        public int wind_grade { get; set; }
        /// <summary>
        /// 风速
        /// </summary>
        public double wind_speed { get; set; }
        /// <summary>
        /// 倾角X
        /// </summary>
        public double dip_x { get; set; }
        /// <summary>
        /// 倾角Y
        /// </summary>
        public double dip_y { get; set; }
        /// <summary>
        /// 门状态
        /// </summary>
        public string door_status { get; set; }
        /// <summary>
        /// 司机识别码
        /// </summary>
        public string driver_id_code { get; set; }
        /// <summary>
        /// 运行序列码
        /// </summary>
        public string work_cycles_no { get; set; }
    }
    /// <summary>
    /// 运行数据
    /// </summary>
    public class Zhgd_iot_lift_working :Zhgd_iot_lift_current
    {
        /// <summary>
        /// 运行周期告警 Y/N
        /// </summary>
        public string work_cycles_warning { get; set; }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="zhgd_Iot_Lift_Current"></param>
        public static Zhgd_iot_lift_working Get_Zhgd_iot_lift_working(Zhgd_iot_lift_current zhgd_Iot_Lift_Current)
        {
            Zhgd_iot_lift_working z = zhgd_Iot_Lift_Current as Zhgd_iot_lift_working;
            return z;
        }
    }

    /// <summary>
    /// 运行数据记录
    /// </summary>
    public class Zhgd_iot_lift_working_state
    {
        /// <summary>
        /// 设备编码
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// 工作循环序列码 0不在进行工作， 非0 正在进行工作
        /// </summary>
        public string work_cycles_no { get; set; }
        /// <summary>
        /// 是否存在报警  默认false
        /// </summary>
        public string is_work_cycles_warning { get; set; }

        public Zhgd_iot_lift_working_state(string sn_temp)
        {
            sn = sn_temp;
            work_cycles_no = "0";
            is_work_cycles_warning = "N";
        }

        public string Get_work_cycles_no(Zhgd_iot_lift_current zhgd_Iot_Lift_Current)
        {
            //大于0.2t就认为触发了工作循环得条件了
            if (zhgd_Iot_Lift_Current.speed > 0)
            {
                if (zhgd_Iot_Lift_Current.is_warning == "Y")
                    is_work_cycles_warning = "Y";
                if (work_cycles_no == "0")
                {
                    work_cycles_no = DPC_Tool.GetTimeStamp().ToString();
                }
            }
            //不满足工作循环得条件
            else
            {
                //不等于0说明这次工作循环该结束了
                if (work_cycles_no != "0")
                {
                    //put运行数据到ES里
                    Zhgd_iot_lift_working ztw = Zhgd_iot_lift_working.Get_Zhgd_iot_lift_working(zhgd_Iot_Lift_Current);
                    ztw.work_cycles_warning = is_work_cycles_warning;
                    //异步运行
                    Lift_operation.Put_work_cycles_event.BeginInvoke(ztw, null, null);
                    //进行初始化操作
                    work_cycles_no = "0";
                    is_work_cycles_warning = "N";
                }
            }
            return work_cycles_no;
        }
    }
}
