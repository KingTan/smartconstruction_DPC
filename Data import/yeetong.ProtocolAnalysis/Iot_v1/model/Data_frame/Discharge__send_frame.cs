using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.model
{
    /// <summary>
    /// 卸料平台数据接受类
    /// </summary>
    public class Discharge__send_frame
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
        /// 倾角X
        /// </summary>
        public double dip_x { get; set; }
        /// <summary>
        /// 倾角Y
        /// </summary>
        public double dip_y { get; set; }
    }
}
