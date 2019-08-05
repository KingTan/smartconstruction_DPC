﻿using System;
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
        public int wind_speed { get; set; }
        /// <summary>
        /// 倾角X
        /// </summary>
        public int dip_x { get; set; }
        /// <summary>
        /// 倾角Y
        /// </summary>
        public int dip_y { get; set; }
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
    }
}
