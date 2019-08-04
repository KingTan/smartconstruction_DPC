using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.FogGun
{
    public  class Frame_TimedControl
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
        /// 持续时间
        /// </summary>
        public string Timeout
        {
            get;
            set;
        }

        public Frame_TimedControl()
        {
            DeviceNo = "";
            Timeout = "";
        }
    }
}
