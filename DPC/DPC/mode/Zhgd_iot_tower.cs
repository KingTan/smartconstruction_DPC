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
    public class Zhgd_iot_tower_current : Zhgd_iot
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
        /// 幅度
        /// </summary>
        public double range { get; set; }
        /// <summary>
        /// 回转
        /// </summary>
        public double rotation { get; set; }
        /// <summary>
        /// 力矩 
        /// </summary>
        public double moment_forces { get; set; }
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
        /// 大臂长
        /// </summary>
        public double boom_arm_length { get; set; }
        /// <summary>
        /// 平衡臂
        /// </summary>
        public double balance_arm_length { get; set; }
        /// <summary>
        /// 塔身高
        /// </summary>
        public double tower_body_height { get; set; }
        /// <summary>
        /// 塔冒高
        /// </summary>
        public double tower_hat_height { get; set; }
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
    public class Zhgd_iot_tower_working : Zhgd_iot_tower_current
    {
        /// <summary>
        /// 运行周期告警 Y/N
        /// </summary>
        public string work_cycles_warning { get; set; }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="zhgd_Iot_Tower_Current"></param>
        //public static Zhgd_iot_tower_working Get_Zhgd_iot_tower_working(Zhgd_iot_tower_current zhgd_Iot_Tower_Current)
        //{
        //    Zhgd_iot_tower_working z = new Zhgd_iot_tower_working();
        //    Zhgd_iot_tower_current zhgd_Iot_Tower_Currenta = z;
        //    zhgd_Iot_Tower_Currenta = zhgd_Iot_Tower_Current;
        //    z.work_cycles_warning = "";
        //    return z;
        //}

        public static Zhgd_iot_tower_working Get_Zhgd_iot_tower_working(Zhgd_iot_tower_current parent)
        {
            Zhgd_iot_tower_working child = new Zhgd_iot_tower_working();
            var ParentType = typeof(Zhgd_iot_tower_current);
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
    public class Zhgd_iot_tower_working_state
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
        /// 是否存在报警  默认N  存在Y
        /// </summary>
        public string is_work_cycles_warning { get; set; }
        /// <summary>
        /// 是否有效 根据高度，幅度，回转
        /// </summary>
        public bool is_change_height { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public double last_height { get; set; }
        /// <summary>
        /// 幅度
        /// </summary>
        public double last_range { get; set; }
        /// <summary>
        /// 回转
        /// </summary>
        public double last_rotation { get; set; }

        public Zhgd_iot_tower_working_state(string sn_temp)
        {
            sn = sn_temp;
            work_cycles_no = "0";
            is_work_cycles_warning = "N";
            is_change_height = false;
            last_height = 0; last_range = 0; last_rotation = 0;
        }

        public string Get_work_cycles_no(Zhgd_iot_tower_current zhgd_Iot_Tower_Current)
        {
            //大于0.2t就认为触发了工作循环得条件了
            if (zhgd_Iot_Tower_Current.weight > 0.2)
            {
                if (zhgd_Iot_Tower_Current.is_warning == "Y")
                    is_work_cycles_warning = "Y";
                if (work_cycles_no != "0")
                {
                    if (last_height != zhgd_Iot_Tower_Current.height || last_range != zhgd_Iot_Tower_Current.range || last_rotation != zhgd_Iot_Tower_Current.rotation)
                        is_change_height = true;
                    last_height = zhgd_Iot_Tower_Current.height;
                    last_range = zhgd_Iot_Tower_Current.range;
                    last_rotation = zhgd_Iot_Tower_Current.rotation;
                }
                else
                {
                    work_cycles_no = DPC_Tool.GetTimeStamp().ToString();
                    is_change_height = false;
                    last_height = zhgd_Iot_Tower_Current.height;
                    last_range = zhgd_Iot_Tower_Current.range;
                    last_rotation = zhgd_Iot_Tower_Current.rotation;
                }
            }
            //不满足工作循环得条件
            else
            {
                //不等于0说明这次工作循环该结束了
                if (work_cycles_no != "0")
                {
                    //put运行数据到ES里
                    Zhgd_iot_tower_working ztw = Zhgd_iot_tower_working.Get_Zhgd_iot_tower_working(zhgd_Iot_Tower_Current);
                    ztw.work_cycles_warning = is_work_cycles_warning;
                    //异步运行
                    Tower_operation.Put_work_cycles_event.BeginInvoke(ztw,null,null);
                    //进行初始化操作
                    work_cycles_no = "0";
                    is_work_cycles_warning = "N";
                    is_change_height = false;
                    last_height = 0; last_range = 0; last_rotation = 0;
                }
            }
            return work_cycles_no;
        }
    }
}
