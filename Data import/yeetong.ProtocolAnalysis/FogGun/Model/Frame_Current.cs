using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.FogGun
{
    [Serializable]
    public  class Frame_Current
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
        /// 警报灯状态
        /// </summary>
        public byte WarningLampStatus
        {
            get;
            set;
        }
        /// <summary>
        /// 雾炮喷淋状态
        /// </summary>
        public byte FogGunStatus
        {
            get;
            set;
        }

        public Frame_Current()
        {
            DeviceNo = "";
            RTC = null;
            WarningLampStatus = 0;
            FogGunStatus = 0;
        }

    }
}
