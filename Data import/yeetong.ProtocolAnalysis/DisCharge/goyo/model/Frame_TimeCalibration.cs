using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOYO_ProtocolAnalysis.DisCharge
{
    public class Frame_TimeCalibration
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

        public Frame_TimeCalibration()
        {
            DeviceNo = "";
            RTC = null;
        }

    }
}
