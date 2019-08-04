using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis
{
    [Serializable]
    public class BorderProtection_Current
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNo
        {
            get;
            set;
        }
        /// <summary>
        /// 接收的RTC
        /// </summary>
        public string RTC
        {
            get;
            set;
        }
        /// <summary>
        /// GPS信号质量
        /// </summary>
        public string GPS
        {
            get;
            set;
        }
        /// <summary>
        /// 电池电量
        /// </summary>
        public string BatteryLevel
        {
            get;
            set;
        }
        /// <summary>
        /// 剩余电量百分比
        /// </summary>
        public string RemainEletricPercent
        {
            get;
            set;
        }
        /// <summary>
        /// 上次失败连接次数
        /// </summary>
        public string LinkFailCount
        {
            get;
            set;
        }
    }
}
