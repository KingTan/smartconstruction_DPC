using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_request_data.科宇.卸料
{
    public class Get_Real_list_result
    {
        /// <summary>
        /// 状态信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 访问状态码 status>=1为访问成功，status<1为访问失败
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public Unload_real[] data { get; set; }
    }

    public class Unload_real
    {
        /// <summary>
        /// 设备id
        /// </summary>
        public string unload_id { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public long time { get; set; }
        /// <summary>
        /// 电量百分比
        /// </summary>
        public double electric_quantity { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        public double weight { get; set; }
        /// <summary>
        /// 偏置值 (-100~100，bias<0往左偏，bias>0往右偏)
        /// </summary>
        public double bias { get; set; }
        /// <summary>
        /// 数据状态 0正常 1预警 2报警
        /// </summary>
        public double upstate { get; set; }
        /// <summary>
        /// 预警重量
        /// </summary>
        public double early_warning_weight { get; set; }
        /// <summary>
        /// 报警重量
        /// </summary>
        public double alarm_weight { get; set; }
    }
}
