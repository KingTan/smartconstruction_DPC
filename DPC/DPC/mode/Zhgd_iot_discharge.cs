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
        public int dip_x { get; set; }
        /// <summary>
        /// 倾角Y
        /// </summary>
        public int dip_y { get; set; }
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
    }
}
