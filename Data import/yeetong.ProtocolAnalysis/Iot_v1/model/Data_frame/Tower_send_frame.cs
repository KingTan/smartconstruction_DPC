using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.model
{
    /// <summary>
    /// 塔吊数据接受类
    /// </summary>
    public class Tower_send_frame
    {
        /// <summary>
        /// 设备序列码
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// 是否报警 Y或N
        /// </summary>
        public string is_warning { get; set; }
        /// <summary>
        /// 报警类型 数组
        /// </summary>
        public string[] warning_type { get; set; }
        /// <summary>
        /// 检索时间 一般为采集时间
        /// </summary>
        public long @timestamp { get; set; }
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
        /// 起重臂长
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
    }
}
