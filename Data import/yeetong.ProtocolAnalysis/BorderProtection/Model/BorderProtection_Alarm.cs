using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis
{
    [Serializable]
    public class BorderProtection_Alarm
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
        /// 设备事故
        /// </summary>
        public string EquipmentAccident
        {
            get;
            set;
        }
        /// <summary>
        /// 电池电量低报警
        /// </summary>
        public string EletricAlarm
        {
            get;
            set;
        }
    }
}
