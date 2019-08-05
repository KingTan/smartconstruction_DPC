using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPC
{
    /// <summary>
    /// 智慧工地
    /// </summary>
    public class Zhgd_iot
    {
        /// <summary>
        /// 检索时间 一般为采集时间
        /// </summary>
        public long @timestamp { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long create_time { get; set; }
        /// <summary>
        /// 项目ID
        /// </summary>
        public string project_id { get; set; }
        /// <summary>
        /// 设备类型 参考设备类型字典
        /// </summary>
        public string equipment_type { get; set; }
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
    }

    /// <summary>
    /// 设备类型静态类
    /// </summary>
    public static class Equipment_type 
    {
        public static readonly string 塔机 = "01_01";
        public static readonly string 升降机 = "01_02";
        public static readonly string 卸料平台 = "01_03";
        public static readonly string 雾泡喷淋 = "02_01";
        public static readonly string 扬尘噪音 = "02_02";
        public static readonly string 人员管理 = "06";
    }
    /// <summary>
    /// 报警类型静态类
    /// </summary>
    public static class Warning_type
    {
        public static readonly string PM2_5报警 = "01";
        public static readonly string PM10报警 = "02";
        public static readonly string 噪音告警 = "03";
        public static readonly string 重量告警 = "04";
        public static readonly string 碰撞报警 = "05";
        public static readonly string 风速报警 = "06";
        public static readonly string 力矩报警 = "07";
        public static readonly string 限位报警 = "08";
        public static readonly string 区域保护报警 = "09";
        public static readonly string 人数报警 = "10";
        public static readonly string 防坠器报警 = "11";
        public static readonly string 高度报警 = "12";
        public static readonly string 倾斜报警 = "13";
    }
}
