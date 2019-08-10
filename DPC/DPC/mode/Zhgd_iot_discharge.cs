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
    public class Zhgd_iot_discharge_current : Zhgd_iot
    {
        /// <summary>
        /// 重量
        /// </summary>
        public double weight { get; set; }
        /// <summary>
        /// 倾角X
        /// </summary>
        public double dip_x { get; set; }
        /// <summary>
        /// 倾角Y
        /// </summary>
        public double dip_y { get; set; }
        /// <summary>
        /// 运行序列码
        /// </summary>
        public string work_cycles_no { get; set; }
    }
    /// <summary>
    /// 运行数据
    /// </summary>
    public class Zhgd_iot_discharge_working : Zhgd_iot_discharge_current
    {
        /// <summary>
        /// 运行周期告警 Y/N
        /// </summary>
        public string work_cycles_warning { get; set; }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="zhgd_Iot_discharge_Current"></param>
        //public static Zhgd_iot_discharge_working Get_Zhgd_iot_discharge_working(Zhgd_iot_discharge_current zhgd_Iot_discharge_Current)
        //{
        //    Zhgd_iot_discharge_working z = zhgd_Iot_discharge_Current as Zhgd_iot_discharge_working;
        //    return z;
        //}
        public static Zhgd_iot_discharge_working Get_Zhgd_iot_discharge_working(Zhgd_iot_discharge_current parent)
        {
            Zhgd_iot_discharge_working child = new Zhgd_iot_discharge_working();
            var ParentType = typeof(Zhgd_iot_discharge_current);
            var Properties = ParentType.GetProperties();
            foreach (var Propertie in Properties)
            {
                if (Propertie.CanRead && Propertie.CanWrite)
                {
                    Propertie.SetValue(child, Propertie.GetValue(parent, null), null);
                }
            }
            return child;
        }
    }

    /// <summary>
    /// 运行数据记录
    /// </summary>
    public class Zhgd_iot_discharge_working_state
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

        public Zhgd_iot_discharge_working_state(string sn_temp)
        {
            sn = sn_temp;
            work_cycles_no = "0";
            is_work_cycles_warning = "N";
        }

        public string Get_work_cycles_no(Zhgd_iot_discharge_current zhgd_Iot_discharge_Current)
        {
            //重量大于0就认为触发了工作循环得条件了
            if (zhgd_Iot_discharge_Current.weight > 0)
            {
                if (zhgd_Iot_discharge_Current.is_warning == "Y")
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
                    Zhgd_iot_discharge_working ztw = Zhgd_iot_discharge_working.Get_Zhgd_iot_discharge_working(zhgd_Iot_discharge_Current);
                    ztw.work_cycles_warning = is_work_cycles_warning;
                    ztw.work_cycles_no = work_cycles_no;
                    //异步运行
                    Discharge_operation.Put_work_cycles_event.BeginInvoke(ztw, null, null);
                    //进行初始化操作
                    work_cycles_no = "0";
                    is_work_cycles_warning = "N";
                }
            }
            return work_cycles_no;
        }
    }
}
