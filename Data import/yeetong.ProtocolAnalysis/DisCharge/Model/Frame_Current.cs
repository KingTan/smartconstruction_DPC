using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.DisCharge
{
    public class Frame_Current
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
        public DateTime? RTC
        {
            get;
            set;
        }
        /// <summary>
        /// 倾角X
        /// </summary>
        public double AngleX
        {
            get;
            set;
        }
        /// <summary>
        /// 倾角Y
        /// </summary>
        public double AngleY
        {
            get;
            set;
        }
        /// <summary>
        /// 当前载重
        /// </summary>
        public ushort Load
        {
            get;
            set;
        }
          /// <summary>
        /// 重量预警
        /// </summary>
        public byte WeightWarning
        {
            get;
            set;
        }
        /// <summary>
        /// 重量报警
        /// </summary>
        public byte WeightAlarm
        {
            get;
            set;
        }
        /// <summary>
        /// 倾斜预警
        /// </summary>
        public byte AngleWarning
        {
            get;
            set;
        }
        /// <summary>
        /// 倾斜报警
        /// </summary>
        public byte AngleAlarm
        {
            get;
            set;
        }
        /// <summary>
        /// 报警状态
        /// </summary>
        public string AlarmType
        {
            get;
            set;
        }

        public Frame_Current()
        {
            DeviceNo = "";
            RTC = null; 
            Load = 0;
            WeightWarning = 0;
            WeightAlarm = 0;
            AngleWarning = 0;
            AngleAlarm = 0;
            AngleX = 0d;
            AngleY = 0d;
        }
    }
}
