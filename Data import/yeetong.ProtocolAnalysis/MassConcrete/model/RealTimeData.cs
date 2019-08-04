using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.MassConcrete
{
    public class RealTimeData
    {
        /// <summary>
        /// 设备id （4位）
        /// </summary>
        public string EquipmentID { set; get; }
        /// <summary>
        /// 子机数 （3位）
        /// </summary>
        public string SubEquipmentCount { set; get; }
        /// <summary>
        /// 子机编号 （4位）
        /// </summary>
        public string SubEquipmentID { set; get; }
        /// <summary>
        /// 时间
        /// </summary>
        public string Time { set; get; }
        /// <summary>
        /// 温度通道最大数
        /// </summary>
        public string PassTemperatureMaxCount { set; get; }
        /// <summary>
        /// 通道温度数组
        /// </summary>
        public string PassTemperatureArray { set; get; }
        /// <summary>
        /// 子机电池电压
        /// </summary>
        public string SubCellVoltage { set; get; }
        /// <summary>
        /// 湿度通道数
        /// </summary>
        public string PassHumidityMaxCount { set; get; }
        /// <summary>
        /// 通道湿度数组
        /// </summary>
        public string PassHumidityArray { set; get; }
        /// <summary>
        /// 主机电池电压
        /// </summary>
        public string CellVoltage { set; get; }
    }
}
