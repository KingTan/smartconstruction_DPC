using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.DisCharge
{
    public class Frame_RunTime
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
        /// 总运行时间
        /// </summary>
        public decimal TotalRunTime
        {
            get;
            set;
        }
        /// <summary>
        /// 本次开机运行时间
        /// </summary>
        public decimal PowerOnTime
        {
            get;
            set;
        }
        public Frame_RunTime()
        {
            DeviceNo = "";
            TotalRunTime = 0;
            PowerOnTime = 0;
        }
    }
}
