using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.FogGun
{
    public  class Frame_ManualControl
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
        /// 设备状态
        /// </summary>
        public string DeviceState
        {
            get;
            set;
        }

        public Frame_ManualControl()
        {
            DeviceNo = "";
            DeviceState = "";
        }
    }
}
