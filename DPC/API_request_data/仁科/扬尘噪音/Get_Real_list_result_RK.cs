using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API_request_data.仁科.扬尘噪音
{
    public class Get_Real_list_result_RK
    {
        /// <summary>
        /// 节点编码
        /// </summary>
        public string DevKey { get; set; }
        /// <summary>
        /// 节点名称
        /// </summary>
        public string DevName { get; set; }
        /// <summary>
        /// 设备类型 （0 为模拟量，1 为开关量）
        /// </summary>
        public string DevType { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DevAddr { get; set; }
        /// <summary>
        /// 模拟量一名称
        /// </summary>
        public string DevTempName { get; set; }
        /// <summary>
        /// 模拟量一值
        /// </summary>
        public string DevTempValue { get; set; }
        /// <summary>
        /// 模拟量二名称
        /// </summary>
        public string DevHumiName { get; set; }
        /// <summary>
        /// 模拟量二值
        /// </summary>
        public string DevHumiValue { get; set; }
        /// <summary>
        /// 设备状态 （false 表示离线，true 表示在线)
        /// </summary>
        public string DevStatus { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public string DevLng { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public string DevLat { get; set; }
        /// <summary>
        /// 模拟量一报警状态（0 表示不报警，1 表示超上限，2 表示超下限）
        /// </summary>
        public string TempStatus { get; set; }
        /// <summary>
        /// 模拟量二报警状态（0 表示不报警，1 表示超上限，2 表示超下限）
        /// </summary>
        public string HumiStatus { get; set; }
        /// <summary>
        /// 模拟量一相关参数设置标志（0 表示不具备设置权限，1 表示有设置权限）
        /// </summary>
        public string devDataType1 { get; set; }
        /// <summary>
        /// 模拟量二相关参数设置标志（0 表示不具备设置权限，1 表示有设置权限）
        /// </summary>
        public string devDataType2 { get; set; }
        /// <summary>
        /// 设备节点号
        /// </summary>
        public string devPos { get; set; }
    }

}
